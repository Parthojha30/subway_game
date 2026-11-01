using System;
using System.Collections.Generic;
using UnityEngine;
using MetroRunner.Core;

namespace MetroRunner.Systems
{
    [Serializable]
    public class Mission
    {
        public string id;
        public string title;
        public string description;
        public int currentProgress;
        public int targetValue;
        public int rewardCoins;
        public int rewardGems;
        public bool isCompleted;
    }

    public class MissionSystem : MonoBehaviour
    {
        public static MissionSystem Instance { get; private set; }

        public List<Mission> activeMissions = new List<Mission>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializeDefaultMissions();
        }

        private void InitializeDefaultMissions()
        {
            activeMissions.Add(new Mission
            {
                id = "m1",
                title = "Metropolitan Runner",
                description = "Run a total distance of 1,000m.",
                targetValue = 1000,
                rewardCoins = 300,
                rewardGems = 2
            });

            activeMissions.Add(new Mission
            {
                id = "m2",
                title = "Gold Rush",
                description = "Collect 250 gold coins.",
                targetValue = 250,
                rewardCoins = 500,
                rewardGems = 5
            });

            activeMissions.Add(new Mission
            {
                id = "m3",
                title = "Jetpack Aviator",
                description = "Use the Jetpack rocket 3 times.",
                targetValue = 3,
                rewardCoins = 400,
                rewardGems = 3
            });
        }

        public void CheckMissionProgress(string type, int progressAmount)
        {
            foreach (var mission in activeMissions)
            {
                if (mission.isCompleted) continue;

                mission.currentProgress += progressAmount;
                if (mission.currentProgress >= mission.targetValue)
                {
                    mission.isCompleted = true;
                    ClaimReward(mission);
                }
            }
        }

        private void ClaimReward(Mission mission)
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveData.totalCoins += mission.rewardCoins;
                SaveManager.Instance.SaveData.totalGems += mission.rewardGems;
                SaveManager.Instance.SaveGame();
            }
        }
    }
}
