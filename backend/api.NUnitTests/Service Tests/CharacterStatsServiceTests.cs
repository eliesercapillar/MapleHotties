using api.Services;
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

namespace api.NUnitTests.Service_Tests
{
    public class CharacterStatsServiceTests
    {
        #region Helper Methods

        private MapleTinderDbContext GetFakedContext()
        {
            var options = new DbContextOptionsBuilder<MapleTinderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new MapleTinderDbContext(options);
        }

        #endregion

        #region Tests

        [Test]
        public async Task UpdateCharacterStatsAsync_TestZeroSwipes_SuccessWithNoDataUpdated()
        {
            // Arrange
            using var context = GetFakedContext();
            var service = new CharacterStatsService(context);
            var swipes = new List<SwipeDTO>();

            // Act
            await service.UpdateCharacterStatsAsync(swipes);

            // Assert
            var stats = await context.CharacterStats.ToListAsync();
            stats.Should().NotBeNull();
            stats.Should().BeEmpty();
        }

        [Test]
        [TestCase("liked")]
        [TestCase("noped")]
        [TestCase("favourited")]
        public async Task UpdateCharacterStatsAsync_TestNewCharacter_IncrementsCorrectStat(string status)
        {
            // Arrange
            using var context = GetFakedContext();
            var service = new CharacterStatsService(context);

            var swipes = new List<SwipeDTO>
            {
                new SwipeDTO { CharacterId = 1, Status = status, SeenAt = DateTime.UtcNow }
            };

            // Act
            await service.UpdateCharacterStatsAsync(swipes);

            // Assert
            var stats = await context.CharacterStats.FirstOrDefaultAsync(cs => cs.CharacterId == 1);
            stats.Should().NotBeNull();
            stats.TotalLikes.Should().Be(status.ToLower() == "liked" || status.ToLower() == "favourited" ? 1 : 0);
            stats.TotalNopes.Should().Be(status.ToLower() == "noped" ? 1 : 0);
            stats.TotalFavourites.Should().Be(status.ToLower() == "favourited" ? 1 : 0);
        }

        [Test]
        [TestCase("liked")]
        [TestCase("Liked")]
        [TestCase("LiKEd")]
        [TestCase("LIKED")]
        public async Task UpdateCharacterStatsAsync_TestStatusCaseInsensitive_WorksCorrectly(string status)
        {
            // Arrange
            using var context = GetFakedContext();
            var service = new CharacterStatsService(context);

            var swipes = new List<SwipeDTO>
            {
                new SwipeDTO { CharacterId = 1, Status = status, SeenAt = DateTime.UtcNow }
            };

            // Act
            await service.UpdateCharacterStatsAsync(swipes);

            // Assert
            var stats = await context.CharacterStats.FirstOrDefaultAsync(cs => cs.CharacterId == 1);
            stats.Should().NotBeNull();
            stats.TotalLikes.Should().Be(1);
        }

        [Test]
        public async Task UpdateCharacterStatsAsync_TestExistingStats_IncrementsCorrectly()
        {
            // Arrange
            using var context = GetFakedContext();
            var service = new CharacterStatsService(context);

            var existingStats = new CharacterStats
            {
                CharacterId = 1,
                TotalLikes = 5,
                TotalNopes = 3,
                TotalFavourites = 2
            };
            await context.CharacterStats.AddAsync(existingStats);
            await context.SaveChangesAsync();

            var swipes = new List<SwipeDTO>
            {
                new SwipeDTO { CharacterId = 1, Status = "liked", SeenAt = DateTime.UtcNow }
            };

            // Act
            await service.UpdateCharacterStatsAsync(swipes);

            // Assert
            var stats = await context.CharacterStats.FirstOrDefaultAsync(cs => cs.CharacterId == 1);
            stats.Should().NotBeNull();
            stats.TotalLikes.Should().Be(6);
            stats.TotalNopes.Should().Be(3);
            stats.TotalFavourites.Should().Be(2);
        }

        [Test]
        public async Task UpdateCharacterStatsAsync_TestSameCharacter_IncrementsMultipleTimes()
        {
            // Arrange
            using var context = GetFakedContext();
            var service = new CharacterStatsService(context);

            var swipes = new List<SwipeDTO>
            {
                new SwipeDTO { CharacterId = 1, Status = "liked", SeenAt = DateTime.UtcNow },
                new SwipeDTO { CharacterId = 1, Status = "liked", SeenAt = DateTime.UtcNow },
                new SwipeDTO { CharacterId = 1, Status = "noped", SeenAt = DateTime.UtcNow }
            };

            // Act
            await service.UpdateCharacterStatsAsync(swipes);

            // Assert
            var stats = await context.CharacterStats.FirstOrDefaultAsync(cs => cs.CharacterId == 1);
            stats.Should().NotBeNull();
            stats.TotalLikes.Should().Be(2);
            stats.TotalNopes.Should().Be(1);
            stats.TotalFavourites.Should().Be(0);
        }

