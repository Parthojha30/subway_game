using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MetroRunner.Core
{
    [Serializable]
    public class PlayerSaveData
    {
        public string username = "MetroRunner";
        public int highScore = 0;
        public int totalCoins = 500;
        public int totalGems = 20;
        public int totalKeys = 5;
        public int totalDistance = 0;
        public int totalGamesPlayed = 0;
        
        public string activeCharacterId = "alex";
        public string activeBoardId = "hover_prime";
        
        public List<string> unlockedCharacterIds = new List<string> { "alex" };
        public List<string> unlockedBoardIds = new List<string> { "hover_prime" };
        public List<string> completedMissionIds = new List<string>();

        // Settings
        public float musicVolume = 0.8f;
        public float sfxVolume = 1.0f;
        public int graphicsQuality = 2; // 0=Low, 1=Medium, 2=High
        public float touchSensitivity = 1.0f;
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        public PlayerSaveData SaveData { get; private set; } = new PlayerSaveData();
        private string saveFilePath;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            saveFilePath = Path.Combine(Application.persistentDataPath, "MetroRunnerSave.json");
            LoadGame();
        }

        public void SaveGame()
        {
            try
            {
                string json = JsonUtility.ToJson(SaveData, true);
                File.WriteAllText(saveFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save game data: {ex.Message}");
            }
        }

        public void LoadGame()
        {
            try
            {
                if (File.Exists(saveFilePath))
                {
                    string json = File.ReadAllText(saveFilePath);
                    SaveData = JsonUtility.FromJson<PlayerSaveData>(json);
                }
                else
                {
                    SaveData = new PlayerSaveData();
                    SaveGame();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load game data: {ex.Message}");
                SaveData = new PlayerSaveData();
            }
        }

        public bool UnlockCharacter(string charId, int coinPrice, int gemPrice)
        {
            if (SaveData.unlockedCharacterIds.Contains(charId)) return true;

            if (SaveData.totalCoins >= coinPrice && SaveData.totalGems >= gemPrice)
            {
                SaveData.totalCoins -= coinPrice;
                SaveData.totalGems -= gemPrice;
                SaveData.unlockedCharacterIds.Add(charId);
                SaveData.activeCharacterId = charId;
                SaveGame();
                return true;
            }
            return false;
        }

        public bool UnlockBoard(string boardId, int coinPrice)
        {
            if (SaveData.unlockedBoardIds.Contains(boardId)) return true;

            if (SaveData.totalCoins >= coinPrice)
            {
                SaveData.totalCoins -= coinPrice;
                SaveData.unlockedBoardIds.Add(boardId);
                SaveData.activeBoardId = boardId;
                SaveGame();
                return true;
            }
            return false;
        }
    }
}
