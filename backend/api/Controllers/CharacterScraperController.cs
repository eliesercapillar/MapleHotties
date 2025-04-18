using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MapleTinder.Shared.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using api.Interfaces;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterScraperController : ControllerBase
    {
        private readonly ICharacterScraperClient _scraperClient;

        public CharacterScraperController(ICharacterScraperClient scraperClient)
        {
            _scraperClient = scraperClient;
        }

        [HttpPost("scrape/character/{characterName}")]
        public async Task<ActionResult<Character>> ScrapeCharacter(string characterName)
        {
            try
            {
                var character = await _scraperClient.ScrapeCharacterAsync(characterName);
                return Ok(character);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error scraping character: {ex.Message}");
            }
        }

        [HttpPost("scrape/all")]
        public async Task<ActionResult> ScrapeAllCharacters([FromQuery] int maxPages = 50000)
        {
            try
            {
                await _scraperClient.TriggerScrapeAllAsync(maxPages);
                return Accepted(new { message = "Scraping all process started" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}