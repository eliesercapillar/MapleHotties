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
        public async Task<ActionResult<IEnumerable<FavouriteCharacterDTO>>> All()
        {
            var userId = User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

            try
            {
                var recentHistory = await _context.UserFavourites
                    .Where(uh => uh.UserId == userId)
                    .Include(uh => uh.Character)
                    .OrderByDescending(uh => uh.SeenAt)
                    .ToListAsync();

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

        // TODO: Consolidate logic into UserHistory's BatchSave instead.
        //       Change frontend from two API calls to just one (UserHistory)
        // POST: api/UserFavourites/batch_save
        [HttpPost("batch_save")]
        [Authorize]
        public async Task<ActionResult> BatchSave([FromBody] List<SwipeDTO> swipes)
        {
            var userId = User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

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
    }
}
