using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using InsanityMod.Managers;
using InsanityMod.Network;

namespace InsanityMod
{
    [BepInPlugin(Plugin.GUID, Plugin.NAME, Plugin.VERSION)]
    [BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("EliteMasterEric.Coroner", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID    = "com.insanitymod.lethalcompany";
        public const string NAME    = "InsanityMod";
        public const string VERSION = "1.0.0";

        internal static ManualLogSource Log = null!;
        internal static Plugin Instance    = null!;

        private readonly Harmony _harmony = new Harmony(GUID);

        private void Awake()
        {
            Instance = this;
            Log      = Logger;

            ModConfig.Initialize(Config);
            LocalizationManager.Initialize();
            AssetBundleLoader.Load();
            BloodNightManager.Initialize();
            VFXManager.Initialize();
            CoronerCompat.Initialize();

            _harmony.PatchAll();
            Log.LogInfo($"{NAME} v{VERSION} loaded.");
        }
    }
}
