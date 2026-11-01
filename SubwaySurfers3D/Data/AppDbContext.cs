using Microsoft.EntityFrameworkCore;
using SubwaySurfers3D.Models;
using System;

namespace SubwaySurfers3D.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<GameSession> GameSessions { get; set; } = null!;
        public DbSet<ShopItem> ShopItems { get; set; } = null!;
        public DbSet<PlayerInventory> PlayerInventories { get; set; } = null!;
        public DbSet<Achievement> Achievements { get; set; } = null!;
        public DbSet<PlayerAchievement> PlayerAchievements { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Indexes
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.HighScore);

            modelBuilder.Entity<GameSession>()
                .HasIndex(s => s.Score);

            // Seed Shop Items
            modelBuilder.Entity<ShopItem>().HasData(
                // Characters
                new ShopItem
                {
                    Id = "jake",
                    Name = "Jake (Default)",
                    Category = "character",
                    Description = "The iconic subway runner with classic denim jacket and cap.",
                    Price = 0,
                    SpeedBonus = 0.0,
                    MagnetMultiplier = 1.0,
                    IconCss = "fa-user-ninja",
                    UnlockedByDefault = true,
                    Color = "#ff4757"
                },
                new ShopItem
                {
                    Id = "tricky",
                    Name = "Tricky Spark",
                    Category = "character",
                    Description = "Agile freestyle dancer with +15% agility and neon aura.",
                    Price = 500,
                    SpeedBonus = 0.15,
                    MagnetMultiplier = 1.1,
                    IconCss = "fa-bolt",
                    UnlockedByDefault = false,
                    Color = "#2ed573"
                },
                new ShopItem
                {
                    Id = "fresh",
                    Name = "Fresh Boombox",
                    Category = "character",
                    Description = "Retro hip-hop street legend. Grants +25% magnet range!",
                    Price = 1200,
                    SpeedBonus = 0.10,
                    MagnetMultiplier = 1.25,
                    IconCss = "fa-drum",
                    UnlockedByDefault = false,
                    Color = "#1e90ff"
                },
                new ShopItem
                {
                    Id = "ninja",
                    Name = "Shadow Ninja",
                    Category = "character",
                    Description = "Master of stealth. Fast lane switching speed and double jump boost.",
                    Price = 2500,
                    SpeedBonus = 0.25,
                    MagnetMultiplier = 1.2,
                    IconCss = "fa-user-secret",
                    UnlockedByDefault = false,
                    Color = "#a55eea"
                },
                new ShopItem
                {
                    Id = "cosmic",
                    Name = "Cosmic Jet",
                    Category = "character",
                    Description = "Cyberpunk astronaut from 2099 with holographic trail.",
                    Price = 5000,
                    SpeedBonus = 0.35,
                    MagnetMultiplier = 1.5,
                    IconCss = "fa-user-astronaut",
                    UnlockedByDefault = false,
                    Color = "#ffa502"
                },

                // Hoverboards
                new ShopItem
                {
                    Id = "starboard",
                    Name = "Starboard Cruiser",
                    Category = "board",
                    Description = "Standard issue hoverboard. Grants crash protection!",
                    Price = 0,
                    SpeedBonus = 0.0,
                    MagnetMultiplier = 1.0,
                    IconCss = "fa-rocket",
                    UnlockedByDefault = true,
                    Color = "#00d2d3"
                },
                new ShopItem
                {
                    Id = "flame_rider",
                    Name = "Flame Rider 3000",
                    Category = "board",
                    Description = "Supercharged board emitting intense plasma fire exhaust trails.",
                    Price = 800,
                    SpeedBonus = 0.2,
                    MagnetMultiplier = 1.2,
                    IconCss = "fa-fire",
                    UnlockedByDefault = false,
                    Color = "#ff6b6b"
                },
                new ShopItem
                {
                    Id = "cyber_pulse",
                    Name = "Cyber Pulse Neon",
                    Category = "board",
                    Description = "Anti-gravity neon hoverboard with magnetic coin attraction fields.",
                    Price = 2000,
                    SpeedBonus = 0.3,
                    MagnetMultiplier = 1.5,
                    IconCss = "fa-atom",
                    UnlockedByDefault = false,
                    Color = "#00d2d3"
                }
            );

            // Seed Achievements
            modelBuilder.Entity<Achievement>().HasData(
                new Achievement
                {
                    Id = "first_run",
                    Title = "First Escape",
                    Description = "Complete your first 3D Subway run.",
                    RewardCoins = 100,
                    TargetValue = 1,
                    MetricType = "games",
                    Icon = "running"
                },
                new Achievement
                {
                    Id = "score_1000",
                    Title = "Subway Rookie",
                    Description = "Score 1,000 points in a single run.",
                    RewardCoins = 250,
                    TargetValue = 1000,
                    MetricType = "score",
                    Icon = "award"
                },
                new Achievement
                {
                    Id = "score_5000",
                    Title = "Track Legend",
                    Description = "Score 5,000 points in a single run.",
                    RewardCoins = 600,
                    TargetValue = 5000,
                    MetricType = "score",
                    Icon = "medal"
                },
                new Achievement
                {
                    Id = "coins_100",
                    Title = "Coin Collector",
                    Description = "Collect 100 total gold coins.",
                    RewardCoins = 200,
                    TargetValue = 100,
                    MetricType = "coins",
                    Icon = "coins"
                },
                new Achievement
                {
                    Id = "coins_1000",
                    Title = "Subway Millionaire",
                    Description = "Collect 1,000 total gold coins.",
                    RewardCoins = 1000,
                    TargetValue = 1000,
                    MetricType = "coins",
                    Icon = "vault"
                },
                new Achievement
                {
                    Id = "distance_500",
                    Title = "Long Distance Runner",
                    Description = "Run a total distance of 500 meters.",
                    RewardCoins = 300,
                    TargetValue = 500,
                    MetricType = "distance",
                    Icon = "route"
                }
            );
        }
    }
}
