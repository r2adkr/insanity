using HarmonyLib;
using InsanityMod.Managers;
using UnityEngine;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(DressGirlAI))]
    internal static class GhostGirlBoostPatcher
    {
        // Guards run unwrapped to avoid per-frame closure allocation; SafePatch only
        // wraps the actual work that runs when the local player is being haunted.
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(DressGirlAI __instance)
        {
            if (!InsanityManager.IsRoundActive) return;
            if (!__instance.hauntingLocalPlayer) return;

            float threshold = ModConfig.GhostGirlBoostThreshold.Value;
            float current   = InsanityManager.Insanity;
            if (current < threshold) return;

            SafePatch.Run(nameof(UpdatePostfix), () =>
            {
                float t = (current - threshold) / Mathf.Max(0.01f, 100f - threshold);
                __instance.timer += Time.deltaTime * t * 0.5f;
            });
        }
    }
}
