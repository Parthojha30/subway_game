using UnityEngine;
using UnityEngine.UI;
using MetroRunner.Core;
using MetroRunner.Systems;

namespace MetroRunner.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject missionsPanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("HUD Elements")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text gemsText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text multiplierText;

        [Header("GameOver Elements")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text finalCoinsText;
        [SerializeField] private Text finalDistanceText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
                GameManager.Instance.OnStatsUpdated += UpdateHUDStats;
            }

            ShowPanel(mainMenuPanel);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
                GameManager.Instance.OnStatsUpdated -= UpdateHUDStats;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            HideAllPanels();

            switch (state)
            {
                case GameState.MainMenu:
                    ShowPanel(mainMenuPanel);
                    break;
                case GameState.Playing:
                    ShowPanel(hudPanel);
                    break;
                case GameState.GameOver:
                    ShowPanel(gameOverPanel);
                    PopulateGameOverSummary();
                    break;
            }
        }

        private void UpdateHUDStats(int score, int coins, float distance)
        {
            if (scoreText != null) scoreText.text = score.ToString("D6");
            if (coinsText != null) coinsText.text = coins.ToString();
            if (distanceText != null) distanceText.text = $"{Mathf.RoundToInt(distance)}m";
            if (multiplierText != null && GameManager.Instance != null)
                multiplierText.text = $"{GameManager.Instance.ScoreMultiplier}X";
        }

        private void PopulateGameOverSummary()
        {
            if (GameManager.Instance == null) return;
            if (finalScoreText != null) finalScoreText.text = GameManager.Instance.Score.ToString("N0");
            if (finalCoinsText != null) finalCoinsText.text = $"+{GameManager.Instance.CoinsCollectedInRun}";
            if (finalDistanceText != null) finalDistanceText.text = $"{Mathf.RoundToInt(GameManager.Instance.DistanceRun)}m";
        }

        public void OnClickStartRun()
        {
            if (GameManager.Instance != null) GameManager.Instance.StartNewRun();
        }

        public void OnClickOpenShop() => ShowPanel(shopPanel);
        public void OnClickOpenSettings() => ShowPanel(settingsPanel);
        public void OnClickOpenMissions() => ShowPanel(missionsPanel);

        private void ShowPanel(GameObject panel)
        {
            HideAllPanels();
            if (panel != null) panel.SetActive(true);
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (missionsPanel != null) missionsPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }
    }
}
