using System.Collections.Generic;
using UnityEngine;
using MetroRunner.Core;
using MetroRunner.Player;

namespace MetroRunner.Systems
{
    public enum PowerUpType
    {
        CoinMagnet,
        Jetpack,
        ScoreBooster,
        Shield,
        DoubleCoins,
        SpeedBoost,
        SlowMotion,
        KeyResurrection
    }

    [System.Serializable]
    public class ActivePowerUp
    {
        public PowerUpType type;
        public float duration;
        public float maxDuration;
    }

    public class PowerUpManager : MonoBehaviour
    {
        public static PowerUpManager Instance { get; private set; }

        [SerializeField] private PlayerController player;
        private List<ActivePowerUp> activePowerUps = new List<ActivePowerUp>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            for (int i = activePowerUps.Count - 1; i >= 0; i--)
            {
                activePowerUps[i].duration -= Time.deltaTime;
                if (activePowerUps[i].duration <= 0)
                {
                    DeactivatePowerUp(activePowerUps[i].type);
                    activePowerUps.RemoveAt(i);
                }
            }
        }

        public void ActivatePowerUp(PowerUpType type, float duration = 10f)
        {
            ActivePowerUp existing = activePowerUps.Find(p => p.type == type);
            if (existing != null)
            {
                existing.duration = duration;
            }
            else
            {
                activePowerUps.Add(new ActivePowerUp { type = type, duration = duration, maxDuration = duration });
                ApplyPowerUpEffect(type, true);
            }
        }

        private void ApplyPowerUpEffect(PowerUpType type, bool enable)
        {
            switch (type)
            {
                case PowerUpType.CoinMagnet:
                    // Enable wide magnetic sphere trigger on player
                    break;
                case PowerUpType.Jetpack:
                    if (player != null) player.IsFlyingJetpack = enable;
                    break;
                case PowerUpType.ScoreBooster:
                    if (GameManager.Instance != null) GameManager.Instance.ScoreMultiplier = enable ? 2 : 1;
                    break;
                case PowerUpType.Shield:
                    if (player != null) player.HasShield = enable;
                    break;
                case PowerUpType.SlowMotion:
                    Time.timeScale = enable ? 0.7f : 1.0f;
                    break;
            }
        }

        private void DeactivatePowerUp(PowerUpType type)
        {
            ApplyPowerUpEffect(type, false);
        }

        public bool IsPowerUpActive(PowerUpType type)
        {
            return activePowerUps.Exists(p => p.type == type);
        }
    }
}
