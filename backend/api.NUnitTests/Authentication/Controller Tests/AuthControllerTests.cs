using api.Authentication.Controllers;
using api.Authentication.Interfaces;
using api.Authentication.Models.DTOs;
using MapleTinder.Shared.Models.Entities;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using api.Authentication.Service;
using api.DTOs;
using Microsoft.Build.Framework;
using NuGet.Common;

namespace api.NUnitTests.Authentication.Controller_Tests
{
    public class AuthControllerTests
    {
        private UserManager<ApplicationUser> _fakedUserManager;
        private SignInManager<ApplicationUser> _fakedSignInManager;
        private ITokenService _fakedTokenService;
        private AuthController _controller;

        [SetUp]
        public void Setup()
        {
            var userStore = A.Fake<IUserStore<ApplicationUser>>();
            _fakedUserManager = A.Fake<UserManager<ApplicationUser>>(x => x.WithArgumentsForConstructor(() =>
                new UserManager<ApplicationUser>(userStore, null, null, null, null, null, null, null, null)));

            var contextAccessor = A.Fake<IHttpContextAccessor>();
            var claimsFactory = A.Fake<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _fakedSignInManager = A.Fake<SignInManager<ApplicationUser>>(x => x.WithArgumentsForConstructor(() =>
                new SignInManager<ApplicationUser>(_fakedUserManager, contextAccessor, claimsFactory, null, null, null, null)));

            _fakedTokenService = A.Fake<ITokenService>();

            _controller = new AuthController(_fakedUserManager, _fakedSignInManager, _fakedTokenService);
        }

        [TearDown]
        public void Dispose()
        {
            _fakedUserManager?.Dispose();
        }

        [Test]
        public async Task Register_TestNewValidUser_ReturnsOk()
        {
            // Arrange
            var dto = new RegisterDTO { Email = "test@example.com", Password = "Password123!" };
            A.CallTo(() => _fakedUserManager.CreateAsync(A<ApplicationUser>._, dto.Password))
                .Returns(IdentityResult.Success);
            A.CallTo(() => _fakedUserManager.AddToRoleAsync(A<ApplicationUser>._, "User"))
                .Returns(IdentityResult.Success);

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var actionResult = result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();

            okResult.Value.Should().Be("User created!");

            A.CallTo(() => _fakedUserManager.CreateAsync(
                A<ApplicationUser>.That.Matches(
                    u => u.Email == dto.Email && u.UserName == dto.Email), dto.Password))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _fakedUserManager.AddToRoleAsync(A<ApplicationUser>._, "User"))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task Register_TestCreateUserFail_ReturnsStatusCode500()
        {
            // Arrange
            var dto = new RegisterDTO { Email = "test@example.com", Password = "Password123!" };
            var errors = new[] { new IdentityError { Description = "User creation failed" } };
            A.CallTo(() => _fakedUserManager.CreateAsync(A<ApplicationUser>._, dto.Password))
                .Returns(IdentityResult.Failed(errors));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Test]
        public async Task Register_TestAddRoleFail_ReturnsStatusCode500()
        {
            // Arrange
            var dto = new RegisterDTO { Email = "test@example.com", Password = "Password123!" };
            var errors = new[] { new IdentityError { Description = "Role assignment failed" } };
            A.CallTo(() => _fakedUserManager.CreateAsync(A<ApplicationUser>._, dto.Password))
                .Returns(IdentityResult.Success);
            A.CallTo(() => _fakedUserManager.AddToRoleAsync(A<ApplicationUser>._, "User"))
                .Returns(IdentityResult.Failed(errors));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Test]
        public async Task Login_TestValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var dto = new LoginDTO { Email = "test@example.com", Password = "Password123!" };
            var user = new ApplicationUser { Email = dto.Email, UserName = dto.Email };
            var roles = new List<string> { "User" };
            var token = "test-jwt-token";

            A.CallTo(() => _fakedUserManager.FindByEmailAsync(dto.Email))
                .Returns(user);
            A.CallTo(() => _fakedSignInManager.CheckPasswordSignInAsync(user, dto.Password, false))
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);
            A.CallTo(() => _fakedUserManager.GetRolesAsync(user))
                .Returns(roles);
            A.CallTo(() => _fakedTokenService.CreateToken(user, roles))
                .Returns(token);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var actionResult = result;
            actionResult.Should().BeOfType<OkObjectResult>();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().NotBeNull();

            var returnedToken = okResult.Value.Should().BeAssignableTo<LoginSuccessDTO>().Subject;
            returnedToken.Should().NotBeNull();
            returnedToken.Token.Should().Be(token);
        }

        [Test]
        public async Task Login_TestEmailNotFound_ReturnsBadRequest()
        {
            // Arrange
            var dto = new LoginDTO { Email = "notfound@example.com", Password = "Password123!" };
            A.CallTo(() => _fakedUserManager.FindByEmailAsync(dto.Email))!
                .Returns(Task.FromResult<ApplicationUser>(null!));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();

            var returnMessage = badRequestResult.Value.Should().BeAssignableTo<string>().Subject;
            returnMessage.Should().NotBeNull();
            returnMessage.Should().Be("Email not found or incorrect password.");
        }

        [Test]
        public async Task Login_TestInvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new LoginDTO { Email = "test@example.com", Password = "WRONGPASSWORD!" };
            var user = new ApplicationUser { Email = dto.Email, UserName = dto.Email };
            
            A.CallTo(() => _fakedUserManager.FindByEmailAsync(dto.Email)).Returns(user);
            A.CallTo(() => _fakedSignInManager.CheckPasswordSignInAsync(user, dto.Password, false))
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();

            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult.Should().NotBeNull();

            var returnMessage = unauthorizedResult.Value.Should().BeAssignableTo<string>().Subject;
            returnMessage.Should().NotBeNull();
            returnMessage.Should().Be("Email not found or incorrect password.");
        }

        //TODO: Add Discord tests once OAuth is implemented
    }


}
