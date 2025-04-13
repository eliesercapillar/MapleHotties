using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterScraperController : ControllerBase
    {
        private readonly CharacterScraper _scraper;

        public CharacterScraperController(CharacterScraper scraper)
        {
            _scraper = scraper;
        }

        [HttpGet("scrape/character/{characterName}")]
        public async Task<ActionResult<Character>> ScrapeCharacter(string characterName)
        {
            try
            {
                var character = await _scraper.ScrapeCharacterAsync(characterName);
                await _scraper.SaveCharacterToDatabase(character);
                return Ok(character);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error scraping character: {ex.Message}");
            }
        }

        [HttpGet("scrape/all")]
        public async Task<ActionResult> ScrapeAllCharacters([FromQuery] int maxPages = 50000)
        {
            // Start a background task for large scraping operations
            _ = Task.Run(async () =>
            {
                try
                {
                    var characters = await _scraper.ScrapeAllCharactersAsync(maxPages);
                    await _scraper.SaveCharactersToDatabase(characters);
                }
                catch (Exception ex)
                {
                    // Log the error
                    Console.WriteLine($"Error in background scraping task: {ex}");
                }
            });

            return Accepted("Scraping process started in the background.");
        }

        [HttpGet("scrape/status")]
        public ActionResult GetScrapingStatus()
        {
            // This would be enhanced with a real status tracking system
            return Ok(new
            {
                Status = "Not implemented yet",
                Message = "Status tracking will be implemented in future versions"
            });
        }
    }
}