        [Test]
        public async Task UpdateCharacterStatsAsync_MultipleCharacters_UpdatesAllCorrectly()
        {
            // Arrange
            using var context = GetFakedContext();
            var service = new CharacterStatsService(context);

            var swipes = new List<SwipeDTO>
            {
                new SwipeDTO { CharacterId = 1, Status = "liked", SeenAt = DateTime.UtcNow },
                new SwipeDTO { CharacterId = 2, Status = "noped", SeenAt = DateTime.UtcNow },
                new SwipeDTO { CharacterId = 3, Status = "favourited", SeenAt = DateTime.UtcNow },
                new SwipeDTO { CharacterId = 1, Status = "liked", SeenAt = DateTime.UtcNow },
            };

            // Act
            await service.UpdateCharacterStatsAsync(swipes);

            // Assert
            var allStats = await context.CharacterStats.ToListAsync();
            allStats.Should().NotBeNull();
            allStats.Should().HaveCount(3);

            var char1Stats = allStats.First(s => s.CharacterId == 1);
            char1Stats.Should().NotBeNull();
            char1Stats.TotalLikes.Should().Be(2);
            char1Stats.TotalNopes.Should().Be(0);
            char1Stats.TotalFavourites.Should().Be(0);

            var char2Stats = allStats.First(s => s.CharacterId == 2);
            char2Stats.Should().NotBeNull();
            char2Stats.TotalLikes.Should().Be(0);
            char2Stats.TotalNopes.Should().Be(1);
            char2Stats.TotalFavourites.Should().Be(0);

            var char3Stats = allStats.First(s => s.CharacterId == 3);
            char3Stats.Should().NotBeNull();
            char3Stats.TotalLikes.Should().Be(1);
            char3Stats.TotalNopes.Should().Be(0);
            char3Stats.TotalFavourites.Should().Be(1);
        }

        [Test]
        public async Task UpdateCharacterStatsAsync_TestNewAndExistingCharacters_UpdatesAllCorrectly()
        {
            // Arrange
            using var context = GetFakedContext();
            var service = new CharacterStatsService(context);

            var existingStats = new CharacterStats
            {
                CharacterId = 1,
                TotalLikes = 5,
                TotalNopes = 3,
                TotalFavourites = 2
            };
            await context.CharacterStats.AddAsync(existingStats);
            await context.SaveChangesAsync();

            var swipes = new List<SwipeDTO>
            {
                new SwipeDTO { CharacterId = 1, Status = "liked", SeenAt = DateTime.UtcNow },
                new SwipeDTO { CharacterId = 2, Status = "noped", SeenAt = DateTime.UtcNow },
                new SwipeDTO { CharacterId = 3, Status = "favourited", SeenAt = DateTime.UtcNow },
            };

            // Act
            await service.UpdateCharacterStatsAsync(swipes);

            // Assert
            var allStats = await context.CharacterStats.ToListAsync();
            allStats.Should().NotBeNull();
            allStats.Should().HaveCount(3);

            var char1Stats = allStats.First(s => s.CharacterId == 1);
            char1Stats.Should().NotBeNull();
            char1Stats.TotalLikes.Should().Be(6);
            char1Stats.TotalNopes.Should().Be(3);
            char1Stats.TotalFavourites.Should().Be(2);

            var char2Stats = allStats.First(s => s.CharacterId == 2);
            char2Stats.Should().NotBeNull();
            char2Stats.TotalLikes.Should().Be(0);
            char2Stats.TotalNopes.Should().Be(1);
            char2Stats.TotalFavourites.Should().Be(0);

            var char3Stats = allStats.First(s => s.CharacterId == 3);
            char3Stats.Should().NotBeNull();
            char3Stats.TotalLikes.Should().Be(1);
            char3Stats.TotalNopes.Should().Be(0);
            char3Stats.TotalFavourites.Should().Be(1);
        }

        [Test]
        [TestCase("abcefg")]
        [TestCase("")]
        [TestCase(null)]
        public async Task UpdateCharacterStatsAsync_TestInvalidStatus_DoesNotIncrement(string? status)
        {
            // Arrange
            using var context = GetFakedContext();
            var service = new CharacterStatsService(context);

            var swipes = new List<SwipeDTO>
            {
                new SwipeDTO { CharacterId = 1, Status = status, SeenAt = DateTime.UtcNow }
            };

            // Act
            await service.UpdateCharacterStatsAsync(swipes);

            // Assert
            var stats = await context.CharacterStats.FirstOrDefaultAsync(cs => cs.CharacterId == 1);
            stats.Should().NotBeNull(); // Should still create the CharacterStats
            stats.TotalLikes.Should().Be(0);
            stats.TotalNopes.Should().Be(0);
            stats.TotalFavourites.Should().Be(0);
        }

        [Test]
        public async Task UpdateCharacterStatsAsync_TestLargeNumberOfSwipes_UpdatesAllCorrectly()
        {
            // Arrange
            using var context = GetFakedContext();
            var service = new CharacterStatsService(context);

            // 10 character ids
            // 10 swipes each
            var swipes = new List<SwipeDTO>();
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    swipes.Add(new SwipeDTO
                    {
                        CharacterId = i,
                        Status = j % 3 == 0 ? "liked" : j % 3 == 1 ? "noped" : "favourited",
                        SeenAt = DateTime.UtcNow
                    });
                }
            }

            // Act
            await service.UpdateCharacterStatsAsync(swipes);

            // Assert
            var allStats = await context.CharacterStats.ToListAsync();
            allStats.Should().NotBeNull();
            allStats.Should().HaveCount(10);

            // Each character should have 10 swipes:
            // love (0,3,6,9) = 4
            // nope (1,4,7) = 3
            // favourite (2,5,8) = 3
            var expectedFavourites = 3;
            var expectedLikes = 4 + expectedFavourites;
            var expectedNopes = 3;
            foreach (var stats in allStats)
            {
                stats.TotalLikes.Should().Be(expectedLikes);
                stats.TotalNopes.Should().Be(expectedNopes);
                stats.TotalFavourites.Should().Be(expectedFavourites);
            }
        }

        #endregion Tests

    }
}
