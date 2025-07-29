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

        // GET: api/CharacterStats/top_liked?page=1&pageSize=10
        [HttpGet("top_liked")]
        public async Task<IActionResult> TopLiked([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var totalCount = await _context.CharacterStats.CountAsync();

                var list = await _context.CharacterStats
                .Include(cs => cs.Character)
                .OrderByDescending(cs => cs.TotalLikes)
                .ThenBy(cs => cs.CharacterId) // Tiebreaker
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cs => new LeaderboardCharacterLikeDTO
                {
                    Character = cs.Character,
                    TotalLikes = cs.TotalLikes
                })
                .ToListAsync();

                var result = new PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterLikeDTO>
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

        // GET: api/CharacterStats/top_noped?page=1&pageSize=10
        [HttpGet("top_noped")]
        public async Task<IActionResult> TopNoped([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var totalCount = await _context.CharacterStats.CountAsync();

                var list = await _context.CharacterStats
                .Include(cs => cs.Character)
                .OrderByDescending(cs => cs.TotalNopes)
                .ThenBy(cs => cs.CharacterId) // Tiebreaker
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cs => new LeaderboardCharacterNopeDTO
                {
                    Character = cs.Character,
                    TotalNopes = cs.TotalNopes
                })
                .ToListAsync();

                var result = new PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterNopeDTO>
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

        // GET: api/CharacterStats/search?page=1&pageSize=10
        [HttpGet("search")]

        public async Task<IActionResult> Search([FromQuery] string characterName = "", [FromQuery] string rankingType = "hotties",
            [FromQuery] string classType = "all", [FromQuery] string timeType = "all", [FromQuery] string worldType ="all",
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var totalCount = await _context.CharacterStats.CountAsync();

                var list = await _context.CharacterStats
                .Include(cs => cs.Character)
                .OrderByDescending(cs => cs.TotalNopes)
                .ThenBy(cs => cs.CharacterId) // Tiebreaker
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cs => new LeaderboardCharacterNopeDTO
                {
                    Character = cs.Character,
                    TotalNopes = cs.TotalNopes
                })
                .ToListAsync();

                var result = new PaginatedLeaderboardWithMetaDTO<LeaderboardCharacterNopeDTO>
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
    }
}
