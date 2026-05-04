using HarmonyLib;
using InsanityMod;
using InsanityMod.Managers;
using InsanityMod.Network;
using Unity.Netcode;
using UnityEngine;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class RoundPatcher
    {
        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        private static void StartGamePostfix()
        {
            InsanityManager.ResetForRound();
            InsanityNetworkHandler.PlayerMaxInsanity.Clear();
            InsanityNetworkHandler.RegisterHandlers();
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
        }
    }

    [HarmonyPatch(typeof(RoundManager))]
    internal static class LucidDoomSpawnPatcher
    {
        [HarmonyPatch("LoadNewLevelWait")]
        [HarmonyPostfix]
        private static System.Collections.IEnumerator SpawnLucidDooms(
            System.Collections.IEnumerator __result)
        {
            yield return __result;

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) yield break;
            if (AssetBundleLoader.LucidDoomPrefab == null) yield break;

            var spawnPoints = Object.FindObjectsOfType<RandomScrapSpawn>();
            if (spawnPoints.Length == 0) yield break;

            int count = ModConfig.PotionFacilitySpawnCount.Value;
            for (int i = 0; i < count; i++)
            {
                var point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
                var go = Object.Instantiate(
                    AssetBundleLoader.LucidDoomPrefab,
                    point.transform.position,
                    UnityEngine.Random.rotation);
                go.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
            }
        }
    }
}
