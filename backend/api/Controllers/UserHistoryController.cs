using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using api.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using api.Interfaces;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserHistoryController : ControllerBase
    {
        private readonly MapleTinderDbContext _context;
        private readonly ICharacterStatsService _characterStatsService;

        public UserHistoryController(MapleTinderDbContext context, ICharacterStatsService characterStatsService)
        {
            _context = context;
            _characterStatsService = characterStatsService;
        }

        // POST: api/UserHistories/batch_save
        [HttpPost("batch_save")]
        [Authorize]
        public async Task<ActionResult> BatchSave([FromBody] List<SwipeDTO> swipes)
        {
            var userId = User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

            var entities = swipes.Select(e => new UserHistory
            {
                UserId = userId,
                CharacterId = e.CharacterId,
                Status = e.Status!,
                SeenAt = e.SeenAt
            });
            await _context.UserHistory.AddRangeAsync(entities);
            await _characterStatsService.UpdateCharacterStatsAsync(swipes);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("recent")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<HistoryCharacterDTO>>> Recent([FromQuery] int quantity = 4)
        {
            if (quantity <= 0) return BadRequest("Query parameter 'quantity' must be greater than 0.");
            
            var userId = User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

            try
            {
                var recentHistory = await _context.UserHistory
                    .Where(uh => uh.UserId == userId)
                    .OrderByDescending(uh => uh.SeenAt)
                    .Take(quantity)
                    .ToListAsync();

                var characterIds = recentHistory.Select(uh => uh.CharacterId).ToList();

                var characters = await _context.Characters
                    .Where(c => characterIds.Contains(c.Id))
                    .ToListAsync();

                var result = recentHistory.Select(history => new HistoryCharacterDTO
                {
                    Character = characters.First(c => c.Id == history.CharacterId),
                    Status = history.Status,
                    SeenAt = history.SeenAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex) 
            {
                return Problem(ex.Message);
            }
        }

        // GET: api/UserHistories
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<HistoryCharacterDTO>>> All()
        {
            var userId = User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

            try
            {
                var recentHistory = await _context.UserHistory
                    .Where(uh => uh.UserId == userId)
                    .Include(uh => uh.Character)
                    .OrderByDescending(uh => uh.SeenAt)
                    .ToListAsync();

                var result = recentHistory.Select(history => new HistoryCharacterDTO
                {
                    Character = history.Character,
                    Status = history.Status,
                    SeenAt = history.SeenAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
