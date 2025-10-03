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

        [Test]
        public async Task Search_TestFilterByRankingTypeCaseInsensitive_ReturnsAllCharactersInCorrectOrder()
        {
            // Arrange
            using var context = GetFakedContext();

            var character1 = CreateTestCharacter(id: 1, name: "Xaera", level: 285, job: "Xenon", world: "Kronos");
            var character2 = CreateTestCharacter(id: 2, name: "ROCKOGUY", level: 290, job: "Adele", world: "Kronos");
            var character3 = CreateTestCharacter(id: 3, name: "DWlGHT", level: 289, job: "Dual Blade", world: "Kronos");
            var character4 = CreateTestCharacter(id: 4, name: "Vinsaint", level: 285, job: "Shadower", world: "Kronos");
            var character5 = CreateTestCharacter(id: 5, name: "Ciao", level: 282, job: "Dawn Warrior", world: "Kronos");
            var character6 = CreateTestCharacter(id: 6, name: "Ereklo", level: 295, job: "Adele", world: "Bera");
            var character7 = CreateTestCharacter(id: 7, name: "Game", level: 293, job: "Bow Master", world: "Scania");

            context.Characters.AddRange(character1, character2, character3, character4, character5, character6, character7);

            var stats1 = CreateTestCharacterStats(id: character1.Id, character: character1, weeklyLikes: 10, totalLikes: 1000, totalFavourites: 1);
            var stats2 = CreateTestCharacterStats(id: character2.Id, character: character2, weeklyLikes: 0, totalLikes: 0);
            var stats3 = CreateTestCharacterStats(id: character3.Id, character: character3, weeklyLikes: 50, totalLikes: 50, totalFavourites: 2);
            var stats4 = CreateTestCharacterStats(id: character4.Id, character: character4, weeklyLikes: 42, totalLikes: 69, totalFavourites: 3);
            var stats5 = CreateTestCharacterStats(id: character5.Id, character: character5, weeklyLikes: 300, totalLikes: 300);
            var stats6 = CreateTestCharacterStats(id: character6.Id, character: character6, weeklyLikes: 1, totalLikes: 2);
            var stats7 = CreateTestCharacterStats(id: character7.Id, character: character7, weeklyLikes: 0, totalLikes: 1);


            context.CharacterStats.AddRange(stats1, stats2, stats3, stats4, stats5, stats6, stats7);

            await context.SaveChangesAsync();

            var controller = new CharacterStatsController(context);

            // Act
            var result = await controller.Search(rankingType: "fAVoUrItES");

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

            var characterList = returnedCharacters.Data.ToList();
            characterList.Count.Should().Be(7);
            characterList[0].Character.Id.Should().Be(4);
            characterList[0].Count.Should().Be(3);
            characterList[0].Character.Name.Should().Be("Vinsaint");
            characterList[1].Character.Id.Should().Be(3);
            characterList[1].Count.Should().Be(2);
            characterList[1].Character.Name.Should().Be("DWlGHT");
            characterList[2].Character.Id.Should().Be(1);
            characterList[2].Count.Should().Be(1);
            characterList[2].Character.Name.Should().Be("Xaera");

            for (int i = 3; i < characterList.Count; i++)
            {
                characterList[i].Count.Should().Be(0);
            }
        }

        [Test]
        public async Task Search_TestFilterByClassTypeCaseInsensitive_ReturnsOnlyCharactersWithMatchingClass()
        {
            // Arrange
            using var context = GetFakedContext();

            var character1 = CreateTestCharacter(id: 1, name: "Xaera", level: 285, job: "Xenon", world: "Kronos");
            var character2 = CreateTestCharacter(id: 2, name: "ROCKOGUY", level: 290, job: "Adele", world: "Kronos");
            var character3 = CreateTestCharacter(id: 3, name: "DWlGHT", level: 289, job: "Dual Blade", world: "Kronos");
            var character4 = CreateTestCharacter(id: 4, name: "Vinsaint", level: 285, job: "Shadower", world: "Kronos");
            var character5 = CreateTestCharacter(id: 5, name: "Ciao", level: 282, job: "Dawn Warrior", world: "Kronos");
            var character6 = CreateTestCharacter(id: 6, name: "Ereklo", level: 295, job: "Adele", world: "Bera");
            var character7 = CreateTestCharacter(id: 7, name: "Game", level: 293, job: "Bow Master", world: "Scania");

            context.Characters.AddRange(character1, character2, character3, character4, character5, character6, character7);

            var stats1 = CreateTestCharacterStats(id: character1.Id, character: character1, weeklyLikes: 10, totalLikes: 1000);
            var stats2 = CreateTestCharacterStats(id: character2.Id, character: character2, weeklyLikes: 0, totalLikes: 0);
            var stats3 = CreateTestCharacterStats(id: character3.Id, character: character3, weeklyLikes: 50, totalLikes: 50);
            var stats4 = CreateTestCharacterStats(id: character4.Id, character: character4, weeklyLikes: 42, totalLikes: 69);
            var stats5 = CreateTestCharacterStats(id: character5.Id, character: character5, weeklyLikes: 300, totalLikes: 300);
            var stats6 = CreateTestCharacterStats(id: character6.Id, character: character6, weeklyLikes: 1, totalLikes: 2);
            var stats7 = CreateTestCharacterStats(id: character7.Id, character: character7, weeklyLikes: 0, totalLikes: 1);


            context.CharacterStats.AddRange(stats1, stats2, stats3, stats4, stats5, stats6, stats7);

            await context.SaveChangesAsync();

            var controller = new CharacterStatsController(context);

            // Act
            var result = await controller.Search(classType: "aDeLE");

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.TotalCount.Should().Be(2);
            returnedCharacters.CurrentPage.Should().Be(1);
            returnedCharacters.PageSize.Should().Be(10);
            returnedCharacters.TotalPages.Should().Be(1);

            var characterList = returnedCharacters.Data.ToList();
            characterList.Count.Should().Be(2);
            characterList[0].Character.Id.Should().Be(6);
            characterList[0].Count.Should().Be(2);
            characterList[0].Character.Name.Should().Be("Ereklo");
            characterList[1].Character.Id.Should().Be(2);
            characterList[1].Count.Should().Be(0);
            characterList[1].Character.Name.Should().Be("ROCKOGUY");
        }

        [Test]
        public async Task Search_TestFilterByTimeTypeCaseInsensitive_ReturnsAllCharactersInCorrectOrder()
        {
            // Arrange
            using var context = GetFakedContext();

            var character1 = CreateTestCharacter(id: 1, name: "Xaera", level: 285, job: "Xenon", world: "Kronos");
            var character2 = CreateTestCharacter(id: 2, name: "ROCKOGUY", level: 290, job: "Adele", world: "Kronos");
            var character3 = CreateTestCharacter(id: 3, name: "DWlGHT", level: 289, job: "Dual Blade", world: "Kronos");
            var character4 = CreateTestCharacter(id: 4, name: "Vinsaint", level: 285, job: "Shadower", world: "Kronos");
            var character5 = CreateTestCharacter(id: 5, name: "Ciao", level: 282, job: "Dawn Warrior", world: "Kronos");
            var character6 = CreateTestCharacter(id: 6, name: "Ereklo", level: 295, job: "Adele", world: "Bera");
            var character7 = CreateTestCharacter(id: 7, name: "Game", level: 293, job: "Bow Master", world: "Scania");

            context.Characters.AddRange(character1, character2, character3, character4, character5, character6, character7);

            var stats1 = CreateTestCharacterStats(id: character1.Id, character: character1, weeklyLikes: 10, monthlyLikes: 6, totalLikes: 1000);
            var stats2 = CreateTestCharacterStats(id: character2.Id, character: character2, weeklyLikes: 0, monthlyLikes: 0, totalLikes: 0);
            var stats3 = CreateTestCharacterStats(id: character3.Id, character: character3, weeklyLikes: 50, monthlyLikes: 5, totalLikes: 50);
            var stats4 = CreateTestCharacterStats(id: character4.Id, character: character4, weeklyLikes: 42, monthlyLikes: 4, totalLikes: 69);
            var stats5 = CreateTestCharacterStats(id: character5.Id, character: character5, weeklyLikes: 300, monthlyLikes: 3, totalLikes: 300);
            var stats6 = CreateTestCharacterStats(id: character6.Id, character: character6, weeklyLikes: 1, monthlyLikes: 2, totalLikes: 2);
            var stats7 = CreateTestCharacterStats(id: character7.Id, character: character7, weeklyLikes: 0, monthlyLikes: 0, totalLikes: 1);


            context.CharacterStats.AddRange(stats1, stats2, stats3, stats4, stats5, stats6, stats7);

            await context.SaveChangesAsync();

            var controller = new CharacterStatsController(context);

            // Act
            var result = await controller.Search(timeType: "MonTHLy");

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

            var characterList = returnedCharacters.Data.ToList();
            characterList.Count.Should().Be(7);
            characterList[0].Character.Id.Should().Be(1);
            characterList[0].Count.Should().Be(6);
            characterList[0].Character.Name.Should().Be("Xaera");
            characterList[1].Character.Id.Should().Be(3);
            characterList[1].Count.Should().Be(5);
            characterList[1].Character.Name.Should().Be("DWlGHT");
            characterList[2].Character.Id.Should().Be(4);
            characterList[2].Count.Should().Be(4);
            characterList[2].Character.Name.Should().Be("Vinsaint");
            characterList[3].Character.Id.Should().Be(5);
            characterList[3].Count.Should().Be(3);
            characterList[3].Character.Name.Should().Be("Ciao");
            characterList[4].Character.Id.Should().Be(6);
            characterList[4].Count.Should().Be(2);
            characterList[4].Character.Name.Should().Be("Ereklo");
            characterList[5].Character.Id.Should().Be(2);
            characterList[5].Count.Should().Be(0);
            characterList[5].Character.Name.Should().Be("ROCKOGUY"); // Tiebreaker, Lower ID
            characterList[6].Character.Id.Should().Be(7);
            characterList[6].Count.Should().Be(0);
            characterList[6].Character.Name.Should().Be("Game");     // Tiebreaker, Lower ID
        }

        [Test]
        public async Task Search_TestFilterByWorldTypeCaseInsensitive_ReturnsOnlyCharactersWithMatchingWorld()
        {
            // Arrange
            using var context = GetFakedContext();

            var character1 = CreateTestCharacter(id: 1, name: "Xaera", level: 285, job: "Xenon", world: "Kronos");
            var character2 = CreateTestCharacter(id: 2, name: "ROCKOGUY", level: 290, job: "Adele", world: "Hyperion");
            var character3 = CreateTestCharacter(id: 3, name: "DWlGHT", level: 289, job: "Dual Blade", world: "Reboot");
            var character4 = CreateTestCharacter(id: 4, name: "Vinsaint", level: 285, job: "Shadower", world: "Reboot");
            var character5 = CreateTestCharacter(id: 5, name: "Ciao", level: 282, job: "Dawn Warrior", world: "Scania");
            var character6 = CreateTestCharacter(id: 6, name: "Ereklo", level: 295, job: "Adele", world: "Bera");
            var character7 = CreateTestCharacter(id: 7, name: "Game", level: 293, job: "Bow Master", world: "Scania");

            context.Characters.AddRange(character1, character2, character3, character4, character5, character6, character7);

            var stats1 = CreateTestCharacterStats(id: character1.Id, character: character1, weeklyLikes: 10, totalLikes: 1000);
            var stats2 = CreateTestCharacterStats(id: character2.Id, character: character2, weeklyLikes: 0, totalLikes: 0);
            var stats3 = CreateTestCharacterStats(id: character3.Id, character: character3, weeklyLikes: 50, totalLikes: 50);
            var stats4 = CreateTestCharacterStats(id: character4.Id, character: character4, weeklyLikes: 42, totalLikes: 69);
            var stats5 = CreateTestCharacterStats(id: character5.Id, character: character5, weeklyLikes: 300, totalLikes: 300);
            var stats6 = CreateTestCharacterStats(id: character6.Id, character: character6, weeklyLikes: 1, totalLikes: 2);
            var stats7 = CreateTestCharacterStats(id: character7.Id, character: character7, weeklyLikes: 0, totalLikes: 1);


            context.CharacterStats.AddRange(stats1, stats2, stats3, stats4, stats5, stats6, stats7);

            await context.SaveChangesAsync();

            var controller = new CharacterStatsController(context);

            // Act
            var result = await controller.Search(worldType: "rebOOt");

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.TotalCount.Should().Be(2);
            returnedCharacters.CurrentPage.Should().Be(1);
            returnedCharacters.PageSize.Should().Be(10);
            returnedCharacters.TotalPages.Should().Be(1);

            var characterList = returnedCharacters.Data.ToList();
            characterList.Count.Should().Be(2);
            characterList[0].Character.Id.Should().Be(4);
            characterList[0].Count.Should().Be(69);
            characterList[0].Character.Name.Should().Be("Vinsaint");
            characterList[1].Character.Id.Should().Be(3);
            characterList[1].Count.Should().Be(50);
            characterList[1].Character.Name.Should().Be("DWlGHT");
        }

        [Test]
        public async Task Search_NoMatchingResults_ReturnsEmptyList()
        {
            // Search for something that doesn't exist
        }


        [Test]
        [TestCase("hotties", "weekly")]
        [TestCase("hotties", "monthly")]
        [TestCase("hotties", "all")]
        [TestCase("notties", "weekly")]
        [TestCase("notties", "monthly")]
        [TestCase("notties", "all")]
        [TestCase("favourites", "weekly")]
        [TestCase("favourites", "monthly")]
        [TestCase("favourites", "all")]
        public async Task Search_TestRankingAndTimeTypeCombinations_UsesCorrectProperty(string rankingType, string timeType)
        {
            // Arrange
            using var context = GetFakedContext();
            var character1 = CreateTestCharacter(id: 1, name: "Character1");
            var character2 = CreateTestCharacter(id: 2, name: "Character2");
            context.Characters.AddRange(character1, character2);

            var stats1 = CreateTestCharacterStats(
                id: 1, character: character1,
                weeklyLikes: 10, monthlyLikes: 100, totalLikes: 1000,
                weeklyNopes: 5, monthlyNopes: 50, totalNopes: 500,
                weeklyFavourites: 2, monthlyFavourites: 20, totalFavourites: 200);

            var stats2 = CreateTestCharacterStats(
                id: 2, character: character2,
                weeklyLikes: 20, monthlyLikes: 200, totalLikes: 2000,
                weeklyNopes: 10, monthlyNopes: 100, totalNopes: 1000,
                weeklyFavourites: 4, monthlyFavourites: 40, totalFavourites: 400);

            context.CharacterStats.AddRange(stats1, stats2);
            await context.SaveChangesAsync();

            var controller = new CharacterStatsController(context);

            // Act
            var result = await controller.Search(rankingType: rankingType, timeType: timeType);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.TotalCount.Should().Be(2);
            returnedCharacters.CurrentPage.Should().Be(1);
            returnedCharacters.PageSize.Should().Be(10);
            returnedCharacters.TotalPages.Should().Be(1);

            var characterList = returnedCharacters.Data.ToList();
            characterList.Count.Should().Be(2);
            characterList[0].Character.Id.Should().Be(2);
            characterList[0].Character.Name.Should().Be("Character2");
            characterList[1].Character.Id.Should().Be(1);
            characterList[1].Character.Name.Should().Be("Character1");

            var expectedCount = GetExpectedCount(stats2, rankingType, timeType);
            characterList[0].Count.Should().Be(expectedCount);
            
            expectedCount = GetExpectedCount(stats1, rankingType, timeType);
            characterList[1].Count.Should().Be(expectedCount);

            int GetExpectedCount(CharacterStats stats, string rankingType, string timeType)
            {
                return (rankingType.ToLower(), timeType.ToLower()) switch
                {
                    ("hotties", "weekly") => stats.WeeklyLikes,
                    ("hotties", "monthly") => stats.MonthlyLikes,
                    ("hotties", _) => stats.TotalLikes,
                    ("notties", "weekly") => stats.WeeklyNopes,
                    ("notties", "monthly") => stats.MonthlyNopes,
                    ("notties", _) => stats.TotalNopes,
                    ("favourites", "weekly") => stats.WeeklyFavourites,
                    ("favourites", "monthly") => stats.MonthlyFavourites,
                    ("favourites", _) => stats.TotalFavourites,
                    _ => stats.TotalLikes
                };
            }
        }

        [Test]
        [TestCase(0, 10)]
        [TestCase(-1, 10)]
        [TestCase(10, 0)]
        [TestCase(10, -1)]
        [TestCase(-1, -1)]
        [TestCase(0, 0)]
        public async Task Search_TestInvalidPagination_ReturnsBadRequest(int page, int pageSize)
        {
            // Arrange
            using var context = GetFakedContext();
            var controller = new CharacterStatsController(context);

            // Act
            var result = await controller.Search(page, pageSize);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<BadRequestObjectResult>();

            var okResult = actionResult as BadRequestObjectResult;
            okResult.Should().NotBeNull();
        }

        #endregion Tests
    }
}
