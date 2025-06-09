using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using api.DTOs;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFavouritesController : ControllerBase
    {
        private readonly MapleTinderDbContext _context;

        public UserFavouritesController(MapleTinderDbContext context)
        {
            _context = context;
        }

        // GET: api/UserFavourites
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> All()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)! ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (userId == null) return Unauthorized();

            try
            {
                // Get the most recent UserHistory entries for this user
                var recentHistory = await _context.UserFavourites
                    .Where(uh => uh.UserId == userId)
                    .Include(uh => uh.Character)
                    .OrderByDescending(uh => uh.SeenAt)
                    .ToListAsync();

                // Create the HistoryCharacterDTO list by joining the data
                var result = recentHistory.Select(history => new FavouriteCharacterDTO
                {
                    Character = history.Character,
                    SeenAt = history.SeenAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: api/UserFavourites/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserFavourite>> GetUserFavourite(string id)
        {
            var userFavourite = await _context.UserFavourites.FindAsync(id);

            if (userFavourite == null)
            {
                return NotFound();
            }

            return userFavourite;
        }

        // POST: api/UserFavourites/batch_save
        [HttpPost("batch_save")]
        [Authorize]
        public async Task<IActionResult> BatchSave([FromBody] List<SwipeDTO> swipes)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)! ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var entities = swipes.Select(e => new UserFavourite
            {
                UserId = userId,
                CharacterId = e.CharacterId,
                SeenAt = e.SeenAt
            });
            await _context.UserFavourites.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/UserFavourites/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserFavourite(string id)
        {
            var userFavourite = await _context.UserFavourites.FindAsync(id);
            if (userFavourite == null)
            {
                return NotFound();
            }

            _context.UserFavourites.Remove(userFavourite);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserFavouriteExists(string id)
        {
            return _context.UserFavourites.Any(e => e.UserId == id);
        }
    }
}
