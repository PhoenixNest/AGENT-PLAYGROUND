using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OrbitalSalvage.Core
{
    [Serializable]
    public class PlayerSaveData
    {
        public int scrapCredits = 150;
        public int highestUnlockedLevel = 1;
        public Dictionary<int, int> sectorStars = new Dictionary<int, int>();
        public List<string> unlockedSkins = new List<string>() { "skin_ring_default", "skin_hole_default" };
        public string equippedRingSkin = "skin_ring_default";
        public string equippedHoleSkin = "skin_hole_default";
        public string lastDailyShiftDate = "";
    }

    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        public PlayerSaveData SaveData { get; private set; } = new PlayerSaveData();

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "orbital_salvage_save.json");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void AddScrap(int amount)
        {
            SaveData.scrapCredits = Mathf.Max(0, SaveData.scrapCredits + amount);
            Save();
        }

        public bool SpendScrap(int amount)
        {
            if (SaveData.scrapCredits >= amount)
            {
                SaveData.scrapCredits -= amount;
                Save();
                return true;
            }
            return false;
        }

        public void RecordSectorVictory(int levelNum, int stars)
        {
            if (!SaveData.sectorStars.ContainsKey(levelNum) || SaveData.sectorStars[levelNum] < stars)
            {
                SaveData.sectorStars[levelNum] = stars;
            }

            if (levelNum >= SaveData.highestUnlockedLevel && levelNum < 50)
            {
                SaveData.highestUnlockedLevel = levelNum + 1;
            }

            Save();
        }

        public int GetSectorStars(int levelNum)
        {
            if (SaveData.sectorStars.TryGetValue(levelNum, out int s)) return s;
            return 0;
        }

        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(SaveData, true);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EconomyManager] Save failed: {ex.Message}");
            }
        }

        public void Load()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    SaveData = JsonUtility.FromJson<PlayerSaveData>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EconomyManager] Load failed, using defaults: {ex.Message}");
                SaveData = new PlayerSaveData();
            }
        }
    }
}
