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
        private readonly ILogger<CharacterScraperController> _logger;

        public CharacterScraperController(ICharacterScraperClient scraperClient, ILogger<CharacterScraperController> logger)
        {
            _scraperClient = scraperClient;
            _logger = logger;
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
        public ActionResult ScrapeAllCharacters([FromQuery] int maxPages = 50000)
        {
            _logger.LogInformation("Manual ALL scrape triggered via API. Max pages: {maxPages}", maxPages);
            try
            {
                var jobId = _scraperClient.TriggerScrapeAllAsync(maxPages);
                return Accepted(new {
                    message = "Scraping ALL process started", 
                    jobId, 
                    statusUrl = Url.Action(nameof(GetScrapeStatus), new {jobId}) 
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("scrape/status/{jobId}")]
        public async Task<ActionResult> GetScrapeStatus(string jobId)
        {
            var json = await _scraperClient.GetScrapeStatus(jobId);
            return Content(json, "application/json");
        }
    }
}