using System.Collections.Generic;
using GameNetcodeStuff;

namespace InsanityMod
{
    internal static class CoronerCompat
    {
        private static bool _available;
        private static Coroner.AdvancedCauseOfDeath? _chokeCause;
        private static Coroner.AdvancedCauseOfDeath? _lucidDoomCause;

        private static readonly Dictionary<int, bool> _pendingLucidDoom = new();

        public static void Initialize()
        {
            try
            {
                _chokeCause     = Coroner.API.Register("insanitymod.choke_bread");
                _lucidDoomCause = Coroner.API.Register("insanitymod.lucid_doom_death");
                _available = true;
                Plugin.Log.LogInfo("Coroner integration enabled.");
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogWarning($"Coroner integration failed: {e.Message}");
                _available = false;
            }
        }

        public static void SetChokeCause(PlayerControllerB player)
        {
            if (!_available || _chokeCause == null) return;
            Coroner.API.SetCauseOfDeath(player, _chokeCause);
        }

        public static void MarkLucidDoomUser(PlayerControllerB player) =>
            _pendingLucidDoom[player.GetInstanceID()] = true;

        public static void ApplyLucidDoomCause(PlayerControllerB player)
        {
            if (!_available || _lucidDoomCause == null) return;
            int id = player.GetInstanceID();
            if (!_pendingLucidDoom.ContainsKey(id)) return;
            _pendingLucidDoom.Remove(id);
            Coroner.API.SetCauseOfDeath(player, _lucidDoomCause);
        }
    }
}
