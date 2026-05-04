using System.IO;
using System.Reflection;
using UnityEngine;

namespace InsanityMod
{
    internal static class AssetBundleLoader
    {
        public static Material?    TunnelVisionMaterial  { get; private set; }
        public static AudioClip[]  InsanityAudioClips    { get; private set; } = System.Array.Empty<AudioClip>();
        public static GameObject?  ValueBreadPrefab      { get; private set; }
        public static GameObject?  LucidDoomPrefab       { get; private set; }
        public static Item?        ValueBreadItem        { get; private set; }
        public static Item?        LucidDoomItem         { get; private set; }

        public static void Load()
        {
            string pluginDir  = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string bundlePath = Path.Combine(pluginDir, "insanitymod.bundle");

            if (!File.Exists(bundlePath))
            {
                Plugin.Log.LogWarning("insanitymod.bundle not found — items and VFX will be missing. Build the Unity AssetBundle first (Task 13).");
                return;
            }

            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Plugin.Log.LogError("Failed to load insanitymod.bundle.");
                return;
            }

            TunnelVisionMaterial  = bundle.LoadAsset<Material>("TunnelVisionMat");
            InsanityAudioClips    = new[]
            {
                bundle.LoadAsset<AudioClip>("insanity_breath_lo"),
                bundle.LoadAsset<AudioClip>("insanity_mutter"),
                bundle.LoadAsset<AudioClip>("insanity_breath_hi"),
            };
            ValueBreadPrefab = bundle.LoadAsset<GameObject>("ValueBreadPrefab");
            LucidDoomPrefab  = bundle.LoadAsset<GameObject>("LucidDoomPrefab");
            ValueBreadItem   = bundle.LoadAsset<Item>("ValueBreadItem");
            LucidDoomItem    = bundle.LoadAsset<Item>("LucidDoomItem");

            Plugin.Log.LogInfo("AssetBundle loaded successfully.");
        }
    }
}
