using System;
using UnityEngine;

namespace MetroRunner.Core
{
    public enum GameState
    {
        Splash,
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State Settings")]
        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        [Header("Speed & Difficulty Configuration")]
        [SerializeField] private float initialSpeed = 14f;
        [SerializeField] private float maxSpeed = 32f;
        [SerializeField] private float speedAcceleration = 0.08f;

        [Header("Runtime Stats")]
        public float CurrentSpeed { get; private set; }
        public float DistanceRun { get; private set; }
        public int Score { get; private set; }
        public int CoinsCollectedInRun { get; private set; }
        public int GemsCollectedInRun { get; private set; }
        public int KeysCollectedInRun { get; private set; }
        public int ScoreMultiplier { get; set; } = 1;

        public event Action<GameState> OnGameStateChanged;
        public event Action<int, int, float> OnStatsUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetState(GameState.MainMenu);
        }

        private void Update()
        {
            if (CurrentState != GameState.Playing) return;

            // Accelerate speed gradually
            if (CurrentSpeed < maxSpeed)
            {
                CurrentSpeed += speedAcceleration * Time.deltaTime;
            }

            // Accumulate distance and score
            float distThisFrame = CurrentSpeed * Time.deltaTime;
            DistanceRun += distThisFrame * 0.5f;
            Score += Mathf.RoundToInt(distThisFrame * ScoreMultiplier * 10f);

            OnStatsUpdated?.Invoke(Score, CoinsCollectedInRun, DistanceRun);
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);

            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0.2f; // Bullet-time effect on crash
                    break;
            }
        }

        public void StartNewRun()
        {
            CurrentSpeed = initialSpeed;
            DistanceRun = 0f;
            Score = 0;
            CoinsCollectedInRun = 0;
            GemsCollectedInRun = 0;
            KeysCollectedInRun = 0;
            ScoreMultiplier = 1;

            SetState(GameState.Playing);
        }

        public void AddCoin(int amount = 1)
        {
            CoinsCollectedInRun += amount;
            Score += 50 * ScoreMultiplier;
        }

        public void AddGem(int amount = 1)
        {
            GemsCollectedInRun += amount;
            Score += 200 * ScoreMultiplier;
        }

        public void AddKey(int amount = 1)
        {
            KeysCollectedInRun += amount;
        }

        public void GameOver()
        {
            SetState(GameState.GameOver);
        }
    }
}
