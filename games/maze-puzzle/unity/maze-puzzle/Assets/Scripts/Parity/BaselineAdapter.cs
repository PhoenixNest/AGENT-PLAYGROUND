using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OrbitalSalvage.Parity
{
    /// <summary>
    /// Capture still calls this target for boot and step bookkeeping. Painted states are drawn by
    /// <c>HomeChrome</c> in <c>ParityRunner</c>, so this adapter no longer drives <c>UIManager</c>.
    /// </summary>
    public sealed class BaselineAdapter : IParityTarget
    {
        const string SaveFileName = "orbital_salvage_save.json";

        ParityContext ctx;
        bool saveBackedUp;

        public string Name => "canvas2d";

        static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        static string BackupPath => SavePath + ".parity-backup";

        public void Attach(ParityContext context)
        {
            ctx = context;
        }

        public void PrepareFreshStart()
        {
            BackupUserData();
            if (File.Exists(SavePath)) File.Delete(SavePath);
            PlayerPrefs.DeleteAll();
            Time.timeScale = 1f;
        }

        public void Shutdown()
        {
            if (!saveBackedUp) return;
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
                if (File.Exists(BackupPath)) File.Move(BackupPath, SavePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[Parity] Could not restore user save: " + ex.Message);
            }
            saveBackedUp = false;
        }

        public void Boot(bool completeLoading)
        {
            if (ctx != null) ctx.AddTrace("page", completeLoading ? "home" : "loading");
        }

        public OpResult ShowPage(string page)
        {
            if (ctx != null) ctx.AddTrace("page", page);
            return OpResult.Ok();
        }

        public OpResult SetProfile(string path, string valueJson) => OpResult.Noop("profile is owned by AppController");
        public OpResult SetApp(string field, string valueJson) => OpResult.Noop("app fields are owned by AppController");
        public OpResult Call(string method, string argsJson) => OpResult.Noop(method);
        public OpResult Click(string id) => OpResult.Noop(id);
        public OpResult ClickSelector(string selector) => OpResult.Noop(selector);
        public OpResult Pointer(string kind, float xCss, float yCss) => OpResult.Noop(kind);
        public OpResult Key(string key) => OpResult.Noop(key);
        public OpResult LoadSector(int index) => OpResult.Noop("sector " + index);
        public OpResult HoldPressed(string id) => OpResult.Noop(id);
        public OpResult Release() => OpResult.Ok();
        public OpResult Scroll(string target, float yCss) => OpResult.Noop(target);

        public List<LayoutElement> CollectLayout()
        {
            return new List<LayoutElement>();
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
            catch (System.Exception ex)
            {
                Debug.LogWarning("[Parity] Could not back up user save: " + ex.Message);
            }
        }
    }
}
