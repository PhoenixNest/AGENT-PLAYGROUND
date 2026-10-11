using System.Runtime.InteropServices;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Physical footprint from the iOS player. Editor, Mac, Windows, and Android report 0.
    /// </summary>
    public static class DeviceMemory
    {
        public static long FootprintMb()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return OS_PhysFootprintMB();
#else
            return 0;
#endif
        }

        public static long AvailableMb()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return OS_AvailableMemoryMB();
#else
            return 0;
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern long OS_PhysFootprintMB();

        [DllImport("__Internal")]
        static extern long OS_AvailableMemoryMB();
#endif
    }
}
