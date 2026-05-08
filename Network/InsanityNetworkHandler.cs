using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace InsanityMod.Network
{
    internal static class InsanityNetworkHandler
    {
        private const string MSG_SUBMIT       = "InsanityMod.SubmitMaxInsanity";
        private const string MSG_RESULTS      = "InsanityMod.BroadcastResults";
        private const string MSG_SPAWN_MASKED = "InsanityMod.SpawnMasked";

        private static EnemyType? _maskedEnemyType;

        public static readonly Dictionary<ulong, float> PlayerMaxInsanity = new();

        public static void RegisterHandlers()
        {
            if (NetworkManager.Singleton == null) return;
            var mgr = NetworkManager.Singleton.CustomMessagingManager;

            if (NetworkManager.Singleton.IsServer)
            {
                mgr.RegisterNamedMessageHandler(MSG_SUBMIT,       ReceiveMaxInsanity);
                mgr.RegisterNamedMessageHandler(MSG_SPAWN_MASKED, ReceiveSpawnMasked);
            }

            mgr.RegisterNamedMessageHandler(MSG_RESULTS, ReceiveBroadcastResults);
        }

        public static void UnregisterHandlers()
        {
            if (NetworkManager.Singleton == null) return;
            var mgr = NetworkManager.Singleton.CustomMessagingManager;
            mgr.UnregisterNamedMessageHandler(MSG_SUBMIT);
            mgr.UnregisterNamedMessageHandler(MSG_RESULTS);
            mgr.UnregisterNamedMessageHandler(MSG_SPAWN_MASKED);
        }

        public static void SendMaxInsanity(float maxInsanity)
        {
            if (NetworkManager.Singleton == null) return;
            var writer = new FastBufferWriter(sizeof(float), Allocator.Temp);
            writer.WriteValueSafe(maxInsanity);
            NetworkManager.Singleton.CustomMessagingManager
                .SendNamedMessage(MSG_SUBMIT, NetworkManager.ServerClientId, writer);
            writer.Dispose();
        }

        private static void ReceiveMaxInsanity(ulong senderId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out float val);
            PlayerMaxInsanity[senderId] = val;
        }

        public static void BroadcastResults()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

            int count  = PlayerMaxInsanity.Count;
            int size   = sizeof(int) + count * (sizeof(ulong) + sizeof(float));
            var writer = new FastBufferWriter(size, Allocator.Temp);

            writer.WriteValueSafe(count);
            foreach (var kv in PlayerMaxInsanity)
            {
                writer.WriteValueSafe(kv.Key);
                writer.WriteValueSafe(kv.Value);
            }
            NetworkManager.Singleton.CustomMessagingManager
                .SendNamedMessageToAll(MSG_RESULTS, writer);
            writer.Dispose();
        }

        private static void ReceiveBroadcastResults(ulong _, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int count);
            PlayerMaxInsanity.Clear();
            for (int i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out ulong id);
                reader.ReadValueSafe(out float val);
                PlayerMaxInsanity[id] = val;
            }
        }

        public static void SendSpawnMasked(GameNetcodeStuff.PlayerControllerB local)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) return;

            var writer = new FastBufferWriter(sizeof(float) * 3 + sizeof(ulong), Allocator.Temp);
            Vector3 pos = local.transform.position;
            writer.WriteValueSafe(pos.x);
            writer.WriteValueSafe(pos.y);
            writer.WriteValueSafe(pos.z);
            writer.WriteValueSafe(local.playerClientId);

            if (nm.IsServer)
            {
                var reader = new FastBufferReader(writer, Allocator.Temp);
                ReceiveSpawnMasked(nm.LocalClientId, reader);
                reader.Dispose();
            }
            else
            {
                nm.CustomMessagingManager.SendNamedMessage(MSG_SPAWN_MASKED, NetworkManager.ServerClientId, writer);
            }
            writer.Dispose();
        }

        private static void ReceiveSpawnMasked(ulong _, FastBufferReader reader)
        {
            reader.ReadValueSafe(out float px);
            reader.ReadValueSafe(out float py);
            reader.ReadValueSafe(out float pz);
            reader.ReadValueSafe(out ulong clientId);

            var position = new Vector3(px, py, pz);

            var maskedType = GetMaskedEnemyType();
            if (maskedType == null || RoundManager.Instance == null) return;

            var enemyRef = RoundManager.Instance.SpawnEnemyGameObject(position, 0f, -1, maskedType);
            if (!enemyRef.TryGet(out NetworkObject netObj)) return;

            var masked = netObj.GetComponent<MaskedPlayerEnemy>()
                      ?? netObj.GetComponentInChildren<MaskedPlayerEnemy>();
            if (masked == null) return;

            var players = StartOfRound.Instance?.allPlayerScripts;
            if (players == null) return;

            foreach (var p in players)
            {
                if (p.playerClientId != clientId) continue;
                masked.mimickingPlayer = p;
                masked.SetSuit(p.currentSuitID);
                break;
            }
        }

        private static EnemyType? GetMaskedEnemyType()
        {
            if (_maskedEnemyType != null) return _maskedEnemyType;
            var types = Resources.FindObjectsOfTypeAll<EnemyType>();
            foreach (var t in types)
            {
                if (t.enemyName == "Masked")
                {
                    _maskedEnemyType = t;
                    return t;
                }
            }
            return null;
        }
    }
}
