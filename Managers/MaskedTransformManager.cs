using GameNetcodeStuff;
using InsanityMod.Network;
using UnityEngine;

namespace InsanityMod.Managers
{
    internal static class MaskedTransformManager
    {
        private enum Phase { Idle, Slowing, Cutscene, Done }

        private static Phase  _phase;
        private static float  _timer;
        private static float  _savedSpeed;
        private static PlayerControllerB? _local;

        private const float SlowDuration     = 6f;
        private const float CutsceneDuration = 2.5f;

        public static bool IsActive => _phase != Phase.Idle && _phase != Phase.Done;

        public static void TriggerTransform(PlayerControllerB local)
        {
            if (_phase != Phase.Idle) return;
            _local      = local;
            _savedSpeed = local.movementSpeed;
            _phase      = Phase.Slowing;
            _timer      = 0f;
        }

        public static void Tick(PlayerControllerB local, float deltaTime)
        {
            if (_phase == Phase.Idle || _phase == Phase.Done) return;

            _timer += deltaTime;

            if (_phase == Phase.Slowing)
            {
                float t = Mathf.Clamp01(_timer / SlowDuration);
                local.movementSpeed = Mathf.Lerp(_savedSpeed, 0f, t * t);

                // Fade black overlay in over the last 2 seconds of slowdown
                float blackT = Mathf.Clamp01((_timer - (SlowDuration - 2f)) / 2f);
                VFXManager.SetBlackout(blackT);

                if (t >= 1f)
                {
                    local.movementSpeed    = 0f;
                    local.disableMoveInput = true;
                    local.disableLookInput = true;
                    VFXManager.SetBlackout(1f);
                    InsanityNetworkHandler.SendSpawnMasked(local);
                    _phase = Phase.Cutscene;
                    _timer = 0f;
                }
            }
            else if (_phase == Phase.Cutscene)
            {
                if (_timer >= CutsceneDuration)
                {
                    _phase = Phase.Done;
                    VFXManager.SetBlackout(0f);
                    local.KillPlayer(Vector3.zero, true, CauseOfDeath.Unknown, 0);
                }
            }
        }

        public static void Reset()
        {
            if (_local != null && _phase != Phase.Idle)
            {
                _local.movementSpeed    = _savedSpeed > 0f ? _savedSpeed : 4.6f;
                _local.disableMoveInput = false;
                _local.disableLookInput = false;
            }
            VFXManager.SetBlackout(0f);
            _phase = Phase.Idle;
            _timer = 0f;
            _local = null;
        }
    }
}
