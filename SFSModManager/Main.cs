using ModLoader;
using UnityEngine;

namespace SFSModManager
{
    public sealed class Main : Mod
    {
        private static GameObject _host;

        public override string ModNameID => "sfsmodmanager.custom";
        public override string DisplayName => "SFS Mod Manager";
        public override string Author => "Custom";
        public override string ModVersion => "1.0.0";
        public override string MinimumGameVersionNecessary => "1.6.00.16";
        public override string Description => "Manage installed mods from an in-game panel.";

        public override void Early_Load()
        {
            if (_host != null) return;
            _host = new GameObject("SFSModManagerHost");
            UnityEngine.Object.DontDestroyOnLoad(_host);
            _host.AddComponent<ManagerOverlay>();
            UnityEngine.Debug.Log("[SFS Mod Manager] Loaded. Press F7 to open the manager.");
        }

        public override void Load()
        {
            UnityEngine.Debug.Log("[SFS Mod Manager] Ready.");
        }
    }
}
