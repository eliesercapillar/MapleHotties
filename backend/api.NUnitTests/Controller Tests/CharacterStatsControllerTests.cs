using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using api.Controllers;
using api.DTOs;
using api.Interfaces;
using FluentAssertions;
using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.NUnitTests.Controller_Tests
{
    public class CharacterStatsControllerTests
    {
        #region Helper Methods

        private MapleTinderDbContext GetFakedContext()
        {
            var options = new DbContextOptionsBuilder<MapleTinderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new MapleTinderDbContext(options);
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
        public async Task TopLiked_TestValidPagination_ReturnsCorrectPageAndSize()
        {
            // Assign
            using var context = GetFakedContext();
            var characters = new List<Character>
            {
                CreateTestCharacter(id: 1, name: "Most Liked"),
                CreateTestCharacter(id: 2, name: "Less Liked"),
                CreateTestCharacter(id: 3, name: "Most Noped"),
                CreateTestCharacter(id: 4, name: "Less Noped"),
                CreateTestCharacter(id: 5, name: "Most Favourited"),
                CreateTestCharacter(id: 6, name: "Less Favourited"),
                CreateTestCharacter(id: 7, name: "No Ratings"),
            };

            context.Characters.AddRange(characters);

            var characterStats = new List<CharacterStats>
            {
                CreateTestCharacterStats(id: characters[0].Id, character: characters[0], weeklyLikes: 50, monthlyLikes: 250, totalLikes: 500),
                CreateTestCharacterStats(id: characters[1].Id, character: characters[1], weeklyLikes: 10, monthlyLikes: 50 , totalLikes: 100),
                CreateTestCharacterStats(id: characters[2].Id, character: characters[2], weeklyNopes: 50, monthlyNopes: 250, totalNopes: 500),
                CreateTestCharacterStats(id: characters[3].Id, character: characters[3], weeklyNopes: 10, monthlyNopes: 50 , totalNopes: 100),
                CreateTestCharacterStats(id: characters[4].Id, character: characters[4], weeklyFavourites: 50, monthlyFavourites: 250, totalFavourites: 500),
                CreateTestCharacterStats(id: characters[5].Id, character: characters[5], weeklyFavourites: 10, monthlyFavourites: 50 , totalFavourites: 100),
                CreateTestCharacterStats(id: characters[6].Id, character: characters[6]),
            };

            context.CharacterStats.AddRange(characterStats);

            await context.SaveChangesAsync();

            var controller = new CharacterStatsController(context);

            // Act
            var result = await controller.TopLiked(page: 1, pageSize: 10);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.TotalCount.Should().Be(7);
            returnedCharacters.CurrentPage.Should().Be(1);
            returnedCharacters.PageSize.Should().Be(10);
            returnedCharacters.TotalPages.Should().Be(1);

            // Should be ordered by likes descending
            var characterList = returnedCharacters.Data.ToList();
            characterList[0].Character.Id.Should().Be(1);
            characterList[0].Count.Should().Be(500);
            characterList[1].Character.Id.Should().Be(2);
            characterList[1].Count.Should().Be(100);

            characterList.Count.Should().Be(7);
        }

        [Test]
        [TestCase(0, 10)]
        [TestCase(-1, 10)]
        [TestCase(10, 0)]
        [TestCase(10, -1)]
        [TestCase(-1, -1)]
        [TestCase(0, 0)]
        public async Task TopLiked_TestInvalidPagination_ReturnsBadRequest(int page, int pageSize)
        {
            // Arrange
            using var context = GetFakedContext();
            var controller = new CharacterStatsController(context);

            // Act
            var result = await controller.TopLiked(page, pageSize);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<BadRequestObjectResult>();

            var okResult = actionResult as BadRequestObjectResult;
            okResult.Should().NotBeNull();
        }

        [Test]
        public async Task TopNoped_TestValidPagination_ReturnsCorrectPageAndSize()
        {
            // Assign
            using var context = GetFakedContext();
            var characters = new List<Character>
            {
                CreateTestCharacter(id: 1, name: "Most Liked"),
                CreateTestCharacter(id: 2, name: "Less Liked"),
                CreateTestCharacter(id: 3, name: "Most Noped"),
                CreateTestCharacter(id: 4, name: "Less Noped"),
                CreateTestCharacter(id: 5, name: "Most Favourited"),
                CreateTestCharacter(id: 6, name: "Less Favourited"),
                CreateTestCharacter(id: 7, name: "No Ratings"),
            };

            context.Characters.AddRange(characters);

            var characterStats = new List<CharacterStats>
            {
                CreateTestCharacterStats(id: characters[0].Id, character: characters[0], weeklyLikes: 50, monthlyLikes: 250, totalLikes: 500),
                CreateTestCharacterStats(id: characters[1].Id, character: characters[1], weeklyLikes: 10, monthlyLikes: 50 , totalLikes: 100),
                CreateTestCharacterStats(id: characters[2].Id, character: characters[2], weeklyNopes: 50, monthlyNopes: 250, totalNopes: 500),
                CreateTestCharacterStats(id: characters[3].Id, character: characters[3], weeklyNopes: 10, monthlyNopes: 50 , totalNopes: 100),
                CreateTestCharacterStats(id: characters[4].Id, character: characters[4], weeklyFavourites: 50, monthlyFavourites: 250, totalFavourites: 500),
                CreateTestCharacterStats(id: characters[5].Id, character: characters[5], weeklyFavourites: 10, monthlyFavourites: 50 , totalFavourites: 100),
                CreateTestCharacterStats(id: characters[6].Id, character: characters[6]),
            };

            context.CharacterStats.AddRange(characterStats);

            await context.SaveChangesAsync();

            var controller = new CharacterStatsController(context);

            // Act
            var result = await controller.TopNoped(page: 1, pageSize: 10);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.TotalCount.Should().Be(7);
            returnedCharacters.CurrentPage.Should().Be(1);
            returnedCharacters.PageSize.Should().Be(10);
            returnedCharacters.TotalPages.Should().Be(1);

            // Should be ordered by nopes descending
            var characterList = returnedCharacters.Data.ToList();
            characterList[0].Character.Id.Should().Be(3);
            characterList[0].Count.Should().Be(500);
            characterList[1].Character.Id.Should().Be(4);
            characterList[1].Count.Should().Be(100);

            characterList.Count.Should().Be(7);
        }

        [Test]
        [TestCase(0, 10)]
        [TestCase(-1, 10)]
        [TestCase(10, 0)]
        [TestCase(10, -1)]
        [TestCase(-1, -1)]
        [TestCase(0, 0)]
        public async Task TopNoped_TestInvalidPagination_ReturnsBadRequest(int page, int pageSize)
        {
            // Arrange
            using var context = GetFakedContext();
            var controller = new CharacterStatsController(context);

            // Act
            var result = await controller.TopNoped(page, pageSize);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<BadRequestObjectResult>();

            var okResult = actionResult as BadRequestObjectResult;
            okResult.Should().NotBeNull();
        }

        [Test]
        public async Task Search_TestFilterByCharacterNameWithDefaults_ReturnsCorrectCharacter()
        {
            // Arrange
            using var context = GetFakedContext();

            var character1 = CreateTestCharacter(id: 1, name: "Xaera", level: 285, job: "Xenon", world: "Kronos");
            var character2 = CreateTestCharacter(id: 2, name: "ROCKOGUY", level: 290, job: "Adele", world: "Kronos");
            var character3 = CreateTestCharacter(id: 3, name: "DWlGHT", level: 289, job: "Dual Blade", world: "Kronos");

            context.Characters.AddRange(character1, character2, character3);

            var stats1 = CreateTestCharacterStats(id: character1.Id, character: character1, weeklyLikes: 10, totalLikes: 1000);
            var stats2 = CreateTestCharacterStats(id: character2.Id, character: character2, weeklyLikes: 0, totalLikes: 0);
            var stats3 = CreateTestCharacterStats(id: character3.Id, character: character3, weeklyLikes: 50, totalLikes: 50);

            context.CharacterStats.AddRange(stats1, stats2, stats3);

            await context.SaveChangesAsync();

            var controller = new CharacterStatsController(context);

            // Act
            var result = await controller.Search(characterName: "Xaera");

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.TotalCount.Should().Be(1);
            returnedCharacters.CurrentPage.Should().Be(1);
            returnedCharacters.PageSize.Should().Be(10);
            returnedCharacters.TotalPages.Should().Be(1);

            var characterList = returnedCharacters.Data.ToList();
            characterList.Count.Should().Be(1);
            characterList[0].Character.Id.Should().Be(1);
            characterList[0].Count.Should().Be(1000);
            characterList[0].Character.Name.Should().Be("Xaera");
        }

        #endregion Tests
    }
}
