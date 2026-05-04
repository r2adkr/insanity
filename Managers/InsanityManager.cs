using GameNetcodeStuff;
using InsanityMod.Network;
using UnityEngine;

namespace InsanityMod.Managers
{
    internal static class InsanityManager
    {
        private static float _insanity;
        private static float _maxInsanityThisRound;
        private static float _audioTimer;

        public static float Insanity            => _insanity;
        public static float MaxInsanityThisRound => _maxInsanityThisRound;

        public static void ResetForRound()
        {
            _insanity             = 0f;
            _maxInsanityThisRound = 0f;
            _audioTimer           = 0f;
        }

        public static void Tick(PlayerControllerB player, float deltaTime)
        {
            float delta = InsanityCalculator.TickDelta(
                player.isInsideFactory,
                player.isInHangarShipRoom,
                ModConfig.InsanityRateInFacility.Value,
                ModConfig.InsanityRateOnShip.Value,
                ModConfig.InsanityDecayOutdoor.Value,
                BloodNightManager.IsActive ? ModConfig.BloodNightMultiplier.Value : 1f,
                deltaTime);

            _insanity = InsanityCalculator.Clamp(_insanity + delta);
            if (_insanity > _maxInsanityThisRound)
                _maxInsanityThisRound = _insanity;

            VFXManager.UpdateTunnelVision(_insanity);
            TryTriggerInsanityAudio(deltaTime);
        }

        public static void AddInsanity(float amount)
        {
            _insanity = InsanityCalculator.Clamp(_insanity + amount);
            if (_insanity > _maxInsanityThisRound)
                _maxInsanityThisRound = _insanity;
        }

        private static void TryTriggerInsanityAudio(float deltaTime)
        {
            if (_insanity < ModConfig.InsanityAudioThreshold.Value) return;

            float interval = _insanity >= 90f ? 5f : _insanity >= 70f ? 10f : 20f;
            _audioTimer -= deltaTime;
            if (_audioTimer > 0f) return;

            _audioTimer = interval;
            int clipIndex = _insanity >= 90f ? 2 : _insanity >= 70f ? 1 : 0;
            InsanityNetworkHandler.SendPlayInsanityAudio(clipIndex);
        }
    }
}
