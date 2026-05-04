using GameNetcodeStuff;
using HarmonyLib;
using InsanityMod.Managers;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class DeathWitnessPatcher
    {
        [HarmonyPatch("KillPlayer")]
        [HarmonyPostfix]
        private static void KillPlayerPostfix(PlayerControllerB __instance)
        {
            if (!InsanityManager.IsRoundActive) return;
            if (__instance == null) return;

            var local = GameNetworkManager.Instance?.localPlayerController;
            if (local == null || local == __instance || local.isPlayerDead) return;

            float maxRange = ModConfig.DeathWitnessRange.Value;
            if (!InsanityModifiers.IsPositionVisible(local, __instance.transform.position, maxRange)) return;

            InsanityManager.AddInsanity(ModConfig.DeathWitnessSpike.Value);
        }
    }
}
