using api.Controllers;
using api.DTOs;
using api.Interfaces;
using FakeItEasy;
using FluentAssertions;
using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace api.NUnitTests.Controller_Tests
{
    public class UserHistoryControllerTests
    {
        #region Helper Methods

        private MapleTinderDbContext GetFakedContext()
        {
            var options = new DbContextOptionsBuilder<MapleTinderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new MapleTinderDbContext(options);
        }

        private void AuthorizeUser(ControllerBase controller, string userId = "test-user-123")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        private Character CreateTestCharacter(
            int? id = null,
            string? name = null,
            int? level = null,
            string? job = null,
            string? world = null,
            string? imageUrl = null,
            DateTime? scrapedAt = null)
        {
            return new Character
            {
                Id = id ?? 0,
                Name = name ?? "Default",
                Level = level ?? 100,
                Job = job ?? "Warrior",
                World = world ?? "Kronos",
                ImageUrl = imageUrl ?? "https://msavatar1.nexon.net/Character/LONG_HASH.png",
                ScrapedAt = scrapedAt ?? DateTime.UtcNow
            };
        }

        private CharacterStats CreateTestCharacterStats(
            int? id = null,
            int? weeklyLikes = null,
            int? weeklyNopes = null,
            int? weeklyFavourites = null,
            int? monthlyLikes = null,
            int? monthlyNopes = null,
            int? monthlyFavourites = null,
            int? totalLikes = null,
            int? totalNopes = null,
            int? totalFavourites = null,
            Character? character = null)
        {
            return new CharacterStats
            {
                CharacterId = id ?? 0,
                WeeklyLikes = weeklyLikes ?? 0,
                WeeklyNopes = weeklyNopes ?? 0,
                WeeklyFavourites = weeklyFavourites ?? 0,
                MonthlyLikes = monthlyLikes ?? 0,
                MonthlyNopes = monthlyNopes ?? 0,
                MonthlyFavourites = monthlyFavourites ?? 0,
                TotalLikes = totalLikes ?? 0,
                TotalNopes = totalNopes ?? 0,
                TotalFavourites = totalFavourites ?? 0,
                Character = character ?? null!
            };
        }

        #endregion Helper Methods

        #region Tests

        [Test]
        public async Task BatchSave_TestUnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            using var context = GetFakedContext();
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);

            var swipes = new List<SwipeDTO>
            {
                new SwipeDTO { CharacterId = 1, Status = "liked", SeenAt = DateTime.UtcNow },
                new SwipeDTO { CharacterId = 2, Status = "noped", SeenAt = DateTime.UtcNow }
            };

            // Act
            // No user setup -> anonymous user
            var result = await controller.BatchSave(swipes);

            // Assert
            var actionResult = result;
            actionResult.Should().BeOfType<UnauthorizedResult>();

            var unauthorizedResult = actionResult as UnauthorizedResult;
            unauthorizedResult.Should().NotBeNull();
        }

        [Test]
        public async Task BatchSave_TestAuthorizedUser_SavesHistoryAndUpdatesStats()
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
            AuthorizeUser(controller, userId);

            var swipes = new List<SwipeDTO>
            {
                new SwipeDTO { CharacterId = 1, Status = "liked", SeenAt = DateTime.UtcNow },
                new SwipeDTO { CharacterId = 2, Status = "noped", SeenAt = DateTime.UtcNow }
            };

            // Act
            var result = await controller.BatchSave(swipes);

            // Assert
            var actionResult = result;
            actionResult.Should().BeOfType<OkResult>();

            var okResult = actionResult as OkResult;
            okResult.Should().NotBeNull();

            var savedHistory = await context.UserHistory.ToListAsync();
            savedHistory.Should().NotBeNull();
            savedHistory.Should().HaveCount(2);
            savedHistory.Should().Contain(h => h.CharacterId == 1 && h.Status == "liked");
            savedHistory.Should().Contain(h => h.CharacterId == 2 && h.Status == "noped");
        }

        [Test]
        public async Task BatchSave_TestAddZeroSwipes_SuccessWithNoDataAdded()
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
            AuthorizeUser(controller, userId);

            var swipes = new List<SwipeDTO>();           

            // Act
            var result = await controller.BatchSave(swipes);

            // Assert
            var actionResult = result;
            actionResult.Should().BeOfType<OkResult>();

            var okResult = actionResult as OkResult;
            okResult.Should().NotBeNull();

            var savedHistory = await context.UserHistory.ToListAsync();
            savedHistory.Should().NotBeNull();
            savedHistory.Should().HaveCount(0);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public async Task Recent_TestValidAuthorized_ReturnsFirstXMostRecentCharacters(int quantity)
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
            AuthorizeUser(controller, userId);

            var characters = new List<Character>
            {
                CreateTestCharacter(id: 1, name: "Character1"),
                CreateTestCharacter(id: 2, name: "Character2"),
                CreateTestCharacter(id: 3, name: "Character3"),
                CreateTestCharacter(id: 4, name: "Character4")
            };
            await context.Characters.AddRangeAsync(characters);
            await context.SaveChangesAsync();

            var startTime = DateTime.UtcNow;
            var swipes = new List<SwipeDTO>
            {
                new SwipeDTO { CharacterId = 1, Status = "liked", SeenAt = startTime.AddSeconds(-3) },
                new SwipeDTO { CharacterId = 2, Status = "noped", SeenAt = startTime.AddSeconds(-2) },
                new SwipeDTO { CharacterId = 3, Status = "favourited", SeenAt = startTime.AddSeconds(-1) },
                new SwipeDTO { CharacterId = 4, Status = "liked", SeenAt = startTime }
            };

            await controller.BatchSave(swipes);

            // Act
            var result = await controller.Recent(quantity);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount(quantity);
            returnedHistory.Should().BeInDescendingOrder(h => h.SeenAt);

            // Expected order: 4, 3, 2, 1 (newest to oldest)
            var expectedCharacterIds = new[] { 4, 3, 2, 1 }.Take(quantity).ToList();
            var actualCharacterIds = returnedHistory.Select(h => h.Character.Id).ToList();
            actualCharacterIds.Should().Equal(expectedCharacterIds);

            foreach (var history in returnedHistory)
            {
                history.Character.Should().NotBeNull();
                history.Character.Id.Should().BeGreaterThan(0);
                history.Character.Name.Should().NotBeNullOrEmpty();
                history.Status.Should().NotBeNullOrEmpty();
                history.SeenAt.Should().BeOnOrBefore(DateTime.UtcNow);
            }

            var historyList = returnedHistory.ToList();
            if (quantity >= 1) historyList[0].Status.Should().Be("liked"); // Character 4
            if (quantity >= 2) historyList[1].Status.Should().Be("favourited"); // Character 3
            if (quantity >= 3) historyList[2].Status.Should().Be("noped"); // Character 2
            if (quantity >= 4) historyList[3].Status.Should().Be("liked"); // Character 1
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task Recent_TestInvalidAuthorized_ReturnsRadRequest(int quantity)
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
            AuthorizeUser(controller, userId);

            var swipes = new List<SwipeDTO>();
            await controller.BatchSave(swipes);

            // Act
            var result = await controller.Recent(quantity);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<BadRequestObjectResult>();

            var okResult = actionResult as BadRequestObjectResult;
            okResult.Should().NotBeNull();
        }

        public async Task Recent_TestUnauthorized_ReturnsUnauthorized()
        {
            // Arrange
            using var context = GetFakedContext();
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);

            // Act
            // No user setup -> anonymous user
            var result = await controller.Recent(5);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<UnauthorizedResult>();

            var unauthorizedResult = actionResult as UnauthorizedResult;
            unauthorizedResult.Should().NotBeNull();
        }

        #endregion Tests
    }
}
