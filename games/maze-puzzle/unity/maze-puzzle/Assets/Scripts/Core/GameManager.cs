using System;
using System.Collections.Generic;
using UnityEngine;
using OrbitalSalvage.Data;
using OrbitalSalvage.Physics;
using OrbitalSalvage.UI;

namespace OrbitalSalvage.Core
{
    public enum GameState
    {
        MainMenu,
        SectorSelect,
        Playing,
        Paused,
        VictoryDebrief,
        DefeatDebrief
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private int currentLevelNumber = 1;

        [Header("Game Session Stats")]
        [SerializeField] private int currentQuota = 20;
        [SerializeField] private int swallowedDebrisCount = 0;
        [SerializeField] private int totalDebrisSpawned = 0;
        [SerializeField] private bool backupPodUsed = false;

        [Header("Prefabs & Roots")]
        [SerializeField] private GameObject debrisPrefab;
        [SerializeField] private CentrifugeController centrifuge;
        [SerializeField] private Transform debrisContainer;
        [SerializeField] private TextAsset sectorsJsonAsset;

        public GameState CurrentState => currentState;
        public int CurrentLevelNumber => currentLevelNumber;
        public int CurrentQuota => currentQuota;
        public int SwallowedDebrisCount => swallowedDebrisCount;

        private List<SectorDefinition> allSectors = new List<SectorDefinition>();
        private SectorDefinition currentSector;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadAllSectors();
        }

        private void Start()
        {
            if (centrifuge == null) centrifuge = FindAnyObjectByType<CentrifugeController>();
            if (debrisContainer == null)
            {
                var dc = GameObject.Find("DebrisContainer");
                if (dc != null) debrisContainer = dc.transform;
            }

            ConfigureContainmentRails();
            SetGameplayWorldVisible(false);
            SetState(GameState.MainMenu);
            UIManager.Instance?.ShowHome();
        }

        public void SetGameplayWorldVisible(bool visible)
        {
            if (centrifuge != null) centrifuge.gameObject.SetActive(visible);
            if (debrisContainer != null) debrisContainer.gameObject.SetActive(visible);

            var funnel = GameObject.Find("ContainmentFunnel");
            if (funnel != null) funnel.SetActive(visible);

            var nibbles = GameObject.Find("NibblesMascot");
            if (nibbles != null) nibbles.SetActive(visible);
        }

        private void ConfigureContainmentRails()
        {
            var funnelGo = GameObject.Find("ContainmentFunnel");
            if (funnelGo == null) return;

            Transform left = funnelGo.transform.Find("LeftGuideRail");
            Transform right = funnelGo.transform.Find("RightGuideRail");

            if (left != null && left.GetComponent<LineRenderer>() == null)
            {
                var lr = left.gameObject.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.startWidth = 0.08f;
                lr.endWidth = 0.08f;
                lr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                lr.startColor = new Color(0.85f, 0.25f, 1f, 0.9f);
                lr.endColor = new Color(0.6f, 0.15f, 0.95f, 0.9f);
                lr.positionCount = 2;
                lr.SetPositions(new Vector3[] { new Vector3(-2.2f, 0.8f, 0f), new Vector3(-0.6f, -1.8f, 0f) });
            }

            if (right != null && right.GetComponent<LineRenderer>() == null)
            {
                var lr = right.gameObject.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.startWidth = 0.08f;
                lr.endWidth = 0.08f;
                lr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                lr.startColor = new Color(0.85f, 0.25f, 1f, 0.9f);
                lr.endColor = new Color(0.6f, 0.15f, 0.95f, 0.9f);
                lr.positionCount = 2;
                lr.SetPositions(new Vector3[] { new Vector3(2.2f, 0.8f, 0f), new Vector3(0.6f, -1.8f, 0f) });
            }
        }

        private void LoadAllSectors()
        {
            if (sectorsJsonAsset == null)
            {
                sectorsJsonAsset = Resources.Load<TextAsset>("Data/sectors_50");
            }

            if (sectorsJsonAsset != null)
            {
                string json = "{\"sectors\":" + sectorsJsonAsset.text + "}";
                var wrapper = JsonUtility.FromJson<SectorListWrapper>(json);
                if (wrapper != null && wrapper.sectors != null)
                {
                    allSectors = wrapper.sectors;
                    Debug.Log($"[GameManager] Successfully loaded {allSectors.Count} sectors.");
                }
            }
        }

