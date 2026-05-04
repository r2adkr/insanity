using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace InsanityMod.Network
{
    internal static class InsanityNetworkHandler
    {
        private const string MSG_SUBMIT  = "InsanityMod.SubmitMaxInsanity";
        private const string MSG_RESULTS = "InsanityMod.BroadcastResults";

        public static readonly Dictionary<ulong, float> PlayerMaxInsanity = new();

        public static void RegisterHandlers()
        {
            if (NetworkManager.Singleton == null) return;
            var mgr = NetworkManager.Singleton.CustomMessagingManager;

            if (NetworkManager.Singleton.IsServer)
                mgr.RegisterNamedMessageHandler(MSG_SUBMIT, ReceiveMaxInsanity);

            mgr.RegisterNamedMessageHandler(MSG_RESULTS, ReceiveBroadcastResults);
        }

        public static void UnregisterHandlers()
        {
            if (NetworkManager.Singleton == null) return;
            var mgr = NetworkManager.Singleton.CustomMessagingManager;
            mgr.UnregisterNamedMessageHandler(MSG_SUBMIT);
            mgr.UnregisterNamedMessageHandler(MSG_RESULTS);
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
    }
}
