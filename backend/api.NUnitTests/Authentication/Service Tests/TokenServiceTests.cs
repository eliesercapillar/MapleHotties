using NUnit.Framework;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MapleTinder.Shared.Models.Entities;
using api.Authentication.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace api.NUnitTests.Authentication.Service_Tests
{
    public class TokenServiceTests
    {
        private IConfiguration _configuration;
        private TokenService _tokenService;
        private const string TestSigningKey = "DefinitelyAVerySecureTestSigningKeyThatIsLongEnoughYepYep12345!";
        private const string TestIssuer = "TestIssuer";
        private const string TestAudience = "TestAudience";

        [SetUp]
        public void Setup()
        {
            var configDict = new Dictionary<string, string>
            {
                { "JWT:SigningKey", TestSigningKey },
                { "JWT:Issuer", TestIssuer },
                { "JWT:Audience", TestAudience }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict!)
                .Build();

            _tokenService = new TokenService(_configuration);
        }

        [Test]
        public void CreateToken_TestValidUser_ReturnsNonEmptyToken()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-123",
                Email = "test@example.com",
                UserName = "test@example.com"
            };
            var roles = new List<string> { "User" };

            // Act
            var token = _tokenService.CreateToken(user, roles);

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        [Test]
        [TestCase(JwtRegisteredClaimNames.Sub)]
        [TestCase(JwtRegisteredClaimNames.Email)]
        [TestCase(JwtRegisteredClaimNames.Jti)]
        [TestCase("role")]
        public void CreateToken_TestValidUser_TokenContainsClaims(string claim)
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-123",
                Email = "test@example.com",
                UserName = "test@example.com"
            };
            var roles = new List<string> { "User" };

            // Act
            var token = _tokenService.CreateToken(user, roles);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            var clm = jwtToken.Claims.FirstOrDefault(c => c.Type == claim);
            clm.Should().NotBeNull();
           
            switch (claim)
            {
                case JwtRegisteredClaimNames.Sub:
                    clm.Value.Should().Be(user.Id);
                    break;
                case JwtRegisteredClaimNames.Email:
                    clm.Value.Should().Be(user.Email);
                    break;
                case JwtRegisteredClaimNames.Jti:
                    Guid.TryParse(clm.Value, out _).Should().BeTrue();
                    break;
                case "role":
                    clm.Value.Should().Be("User");
                    break;
            }
        }

        [Test]
        public void CreateToken_TestValidUserWithMultipleRoles_TokenContainsAllRoleClaims()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-123",
                Email = "test@example.com",
                UserName = "testuser"
            };
            var roles = new List<string> { "User", "Admin", "Test" };

            // Act
            var token = _tokenService.CreateToken(user, roles);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            var claims = jwtToken.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
            claims.Should().NotBeNull();
            claims.Should().HaveCount(3);
            claims.Should().Contain("User");
            claims.Should().Contain("Admin");
            claims.Should().Contain("Test");
        }

        [Test]
        public void CreateToken_TestValidUserWithNoRoles_TokenContainsNoRoleClaims()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-123",
                Email = "test@example.com",
                UserName = "testuser"
            };
            var roles = new List<string>();

            // Act
            var token = _tokenService.CreateToken(user, roles);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            var claims = jwtToken.Claims.Where(c => c.Type == "role").ToList();
            claims.Should().NotBeNull();
            claims.Should().BeEmpty();
        }

        [Test]
        public void CreateToken_TestValidUser_TokenHasCorrectIssuer()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-123",
                Email = "test@example.com",
                UserName = "testuser"
            };
            var roles = new List<string> { "User" };

            // Act
            var token = _tokenService.CreateToken(user, roles);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            jwtToken.Issuer.Should().Be(TestIssuer);
        }

        [Test]
        public void CreateToken_TestValidUser_TokenHasCorrectAudience()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-123",
                Email = "test@example.com",
                UserName = "testuser"
            };
            var roles = new List<string> { "User" };

            // Act
            var token = _tokenService.CreateToken(user, roles);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            jwtToken.Audiences.Should().Contain(TestAudience);
        }

        [Test]
        public void CreateToken_TestValidUser_TokenExpiresInSevenDays()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-123",
                Email = "test@example.com",
                UserName = "testuser"
            };
            var roles = new List<string> { "User" };
            var beforeCreation = DateTime.UtcNow;

            // Act
            var token = _tokenService.CreateToken(user, roles);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            // Changed test. It seems that DateTime is more precise than jwtToken.ValidTo
            var expectedExpiry = beforeCreation.AddDays(7);
            var tolerance = TimeSpan.FromSeconds(2);

            jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, tolerance);
        }

        [Test]
        public void CreateToken_TestCreatingTwoTokens_GeneratesDifferentJtis()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-123",
                Email = "test@example.com",
                UserName = "testuser"
            };
            var roles = new List<string> { "User" };

            // Act
            var token1 = _tokenService.CreateToken(user, roles);
            var token2 = _tokenService.CreateToken(user, roles);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken1 = handler.ReadJwtToken(token1);
            var jwtToken2 = handler.ReadJwtToken(token2);

            var jti1 = jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
            var jti2 = jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

            // Assert
            jti1.Should().NotBe(jti2);
        }
    }
}
