using HarmonyLib;
using InsanityMod.Managers;
using InsanityMod.Network;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal static class RoundResultsPatcher
    {
        [HarmonyPatch("ApplyPenalty")]
        [HarmonyPostfix]
        private static void ShowInsanityStats(HUDManager __instance)
        {
            if (InsanityNetworkHandler.PlayerMaxInsanity.Count == 0) return;

            var addition = __instance.statsUIElements?.penaltyAddition;
            if (addition == null) return;

            string header = LocalizationManager.Get("hud.max_insanity");
            var sb = new System.Text.StringBuilder();
            sb.AppendLine();
            sb.Append($"<color=#CC4444>— {header} —</color>");

            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                ulong id = player.actualClientId;
                if (!InsanityNetworkHandler.PlayerMaxInsanity.TryGetValue(id, out float max)) continue;

                string color = max >= 100f ? "<color=#FF4444>" : max >= 80f ? "<color=#FF9933>" : "<color=#BBBBBB>";
                sb.AppendLine();
                sb.Append($"{color}{player.playerUsername}: {max:F0}%</color>");
            }

            addition.text += sb.ToString();
        }
    }
}
