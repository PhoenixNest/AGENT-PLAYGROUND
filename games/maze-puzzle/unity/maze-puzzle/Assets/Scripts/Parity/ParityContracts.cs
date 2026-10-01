using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitalSalvage.Parity
{
    public enum OpStatus
    {
        Ok,
        Noop,
        Unsupported,
        Failed
    }

    public readonly struct OpResult
    {
        public readonly OpStatus Status;
        public readonly string Detail;

        OpResult(OpStatus status, string detail)
        {
            Status = status;
            Detail = detail;
        }

        public static OpResult Ok(string detail = null) => new OpResult(OpStatus.Ok, detail);
        public static OpResult Noop(string detail) => new OpResult(OpStatus.Noop, detail);
        public static OpResult Unsupported(string detail) => new OpResult(OpStatus.Unsupported, detail);
        public static OpResult Failed(string detail) => new OpResult(OpStatus.Failed, detail);
    }

    public sealed class LayoutElement
    {
        public string Id;
        public string Tag = "div";
        public List<string> Classes = new List<string>();
        public float X, Y, W, H;
        public bool Visible;
        public float Opacity = 1f;
        public string Text = "";
    }

    public interface IParityTarget
    {
        string Name { get; }
        void Attach(ParityContext context);
        void PrepareFreshStart();
        void Shutdown();
        void Boot(bool completeLoading);
        OpResult ShowPage(string page);
        OpResult SetProfile(string path, string valueJson);
        OpResult SetApp(string field, string valueJson);
        OpResult Call(string method, string argsJson);
        OpResult Click(string id);
        OpResult ClickSelector(string selector);
        OpResult Pointer(string kind, float xCss, float yCss);
        OpResult Key(string key);
        OpResult LoadSector(int index);
        OpResult HoldPressed(string id);
        OpResult Release();
        OpResult Scroll(string target, float yCss);
        List<LayoutElement> CollectLayout();
    }

    public interface IParityClock
    {
        long NowMs { get; }
        long Frame { get; }
        DateTime UtcNow { get; }
        DateTime LocalNow { get; }
        string UtcDayKey { get; }
        TimeZoneInfo Zone { get; }
    }

    public interface ISafeAreaProvider
    {
        Rect SafeAreaPx { get; }
    }

    public sealed class TraceEvent
    {
        public long T;
        public string Type;
        public string Name;
        public List<object> Args = new List<object>();
    }

    public sealed class ParityNote
    {
        public int Step;
        public string Op;
        public string Status;
        public string Detail;
    }

    public sealed class ParityContext
    {
        public string StateId;
        public string Profile;
        public ParityClock Clock;
        public ParityRenderRig Rig;
        public readonly List<ParityNote> Notes = new List<ParityNote>();
        public readonly List<TraceEvent> Trace = new List<TraceEvent>();
        public readonly Dictionary<string, object> Extra = new Dictionary<string, object>();
        public int CurrentStep;
        public string CurrentOp;

        public void Note(string status, string detail)
        {
            Notes.Add(new ParityNote { Step = CurrentStep, Op = CurrentOp, Status = status, Detail = detail });
        }

        public void AddTrace(string type, string name, params object[] args)
        {
            var ev = new TraceEvent { T = Clock.NowMs, Type = type, Name = name };
            if (args != null) ev.Args.AddRange(args);
            Trace.Add(ev);
        }
    }

    public static class ParityProfiles
    {
        public const float Scale = 3f;
        public const int ViewportW = 430;
        public const int ViewportH = 932;
        public const int PixelW = 1290;
        public const int PixelH = 2796;

        public static bool TryGetInsets(string profile, out float top, out float right, out float bottom, out float left)
        {
            top = right = bottom = left = 0f;
            switch (profile)
            {
                case "zero":
                    return true;
                case "iphone":
                    top = 59f;
                    bottom = 34f;
                    return true;
                default:
                    return false;
            }
        }
    }
}
