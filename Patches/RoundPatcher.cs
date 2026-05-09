using HarmonyLib;
using InsanityMod.Managers;
using InsanityMod.Network;
using InsanityMod.Voice;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal static class LevelGenerationPatcher
    {
        [HarmonyPatch("FinishGeneratingNewLevelClientRpc")]
        [HarmonyPostfix]
        private static void FinishGeneratingPostfix()
        {
            BloodNightManager.OnLevelLoaded();
        }
    }

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
            BloodNightManager.OnRoundStart();
        }

        [HarmonyPatch("ShipLeave")]
        [HarmonyPostfix]
        private static void ShipLeavePostfix()
        {
            InsanityNetworkHandler.SendMaxInsanity(InsanityManager.MaxInsanityThisRound);
            BloodNightManager.OnRoundEnd();
            VoiceHaunt.ResetForRound();
        }

        [HarmonyPatch("EndGameClientRpc")]
        [HarmonyPostfix]
        private static void EndGamePostfix()
        {
            InsanityNetworkHandler.BroadcastResults();
            InsanityNetworkHandler.UnregisterHandlers();
            InsanityManager.EndRound();
            BloodNightManager.OnRoundEnd();
        }

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroyPostfix()
        {
            InsanityManager.EndRound();
            BloodNightManager.OnRoundEnd();
            VFXManager.ClearEffect();
        }
    }
}
