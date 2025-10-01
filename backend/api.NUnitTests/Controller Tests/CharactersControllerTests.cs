using System;
using System.Collections.Generic;
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
    public class CharactersControllerTests
    {
        #region Helper Methods

        private MapleTinderDbContext GetFakedContext()
        {
            var options = new DbContextOptionsBuilder<MapleTinderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new MapleTinderDbContext(options);
        }

        private void AuthorizeUser(CharactersController controller, string userId = "test-user-123")
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

        #endregion Helper Methods

        #region Tests

        [Test]
        public async Task GetMapleCharacters_TestValidPagination_ReturnsCorrectPageAndSize()
        {
            // Assign
            using var context = GetFakedContext();
            var characters = new List<Character>
            {
                CreateTestCharacter(id: 1, name: "Character1"),
                CreateTestCharacter(id: 2, name: "Character2"),
                CreateTestCharacter(id: 3, name: "Character3"),
            };

            context.Characters.AddRange(characters);
            await context.SaveChangesAsync();

            var controller = new CharactersController(context);

            // Act
            var result = await controller.GetMapleCharacters(page: 1, pageSize: 2);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();
            
            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<IEnumerable<Character>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.Should().HaveCount(2);

            var characterList = returnedCharacters.ToList();
            characterList[0].Id.Should().Be(1);
            characterList[1].Id.Should().Be(2);
        }

        [Test]
        [TestCase(0, 10)]
        [TestCase(-1, 10)]
        [TestCase(10, 0)]
        [TestCase(10, -1)]
        [TestCase(-1, -1)]
        [TestCase(0, 0)]
        public async Task GetMapleCharacters_TestInvalidPagination_ReturnsBadRequest(int page, int pageSize)
        {
            // Arrange
            using var context = GetFakedContext();
            var controller = new CharactersController(context);

            // Act
            var result = await controller.GetMapleCharacters(page, pageSize);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<BadRequestObjectResult>();

            var okResult = actionResult as BadRequestObjectResult;
            okResult.Should().NotBeNull();
        }

        [Test]
        public async Task GetRandomCharacters_TestUnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            using var context = GetFakedContext();
            var controller = new CharactersController(context);

            // Act
            // No user setup -> anonymous user
            var result = await controller.GetRandomCharacters(10);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<UnauthorizedResult>();
        }

        [Test]
        public async Task GetRandomCharacters_AuthorizedUser_ReturnsUnseenCharacters()
        {
            // Arrange
            using var context = GetFakedContext();
            var userId = "test-user-123";

            var seenCharacter = CreateTestCharacter(id: 1, name: "Character1");

            // Add test characters
            var characters = new List<Character>
            {
                seenCharacter,
                CreateTestCharacter(id: 2, name: "Character2"),
                CreateTestCharacter(id: 3, name: "Character3"),
            };
            context.Characters.AddRange(characters);

            // Add user history (user has seen character 1)
            context.UserHistory.Add(new UserHistory
            {
                UserId = userId,
                CharacterId = seenCharacter.Id,
                Status = "Like",
                SeenAt = DateTime.UtcNow,
            });

            await context.SaveChangesAsync();

            var controller = new CharactersController(context);
            AuthorizeUser(controller, userId);

            // Act
            var result = await controller.GetRandomCharacters(2);

            // Assert
            var actionResult = result.Result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedCharacters = okResult.Value.Should().BeAssignableTo<IEnumerable<Character>>().Subject;
            returnedCharacters.Should().NotBeNull();
            returnedCharacters.Should().HaveCount(2);
            returnedCharacters.Should().NotContain(c => c.Id == seenCharacter.Id);
            returnedCharacters.Should().OnlyContain(c => c.Id != seenCharacter.Id);
        }


        #endregion Tests
    }
}
