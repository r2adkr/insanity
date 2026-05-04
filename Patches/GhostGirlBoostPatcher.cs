using HarmonyLib;
using InsanityMod.Managers;
using UnityEngine;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(DressGirlAI))]
    internal static class GhostGirlBoostPatcher
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(DressGirlAI __instance)
        {
            if (!InsanityManager.IsRoundActive) return;
            if (!__instance.hauntingLocalPlayer) return;

            float threshold = ModConfig.GhostGirlBoostThreshold.Value;
            float current   = InsanityManager.Insanity;
            if (current < threshold) return;

            float t = (current - threshold) / Mathf.Max(0.01f, 100f - threshold);
            __instance.timer += Time.deltaTime * t * 0.5f;
        }
    }
}
