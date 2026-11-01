using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubwaySurfers3D.Models
{
    public class Player
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = "RunnerPro";

        public string Avatar { get; set; } = "jake";

        public int HighScore { get; set; } = 0;

        public int TotalCoins { get; set; } = 250; // Starting bonus coins

        public int GamesPlayed { get; set; } = 0;

        public double TotalDistance { get; set; } = 0.0;

        public string SelectedCharacterId { get; set; } = "jake";

        public string SelectedBoardId { get; set; } = "starboard";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<GameSession> Sessions { get; set; } = new List<GameSession>();
        public ICollection<PlayerInventory> Inventory { get; set; } = new List<PlayerInventory>();
        public ICollection<PlayerAchievement> Achievements { get; set; } = new List<PlayerAchievement>();
    }

    public class GameSession
    {
        [Key]
        public int Id { get; set; }

        public int PlayerId { get; set; }

        public int Score { get; set; }

        public int CoinsCollected { get; set; }

        public double DistanceRun { get; set; }

        public int MaxMultiplier { get; set; } = 1;

        public string PowerupsUsed { get; set; } = ""; // e.g. "Magnet,Jetpack,Hoverboard"

        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PlayerId")]
        public Player? Player { get; set; }
    }

    public class ShopItem
    {
        [Key]
        public string Id { get; set; } = ""; // e.g. "jake", "tricky", "starboard", "flame_rider", "magnet_upgrade"

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Category { get; set; } = ""; // "character", "board", "powerup"

        public string Description { get; set; } = "";

        public int Price { get; set; }

        public double SpeedBonus { get; set; } = 0.0;

        public double MagnetMultiplier { get; set; } = 1.0;

        public string IconCss { get; set; } = "fas fa-user";

        public bool UnlockedByDefault { get; set; } = false;

        public string Color { get; set; } = "#ff4757";
    }

    public class PlayerInventory
    {
        [Key]
        public int Id { get; set; }

        public int PlayerId { get; set; }

        public string ShopItemId { get; set; } = "";

        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PlayerId")]
        public Player? Player { get; set; }

        [ForeignKey("ShopItemId")]
        public ShopItem? ShopItem { get; set; }
    }

    public class Achievement
    {
        [Key]
        public string Id { get; set; } = "";

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public int RewardCoins { get; set; }

        public int TargetValue { get; set; }

        public string MetricType { get; set; } = "score"; // "score", "coins", "games", "distance"

        public string Icon { get; set; } = "trophy";
    }

    public class PlayerAchievement
    {
        [Key]
        public int Id { get; set; }

        public int PlayerId { get; set; }

        public string AchievementId { get; set; } = "";

        public bool IsUnlocked { get; set; } = false;

        public DateTime? UnlockedAt { get; set; }

        [ForeignKey("PlayerId")]
        public Player? Player { get; set; }

        [ForeignKey("AchievementId")]
        public Achievement? Achievement { get; set; }
    }
}
