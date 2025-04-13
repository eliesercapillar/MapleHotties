using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.Entities;

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
        public async Task<ActionResult<IEnumerable<Character>>> GetMapleCharacters()
        {
            return await _context.MapleCharacters.ToListAsync();
        }

        // GET: api/Characters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Character>> GetCharacter(int id)
        {
            var character = await _context.MapleCharacters.FindAsync(id);

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
            _context.MapleCharacters.Add(character);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCharacter", new { id = character.Id }, character);
        }

        // DELETE: api/Characters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCharacter(int id)
        {
            var character = await _context.MapleCharacters.FindAsync(id);
            if (character == null)
            {
                return NotFound();
            }

            _context.MapleCharacters.Remove(character);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CharacterExists(int id)
        {
            return _context.MapleCharacters.Any(e => e.Id == id);
        }
    }
}
