using System;
using System.Collections.Generic;
using System.IO;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.App
{
    /// <summary>Profile persistence. Unity uses <see cref="FileProfileStore"/>; tests use <see cref="MemoryProfileStore"/>.</summary>
    public interface IProfileStore
    {
        string GetItem(string key);
        void SetItem(string key, string value);
    }

    public sealed class MemoryProfileStore : IProfileStore
    {
        readonly Dictionary<string, string> _items = new Dictionary<string, string>();

        public string GetItem(string key)
        {
            string value;
            return _items.TryGetValue(key, out value) ? value : null;
        }

        public void SetItem(string key, string value)
        {
            _items[key] = value;
        }
    }

    /// <summary>One JSON object whose properties are store keys. The economy profile is <see cref="EconomyManager.StorageKey"/>.</summary>
    public sealed class FileProfileStore : IProfileStore
    {
        public const string FileName = "orbital-salvage-profile.json";

        readonly string _path;
        readonly Dictionary<string, string> _items = new Dictionary<string, string>();

        public FileProfileStore(string directory)
        {
            _path = Path.Combine(directory, FileName);
            if (!File.Exists(_path)) return;
            try
            {
                var parsed = JsCompat.JsonParser.Parse(File.ReadAllText(_path));
                if (parsed == null || parsed.Kind != JsCompat.JsonKind.Object || parsed.Members == null) return;
                for (int i = 0; i < parsed.Members.Count; i++)
                {
                    var v = parsed.Members[i].Value;
                    if (v != null && v.Kind == JsCompat.JsonKind.String) _items[parsed.Members[i].Key] = v.String;
                }
            }
            catch (Exception)
            {
            }
        }

        public string GetItem(string key)
        {
            string value;
            return _items.TryGetValue(key, out value) ? value : null;
        }

        public void SetItem(string key, string value)
        {
            _items[key] = value;
            var obj = JsCompat.JsonValue.NewObject();
            foreach (var kv in _items) obj.Set(kv.Key, kv.Value);
            string dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(_path, JsCompat.JsonWriter.Stringify(obj));
        }
    }

    sealed class StoreAdapter : IKeyValueStore
    {
        readonly IProfileStore _store;
        public StoreAdapter(IProfileStore store) { _store = store; }
        public string GetItem(string key) { return _store.GetItem(key); }
        public void SetItem(string key, string value) { _store.SetItem(key, value); }
    }

    /// <summary>The live playfield the controller steps in lockstep with a recorded web trace.</summary>
    public interface IFixedPlayfield
    {
        void Step(double dtSeconds, bool simulate);
        void FinishReplay();
        void PointerDown(double clientX, double canvasLeft, double canvasWidth);
        void PointerMove(double clientX);
        void PointerUp();
        void KeyDown(string key);
        string StateHash();
    }

    public interface IFailureClock
    {
        void ResetFailureClock();
    }

    public interface IAppClock
    {
        double NowMs { get; set; }
        double EpochMs { get; set; }
    }

    public sealed class ManualAppClock : IAppClock
    {
        public double NowMs { get; set; }
        public double EpochMs { get; set; }

        public ManualAppClock(double nowMs, double epochMs)
        {
            NowMs = nowMs;
            EpochMs = epochMs;
        }
    }

    sealed class EpochClock : IWallClock
    {
        public double NowEpochMs { get; set; }
    }

    public interface IAppAudio
    {
        bool IsMuted { get; }
        void PublicInit();
        bool ToggleMute();
        void SetSfxVolume(double volume);
        void SetAmbienceVolume(double volume);
        void PlayBoosterPass();
        void PlayVictoryFanfare();
        void PlayDefeatSound();
        void PlayBackupPodSound();
        void PlayDebrisSwallowed();
    }

    public sealed class SilentAudio : IAppAudio
    {
        public bool IsMuted { get; private set; }
        public readonly List<string> Cues = new List<string>();

        public void PublicInit() { }

        public bool ToggleMute()
        {
            IsMuted = !IsMuted;
            Cues.Add(IsMuted ? "mute" : "unmute");
            return IsMuted;
        }

        public void SetSfxVolume(double volume) { }
        public void SetAmbienceVolume(double volume) { }
        public void PlayBoosterPass() { Cues.Add("booster"); }
        public void PlayVictoryFanfare() { Cues.Add("victory"); }
        public void PlayDefeatSound() { Cues.Add("defeat"); }
        public void PlayBackupPodSound() { Cues.Add("backup"); }
        public void PlayDebrisSwallowed() { Cues.Add("swallow"); }
    }

    public sealed class ElementOverride
    {
        public string Text;
        public bool HasText;
        public double? WidthPercent;
        public double? WidthPx;
        public double? Opacity;
        public bool? Visible;
        public string Skin;
    }

    public sealed class ToastModel
    {
        public string Text;
        public double ExpiresAtMs;
    }

    /// <summary>What the painter applies on top of the current page template.</summary>
    public sealed class PageModel
    {
        public string PageId;
        public string TemplateId;
        public readonly List<string> Modals = new List<string>();
        public readonly List<ToastModel> Toasts = new List<ToastModel>();
        public readonly Dictionary<string, ElementOverride> Elements = new Dictionary<string, ElementOverride>();
    }
}
