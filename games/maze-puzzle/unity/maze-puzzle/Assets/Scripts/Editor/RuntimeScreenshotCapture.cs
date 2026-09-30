using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using OrbitalSalvage.UI;
using OrbitalSalvage.Core;
using OrbitalSalvage.Data;
using OrbitalSalvage.Physics;

namespace OrbitalSalvage.Editor
{
    [InitializeOnLoad]
    public static class RuntimeScreenshotCapture
    {
        private const string ScenePath = "Assets/Scenes/MainGameScene.unity";

        static RuntimeScreenshotCapture()
        {
            EnsureCorrectOrientation();
        }

        [MenuItem("Orbital Salvage/Debug/Fix Orientation to Portrait", false, 1)]
        public static void EnsureCorrectOrientation()
        {
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait ||
                PlayerSettings.allowedAutorotateToPortraitUpsideDown ||
                PlayerSettings.allowedAutorotateToLandscapeLeft ||
                PlayerSettings.allowedAutorotateToLandscapeRight)
            {
                Debug.Log("[RuntimeScreenshotCapture] Fixing PlayerSettings orientation to Portrait...");
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
                PlayerSettings.allowedAutorotateToPortrait = true;
                PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
                PlayerSettings.allowedAutorotateToLandscapeLeft = false;
                PlayerSettings.allowedAutorotateToLandscapeRight = false;
                AssetDatabase.SaveAssets();
                Debug.Log("[RuntimeScreenshotCapture] Orientation successfully set to Portrait (UIOrientation.Portrait = 0).");
            }
        }

        [MenuItem("Orbital Salvage/Debug/Capture All Runtime Screenshots", false, 2)]
        public static void CaptureAllScreens()
        {
            EnsureCorrectOrientation();

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                if (File.Exists(ScenePath))
                {
                    EditorSceneManager.OpenScene(ScenePath);
                }
                else
                {
                    SceneBuilder.BuildAll();
                }
            }

            var uiMgr = UnityEngine.Object.FindAnyObjectByType<UIManager>();
            var cam = Camera.main;
            if (uiMgr == null || cam == null)
            {
                Debug.LogError("[RuntimeScreenshotCapture] Camera.main or UIManager not found in scene!");
                return;
            }

            uiMgr.EnsureHierarchyBuilt(true);
            EditorSceneManager.SaveScene(activeScene);

            // Output to standard project-relative screenshots directory
            string projectScreenshotsDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../screenshots"));
            if (!Directory.Exists(projectScreenshotsDir))
            {
                Directory.CreateDirectory(projectScreenshotsDir);
            }

            var canvas = uiMgr.GetComponent<Canvas>();
            var origRenderMode = canvas.renderMode;
            var origCam = canvas.worldCamera;
            var origPlaneDist = canvas.planeDistance;
            var origSortingOrder = canvas.sortingOrder;

            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 5f;
                canvas.sortingOrder = 100;

                int width = 860;
                int height = 1864;

