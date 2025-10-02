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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Linq.Expressions;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacterStatsController : ControllerBase
    {
        private readonly MapleTinderDbContext _context;

        public CharacterStatsController(MapleTinderDbContext context)
        {
            _context = context;
        }

        private async Task<ActionResult<PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>>> GetTopCharacters(int page, int pageSize, Expression<Func<CharacterStats, int>> orderBySelector)
        {
            try
            {
                if (page <= 0 || pageSize <= 0)
                    return BadRequest("Page and PageSize must be greater than zero.");

                var totalCount = await _context.CharacterStats.CountAsync();

                var compiledSelector = orderBySelector.Compile();

                var list = await _context.CharacterStats
                    .Include(cs => cs.Character)
                    .OrderByDescending(orderBySelector)
                    .ThenBy(cs => cs.CharacterId) // Tiebreaker
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(cs => new LeaderboardCharacterDTO
                    {
                        Character = cs.Character,
                        Count = compiledSelector(cs)
                    })
                    .ToListAsync();

                var result = new PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>
                {
                    Data = list,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: api/CharacterStats/top_liked?page=1&pageSize=10
        [HttpGet("top_liked")]
        public async Task<ActionResult<PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>>> TopLiked([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await GetTopCharacters(page, pageSize, cs => cs.TotalLikes);
        }

        // GET: api/CharacterStats/top_noped?page=1&pageSize=10
        [HttpGet("top_noped")]
        public async Task<ActionResult<PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>>> TopNoped([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await GetTopCharacters(page, pageSize, cs => cs.TotalNopes);
        }

        // GET: api/CharacterStats/search?page=1&pageSize=10
        [HttpGet("search")]
        public async Task<ActionResult<PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>>> Search([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
            [FromQuery] string characterName = "", [FromQuery] string rankingType = "hotties",
            [FromQuery] string classType = "all", [FromQuery] string timeType = "all", [FromQuery] string worldType ="all")
        {
            try
            {
                IQueryable<CharacterStats> query =  _context.CharacterStats.Include(cs => cs.Character);

                // Filter by character name
                if (!string.IsNullOrEmpty(characterName))
                {
                    query = query.Where(cs => cs.Character.Name.Contains(characterName));
                }

                // Filter by world type
                if (!string.IsNullOrEmpty(worldType) && worldType.ToLower() != "all")
                {
                    query = query.Where(cs => cs.Character.World.Equals(worldType, StringComparison.OrdinalIgnoreCase));
                }

                // Filter by class type
                if (!string.IsNullOrEmpty(classType) && classType.ToLower() != "all")
                {
                    query = query.Where(cs => cs.Character.Job.Equals(classType, StringComparison.OrdinalIgnoreCase));
                }

                // Order by time and ranking types
                IOrderedQueryable<CharacterStats> orderedQuery = (rankingType.ToLower(), timeType.ToLower()) switch
                {
                    ("hotties", "weekly") => query.OrderByDescending(cs => cs.WeeklyLikes),
                    ("hotties", "monthly") => query.OrderByDescending(cs => cs.MonthlyLikes),
                    ("hotties", _) => query.OrderByDescending(cs => cs.TotalLikes), 

                    ("notties", "weekly") => query.OrderByDescending(cs => cs.WeeklyNopes),
                    ("notties", "monthly") => query.OrderByDescending(cs => cs.MonthlyNopes),
                    ("notties", _) => query.OrderByDescending(cs => cs.TotalNopes),

                    ("favourites", "weekly") => query.OrderByDescending(cs => cs.WeeklyFavourites),
                    ("favourites", "monthly") => query.OrderByDescending(cs => cs.MonthlyFavourites),
                    ("favourites", _) => query.OrderByDescending(cs => cs.TotalFavourites),

                    _ => query.OrderByDescending(cs => cs.TotalLikes) // Default 
                };

                var totalCount = await query.CountAsync();

                var list = await orderedQuery
                .ThenBy(cs => cs.CharacterId) // Tiebreaker
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cs => new LeaderboardCharacterDTO
                {
                    Character = cs.Character,
                    
                    // A bit hacky.
                    // TODO: If new columns are added in the future, refactor this instead of hard coding all cases.
                    Count = rankingType.ToLower() == "hotties" && timeType.ToLower() == "weekly" ? cs.WeeklyLikes :
                            rankingType.ToLower() == "hotties" && timeType.ToLower() == "monthly" ? cs.MonthlyLikes :
                            rankingType.ToLower() == "hotties" ? cs.TotalLikes :
                            rankingType.ToLower() == "notties" && timeType.ToLower() == "weekly" ? cs.WeeklyNopes :
                            rankingType.ToLower() == "notties" && timeType.ToLower() == "monthly" ? cs.MonthlyNopes :
                            rankingType.ToLower() == "notties" ? cs.TotalNopes :
                            rankingType.ToLower() == "favourites" && timeType.ToLower() == "weekly" ? cs.WeeklyFavourites :
                            rankingType.ToLower() == "favourites" && timeType.ToLower() == "monthly" ? cs.MonthlyFavourites :
                            rankingType.ToLower() == "favourites" ? cs.TotalFavourites :
                            cs.TotalLikes, // default
                })
                .ToListAsync();

                var result = new PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterDTO>
                {
                    Data = list,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: api/CharacterStats/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCharacterStats(int id)
        {
            var characterStats = await _context.CharacterStats.FindAsync(id);

            if (characterStats == null) return NotFound();

            return Ok(characterStats);
        }

        // Instance or static, it cant translate to SQL. 
        // If I want to use this method, I'll need to filter for specific cols in memory, after getting rows from db.
        private int GetStatValue(CharacterStats cs, string rankingType, string timeType)
        {
            return (rankingType.ToLower(), timeType.ToLower()) switch
            {
                ("hotties", "weekly") => cs.WeeklyLikes,
                ("hotties", "monthly") => cs.MonthlyLikes,
                ("hotties", _) => cs.TotalLikes,

                ("notties", "weekly") => cs.WeeklyNopes,
                ("notties", "monthly") => cs.MonthlyNopes,
                ("notties", _) => cs.TotalNopes,

                ("favourites", "weekly") => cs.WeeklyFavourites,
                ("favourites", "monthly") => cs.MonthlyFavourites,
                ("favourites", _) => cs.TotalFavourites,

                _ => cs.TotalLikes
            };
        }
    }
}
