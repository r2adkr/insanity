using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using InsanityMod.Managers;

namespace InsanityMod
{
    [BepInPlugin(Plugin.GUID, Plugin.NAME, Plugin.VERSION)]
    [BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID    = "com.insanitymod.lethalcompany";
        public const string NAME    = "InsanityMod";
        public const string VERSION = "1.0.1";

        internal static ManualLogSource Log = null!;
        internal static Plugin Instance    = null!;

        private readonly Harmony _harmony = new Harmony(GUID);

        private void Awake()
        {
            Instance = this;
            Log      = Logger;

            ModConfig.Initialize(Config);
            LocalizationManager.Initialize();
            BloodNightManager.Initialize();
            VFXManager.Initialize();
            InsanityHud.Initialize();

            _harmony.PatchAll();
            Log.LogInfo($"{NAME} v{VERSION} loaded.");
        }

        private void LateUpdate()
        {
            if (!InsanityManager.IsRoundActive) return;
            var local = GameNetworkManager.Instance?.localPlayerController;
            if (local == null) return;

            if (local.isPlayerDead)
            {
                MaskedTransformManager.Reset();
                VFXManager.ClearEffect();
                InsanityManager.ResetOnDeath();
                return;
            }

            CameraShakeManager.ApplyShake(local, InsanityManager.Insanity);
        }
    }
}
