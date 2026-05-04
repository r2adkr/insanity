using GameNetcodeStuff;

namespace InsanityMod.Managers
{
    internal static class InsanityManager
    {
        private static float _insanity;
        private static float _maxInsanityThisRound;

        public static float Insanity             => _insanity;
        public static float MaxInsanityThisRound => _maxInsanityThisRound;

        public static void ResetForRound()
        {
            _insanity             = 0f;
            _maxInsanityThisRound = 0f;
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
        }

        public static void AddInsanity(float amount)
        {
            _insanity = InsanityCalculator.Clamp(_insanity + amount);
            if (_insanity > _maxInsanityThisRound)
                _maxInsanityThisRound = _insanity;
        }
    }
}
