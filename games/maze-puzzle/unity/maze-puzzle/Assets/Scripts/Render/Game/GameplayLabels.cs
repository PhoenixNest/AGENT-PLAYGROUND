using OrbitalSalvage.Loc;

namespace OrbitalSalvage.Render
{
    using Loc = OrbitalSalvage.Loc.Loc;

    /// <summary>
    /// Playfield words drawn every frame. Resolved text is cached per language version
    /// so English steady state does not allocate once the value has been seen.
    /// </summary>
    public sealed class GameplayLabels
    {
        readonly LocNumCache _values = new LocNumCache(12);
        LocMemo _boost;
        int _hubVersion = -1;
        string _hubId;
        bool _hubGear;
        string _hubEmoji;
        string _hubSub;

        public string Zap(double value)
        {
            return _values.Get("fx.gate.zap", value);
        }

        public string Shred(double value)
        {
            return _values.Get("fx.gate.shred", value);
        }

        public string Magnet(double value)
        {
            return _values.Get("fx.gate.magnet", value);
        }

        public string Boost()
        {
            return _boost.Plain("fx.gate.boost");
        }

        public void Hub(string id, bool concentric, out string emoji, out string subtitle)
        {
            int version = Loc.Version;
            if (_hubEmoji != null && _hubVersion == version && _hubId == id && _hubGear == concentric)
            {
                emoji = _hubEmoji;
                subtitle = _hubSub;
                return;
            }
            _hubVersion = version;
            _hubId = id;
            _hubGear = concentric;
            if (concentric)
            {
                _hubEmoji = Loc.Tr("fx.hub.gear");
                _hubSub = Loc.Tr("fx.hub.planetary");
            }
            else if (id == "ring_duckie")
            {
                _hubEmoji = Loc.Tr("fx.hub.duckie");
                _hubSub = Loc.Tr("fx.hub.float");
            }
            else if (id == "ring_candyland")
            {
                _hubEmoji = Loc.Tr("fx.hub.candy");
                _hubSub = Loc.Tr("fx.hub.mint");
            }
            else if (id == "ring_spacecat")
            {
                _hubEmoji = Loc.Tr("fx.hub.kitty");
                _hubSub = Loc.Tr("fx.hub.cosmic");
            }
            else if (id == "ring_donut")
            {
                _hubEmoji = Loc.Tr("fx.hub.donut");
                _hubSub = Loc.Tr("fx.hub.sweet");
            }
            else
            {
                _hubEmoji = Loc.Tr("fx.hub.orbital");
                _hubSub = Loc.Tr("fx.hub.sort");
            }
            emoji = _hubEmoji;
            subtitle = _hubSub;
        }
    }
}
