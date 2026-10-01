using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using OrbitalSalvage.Core;
using OrbitalSalvage.UI;

namespace OrbitalSalvage.Parity
{
    public sealed class BaselineAdapter : IParityTarget
    {
        const string SaveFileName = "orbital_salvage_save.json";

        ParityContext ctx;
        ParityPointer pointer;
        int currentSectorIndex;
        string storeTab = "rings";
        bool saveBackedUp;

        readonly Vector3[] corners = new Vector3[4];

        public string Name => "baseline-uimanager";

        static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        static string BackupPath => SavePath + ".parity-backup";

        public void Attach(ParityContext context)
        {
            ctx = context;
            pointer = new ParityPointer(ctx.Rig);
        }

        public void PrepareFreshStart()
        {
            BackupUserData();
            if (EconomyManager.Instance != null) UnityEngine.Object.DestroyImmediate(EconomyManager.Instance.gameObject);
            if (File.Exists(SavePath)) File.Delete(SavePath);
            PlayerPrefs.DeleteAll();

            var field = typeof(GameManager).GetField("OnStateChanged", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            field?.SetValue(null, null);

            Time.timeScale = 1f;
            currentSectorIndex = 0;
            storeTab = "rings";
            pointer?.Reset();
        }

        public void Shutdown()
        {
            if (!saveBackedUp) return;
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
                if (File.Exists(BackupPath)) File.Move(BackupPath, SavePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Parity] Could not restore user save: {ex.Message}");
            }
            saveBackedUp = false;
        }

        void BackupUserData()
        {
            if (saveBackedUp) return;
            saveBackedUp = true;
            try
            {
                if (File.Exists(BackupPath)) return;
                if (File.Exists(SavePath)) File.Move(SavePath, BackupPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Parity] Could not back up user save: {ex.Message}");
            }
        }

        public void Boot(bool completeLoading)
        {
            pointer.Reset();
            pointer.DisableInputModules();
            var ui = UIManager.Instance;
            if (ui == null)
            {
                ctx.Note("failed", "UIManager.Instance is null after scene load");
                return;
            }
            if (completeLoading) ui.ShowHome();
            else ui.ShowLoading();
            ctx.AddTrace("page", completeLoading ? "home" : "loading");
        }

        UIManager Ui => UIManager.Instance;
        GameManager Gm => GameManager.Instance;
        Transform Root => UIManager.Instance != null ? UIManager.Instance.transform : null;

        public OpResult ShowPage(string page)
        {
            var ui = Ui;
            if (ui == null) return OpResult.Failed("UIManager missing");
            switch (page)
            {
                case "loading": ui.ShowLoading(); break;
                case "home": ui.ShowHome(); break;
                case "game": ui.ShowGameplay(); break;
                case "maps": ui.ShowMaps(); break;
                case "daily": ui.ShowDaily(); break;
                case "shop": ui.ShowShop(StoreTabIndex()); break;
                case "achievements": ui.ShowTrophies(); break;
                case "settings": ui.ShowSettings(); break;
                default: return OpResult.Unsupported($"unknown page '{page}'");
            }
            ctx.AddTrace("page", page);
            return OpResult.Ok();
        }

        int StoreTabIndex()
        {
            switch (storeTab)
            {
                case "singularities": return 1;
                case "skyboxes": return 2;
                default: return 0;
            }
        }

        public OpResult SetProfile(string path, string valueJson)
        {
            var eco = EconomyManager.Instance;
            if (eco == null) return OpResult.Failed("EconomyManager missing");
            object value = MiniJson.Parse(valueJson);
            switch (path)
            {
                case "credits":
                    eco.SaveData.scrapCredits = Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
                    return OpResult.Ok("mapped to SaveData.scrapCredits (no save)");
                default:
                    return OpResult.Unsupported($"profile path '{path}' has no equivalent in PlayerSaveData");
            }
        }

        public OpResult SetApp(string field, string valueJson)
        {
            object value = MiniJson.Parse(valueJson);
            if (field == "currentStoreTab")
            {
                storeTab = Convert.ToString(value);
                return OpResult.Ok("remembered until next showPage(shop)");
            }
            return OpResult.Unsupported($"app field '{field}' has no equivalent");
        }

