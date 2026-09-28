using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OrbitalSalvage.Core;

namespace OrbitalSalvage.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Screen Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject sectorSelectPanel;
        [SerializeField] private GameObject inGameHudPanel;
        [SerializeField] private GameObject victoryModal;
        [SerializeField] private GameObject defeatModal;

        [Header("In-Game HUD References")]
        [SerializeField] private TextMeshProUGUI levelTitleText;
        [SerializeField] private TextMeshProUGUI quotaCounterText;
        [SerializeField] private Slider quotaProgressBar;
        [SerializeField] private TextMeshProUGUI scrapCreditsText;

        [Header("Debrief Modal References")]
        [SerializeField] private TextMeshProUGUI victoryStatsText;
        [SerializeField] private TextMeshProUGUI victoryStarsText;
        [SerializeField] private TextMeshProUGUI defeatMessageText;
        [SerializeField] private Button backupPodButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            UpdateHud();
        }

        private void UpdateHud()
        {
            if (GameManager.Instance == null) return;

            if (scrapCreditsText != null && EconomyManager.Instance != null)
            {
                scrapCreditsText.text = $"{EconomyManager.Instance.SaveData.scrapCredits} $SC";
            }

            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                if (quotaCounterText != null)
                {
                    quotaCounterText.text = $"{GameManager.Instance.SwallowedDebrisCount} / {GameManager.Instance.CurrentQuota}";
                }

                if (quotaProgressBar != null)
                {
                    float p = (float)GameManager.Instance.SwallowedDebrisCount / Mathf.Max(1, GameManager.Instance.CurrentQuota);
                    quotaProgressBar.value = Mathf.Clamp01(p);
                }

                if (levelTitleText != null)
                {
                    levelTitleText.text = $"SECTOR {GameManager.Instance.CurrentLevelNumber}";
                }
            }
        }

        public void ShowMainMenu()
        {
            SetPanel(mainMenuPanel);
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.SetState(GameState.MainMenu);
        }

        public void ShowSectorSelect()
        {
            SetPanel(sectorSelectPanel);
            AudioManager.Instance?.PlayTabSwitch();
            GameManager.Instance?.SetState(GameState.SectorSelect);
        }

        public void StartGame(int sectorNum)
        {
            SetPanel(inGameHudPanel);
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.StartSector(sectorNum);
        }

        public void OnVictory()
        {
            if (victoryModal != null) victoryModal.SetActive(true);
            int stars = GameManager.Instance != null ? GameManager.Instance.CalculateStars() : 3;

            if (victoryStarsText != null)
            {
                victoryStarsText.text = new string('★', stars) + new string('☆', 3 - stars);
            }

            if (victoryStatsText != null && GameManager.Instance != null)
            {
                victoryStatsText.text = $"Swallowed {GameManager.Instance.SwallowedDebrisCount} pieces of space scrap!";
            }
        }

        public void OnDefeat()
        {
            if (defeatModal != null) defeatModal.SetActive(true);
            if (defeatMessageText != null)
            {
                defeatMessageText.text = "Nibbles is still hungry!\nLaunch backup pod or retry?";
            }
        }

        public void OnClickBackupPod()
        {
            if (defeatModal != null) defeatModal.SetActive(false);
            GameManager.Instance?.LaunchBackupPod();
        }

        public void OnClickRetry()
        {
            if (defeatModal != null) defeatModal.SetActive(false);
            if (victoryModal != null) victoryModal.SetActive(false);
            if (GameManager.Instance != null)
            {
                StartGame(GameManager.Instance.CurrentLevelNumber);
            }
        }

        public void OnClickNextLevel()
        {
            if (victoryModal != null) victoryModal.SetActive(false);
            if (GameManager.Instance != null)
            {
                StartGame(GameManager.Instance.CurrentLevelNumber + 1);
            }
        }

        private void SetPanel(GameObject activePanel)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(mainMenuPanel == activePanel);
            if (sectorSelectPanel != null) sectorSelectPanel.SetActive(sectorSelectPanel == activePanel);
            if (inGameHudPanel != null) inGameHudPanel.SetActive(inGameHudPanel == activePanel);
            if (victoryModal != null) victoryModal.SetActive(false);
            if (defeatModal != null) defeatModal.SetActive(false);
        }
    }
}