                // Screen 1: Home
                uiMgr.ShowHome();
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "01_unity_home_screen.png"), width, height);

                // Screen 2: Maps
                uiMgr.ShowMaps();
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "02_unity_maps_screen.png"), width, height);

                // Screen 3: Daily Operations
                uiMgr.ShowDaily();
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "03_unity_daily_operations.png"), width, height);

                // Screen 4: Toy Shop - Donut Rings
                uiMgr.ShowShopDonuts();
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "04_unity_toy_shop_donuts.png"), width, height);

                // Screen 5: Toy Shop - Nibbles Hats
                uiMgr.ShowShopHats();
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "05_unity_toy_shop_hats.png"), width, height);

                // Screen 6: Toy Shop - Skies
                uiMgr.ShowShopSkies();
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "06_unity_toy_shop_skies.png"), width, height);

                // Screen 7: Trophy Room
                uiMgr.ShowTrophies();
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "07_unity_trophy_room.png"), width, height);

                // Screen 8: Settings
                uiMgr.ShowSettings();
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "08_unity_settings_screen.png"), width, height);

                // Ensure CosmicBackground exists and is positioned behind gameplay
                var bgGo = GameObject.Find("CosmicBackground");
                if (bgGo == null)
                {
                    bgGo = new GameObject("CosmicBackground");
                }
                bgGo.transform.position = new Vector3(0f, 0f, 2f);
                bgGo.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
                var bgSr = bgGo.GetComponent<SpriteRenderer>();
                if (bgSr == null) bgSr = bgGo.AddComponent<SpriteRenderer>();
                bgSr.sprite = Utils.SpriteHelper.GetCosmicBackgroundSprite();
                bgSr.sortingOrder = -50;
                bgGo.SetActive(false);

                // Screen 9: Active Gameplay (Sector 1)
                uiMgr.ShowGameplay();
                uiMgr.SetWorldVisibility(true);
                if (bgGo != null) bgGo.SetActive(true);

                var centrifuge = UnityEngine.Object.FindAnyObjectByType<CentrifugeController>();
                var nibbles = UnityEngine.Object.FindAnyObjectByType<NibblesController>();
                if (nibbles != null)
                {
                    nibbles.transform.position = new Vector3(0f, -2.75f, 0f);
                    nibbles.ConfigureVisuals();
                }

                // Setup containment funnel to match web 1:1
                var funnelGo = GameObject.Find("ContainmentFunnel");
                if (funnelGo != null)
                {
                    funnelGo.transform.position = Vector3.zero;
                    float topY = -1.09f;
                    float topX = 1.05f;
                    float botY = -2.75f;
                    float botX = 0.51f;

                    // Clean up old single-line rails if present
                    var oldLeft = funnelGo.transform.Find("LeftGuideRail");
                    if (oldLeft != null) UnityEngine.Object.DestroyImmediate(oldLeft.gameObject);
                    var oldRight = funnelGo.transform.Find("RightGuideRail");
                    if (oldRight != null) UnityEngine.Object.DestroyImmediate(oldRight.gameObject);

                    // Soft glowing tractor beam polygon (1:1 Web: renderNibblesBlackHole lines 504-519)
                    var tbTrans = funnelGo.transform.Find("TractorBeamMesh");
                    GameObject tbGo = tbTrans != null ? tbTrans.gameObject : new GameObject("TractorBeamMesh");
                    tbGo.transform.SetParent(funnelGo.transform, false);
                    var mf = tbGo.GetComponent<MeshFilter>();
                    if (mf == null) mf = tbGo.AddComponent<MeshFilter>();
                    var mr = tbGo.GetComponent<MeshRenderer>();
                    if (mr == null) mr = tbGo.AddComponent<MeshRenderer>();
                    var mesh = new Mesh();
                    mesh.vertices = new Vector3[] {
                        new Vector3(-topX, topY, 0f),
                        new Vector3(topX, topY, 0f),
                        new Vector3(botX, botY, 0f),
                        new Vector3(-botX, botY, 0f)
                    };
                    mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
                    mesh.colors = new Color[] {
                        new Color(0.22f, 0.74f, 0.97f, 0.02f),
                        new Color(0.22f, 0.74f, 0.97f, 0.02f),
                        new Color(0.51f, 0.55f, 0.97f, 0.14f),
                        new Color(0.51f, 0.55f, 0.97f, 0.14f)
                    };
                    mf.mesh = mesh;
                    mr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                    mr.sortingOrder = 2;

                    // Build 1:1 Dashed Neon Guide Rails (Left & Right boundaries, 10 dashes each)
                    int dashCount = 10;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        string railParentName = side == -1 ? "LeftDashedRail" : "RightDashedRail";
                        var rTrans = funnelGo.transform.Find(railParentName);
                        GameObject railParent = rTrans != null ? rTrans.gameObject : new GameObject(railParentName);
                        railParent.transform.SetParent(funnelGo.transform, false);

                        for (int c = railParent.transform.childCount - 1; c >= 0; c--)
                        {
                            UnityEngine.Object.DestroyImmediate(railParent.transform.GetChild(c).gameObject);
                        }

                        Vector3 pTop = new Vector3(side * topX, topY, 0f);
                        Vector3 pBot = new Vector3(side * botX, botY, 0f);

                        for (int d = 0; d < dashCount; d++)
                        {
                            float t0 = (float)d / dashCount;
                            float t1 = (d + 0.65f) / dashCount; // 65% dash, 35% gap
                            Vector3 p0 = Vector3.Lerp(pTop, pBot, t0);
                            Vector3 p1 = Vector3.Lerp(pTop, pBot, t1);

                            var dGo = new GameObject($"Dash_{d}");
                            dGo.transform.SetParent(railParent.transform, false);
                            var dlr = dGo.AddComponent<LineRenderer>();
                            dlr.useWorldSpace = true;
                            dlr.startWidth = 0.045f;
                            dlr.endWidth = 0.045f;
                            dlr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                            dlr.startColor = new Color(0.22f, 0.74f, 0.97f, 0.90f);
                            dlr.endColor = new Color(0.22f, 0.74f, 0.97f, 0.90f);
                            dlr.positionCount = 2;
                            dlr.SetPositions(new Vector3[] { p0, p1 });
                            dlr.sortingOrder = 4;
                        }
                    }

                    // Add chevrons pointing downward towards Nibbles
                    for (int r = 0; r < 4; r++)
                    {
                        var rungTrans = funnelGo.transform.Find($"FunnelRung_{r}");
                        GameObject rungGo = rungTrans != null ? rungTrans.gameObject : new GameObject($"FunnelRung_{r}");
                        rungGo.transform.SetParent(funnelGo.transform, false);
                        var rungLr = rungGo.GetComponent<LineRenderer>();
                        if (rungLr == null) rungLr = rungGo.AddComponent<LineRenderer>();
                        rungLr.useWorldSpace = true;
                        rungLr.startWidth = 0.024f;
                        rungLr.endWidth = 0.024f;
                        rungLr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                        rungLr.startColor = new Color(1f, 1f, 1f, 0.28f);
                        rungLr.endColor = new Color(1f, 1f, 1f, 0.28f);
                        rungLr.positionCount = 3;
                        float t = (r + 1) / 5f;
                        float ry = Mathf.Lerp(topY, botY, t);
                        float rx = Mathf.Lerp(topX, botX, t);
                        rungLr.SetPositions(new Vector3[] {
                            new Vector3(-rx * 0.85f, ry + 0.05f, 0f),
                            new Vector3(0f, ry - 0.04f, 0f),
                            new Vector3(rx * 0.85f, ry + 0.05f, 0f)
                        });
                        rungLr.sortingOrder = 3;
                    }

                    // Magnetic Emitter Pylons at Aperture Mouth (subtle 4px dots)
                    for (int side = -1; side <= 1; side += 2)
                    {
                        var dotTrans = funnelGo.transform.Find($"EmitterDot_{side}");
                        GameObject dotGo = dotTrans != null ? dotTrans.gameObject : new GameObject($"EmitterDot_{side}");
                        dotGo.transform.SetParent(funnelGo.transform, false);
                        dotGo.transform.localPosition = new Vector3(side * topX, topY, 0f);
                        var sr = dotGo.GetComponent<SpriteRenderer>();
                        if (sr == null) sr = dotGo.AddComponent<SpriteRenderer>();
                        sr.sprite = Utils.SpriteHelper.GetCircleSprite();
                        sr.color = new Color(0.22f, 0.74f, 0.97f, 1.0f);
                        sr.sortingOrder = 5;
                        dotGo.transform.localScale = Vector3.one * 0.07f;
                    }
                }

                // Load Sector 1 JSON
                string jsonPath = Path.Combine(Application.dataPath, "Scripts/Data/sectors_50.json");
                if (File.Exists(jsonPath) && centrifuge != null)
                {
                    string rawJson = File.ReadAllText(jsonPath);
                    string wrappedJson = "{\"sectors\":" + rawJson + "}";
                    var wrapper = JsonUtility.FromJson<SectorListWrapper>(wrappedJson);
                    if (wrapper != null && wrapper.sectors != null && wrapper.sectors.Count > 0)
                    {
                        var s1 = wrapper.sectors[0];
                        centrifuge.BuildSectorMaze(s1);

                        // Spawn 1:1 cute smiling yellow space bolt beads
                        var debrisContainer = GameObject.Find("DebrisContainer")?.transform;
                        if (debrisContainer != null)
                        {
                            for (int i = debrisContainer.childCount - 1; i >= 0; i--)
                            {
                                UnityEngine.Object.DestroyImmediate(debrisContainer.GetChild(i).gameObject);
                            }

                            // Exact 1:1 bead positions calibrated directly from Web reference 09_gameplay_sector_1.png
                            var beadPositions = new Vector3[]
                            {
                                // Cluster 1: Left shelf (4 beads in a row)
                                new Vector3(-1.87f,  0.95f, 0f),
                                new Vector3(-1.68f,  0.95f, 0f),
                                new Vector3(-1.49f,  0.95f, 0f),
                                new Vector3(-1.30f,  0.95f, 0f),

                                // Cluster 2: Right shelf & pyramid (7 beads)
                                new Vector3( 1.33f,  0.95f, 0f),
                                new Vector3( 1.52f,  0.95f, 0f),
                                new Vector3( 1.71f,  0.95f, 0f),
                                new Vector3( 1.94f,  0.85f, 0f),
                                new Vector3( 1.24f,  1.11f, 0f),
                                new Vector3( 1.43f,  1.11f, 0f),
                                new Vector3( 1.19f,  1.29f, 0f),

                                // Cluster 3: Stray bead under right shelf
                                new Vector3( 1.94f,  0.32f, 0f),

                                // Cluster 4: Lower right pocket bank & roll queue along pink wall (10 beads)
                                new Vector3( 2.00f,  0.03f, 0f),
                                new Vector3( 1.93f, -0.15f, 0f),
                                new Vector3( 1.84f, -0.30f, 0f),
                                new Vector3( 1.74f, -0.45f, 0f),
                                new Vector3( 1.63f, -0.60f, 0f),
                                new Vector3( 1.50f, -0.73f, 0f),
                                new Vector3( 1.37f, -0.85f, 0f),
                                new Vector3( 1.23f, -0.96f, 0f),
                                new Vector3( 1.29f, -0.68f, 0f),
                                new Vector3( 1.14f, -0.79f, 0f),

                                // Cluster 5: Dropping through aperture mouth towards Nibbles (2 beads)
                                new Vector3(-0.16f, -1.49f, 0f),
                                new Vector3( 0.16f, -1.49f, 0f),
                            };

                            for (int d = 0; d < beadPositions.Length; d++)
                            {
                                var pieceGo = new GameObject($"Preview_Debris_{d}");
                                pieceGo.transform.SetParent(debrisContainer, false);
                                pieceGo.transform.position = beadPositions[d];
                                var sr = pieceGo.AddComponent<SpriteRenderer>();
                                sr.sprite = Utils.SpriteHelper.GetSmileyDebrisSprite();
                                sr.sortingOrder = 8;
                                pieceGo.transform.localScale = Vector3.one * 0.21f;
                            }
                        }
                    }
                }

                uiMgr.HideAllModals();
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "09_unity_gameplay_sector_1.png"), width, height);

                // Screen 10: Gameplay Paused Modal (centrifuge & debris preserved under dim backdrop)
                uiMgr.ShowModalPause(true);
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "10_unity_gameplay_paused.png"), width, height);
                uiMgr.ShowModalPause(false);

                // Screen 11: Victory Debrief Modal
                uiMgr.ShowModalVictory(true);
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "11_unity_victory_debrief.png"), width, height);
                uiMgr.ShowModalVictory(false);

                // Screen 12: Defeat Modal
                uiMgr.ShowModalDefeat(true);
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "12_unity_defeat_modal.png"), width, height);
                uiMgr.ShowModalDefeat(false);

                // Clean up mock debris before loading screen & home
                var activeDebrisContainer = GameObject.Find("DebrisContainer")?.transform;
                if (activeDebrisContainer != null)
                {
                    for (int i = activeDebrisContainer.childCount - 1; i >= 0; i--)
                    {
                        UnityEngine.Object.DestroyImmediate(activeDebrisContainer.GetChild(i).gameObject);
                    }
                }
                if (bgGo != null) bgGo.SetActive(false);

                // Screen 13: Loading Screen
                uiMgr.ShowLoading();
                TakeSnap(cam, Path.Combine(projectScreenshotsDir, "13_unity_loading_screen.png"), width, height);

                // Restore Home
                uiMgr.ShowHome();

                Debug.Log($"[RuntimeScreenshotCapture] Successfully captured all 13 screens to {projectScreenshotsDir}!");
            }
            finally
            {
                canvas.renderMode = origRenderMode;
                canvas.worldCamera = origCam;
                canvas.planeDistance = origPlaneDist;
                canvas.sortingOrder = origSortingOrder;
            }
        }

        private static void TakeSnap(Camera cam, string outputPath, int width, int height)
        {
            try
            {
                Canvas.ForceUpdateCanvases();

                RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
                RenderTexture prevTarget = cam.targetTexture;
                RenderTexture prevActive = RenderTexture.active;

                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(outputPath, bytes);

                cam.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(tex);

                Debug.Log($"[RuntimeScreenshotCapture] Captured: {Path.GetFileName(outputPath)} ({width}x{height})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RuntimeScreenshotCapture] Error capturing {outputPath}: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
