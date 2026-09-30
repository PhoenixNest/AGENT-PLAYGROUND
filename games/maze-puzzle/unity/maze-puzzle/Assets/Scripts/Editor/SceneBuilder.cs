using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using OrbitalSalvage.Data;
using OrbitalSalvage.Physics;
using OrbitalSalvage.Animation;
using OrbitalSalvage.Core;
using OrbitalSalvage.UI;

namespace OrbitalSalvage.Editor
{
    public static class SceneBuilder
    {
        public static void BuildAll()
        {
            Debug.Log("[SceneBuilder] Starting complete 1:1 Unity Game Scene and Prefabs build...");

            ConfigureEmojiSprites.ReimportAllEmojisAsSprites();
            SetupResourcesFolder();
            GameObject debrisPrefab = CreateDebrisPrefabs();
            CreateGatePrefabs();
            BuildMainScene(debrisPrefab);

            Debug.Log("[SceneBuilder] Complete 1:1 game build finished successfully!");
        }

        private static void SetupResourcesFolder()
        {
            string resDir = Path.Combine(Application.dataPath, "Resources");
            string resData = Path.Combine(resDir, "Data");
            string resAudio = Path.Combine(resDir, "Audio");

            if (!Directory.Exists(resData)) Directory.CreateDirectory(resData);
            if (!Directory.Exists(resAudio)) Directory.CreateDirectory(resAudio);

            // Copy sectors_50.json
            string srcJson = Path.Combine(Application.dataPath, "Scripts/Data/sectors_50.json");
            string destJson = Path.Combine(resData, "sectors_50.json");
            if (File.Exists(srcJson))
            {
                File.Copy(srcJson, destJson, true);
            }

            // Copy audio files
            string srcAudioDir = Path.Combine(Application.dataPath, "Audio");
            if (Directory.Exists(srcAudioDir))
            {
                foreach (var file in Directory.GetFiles(srcAudioDir, "*.wav"))
                {
                    string fname = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(resAudio, fname), true);
                }
            }

            AssetDatabase.Refresh();
        }

        private static GameObject CreateDebrisPrefabs()
        {
            string prefabsDir = Path.Combine(Application.dataPath, "Prefabs");
            if (!Directory.Exists(prefabsDir)) Directory.CreateDirectory(prefabsDir);

            // Base Debris GameObject
            GameObject debrisGo = new GameObject("Debris_Titanium");
            var col = debrisGo.AddComponent<CircleCollider2D>();
            col.radius = 0.12f;

            var rb = debrisGo.AddComponent<Rigidbody2D>();
            rb.mass = 1.2f;
            rb.gravityScale = 1.0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var sr = debrisGo.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.7f, 0.8f, 0.95f, 1f); // Metallic titanium tint

            var piece = debrisGo.AddComponent<DebrisPiece>();
            piece.SetType(DebrisType.Titanium);

            string prefabPath = "Assets/Prefabs/Debris_Titanium.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(debrisGo, prefabPath);
            GameObject.DestroyImmediate(debrisGo);

