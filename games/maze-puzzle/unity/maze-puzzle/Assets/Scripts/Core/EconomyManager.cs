using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OrbitalSalvage.Core
{
    [Serializable]
    public class SectorStarEntry
    {
        public int level;
        public int stars;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public int scrapCredits = 150;
        public int highestUnlockedLevel = 1;
        public List<SectorStarEntry> sectorStarEntries = new List<SectorStarEntry>();
        public List<string> unlockedSkins = new List<string>() { "skin_ring_default", "skin_hole_default" };
        public string equippedRingSkin = "skin_ring_default";
        public string equippedHoleSkin = "skin_hole_default";
        public string lastDailyShiftDate = "";
    }

    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        public PlayerSaveData SaveData { get; private set; } = new PlayerSaveData();

        private readonly Dictionary<int, int> sectorStarsLookup = new Dictionary<int, int>();

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
            if (!sectorStarsLookup.TryGetValue(levelNum, out int existing) || existing < stars)
            {
                sectorStarsLookup[levelNum] = stars;
                SyncStarEntriesFromLookup();
            }

            if (levelNum >= SaveData.highestUnlockedLevel && levelNum < 50)
            {
                SaveData.highestUnlockedLevel = levelNum + 1;
            }

            Save();
        }

        public int GetSectorStars(int levelNum)
        {
            if (sectorStarsLookup.TryGetValue(levelNum, out int s)) return s;
            return 0;
        }

        public void Save()
        {
            try
            {
                SyncStarEntriesFromLookup();
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
                    if (SaveData == null) SaveData = new PlayerSaveData();
                    if (SaveData.sectorStarEntries == null) SaveData.sectorStarEntries = new List<SectorStarEntry>();
                    RebuildStarLookup();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EconomyManager] Load failed, using defaults: {ex.Message}");
                SaveData = new PlayerSaveData();
                sectorStarsLookup.Clear();
            }
        }

        private void RebuildStarLookup()
        {
            sectorStarsLookup.Clear();
            if (SaveData?.sectorStarEntries == null) return;
            foreach (var entry in SaveData.sectorStarEntries)
            {
                if (entry == null) continue;
                if (!sectorStarsLookup.ContainsKey(entry.level) || sectorStarsLookup[entry.level] < entry.stars)
                {
                    sectorStarsLookup[entry.level] = entry.stars;
                }
            }
        }

        private void SyncStarEntriesFromLookup()
        {
            if (SaveData.sectorStarEntries == null) SaveData.sectorStarEntries = new List<SectorStarEntry>();
            SaveData.sectorStarEntries.Clear();
            foreach (var kvp in sectorStarsLookup)
            {
                SaveData.sectorStarEntries.Add(new SectorStarEntry { level = kvp.Key, stars = kvp.Value });
            }
        }
    }
}
