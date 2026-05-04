using GameNetcodeStuff;
using HarmonyLib;
using InsanityMod.Managers;
using UnityEngine;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class PlayerPatcher
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(PlayerControllerB __instance)
        {
            var gnm = GameNetworkManager.Instance;
            if (gnm == null || __instance != gnm.localPlayerController) return;
            if (__instance.isPlayerDead) return;
            InsanityManager.Tick(__instance, Time.deltaTime);
        }
    }
}
