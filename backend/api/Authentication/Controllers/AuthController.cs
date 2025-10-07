using MapleTinder.Shared.Models.Entities;
using api.Authentication.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Rewrite;
using api.Authentication.Interfaces;
using api.Authentication.Service;
using System.Security.Policy;

namespace api.Authentication.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private const string DiscordScheme = "Discord";
        private const string DiscordRedirectURI = "/auth/login/discord/success";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ITokenService _tokenService;

        public AuthController(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        // ----------------------------------------------------------
        // Local Auth
        // ----------------------------------------------------------

        #region Local Auth

        // POST: auth/register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDTO dto)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email
                };

                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded) return StatusCode(500, createResult.Errors);

                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded) return StatusCode(500, roleResult.Errors);

                // TODO: Add confirmation email logic.

                return Ok("User created!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        // POST: auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Email not found or incorrect password.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded) return Unauthorized("Email not found or incorrect password.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);
            return Ok(new LoginSuccessDTO
            {
                Token = token,
            });
        }

        #endregion Local Auth

        // ----------------------------------------------------------
        // OAuth - Discord
        // ----------------------------------------------------------

        #region OAuth Discord

        // GET: auth/login/discord
        [HttpGet("login/discord")]
        public ActionResult SignInDiscord()
        {
            // 302 redirect to Discord
            var props = _signInManager.ConfigureExternalAuthenticationProperties(DiscordScheme, DiscordRedirectURI);
            return Challenge(props, DiscordScheme);
        }

        // GET: auth/login/discord/success
        [HttpGet("login/discord/success")]
        public async Task<ActionResult> DiscordLoginSuccess()
        {
            // Get info returned by Discord
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) return BadRequest("External login info not found.");

            // Attempt to find user
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            // First time login through Discord. Create a local ApplicationUser for db.
            if (user == null)
            {
                var discordId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var discordUser = info.Principal.FindFirstValue(ClaimTypes.Name)!;

                user = new ApplicationUser
                {
                    UserName = $"discord_{discordUser}",
                    Email = $"{discordId}@discord.local",
                    EmailConfirmed = true
                };

                var create = await _userManager.CreateAsync(user);
                if (!create.Succeeded) return StatusCode(500, create.Errors);

                // link external login
                var loginResult = await _userManager.AddLoginAsync(user, info);
                if (!loginResult.Succeeded) return StatusCode(500, loginResult.Errors);

                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded) return StatusCode(500, loginResult.Errors);
            }

            // Create JWT for discord user
            var roles = await _userManager.GetRolesAsync(user);
            var jwt = _tokenService.CreateToken(user, roles);

            // Redirect back to Vue with the token
            // TODO: Refactor to use Authorization Code + PKCE
            return Redirect($"http://localhost:5173/oauth?token={jwt}");
        }


        // GET: auth/login/discord/fail
        [HttpGet("login/discord/fail")]
        public ActionResult DiscordLoginFail()
        {
            return Redirect($"http://localhost:5173/login/failed?oauthService=Discord");
        }

        #endregion OAuth Discord
    }
}
