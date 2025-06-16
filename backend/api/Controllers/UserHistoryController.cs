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
        public async Task<IActionResult> BatchSave([FromBody] List<SwipeDTO> swipes)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)! ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;
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
        public async Task<IActionResult> Recent([FromQuery] int quantity = 4)
        {
            if (quantity <= 0) return BadRequest("Query parameter 'quantity' must be greater than 0.");
            
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)! ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (userId == null) return Unauthorized();

            try
            {
                // Get the most recent UserHistory entries for this user
                var recentHistory = await _context.UserHistory
                    .Where(uh => uh.UserId == userId)
                    .OrderByDescending(uh => uh.SeenAt)
                    .Take(quantity)
                    .ToListAsync();

                // Extract the CharacterIds from the history entries
                var characterIds = recentHistory.Select(uh => uh.CharacterId).ToList();

                // Query the Characters table to get the character data
                var characters = await _context.Characters
                    .Where(c => characterIds.Contains(c.Id))
                    .ToListAsync();

                // Create the HistoryCharacterDTO list by joining the data
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
        public async Task<IActionResult> All()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)! ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (userId == null) return Unauthorized();

            try
            {
                // Get the most recent UserHistory entries for this user
                var recentHistory = await _context.UserHistory
                    .Where(uh => uh.UserId == userId)
                    .Include(uh => uh.Character)
                    .OrderByDescending(uh => uh.SeenAt)
                    .ToListAsync();

                // Create the HistoryCharacterDTO list by joining the data
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
