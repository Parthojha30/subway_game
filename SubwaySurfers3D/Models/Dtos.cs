using System;
using System.Collections.Generic;

namespace SubwaySurfers3D.Models
{
    public class PlayerDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Avatar { get; set; } = "";
        public int HighScore { get; set; }
        public int TotalCoins { get; set; }
        public int GamesPlayed { get; set; }
        public double TotalDistance { get; set; }
        public string SelectedCharacterId { get; set; } = "";
        public string SelectedBoardId { get; set; } = "";
        public List<string> OwnedItemIds { get; set; } = new List<string>();
    }

    public class UpdateEquipmentRequest
    {
        public int PlayerId { get; set; }
        public string? CharacterId { get; set; }
        public string? BoardId { get; set; }
    }

    public class LoginOrRegisterRequest
    {
        public string Username { get; set; } = "RunnerPro";
    }

    public class SubmitSessionRequest
    {
        public int PlayerId { get; set; }
        public int Score { get; set; }
        public int CoinsCollected { get; set; }
        public double DistanceRun { get; set; }
        public int MaxMultiplier { get; set; } = 1;
        public string PowerupsUsed { get; set; } = "";
    }

    public class SessionResultDto
    {
        public int SessionId { get; set; }
        public bool IsNewHighScore { get; set; }
        public int HighScore { get; set; }
        public int TotalCoins { get; set; }
        public int CoinsEarned { get; set; }
        public List<string> NewlyUnlockedAchievements { get; set; } = new List<string>();
    }

    public class BuyItemRequest
    {
        public int PlayerId { get; set; }
        public string ItemId { get; set; } = "";
    }

    public class BuyItemResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int RemainingCoins { get; set; }
        public List<string> OwnedItemIds { get; set; } = new List<string>();
    }

    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public string Username { get; set; } = "";
        public string SelectedCharacterId { get; set; } = "";
        public int HighScore { get; set; }
        public int TotalCoins { get; set; }
        public double TotalDistance { get; set; }
        public DateTime PlayedAt { get; set; }
    }

    public class AchievementDto
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int RewardCoins { get; set; }
        public int TargetValue { get; set; }
        public string MetricType { get; set; } = "";
        public string Icon { get; set; } = "";
        public bool IsUnlocked { get; set; }
        public int CurrentProgress { get; set; }
    }
}