        public OpResult Call(string method, string argsJson)
        {
            var args = MiniJson.Parse(argsJson) as List<object> ?? new List<object>();
            var ui = Ui;
            var gm = Gm;
            switch (method)
            {
                case "showPage":
                    return args.Count > 0 ? ShowPage(Convert.ToString(args[0])) : OpResult.Unsupported("showPage needs a page argument");
                case "exitGameToHome":
                    if (ui == null) return OpResult.Failed("UIManager missing");
                    ui.ShowHome();
                    ctx.AddTrace("page", "home");
                    return OpResult.Ok();
                case "restartSector":
                    if (ui == null || gm == null) return OpResult.Failed("managers missing");
                    ui.StartSector(gm.CurrentLevelNumber);
                    return OpResult.Ok();
                case "nextSector":
                    if (ui == null || gm == null) return OpResult.Failed("managers missing");
                    ui.StartSector(gm.CurrentLevelNumber % 50 + 1);
                    return OpResult.Ok();
                case "handleLevelVictory":
                    if (gm == null) return OpResult.Failed("GameManager missing");
                    typeof(GameManager).GetMethod("TriggerVictory", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(gm, null);
                    return OpResult.Ok("TriggerVictory via reflection");
                case "handleLevelFailure":
                    if (gm == null) return OpResult.Failed("GameManager missing");
                    gm.TriggerDefeat();
                    return OpResult.Ok();
                case "showToast":
                    return OpResult.Unsupported("no toast system in the current Unity UI");
                case "dismissGestureHint":
                    return OpResult.Unsupported("no gesture hint in the current Unity UI");
                case "updateAllCreditDisplays":
                case "updateShopDockBadge":
                case "updateDailyDockBadge":
                case "updateHomeView":
                case "renderSectorsPage":
                case "renderDailyPage":
                case "renderStorePage":
                case "renderAchievementsPage":
                case "syncSettingsUI":
                case "updateQuotaDisplay":
                case "initLayout":
                case "resetGameplaySessionUi":
                case "triggerHaptic":
                    return OpResult.Noop("current Unity UI is built once and refreshes itself in Update");
                default:
                    return OpResult.Unsupported($"method '{method}' not in the baseline mapping");
            }
        }

        Transform ResolveId(string id)
        {
            var root = Root;
            if (root == null) return null;
            foreach (var entry in BaselineIdMap.Entries)
            {
                if (entry.DomId != id) continue;
                var t = BaselineIdMap.Resolve(root, entry);
                if (t != null) return t;
            }
            return BaselineIdMap.FindByName(root, id);
        }

        Transform ResolveSelector(string selector)
        {
            string name = BaselineIdMap.DeriveSelectorName(selector);
            if (name == null) return null;
            var root = Root;
            if (BaselineIdMap.SelectorTargets.TryGetValue(name, out var path))
            {
                var mapped = BaselineIdMap.ResolvePath(root, path);
                if (mapped != null) return mapped;
            }
            return BaselineIdMap.FindByName(root, name);
        }

        bool TryCenter(Transform t, out Vector2 centerPx)
        {
            centerPx = default;
            var rt = t as RectTransform;
            if (rt == null) return false;
            ScreenRect(rt, out var min, out var max);
            centerPx = (min + max) * 0.5f;
            return true;
        }

        void ScreenRect(RectTransform rt, out Vector2 min, out Vector2 max)
        {
            rt.GetWorldCorners(corners);
            var cam = ctx.Rig.UiCamera;
            min = new Vector2(float.MaxValue, float.MaxValue);
            max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = cam.WorldToScreenPoint(corners[i]);
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
        }

        OpResult ClickTransform(Transform t, string label)
        {
            if (!t.gameObject.activeInHierarchy) return OpResult.Unsupported($"'{label}' is not visible");
            if (!TryCenter(t, out var center)) return OpResult.Unsupported($"'{label}' has no RectTransform");
            pointer.Move(center);
            pointer.Down();
            pointer.Up();
            if (pointer.LastPressed == null)
                return OpResult.Noop($"'{label}' hit '{(pointer.LastHit != null ? pointer.LastHit.name : "nothing")}' which has no pointer handler");
            return OpResult.Ok($"handled by '{pointer.LastPressed.name}'");
        }

        public OpResult Click(string id)
        {
            var t = ResolveId(id);
            if (t == null) return OpResult.Unsupported($"no Unity element for DOM id '{id}'");
            return ClickTransform(t, id);
        }

        public OpResult ClickSelector(string selector)
        {
            string name = BaselineIdMap.DeriveSelectorName(selector);
            if (name == null) return OpResult.Unsupported($"selector '{selector}' is outside the SPEC 8.1 forms");
            var t = ResolveSelector(selector);
            if (t == null) return OpResult.Unsupported($"no Unity element for selector '{selector}' (derived name '{name}')");
            return ClickTransform(t, name);
        }

        public OpResult Pointer(string kind, float xCss, float yCss)
        {
            var px = ctx.Rig.CssToScreen(xCss, yCss);
            pointer.Move(px);
            switch (kind)
            {
                case "down": pointer.Down(); break;
                case "up": pointer.Up(); break;
                case "move": break;
                default: return OpResult.Unsupported($"unknown pointer kind '{kind}'");
            }
            if (Gm != null && Gm.CurrentState == GameState.Playing)
                return OpResult.Unsupported("gameplay input in the current Unity code reads the legacy Input class, which the parity player cannot inject");
            return OpResult.Ok();
        }

        public OpResult Key(string key)
        {
            return OpResult.Unsupported("the current Unity code has no keyboard handling");
        }

        public OpResult LoadSector(int index)
        {
            var gm = Gm;
            if (gm == null) return OpResult.Failed("GameManager missing");
            currentSectorIndex = index;
            gm.StartSector(index + 1);
            return OpResult.Ok($"GameManager.StartSector({index + 1})");
        }

        public OpResult HoldPressed(string id)
        {
            var t = ResolveId(id);
            if (t == null) return OpResult.Unsupported($"no Unity element for DOM id '{id}'");
            if (!TryCenter(t, out var center)) return OpResult.Unsupported($"'{id}' has no RectTransform");
            pointer.Move(center);
            pointer.Down();
            return OpResult.Ok();
        }

        public OpResult Release()
        {
            if (pointer.IsDown) pointer.Up();
            return OpResult.Ok();
        }

        public OpResult Scroll(string target, float yCss)
        {
            return OpResult.Unsupported("the current Unity pages have no ScrollRect");
        }

        public List<LayoutElement> CollectLayout()
        {
            var result = new List<LayoutElement>();
            var unmapped = new List<string>();
            var root = Root;
            var mappedIds = new HashSet<string>();

            foreach (var entry in BaselineIdMap.Entries)
            {
                Transform t = BaselineIdMap.Resolve(root, entry);
                if (t == null)
                {
                    unmapped.Add(entry.DomId);
                    continue;
                }
                mappedIds.Add(entry.DomId);
                result.Add(Describe(entry.DomId, t));
            }

            foreach (var id in BaselineIdMap.WebStaticIds)
                if (!mappedIds.Contains(id) && !unmapped.Contains(id)) unmapped.Add(id);

            ctx.Extra["layoutMappedCount"] = mappedIds.Count;
            ctx.Extra["layoutUnmappedIds"] = unmapped;
            return result;
        }

        LayoutElement Describe(string id, Transform t)
        {
            var el = new LayoutElement { Id = id };
            var rt = t as RectTransform;
            bool active = t.gameObject.activeInHierarchy;
            if (rt != null && active)
            {
                ScreenRect(rt, out var min, out var max);
                float s = ParityProfiles.Scale;
                el.X = min.x / s;
                el.Y = (ctx.Rig.Height - max.y) / s;
                el.W = (max.x - min.x) / s;
                el.H = (max.y - min.y) / s;
            }
            var group = t.GetComponent<CanvasGroup>();
            el.Opacity = group != null ? group.alpha : 1f;
            el.Visible = active && el.W > 0f && el.H > 0f;
            el.Text = ExtractText(t);
            return el;
        }

        static string ExtractText(Transform t)
        {
            var sb = new StringBuilder();
            foreach (var tmp in t.GetComponentsInChildren<TMP_Text>(true))
            {
                if (!tmp.gameObject.activeInHierarchy) continue;
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(tmp.text);
            }
            var collapsed = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
            return collapsed.Length > 200 ? collapsed.Substring(0, 200) : collapsed;
        }
    }
}
