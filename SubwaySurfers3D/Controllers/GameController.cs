using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubwaySurfers3D.Data;
using SubwaySurfers3D.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubwaySurfers3D.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly AppDbContext _db;

        public GameController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("submit")]
        public async Task<ActionResult<SessionResultDto>> SubmitSession([FromBody] SubmitSessionRequest req)
        {
            var player = await _db.Players
                .Include(p => p.Achievements)
                .FirstOrDefaultAsync(p => p.Id == req.PlayerId);

            if (player == null)
            {
                return NotFound(new { message = "Player not found" });
            }

            // Create GameSession record
            var session = new GameSession
            {
                PlayerId = player.Id,
                Score = Math.Max(0, req.Score),
                CoinsCollected = Math.Max(0, req.CoinsCollected),
                DistanceRun = Math.Max(0, req.DistanceRun),
                MaxMultiplier = Math.Max(1, req.MaxMultiplier),
                PowerupsUsed = req.PowerupsUsed ?? "",
                PlayedAt = DateTime.UtcNow
            };

            _db.GameSessions.Add(session);

            // Update player totals
            bool isNewHighScore = req.Score > player.HighScore;
            if (isNewHighScore)
            {
                player.HighScore = req.Score;
            }

            player.TotalCoins += req.CoinsCollected;
            player.GamesPlayed += 1;
            player.TotalDistance += req.DistanceRun;

            await _db.SaveChangesAsync();

            // Evaluate achievements
            var newlyUnlocked = await CheckAchievementsAsync(player);

            return Ok(new SessionResultDto
            {
                SessionId = session.Id,
                IsNewHighScore = isNewHighScore,
                HighScore = player.HighScore,
                TotalCoins = player.TotalCoins,
                CoinsEarned = req.CoinsCollected,
                NewlyUnlockedAchievements = newlyUnlocked
            });
        }

        private async Task<List<string>> CheckAchievementsAsync(Player player)
        {
            var newlyUnlocked = new List<string>();
            var allAchievements = await _db.Achievements.ToListAsync();
            var playerAchMap = player.Achievements.ToDictionary(pa => pa.AchievementId);

            foreach (var ach in allAchievements)
            {
                bool unlocked = false;

                if (playerAchMap.TryGetValue(ach.Id, out var existingPa) && existingPa.IsUnlocked)
                {
                    continue; // Already unlocked
                }

                switch (ach.MetricType)
                {
                    case "games":
                        if (player.GamesPlayed >= ach.TargetValue) unlocked = true;
                        break;
                    case "score":
                        if (player.HighScore >= ach.TargetValue) unlocked = true;
                        break;
                    case "coins":
                        if (player.TotalCoins >= ach.TargetValue) unlocked = true;
                        break;
                    case "distance":
                        if (player.TotalDistance >= ach.TargetValue) unlocked = true;
                        break;
                }

                if (unlocked)
                {
                    if (existingPa != null)
                    {
                        existingPa.IsUnlocked = true;
                        existingPa.UnlockedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        _db.PlayerAchievements.Add(new PlayerAchievement
                        {
                            PlayerId = player.Id,
                            AchievementId = ach.Id,
                            IsUnlocked = true,
                            UnlockedAt = DateTime.UtcNow
                        });
                    }

                    // Reward coins
                    player.TotalCoins += ach.RewardCoins;
                    newlyUnlocked.Add(ach.Title);
                }
            }

            if (newlyUnlocked.Count > 0)
            {
                await _db.SaveChangesAsync();
            }

            return newlyUnlocked;
        }
    }
}
