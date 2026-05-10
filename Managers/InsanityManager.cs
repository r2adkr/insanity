using GameNetcodeStuff;
using InsanityMod.Voice;
using UnityEngine;

namespace InsanityMod.Managers
{
    internal static class InsanityManager
    {
        private static float _insanity;
        private static float _maxInsanityThisRound;
        private static bool  _roundActive;
        private static bool  _apparatusRemoved;
        private static bool  _hasTransformedThisRound;

        public static float Insanity             => _insanity;
        public static float MaxInsanityThisRound => _maxInsanityThisRound;
        public static bool  IsRoundActive        => _roundActive;

        public static void OnApparatusRemoved()
        {
            if (!_roundActive) return;
            _apparatusRemoved = true;
            AddInsanity(ModConfig.ApparatusSpike.Value);
        }

        public static void StartRound()
        {
            _insanity                = 0f;
            _maxInsanityThisRound    = 0f;
            _roundActive             = true;
            _apparatusRemoved        = false;
            _hasTransformedThisRound = false;
            MaskedTransformManager.Reset();
            InsanityHud.SetVisible(true);
        }

        public static void EndRound()
        {
            _roundActive = false;
            VFXManager.ClearEffect();
            InsanityHud.SetVisible(false);
        }

        public static void ResetForRound() => StartRound();

        public static void Tick(PlayerControllerB player, float deltaTime)
        {
            if (!_roundActive) return;

            bool isOutdoor = !player.isInsideFactory && !player.isInHangarShipRoom;
            float outdoorRate;
            if (isOutdoor && BloodNightManager.IsActive)
                outdoorRate = ModConfig.ParanoiaOutdoorRate.Value;
            else if (isOutdoor && StartOfRound.Instance?.currentLevel?.currentWeather == LevelWeatherType.Eclipsed)
                outdoorRate = ModConfig.EclipseOutdoorRate.Value;
            else if (isOutdoor && ModConfig.NightOutdoorRate.Value > 0f
                && (TimeOfDay.Instance?.hour ?? 0) >= ModConfig.NightStartHour.Value)
                outdoorRate = ModConfig.NightOutdoorRate.Value;
            else
                outdoorRate = -ModConfig.InsanityDecayOutdoor.Value;

            float baseDelta = InsanityCalculator.TickDelta(
                player.isInsideFactory,
                player.isInHangarShipRoom,
                ModConfig.InsanityRateInFacility.Value,
                ModConfig.InsanityRateOnShip.Value,
                outdoorRate,
                BloodNightManager.IsActive ? ModConfig.BloodNightMultiplier.Value : 1f,
                deltaTime);

            float bonusRate  = InsanityModifiers.ComputeBonusRate(player);
            float bonusDelta = bonusRate * deltaTime;
            if (_apparatusRemoved && player.isInsideFactory)
                bonusDelta = System.Math.Max(bonusDelta, 0f);

            _insanity = InsanityCalculator.Clamp(_insanity + baseDelta + bonusDelta);
            if (_insanity > _maxInsanityThisRound)
                _maxInsanityThisRound = _insanity;

            VFXManager.UpdateTunnelVision(_insanity);
            InsanityHud.UpdateValue(_insanity);
            VoiceHaunt.Tick(_insanity, deltaTime);

            if (_insanity >= 100f && !_hasTransformedThisRound && !MaskedTransformManager.IsActive
                && ModConfig.EnableMaskedTransform.Value
                && (!ModConfig.MaskedTransformOnlyDuringParanoia.Value || BloodNightManager.IsActive))
            {
                _hasTransformedThisRound = true;
                MaskedTransformManager.TriggerTransform(player);
            }

            if (MaskedTransformManager.IsActive)
                MaskedTransformManager.Tick(player, deltaTime);
        }

        public static void ResetOnDeath()
        {
            _insanity = 0f;
            InsanityHud.UpdateValue(0f);
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
