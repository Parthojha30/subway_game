using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubwaySurfers3D.Data;
using SubwaySurfers3D.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubwaySurfers3D.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public LeaderboardController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("top")]
        public async Task<ActionResult<List<LeaderboardEntryDto>>> GetTopPlayers([FromQuery] int limit = 10)
        {
            var topPlayers = await _db.Players
                .OrderByDescending(p => p.HighScore)
                .Take(limit)
                .ToListAsync();

            var result = topPlayers.Select((p, index) => new LeaderboardEntryDto
            {
                Rank = index + 1,
                Username = p.Username,
                SelectedCharacterId = p.SelectedCharacterId,
                HighScore = p.HighScore,
                TotalCoins = p.TotalCoins,
                TotalDistance = p.TotalDistance,
                PlayedAt = p.CreatedAt
            }).ToList();

            return Ok(result);
        }

        [HttpGet("recent")]
        public async Task<ActionResult<List<LeaderboardEntryDto>>> GetRecentSessions([FromQuery] int limit = 10)
        {
            var sessions = await _db.GameSessions
                .Include(s => s.Player)
                .OrderByDescending(s => s.PlayedAt)
                .Take(limit)
                .ToListAsync();

            var result = sessions.Select((s, index) => new LeaderboardEntryDto
            {
                Rank = index + 1,
                Username = s.Player?.Username ?? "Anonymous",
                SelectedCharacterId = s.Player?.SelectedCharacterId ?? "jake",
                HighScore = s.Score,
                TotalCoins = s.CoinsCollected,
                TotalDistance = s.DistanceRun,
                PlayedAt = s.PlayedAt
            }).ToList();

            return Ok(result);
        }
    }
}