            Debug.Log($"[SceneBuilder] Created prefab at {prefabPath}");
            return prefab;
        }

        private static void CreateGatePrefabs()
        {
            string prefabsDir = "Assets/Prefabs";

            // Multiplier Gate
            GameObject gateGo = new GameObject("Gate_Multiplier");
            var boxCol = gateGo.AddComponent<BoxCollider2D>();
            boxCol.size = new Vector2(0.8f, 0.3f);
            boxCol.isTrigger = true;

            var sr = gateGo.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.2f, 0.6f, 1f, 0.85f); // Neon cyan gate

            var gateComp = gateGo.AddComponent<MultiplierGate>();
            gateComp.Setup(GateType.Multiply, 2);

            PrefabUtility.SaveAsPrefabAsset(gateGo, $"{prefabsDir}/Gate_Multiplier.prefab");
            GameObject.DestroyImmediate(gateGo);
        }

        private static void BuildMainScene(GameObject debrisPrefab)
        {
            string scenesDir = Path.Combine(Application.dataPath, "Scenes");
            if (!Directory.Exists(scenesDir)) Directory.CreateDirectory(scenesDir);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.2f;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.10f, 1f); // Cosmic dark space
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();

            // 2. Core Managers
            GameObject mgrGo = new GameObject("GameManager");
            var gm = mgrGo.AddComponent<GameManager>();
            var em = mgrGo.AddComponent<EconomyManager>();
            var am = mgrGo.AddComponent<AudioManager>();

            // 3. Centrifuge Ring Structure
            GameObject centrifugeRoot = new GameObject("CentrifugeRoot");
            centrifugeRoot.transform.position = new Vector3(0f, 0.8f, 0f);
            var centrifugeCtrl = centrifugeRoot.AddComponent<CentrifugeController>();

            GameObject outerRing = new GameObject("OuterRing");
            outerRing.transform.SetParent(centrifugeRoot.transform, false);

            // Outer perimeter walls using edge colliders (circular arc with bottom exit aperture)
            var edgeCol = outerRing.AddComponent<EdgeCollider2D>();
            int segments = 40;
            float radius = 2.3f;
            float apertureHalf = 0.5f; // opening at bottom
            List<Vector2> points = new List<Vector2>();

            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(apertureHalf, Mathf.PI * 2f - apertureHalf, (float)i / segments) - Mathf.PI * 0.5f;
                points.Add(new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
            }
            edgeCol.points = points.ToArray();

            // 4. Containment Funnel (Magnetic guide rails directing scrap to Nibbles)
            GameObject funnelGo = new GameObject("ContainmentFunnel");
            funnelGo.transform.position = Vector3.zero;

            float topY = -1.09f;
            float topX = 1.05f;
            float botY = -2.75f;
            float botX = 0.51f;

            var leftRail = new GameObject("LeftGuideRail");
            leftRail.transform.SetParent(funnelGo.transform, false);
            var leftEdge = leftRail.AddComponent<EdgeCollider2D>();
            leftEdge.points = new Vector2[] { new Vector2(-topX, topY), new Vector2(-botX, botY) };
            var leftLr = leftRail.AddComponent<LineRenderer>();
            leftLr.useWorldSpace = true;
            leftLr.startWidth = 0.05f;
            leftLr.endWidth = 0.05f;
            leftLr.material = Utils.SpriteHelper.GetDashedLineMaterial(new Color(0.22f, 0.74f, 0.97f, 0.85f));
            leftLr.textureMode = LineTextureMode.Tile;
            leftLr.positionCount = 2;
            leftLr.SetPositions(new Vector3[] { new Vector3(-topX, topY, 0f), new Vector3(-botX, botY, 0f) });
            leftLr.sortingOrder = 4;

            var rightRail = new GameObject("RightGuideRail");
            rightRail.transform.SetParent(funnelGo.transform, false);
            var rightEdge = rightRail.AddComponent<EdgeCollider2D>();
            rightEdge.points = new Vector2[] { new Vector2(topX, topY), new Vector2(botX, botY) };
            var rightLr = rightRail.AddComponent<LineRenderer>();
            rightLr.useWorldSpace = true;
            rightLr.startWidth = 0.05f;
            rightLr.endWidth = 0.05f;
            rightLr.material = Utils.SpriteHelper.GetDashedLineMaterial(new Color(0.22f, 0.74f, 0.97f, 0.85f));
            rightLr.textureMode = LineTextureMode.Tile;
            rightLr.positionCount = 2;
            rightLr.SetPositions(new Vector3[] { new Vector3(topX, topY, 0f), new Vector3(botX, botY, 0f) });
            rightLr.sortingOrder = 4;

            for (int r = 0; r < 4; r++)
            {
                var rungGo = new GameObject($"FunnelRung_{r}");
                rungGo.transform.SetParent(funnelGo.transform, false);
                var rungLr = rungGo.AddComponent<LineRenderer>();
                rungLr.useWorldSpace = true;
                rungLr.startWidth = 0.025f;
                rungLr.endWidth = 0.025f;
                rungLr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                rungLr.startColor = new Color(0.22f, 0.74f, 0.97f, 0.35f);
                rungLr.endColor = new Color(0.22f, 0.74f, 0.97f, 0.35f);
                rungLr.positionCount = 3;
                float t = (r + 1) / 5f;
                float ry = Mathf.Lerp(topY, botY, t);
                float rx = Mathf.Lerp(topX, botX, t);
                rungLr.SetPositions(new Vector3[] {
                    new Vector3(-rx, ry, 0f),
                    new Vector3(0f, ry - 0.06f, 0f),
                    new Vector3(rx, ry, 0f)
                });
                rungLr.sortingOrder = 3;
            }

            for (int side = -1; side <= 1; side += 2)
            {
                var dotGo = new GameObject($"EmitterDot_{side}");
                dotGo.transform.SetParent(funnelGo.transform, false);
                dotGo.transform.localPosition = new Vector3(side * topX, topY, 0f);
                var sr = dotGo.AddComponent<SpriteRenderer>();
                sr.sprite = Utils.SpriteHelper.GetCircleSprite();
                sr.color = new Color(0.22f, 0.74f, 0.97f, 0.95f);
                sr.sortingOrder = 5;
                dotGo.transform.localScale = Vector3.one * 0.12f;
            }

            // 5. Debris Container
            GameObject debrisContainer = new GameObject("DebrisContainer");
            debrisContainer.transform.position = Vector3.zero;

            // 6. Nibbles the Micro-Black Hole Mascot
            GameObject nibblesGo = new GameObject("NibblesMascot");
            nibblesGo.transform.position = new Vector3(0f, -2.75f, 0f);

            var nibblesCtrl = nibblesGo.AddComponent<NibblesController>();

            var eventHorizonCol = nibblesGo.AddComponent<CircleCollider2D>();
            eventHorizonCol.radius = 0.32f;
            eventHorizonCol.isTrigger = true;

            // Visual body & facial rigging
            GameObject bodyGo = new GameObject("MascotBody");
            bodyGo.transform.SetParent(nibblesGo.transform, false);
            var bodySr = bodyGo.AddComponent<SpriteRenderer>();
            bodySr.color = Color.white;
            bodySr.sortingOrder = 6;
            bodyGo.transform.localScale = Vector3.one * 0.62f;

            var eyeTracker = nibblesGo.AddComponent<MascotEyeTracker>();

            // 7. UI Canvas
            GameObject canvasGo = new GameObject("UI_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(430f, 932f); // iPhone 16 Pro Max Logical Viewport
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject safeAreaGo = new GameObject("SafeAreaPanel");
            safeAreaGo.transform.SetParent(canvasGo.transform, false);
            var safeRect = safeAreaGo.AddComponent<RectTransform>();
            safeRect.anchorMin = Vector2.zero;
            safeRect.anchorMax = Vector2.one;
            safeRect.sizeDelta = Vector2.zero;
            safeAreaGo.AddComponent<SafeAreaFitter>();

            var uiMgr = canvasGo.AddComponent<UIManager>();
            uiMgr.EnsureHierarchyBuilt();
            uiMgr.ShowHome();

            // Wire GameManager references
            var gmSo = new SerializedObject(gm);
            gmSo.FindProperty("debrisPrefab").objectReferenceValue = debrisPrefab;
            gmSo.FindProperty("centrifuge").objectReferenceValue = centrifugeCtrl;
            gmSo.FindProperty("debrisContainer").objectReferenceValue = debrisContainer.transform;
            gmSo.ApplyModifiedProperties();

            // EventSystem
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Save Scene
            string scenePath = "Assets/Scenes/MainGameScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            // Register in EditorBuildSettings
            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;

            Debug.Log($"[SceneBuilder] Saved MainGameScene at {scenePath} and registered in Build Settings!");
        }
    }
}
