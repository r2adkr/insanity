using HarmonyLib;
using InsanityMod.Managers;
using InsanityMod.Network;
using InsanityMod.Voice;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class RoundPatcher
    {
        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        private static void StartGamePostfix()
        {
            InsanityManager.StartRound();
            InsanityNetworkHandler.PlayerMaxInsanity.Clear();
            InsanityNetworkHandler.RegisterHandlers();
            VoiceHaunt.ResetForRound();
            InsanityModifiers.InvalidateLightCache();
        }

        [HarmonyPatch("ShipLeave")]
        [HarmonyPostfix]
        private static void ShipLeavePostfix()
        {
            InsanityNetworkHandler.SendMaxInsanity(InsanityManager.MaxInsanityThisRound);
        }

        [HarmonyPatch("EndGameClientRpc")]
        [HarmonyPostfix]
        private static void EndGamePostfix()
        {
            InsanityNetworkHandler.BroadcastResults();
            InsanityNetworkHandler.UnregisterHandlers();
            InsanityManager.EndRound();
        }
    }
}
