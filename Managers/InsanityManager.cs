using GameNetcodeStuff;
using InsanityMod.Voice;

namespace InsanityMod.Managers
{
    internal static class InsanityManager
    {
        private static float _insanity;
        private static float _maxInsanityThisRound;
        private static bool  _roundActive;

        public static float Insanity             => _insanity;
        public static float MaxInsanityThisRound => _maxInsanityThisRound;
        public static bool  IsRoundActive        => _roundActive;

        public static void StartRound()
        {
            _insanity             = 0f;
            _maxInsanityThisRound = 0f;
            _roundActive          = true;
            InsanityHud.SetVisible(true);
        }

        public static void EndRound()
        {
            _roundActive = false;
            InsanityHud.SetVisible(false);
        }

        public static void ResetForRound() => StartRound();

        public static void Tick(PlayerControllerB player, float deltaTime)
        {
            if (!_roundActive) return;

            float baseDelta = InsanityCalculator.TickDelta(
                player.isInsideFactory,
                player.isInHangarShipRoom,
                ModConfig.InsanityRateInFacility.Value,
                ModConfig.InsanityRateOnShip.Value,
                ModConfig.InsanityDecayOutdoor.Value,
                BloodNightManager.IsActive ? ModConfig.BloodNightMultiplier.Value : 1f,
                deltaTime);

            float bonusRate  = InsanityModifiers.ComputeBonusRate(player);
            float bonusDelta = bonusRate * deltaTime;

            _insanity = InsanityCalculator.Clamp(_insanity + baseDelta + bonusDelta);
            if (_insanity > _maxInsanityThisRound)
                _maxInsanityThisRound = _insanity;

            VFXManager.UpdateTunnelVision(_insanity);
            InsanityHud.UpdateValue(_insanity);
            VoiceHaunt.Tick(_insanity, deltaTime);
        }

        public static void AddInsanity(float amount)
        {
            if (!_roundActive) return;
            _insanity = InsanityCalculator.Clamp(_insanity + amount);
            if (_insanity > _maxInsanityThisRound)
                _maxInsanityThisRound = _insanity;
        }
    }
}