        public void StartSector(int levelNum)
        {
            SetGameplayWorldVisible(true);
            currentLevelNumber = Mathf.Clamp(levelNum, 1, 50);
            currentSector = allSectors.Find(s => s.levelNumber == currentLevelNumber);
            if (currentSector == null && allSectors.Count > 0) currentSector = allSectors[0];

            currentQuota = currentSector != null ? currentSector.quota : 20;
            swallowedDebrisCount = 0;
            backupPodUsed = false;

            // Clean previous debris
            if (debrisContainer != null)
            {
                foreach (Transform child in debrisContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // 1. Build sector topology in Centrifuge
            if (centrifuge != null && currentSector != null)
            {
                centrifuge.BuildSectorMaze(currentSector);
                centrifuge.SetAngle(0f);
            }

            // 2. Spawn initial debris inside centrifuge
            SpawnInitialDebris(currentSector);

            SetState(GameState.Playing);
        }

        private void SpawnInitialDebris(SectorDefinition sector)
        {
            if (sector == null) return;

            int count = sector.initialDebrisCount > 0 ? sector.initialDebrisCount : 10;
            float r = (sector.innerRadius + sector.ringRadius) * 0.5f * 0.015f; // scale factor
            totalDebrisSpawned = count;

            Vector3 centerPos = centrifuge != null ? centrifuge.transform.position : new Vector3(0f, 0.8f, 0f);

            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 0.7f + Mathf.PI * 0.15f;
                Vector3 spawnPos = centerPos + new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, 0f);

                GameObject go;
                if (debrisPrefab != null)
                {
                    go = Instantiate(debrisPrefab, spawnPos, Quaternion.identity, debrisContainer);
                }
                else
                {
                    go = new GameObject($"Debris_{i}");
                    go.transform.position = spawnPos;
                    if (debrisContainer != null) go.transform.SetParent(debrisContainer);
                    var col = go.AddComponent<CircleCollider2D>();
                    col.radius = 0.14f;
                    var rb = go.AddComponent<Rigidbody2D>();
                    rb.mass = 1.2f;
                    rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                    go.AddComponent<DebrisPiece>();
                }

                var piece = go.GetComponent<DebrisPiece>();
                if (piece != null)
                {
                    string typeName = sector.initialDebrisTypes != null && sector.initialDebrisTypes.Length > 0
                        ? sector.initialDebrisTypes[i % sector.initialDebrisTypes.Length]
                        : "titanium";

                    if (typeName.ToLower().Contains("solar")) piece.SetType(DebrisType.Solar);
                    else if (typeName.ToLower().Contains("volatile")) piece.SetType(DebrisType.Volatile);
                    else piece.SetType(DebrisType.Titanium);
                }
            }
        }

        public void OnDebrisSwallowed(DebrisPiece debris)
        {
            swallowedDebrisCount++;
            float progress = (float)swallowedDebrisCount / currentQuota;
            NibblesController.Instance?.UpdateQuotaProgress(progress);

            // Check victory condition
            if (swallowedDebrisCount >= currentQuota && currentState == GameState.Playing)
            {
                TriggerVictory();
            }
        }

        public void LaunchBackupPod()
        {
            if (backupPodUsed || currentState != GameState.DefeatDebrief) return;
            backupPodUsed = true;

            AudioManager.Instance?.PlayBackupPod();

            // Spawn 8 emergency debris pieces into the centrifuge
            float r = 1.8f;
            for (int i = 0; i < 8; i++)
            {
                float ang = Mathf.PI * 0.3f + i * 0.15f;
                Vector3 spawnPos = new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, 0f);
                var go = Instantiate(debrisPrefab, spawnPos, Quaternion.identity, debrisContainer);
                var piece = go.GetComponent<DebrisPiece>();
                if (piece != null) piece.SetType(DebrisType.Titanium);
            }

            SetState(GameState.Playing);
        }

        private void TriggerVictory()
        {
            int stars = CalculateStars();
            EconomyManager.Instance?.RecordSectorVictory(currentLevelNumber, stars);
            EconomyManager.Instance?.AddScrap(swallowedDebrisCount * 2);

            AudioManager.Instance?.PlayVictory();
            SetState(GameState.VictoryDebrief);
        }

        public void TriggerDefeat()
        {
            AudioManager.Instance?.PlayDefeat();
            SetState(GameState.DefeatDebrief);
        }

        public int CalculateStars()
        {
            float ratio = (float)swallowedDebrisCount / Mathf.Max(1, currentQuota);
            if (ratio >= 1.4f) return 3;
            if (ratio >= 1.15f) return 2;
            if (ratio >= 1.0f) return 1;
            return 0;
        }

        public void SetState(GameState newState)
        {
            currentState = newState;
            // UI Manager will respond to state change
        }
    }
}
