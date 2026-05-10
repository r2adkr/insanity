using GameNetcodeStuff;
using HarmonyLib;
using InsanityMod.Managers;
using UnityEngine;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class PlayerPatcher
    {
        // Guards run unwrapped to avoid per-frame closure allocation. SafePatch only
        // wraps the actual work, so the closure is allocated at most once per frame
        // for the local living player — and never for the non-local case that bails out.
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(PlayerControllerB __instance)
        {
            var gnm = GameNetworkManager.Instance;
            if (gnm == null || __instance != gnm.localPlayerController) return;
            if (__instance.isPlayerDead) return;

            SafePatch.Run(nameof(UpdatePostfix), () => InsanityManager.Tick(__instance, Time.deltaTime));
        }
    }
}
