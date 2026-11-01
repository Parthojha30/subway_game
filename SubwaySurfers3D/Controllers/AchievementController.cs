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
    public class AchievementController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AchievementController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("{playerId}")]
        public async Task<ActionResult<List<AchievementDto>>> GetPlayerAchievements(int playerId)
        {
            var player = await _db.Players
                .Include(p => p.Achievements)
                .FirstOrDefaultAsync(p => p.Id == playerId);

            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            var achievements = await _db.Achievements.ToListAsync();
            var playerAchMap = player.Achievements.ToDictionary(pa => pa.AchievementId);

            var result = achievements.Select(a =>
            {
                bool isUnlocked = playerAchMap.TryGetValue(a.Id, out var pa) && pa.IsUnlocked;
                int currentProgress = a.MetricType switch
                {
                    "games" => player.GamesPlayed,
                    "score" => player.HighScore,
                    "coins" => player.TotalCoins,
                    "distance" => (int)player.TotalDistance,
                    _ => 0
                };

                return new AchievementDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    RewardCoins = a.RewardCoins,
                    TargetValue = a.TargetValue,
                    MetricType = a.MetricType,
                    Icon = a.Icon,
                    IsUnlocked = isUnlocked,
                    CurrentProgress = currentProgress
                };
            }).ToList();

            return Ok(result);
        }
    }
}
