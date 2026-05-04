using HarmonyLib;
using InsanityMod.Managers;
using InsanityMod.Network;
using TMPro;
using UnityEngine;

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

            var statsContainer = __instance.statsUIElements?.penaltyTotal?.transform.parent;
            if (statsContainer == null) return;

            var existing = statsContainer.Find("InsanityStats");
            if (existing != null) Object.Destroy(existing.gameObject);

            string header = LocalizationManager.Get("hud.max_insanity");
            var lineGO = new GameObject("InsanityStats", typeof(RectTransform));
            lineGO.transform.SetParent(statsContainer, false);

            var text = lineGO.AddComponent<TextMeshProUGUI>();
            text.fontSize = 14f;
            text.alignment = TextAlignmentOptions.Left;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"<b>— {header} —</b>");

            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                ulong id = player.actualClientId;
                if (!InsanityNetworkHandler.PlayerMaxInsanity.TryGetValue(id, out float max)) continue;

                string name = player.playerUsername;
                string pct = $"{max:F0}%";
                string color = max >= 100f ? "<color=#FF4444>" : "<color=#DDDDDD>";
                sb.AppendLine($"{color}{name}  {pct}</color>");
            }

            text.text = sb.ToString();
        }
    }
}
