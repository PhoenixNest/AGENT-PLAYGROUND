using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OrbitalSalvage.Data;

namespace OrbitalSalvage.Editor
{
    public static class LevelImporter
    {
        public static void BatchImportAll50Levels()
        {
            Debug.Log("[LevelImporter] Starting batch import of all 50 sectors...");

            string jsonPath = Path.Combine(Application.dataPath, "Scripts/Data/sectors_50.json");
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"[LevelImporter] Could not find sectors_50.json at: {jsonPath}");
                return;
            }

            string rawJson = File.ReadAllText(jsonPath);
            string wrappedJson = "{\"sectors\":" + rawJson + "}";

            var wrapper = JsonUtility.FromJson<SectorListWrapper>(wrappedJson);
            if (wrapper == null || wrapper.sectors == null || wrapper.sectors.Count == 0)
            {
                Debug.LogError("[LevelImporter] Failed to parse sectors JSON.");
                return;
            }

            int sanitizedCount = 0;
            for (int i = 0; i < wrapper.sectors.Count; i++)
            {
                var sector = wrapper.sectors[i];
                MazeSanitizer.Sanitize(sector);
                sanitizedCount++;
            }

            Debug.Log($"[LevelImporter] Successfully imported and sanitized {sanitizedCount} / {wrapper.sectors.Count} sectors.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
