using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace InsanityMod.Network
{
    internal static class InsanityNetworkHandler
    {
        private const string MSG_SUBMIT   = "InsanityMod.SubmitMaxInsanity";
        private const string MSG_RESULTS  = "InsanityMod.BroadcastResults";
        private const string MSG_AUDIO    = "InsanityMod.PlayAudio";

        public static readonly Dictionary<ulong, float> PlayerMaxInsanity = new();

        public static void RegisterHandlers()
        {
            if (NetworkManager.Singleton == null) return;
            var mgr = NetworkManager.Singleton.CustomMessagingManager;

            if (NetworkManager.Singleton.IsServer)
                mgr.RegisterNamedMessageHandler(MSG_SUBMIT, ReceiveMaxInsanity);

            mgr.RegisterNamedMessageHandler(MSG_RESULTS, ReceiveBroadcastResults);
            mgr.RegisterNamedMessageHandler(MSG_AUDIO,   ReceivePlayAudio);
        }

        public static void UnregisterHandlers()
        {
            if (NetworkManager.Singleton == null) return;
            var mgr = NetworkManager.Singleton.CustomMessagingManager;
            mgr.UnregisterNamedMessageHandler(MSG_SUBMIT);
            mgr.UnregisterNamedMessageHandler(MSG_RESULTS);
            mgr.UnregisterNamedMessageHandler(MSG_AUDIO);
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
            if (!NetworkManager.Singleton.IsServer) return;

            int    count  = PlayerMaxInsanity.Count;
            int    size   = sizeof(int) + count * (sizeof(ulong) + sizeof(float));
            var    writer = new FastBufferWriter(size, Allocator.Temp);

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

        public static void SendPlayInsanityAudio(int clipIndex)
        {
            if (NetworkManager.Singleton == null) return;
            ulong localId = NetworkManager.Singleton.LocalClientId;
            var   writer  = new FastBufferWriter(sizeof(ulong) + sizeof(int), Allocator.Temp);
            writer.WriteValueSafe(localId);
            writer.WriteValueSafe(clipIndex);
            NetworkManager.Singleton.CustomMessagingManager
                .SendNamedMessageToAll(MSG_AUDIO, writer);
            writer.Dispose();
        }

        private static void ReceivePlayAudio(ulong _, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong playerId);
            reader.ReadValueSafe(out int   clipIndex);

            var clips = AssetBundleLoader.InsanityAudioClips;
            if (clipIndex >= clips.Length || clips[clipIndex] == null) return;

            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.actualClientId != playerId || player.isPlayerDead) continue;
                player.movementAudio.PlayOneShot(clips[clipIndex]);
                break;
            }
        }
    }
}
