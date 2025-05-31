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

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserHistoryController : ControllerBase
    {
        private readonly MapleTinderDbContext _context;

        public UserHistoryController(MapleTinderDbContext context)
        {
            _context = context;
        }

        // GET: api/UserHistories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserHistory>>> GetUserHistory()
        {
            return await _context.UserHistory.ToListAsync();
        }

        // GET: api/UserHistories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserHistory>> GetUserHistory(int id)
        {
            var userHistory = await _context.UserHistory.FindAsync(id);

            if (userHistory == null)
            {
                return NotFound();
            }

            return userHistory;
        }

        // PUT: api/UserHistories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserHistory(int id, UserHistory userHistory)
        {
            if (id != userHistory.Id)
            {
                return BadRequest();
            }

            _context.Entry(userHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserHistoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UserHistories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserHistory>> PostUserHistory(UserHistory userHistory)
        {
            _context.UserHistory.Add(userHistory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserHistory", new { id = userHistory.Id }, userHistory);
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
            await _context.SaveChangesAsync();

            // TODO: Create Likes Leaderboard Entity & Service.
            //await _statsService.UpdateGlobalCounters(swipes);
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

        // DELETE: api/UserHistories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserHistory(int id)
        {
            var userHistory = await _context.UserHistory.FindAsync(id);
            if (userHistory == null)
            {
                return NotFound();
            }

            _context.UserHistory.Remove(userHistory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserHistoryExists(int id)
        {
            return _context.UserHistory.Any(e => e.Id == id);
        }
    }
}
