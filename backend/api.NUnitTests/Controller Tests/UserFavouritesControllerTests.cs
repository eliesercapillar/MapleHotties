using api.Controllers;
using api.DTOs;
using api.Interfaces;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Common;
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
    public class UserFavouritesControllerTests
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
        public async Task All_TestValid_ReturnsAllFavouritesInDescendingOrder()
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var controller = new UserFavouritesController(context);
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
            var favourites = new List<UserFavourite>
            {
                new UserFavourite { UserId = userId, CharacterId = 1, SeenAt = startTime.AddSeconds(-3)},
                new UserFavourite { UserId = userId, CharacterId = 2, SeenAt = startTime.AddSeconds(-2)},
                new UserFavourite { UserId = userId, CharacterId = 3, SeenAt = startTime.AddSeconds(-1)},
                new UserFavourite { UserId = userId, CharacterId = 4, SeenAt = startTime},
            };
            await context.UserFavourites.AddRangeAsync(favourites);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.All();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<FavouriteCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount(4);
            returnedHistory.Should().BeInDescendingOrder(h => h.SeenAt);

            foreach (var history in returnedHistory)
            {
                history.Character.Should().NotBeNull();
                history.Character.Id.Should().BeGreaterThan(0);
                history.Character.Name.Should().NotBeNullOrEmpty();
                history.SeenAt.Should().BeOnOrBefore(DateTime.UtcNow);
            }
        }

        [Test]
        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public async Task All_TestLargeFavourites_ReturnsFullList(int numRecords)
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var controller = new UserFavouritesController(context);
            AuthorizeUser(controller, userId);

            var characters = Enumerable.Range(1, numRecords)
                .Select(i => CreateTestCharacter(id: i, name: $"Character{i}"))
                .ToList();

            await context.Characters.AddRangeAsync(characters);
            await context.SaveChangesAsync();

            var startTime = DateTime.UtcNow;
            var favouriteEntries = Enumerable.Range(1, numRecords)
               .Select(i => new UserFavourite
               {
                   UserId = userId,
                   CharacterId = i,
                   SeenAt = startTime.AddSeconds(-numRecords + i)
               })
               .ToList();
            await context.UserFavourites.AddRangeAsync(favouriteEntries);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.All();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<FavouriteCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount(numRecords);
            returnedHistory.Should().BeInDescendingOrder(h => h.SeenAt);

            foreach (var history in returnedHistory)
            {
                history.Character.Should().NotBeNull();
                history.Character.Id.Should().BeGreaterThan(0);
                history.Character.Name.Should().NotBeNullOrEmpty();
                history.SeenAt.Should().BeOnOrBefore(DateTime.UtcNow);
            }
        }

        [Test]
        public async Task All_TestUnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            using var context = GetFakedContext();
            var controller = new UserFavouritesController(context);

            // Act
            // No user setup -> anonymous user
            var result = await controller.All();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<UnauthorizedResult>();

            var unauthorizedResult = actionResult as UnauthorizedResult;
            unauthorizedResult.Should().NotBeNull();
        }

        [Test]
        public async Task All_TestUserWithNoFavourites_ReturnsEmptyList()
        {
            // Arrange
            using var context = GetFakedContext();
            var userID = "noHistoryUser";
            var controller = new UserFavouritesController(context);
            AuthorizeUser(controller, userID);

            // Act
            var result = await controller.All();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<IEnumerable<FavouriteCharacterDTO>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.Should().BeEmpty();
        }

        [Test]
        public async Task All_TestMultipleUserFavourites_ReturnsOnlyCurrentUserFavourites()
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var otherUserId1 = "test-otherUser-123";
            var otherUserId2 = "test-otherUser-789";
            var controller = new UserFavouritesController(context);
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
            var favourites = new List<UserFavourite>
            {
                new UserFavourite { UserId = userId, CharacterId = 1, SeenAt = startTime.AddSeconds(-2)},
                new UserFavourite { UserId = userId, CharacterId = 3, SeenAt = startTime.AddSeconds(-1)},
                new UserFavourite { UserId = userId, CharacterId = 4, SeenAt = startTime},

                new UserFavourite { UserId = otherUserId1, CharacterId = 1, SeenAt = startTime.AddSeconds(-1)},
                new UserFavourite { UserId = otherUserId1, CharacterId = 2, SeenAt = startTime.AddSeconds(-2)},

                new UserFavourite { UserId = otherUserId2, CharacterId = 1, SeenAt = startTime.AddSeconds(-3)},
                new UserFavourite { UserId = otherUserId2, CharacterId = 4, SeenAt = startTime.AddSeconds(-10)},
            };

            await context.UserFavourites.AddRangeAsync(favourites);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.All();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<FavouriteCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount(3);
            returnedHistory.Should().BeInDescendingOrder(h => h.SeenAt);
            returnedHistory.Should().OnlyContain(dto => dto.Character.Id == 1 || dto.Character.Id == 3 || dto.Character.Id == 4);
            returnedHistory.Should().NotContain(dto => dto.Character.Id == 2);
            returnedHistory.ToList()[0].SeenAt.Should().Be(startTime);
        }

        [Test]
        public async Task All_TestHistoryCharacterDTOIncludesCharacterData_CharacterPropertiesPopulatedCorrectly()
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var controller = new UserFavouritesController(context);
            AuthorizeUser(controller, userId);

            var character = CreateTestCharacter(
                id: 1,
                name: "Xaera",
                level: 285,
                job: "Xenon",
                world: "Kronos",
                imageUrl: "https://msavatar1.nexon.net/Character/MMANDIACIHJNNOPKMBKOPNEFFFBFCPKGDBLOKKLHDCEBDCBBNEPMIOEAGBOKIGAFAOMFEEPICGIJGGMFNCIOLLEBLGPCLCJHNFAIAOBEOICGMLDFDFOLCGCHCFPIGEEAJHPOHEBINNCPAGBOJMNGCAIBFKCIKKHFIFFGCIFAHMJPECIAHFJHIJGCMLKDMPGIPGOGHLMLDDBNIMCPJFACGFPPBHNAFFDMJAANFKIKEOCOEBPMOKBLNCEJLADFCGKC.png"
            );
            await context.Characters.AddAsync(character);
            await context.SaveChangesAsync();

            var seenAt = DateTime.UtcNow;
            var favouriteEntry = new UserFavourite
            {
                UserId = userId,
                CharacterId = 1,
                SeenAt = seenAt
            };
            await context.UserFavourites.AddAsync(favouriteEntry);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.All();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<FavouriteCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount(1);

            var dto = returnedHistory.First();
            dto.Character.Should().NotBeNull();
            dto.Character.Id.Should().Be(character.Id);
            dto.Character.Name.Should().Be(character.Name);
            dto.Character.Level.Should().Be(character.Level);
            dto.Character.Job.Should().Be(character.Job);
            dto.Character.World.Should().Be(character.World);
            dto.Character.ImageUrl.Should().Be(character.ImageUrl);

            dto.SeenAt.Should().Be(seenAt);
        }

        #endregion Tests
    }
}

