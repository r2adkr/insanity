using System.IO;
using System.Reflection;
using UnityEngine;

namespace InsanityMod
{
    internal static class AssetBundleLoader
    {
        public static Material?   TunnelVisionMaterial { get; private set; }
        public static AudioClip[] InsanityAudioClips   { get; private set; } = System.Array.Empty<AudioClip>();

        public static void Load()
        {
            string pluginDir  = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string bundlePath = Path.Combine(pluginDir, "insanitymod.bundle");

            if (!File.Exists(bundlePath))
            {
                Plugin.Log.LogWarning("insanitymod.bundle not found — VFX and audio cues will be missing.");
                return;
            }

            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Plugin.Log.LogError("Failed to load insanitymod.bundle.");
                return;
            }

            TunnelVisionMaterial = bundle.LoadAsset<Material>("TunnelVisionMat");
            InsanityAudioClips   = new[]
            {
                bundle.LoadAsset<AudioClip>("insanity_breath_lo"),
                bundle.LoadAsset<AudioClip>("insanity_mutter"),
                bundle.LoadAsset<AudioClip>("insanity_breath_hi"),
            };

            Plugin.Log.LogInfo("AssetBundle loaded successfully.");
        }
    }
}
