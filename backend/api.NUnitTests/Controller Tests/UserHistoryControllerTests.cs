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
        public async Task BatchSave_TestAuthorizedUser_SavesHistory()
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
        public async Task Recent_TestValidQuanity_ReturnsFirstXMostRecentCharactersDescending(int quantity)
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
        public async Task Recent_TestInvalidQuantity_ReturnsRadRequest(int quantity)
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

        [Test]
        public async Task Recent_TestUnauthorizedUser_ReturnsUnauthorized()
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

        [Test]
        public async Task Recent_TestUserWithNoHistory_ReturnsEmptyList()
        {
            // Arrange
            using var context = GetFakedContext();
            var userID = "noHistoryUser";
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
            AuthorizeUser(controller, userID);

            // Act
            var result = await controller.Recent(4);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.Should().BeEmpty();
        }

        [Test]
        public async Task Recent_TestRequestMoreThanAvailable_ReturnsAllAvailable()
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
            AuthorizeUser(controller, userId);

            var baseTime = DateTime.UtcNow;

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
            var result = await controller.Recent(10);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount(4);
            returnedHistory.Should().BeInDescendingOrder(h => h.SeenAt);
        }

        [Test]
        public async Task Recent_TestMultipleUserHistories_ReturnsOnlyCurrentUserHistory()
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var otherUserId = "test-otherUser-123";
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
            var histories = new List<UserHistory>
            {
                new UserHistory { UserId = userId, CharacterId = 1, Status = "liked", SeenAt = startTime.AddSeconds(-2)},
                new UserHistory { UserId = otherUserId, CharacterId = 2, Status = "liked", SeenAt = startTime.AddSeconds(-2)},
                new UserHistory { UserId = userId, CharacterId = 3, Status = "noped", SeenAt = startTime.AddSeconds(-1)},
                new UserHistory { UserId = userId, CharacterId = 4, Status = "favourited", SeenAt = startTime},
            };

            await context.UserHistory.AddRangeAsync(histories);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Recent(10);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount(3);
            returnedHistory.Should().BeInDescendingOrder(h => h.SeenAt);
            returnedHistory.Should().OnlyContain(dto => dto.Character.Id == 1 || dto.Character.Id == 3 || dto.Character.Id == 4);
            returnedHistory.Should().NotContain(dto => dto.Character.Id == 2);
        }

        [Test]
        public async Task All_TestValid_ReturnsFullHistoryInDescendingOrder()
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
            var result = await controller.All();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount(4);
            returnedHistory.Should().BeInDescendingOrder(h => h.SeenAt);

            foreach (var history in returnedHistory)
            {
                history.Character.Should().NotBeNull();
                history.Character.Id.Should().BeGreaterThan(0);
                history.Character.Name.Should().NotBeNullOrEmpty();
                history.Status.Should().NotBeNullOrEmpty();
                history.SeenAt.Should().BeOnOrBefore(DateTime.UtcNow);
            }

            var historyList = returnedHistory.ToList();
            historyList[0].Status.Should().Be("liked"); // Character 4
            historyList[1].Status.Should().Be("favourited"); // Character 3
            historyList[2].Status.Should().Be("noped"); // Character 2
            historyList[3].Status.Should().Be("liked"); // Character 1
        }

        [Test]
        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public async Task All_TestLargeHistory_ReturnsFullHistory(int numRecords)
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
            AuthorizeUser(controller, userId);

            var characters = Enumerable.Range(1, numRecords)
                .Select(i => CreateTestCharacter(id: i, name: $"Character{i}"))
                .ToList();

            await context.Characters.AddRangeAsync(characters);
            await context.SaveChangesAsync();

            var startTime = DateTime.UtcNow;
            var historyEntries = Enumerable.Range(1, numRecords)
               .Select(i => new UserHistory
               {
                   UserId = userId,
                   CharacterId = i,
                   Status = i % 3 == 0 ? "liked" : i % 3 == 1 ? "noped" : "favourited",
                   SeenAt = startTime.AddSeconds(-numRecords + i)
               })
               .ToList();
            await context.UserHistory.AddRangeAsync(historyEntries);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.All();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount(numRecords);
            returnedHistory.Should().BeInDescendingOrder(h => h.SeenAt);

            foreach (var history in returnedHistory)
            {
                history.Character.Should().NotBeNull();
                history.Character.Id.Should().BeGreaterThan(0);
                history.Character.Name.Should().NotBeNullOrEmpty();
                history.Status.Should().NotBeNullOrEmpty();
                history.SeenAt.Should().BeOnOrBefore(DateTime.UtcNow);
            }
        }

        [Test]
        public async Task All_TestUnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            using var context = GetFakedContext();
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);

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
        public async Task All_TestUserWithNoHistory_ReturnsEmptyList()
        {
            // Arrange
            using var context = GetFakedContext();
            var userID = "noHistoryUser";
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
            AuthorizeUser(controller, userID);

            // Act
            var result = await controller.All();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.Should().BeEmpty();
        }

        [Test]
        public async Task All_TestMultipleUserHistories_ReturnsOnlyCurrentUserHistory()
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var otherUserId1 = "test-otherUser-123";
            var otherUserId2 = "test-otherUser-789";
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
            var histories = new List<UserHistory>
            {
                new UserHistory { UserId = userId, CharacterId = 1, Status = "liked", SeenAt = startTime.AddSeconds(-2)},
                new UserHistory { UserId = userId, CharacterId = 3, Status = "noped", SeenAt = startTime.AddSeconds(-1)},
                new UserHistory { UserId = userId, CharacterId = 4, Status = "favourited", SeenAt = startTime},

                new UserHistory { UserId = otherUserId1, CharacterId = 1, Status = "noped", SeenAt = startTime.AddSeconds(-1)},
                new UserHistory { UserId = otherUserId1, CharacterId = 2, Status = "liked", SeenAt = startTime.AddSeconds(-2)},

                new UserHistory { UserId = otherUserId2, CharacterId = 1, Status = "liked", SeenAt = startTime.AddSeconds(-3)},
                new UserHistory { UserId = otherUserId2, CharacterId = 4, Status = "favourited", SeenAt = startTime.AddSeconds(-10)},
            };

            await context.UserHistory.AddRangeAsync(histories);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.All();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
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
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
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
            var historyEntry = new UserHistory
            {
                UserId = userId,
                CharacterId = 1,
                Status = "favourited",
                SeenAt = seenAt
            };
            await context.UserHistory.AddAsync(historyEntry);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.All();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
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

            dto.Status.Should().Be("favourited");
            dto.SeenAt.Should().Be(seenAt);
        }

        [Test]
        public async Task AllFavourites_TestValid_ReturnsOnlyFavouritesInDescendingOrder()
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
                new SwipeDTO { CharacterId = 4, Status = "favourited", SeenAt = startTime }
            };

            await controller.BatchSave(swipes);

            // Act
            var result = await controller.AllFavourites();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount(2);
            returnedHistory.Should().BeInDescendingOrder(h => h.SeenAt);

            foreach (var history in returnedHistory)
            {
                history.Character.Should().NotBeNull();
                history.Character.Id.Should().BeGreaterThan(0);
                history.Character.Name.Should().NotBeNullOrEmpty();
                history.Status.Should().NotBeNullOrEmpty();
                history.SeenAt.Should().BeOnOrBefore(DateTime.UtcNow);
            }

            var historyList = returnedHistory.ToList();
            historyList.Should().HaveCount(2);
            historyList[0].Status.Should().Be("favourited"); // Character 4
            historyList[1].Status.Should().Be("favourited"); // Character 3
        }

        [Test]
        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public async Task AllFavourites_TestLargeHistory_ReturnsOnlyFavourites(int numRecords)
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
            AuthorizeUser(controller, userId);

            var characters = Enumerable.Range(1, numRecords)
                .Select(i => CreateTestCharacter(id: i, name: $"Character{i}"))
                .ToList();

            await context.Characters.AddRangeAsync(characters);
            await context.SaveChangesAsync();

            var startTime = DateTime.UtcNow;
            var historyEntries = Enumerable.Range(1, numRecords)
               .Select(i => new UserHistory
               {
                   UserId = userId,
                   CharacterId = i,
                   Status = i % 3 == 0 ? "liked" : i % 3 == 1 ? "noped" : "favourited",
                   SeenAt = startTime.AddSeconds(-numRecords + i)
               })
               .ToList();
            await context.UserHistory.AddRangeAsync(historyEntries);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.AllFavourites();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount((numRecords + 1) / 3);
            returnedHistory.Should().BeInDescendingOrder(h => h.SeenAt);

            foreach (var history in returnedHistory)
            {
                history.Character.Should().NotBeNull();
                history.Character.Id.Should().BeGreaterThan(0);
                history.Character.Name.Should().NotBeNullOrEmpty();
                history.Status.Should().NotBeNullOrEmpty();
                history.SeenAt.Should().BeOnOrBefore(DateTime.UtcNow);
            }
        }

        [Test]
        public async Task AllFavourites_TestUnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            using var context = GetFakedContext();
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);

            // Act
            // No user setup -> anonymous user
            var result = await controller.AllFavourites();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<UnauthorizedResult>();

            var unauthorizedResult = actionResult as UnauthorizedResult;
            unauthorizedResult.Should().NotBeNull();
        }

        [Test]
        public async Task AllFavourites_TestUserWithNoHistory_ReturnsEmptyList()
        {
            // Arrange
            using var context = GetFakedContext();
            var userID = "noHistoryUser";
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
            AuthorizeUser(controller, userID);

            // Act
            var result = await controller.AllFavourites();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.Should().BeEmpty();
        }

        [Test]
        public async Task AllFavourites_TestMultipleUserHistories_ReturnsOnlyCurrentUserFavourites()
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var otherUserId1 = "test-otherUser-123";
            var otherUserId2 = "test-otherUser-789";
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
            var histories = new List<UserHistory>
            {
                new UserHistory { UserId = userId, CharacterId = 1, Status = "liked", SeenAt = startTime.AddSeconds(-2)},
                new UserHistory { UserId = userId, CharacterId = 3, Status = "favourited", SeenAt = startTime.AddSeconds(-1)},
                new UserHistory { UserId = userId, CharacterId = 4, Status = "favourited", SeenAt = startTime},

                new UserHistory { UserId = otherUserId1, CharacterId = 1, Status = "noped", SeenAt = startTime.AddSeconds(-1)},
                new UserHistory { UserId = otherUserId1, CharacterId = 2, Status = "liked", SeenAt = startTime.AddSeconds(-2)},

                new UserHistory { UserId = otherUserId2, CharacterId = 1, Status = "liked", SeenAt = startTime.AddSeconds(-3)},
                new UserHistory { UserId = otherUserId2, CharacterId = 4, Status = "favourited", SeenAt = startTime.AddSeconds(-10)},
            };

            await context.UserHistory.AddRangeAsync(histories);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.AllFavourites();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
            returnedHistory.Should().NotBeNull();
            returnedHistory.Should().HaveCount(2);
            returnedHistory.Should().BeInDescendingOrder(h => h.SeenAt);
            returnedHistory.Should().OnlyContain(dto => dto.Character.Id == 3 || dto.Character.Id == 4);
            returnedHistory.Should().NotContain(dto => dto.Character.Id == 1);
            returnedHistory.ToList()[0].SeenAt.Should().Be(startTime);
        }

        [Test]
        public async Task AllFavourites_TestHistoryCharacterDTOIncludesCharacterData_CharacterPropertiesPopulatedCorrectly()
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";
            var characterStatsService = A.Fake<ICharacterStatsService>();
            var controller = new UserHistoryController(context, characterStatsService);
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
            var historyEntry = new UserHistory
            {
                UserId = userId,
                CharacterId = 1,
                Status = "favourited",
                SeenAt = seenAt
            };
            await context.UserHistory.AddAsync(historyEntry);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.AllFavourites();

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedHistory = okResult.Value.Should().BeAssignableTo<IEnumerable<HistoryCharacterDTO>>().Subject;
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

            dto.Status.Should().Be("favourited");
            dto.SeenAt.Should().Be(seenAt);
        }

        #endregion Tests
    }
}
