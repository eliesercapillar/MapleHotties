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
        private static Dictionary<Guid, Task> _runningJobs = new();

        public CharacterScraperController(CharacterScraper scraper)
        {
            _scraper = scraper;
        }

        // POST: api/CharacterScraper/scrape/character/ROCKOGUY
        [HttpPost("scrape/character/{characterName}")]
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

        // POST: api/CharacterScraper/scrape/all?maxPages=50000
        [HttpPost("scrape/all")]
        public ActionResult ScrapeAllCharacters([FromQuery] int maxPages = 50000)
        {
            var jobId = Guid.NewGuid();

            _runningJobs[jobId] = Task.Run(async () =>
            {
                try
                {
                    var characters = await _scraper.ScrapeAllCharactersAsync(maxPages);
                    await _scraper.SaveCharactersToDatabase(characters);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in job {jobId}: {ex}");
                }
                finally
                {
                    // Clean up completed job
                    _runningJobs.Remove(jobId);
                }
            });

            return Accepted(new
            {
                message = "Scraping all process started",
                jobId = jobId
            });
        }

        [HttpGet("scrape/status/{jobId}")]
        public ActionResult GetJobStatus(Guid jobId)
        {
            if (!_runningJobs.TryGetValue(jobId, out var job))
            {
                return NotFound("Job not found or already completed");
            }

            return Ok(new
            {
                jobId = jobId,
                status = job.IsCompleted ? "Completed" :
                         job.IsFaulted ? "Failed" :
                         job.IsCanceled ? "Canceled" : "Running"
            });
        }
    }
}