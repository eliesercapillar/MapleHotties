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
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharactersController : ControllerBase
    {
        private readonly MapleTinderDbContext _context;

        public CharactersController(MapleTinderDbContext context)
        {
            _context = context;
        }

        // GET: api/Characters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Character>>> GetMapleCharacters(int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and PageSize must be greater than zero.");

            var characters = await _context.Characters
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(characters);
        }

        // GET: api/Characters/random?count=10
        [HttpGet("random")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Character>>> GetRandomCharacters([FromQuery] int count = 10)
        {
            var userId = User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null) return Unauthorized();

            try
            {
                var seenCharacterIds = await _context.UserHistory
                    .Where(usc => usc.UserId == userId)
                    .Select(usc => usc.CharacterId)
                    .ToHashSetAsync();

                // TODO: In the future, figure out pros/cons with
                //       raw SQL and ORDER BY NEWID() 
                //       vs.
                //       my current EF Core in-memory shuffling approach.

                var unseenCharacters = await _context.Characters
                    .Where(c => !seenCharacterIds.Contains(c.Id))
                    .Take(count * 3)
                    .ToListAsync();

                // Shuffle in-memory
                var random = new Random();
                return Ok(unseenCharacters
                    .OrderBy(x => random.Next())
                    .Take(count));

                // ORDER BY NEWID() approach
                // might not be doing it right? Im getting the same characters when testing.

                //var unseenCharacters = await _context.Characters
                //    .Where(c => !seenCharacterIds.Contains(c.Id))
                //    .OrderBy(c => EF.Functions.Random()) 
                //    .Take(count)
                //    .ToListAsync();

                //return Ok(unseenCharacters);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: api/Characters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Character>> GetCharacter(int id)
        {
            var character = await _context.Characters.FindAsync(id);

            if (character == null)
            {
                return NotFound();
            }

            return character;
        }

        // PUT: api/Characters/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCharacter(int id, Character character)
        {
            if (id != character.Id)
            {
                return BadRequest();
            }

            _context.Entry(character).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CharacterExists(id))
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

        // POST: api/Characters
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Character>> PostCharacter(Character character)
        {
            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCharacter", new { id = character.Id }, character);
        }

        // TODO: Add verification. Users can delete their own characters, Admins can delete any character.
        // DELETE: api/Characters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCharacter(int id)
        {
            var character = await _context.Characters.FindAsync(id);
            if (character == null)
            {
                return NotFound();
            }

            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CharacterExists(int id)
        {
            return _context.Characters.Any(e => e.Id == id);
        }
    }
}
