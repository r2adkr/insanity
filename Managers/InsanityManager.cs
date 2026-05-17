using GameNetcodeStuff;
using InsanityMod.Voice;
using UnityEngine;

namespace InsanityMod.Managers
{
    internal static class InsanityManager
    {
        private static float       _insanity;
        private static float       _maxInsanityThisRound;
        private static bool        _roundActive;
        private static bool        _apparatusRemoved;
        private static bool        _hasTransformedThisRound;
        private static ShipLights? _cachedShipLights;

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

        // Independent ship-interior check that works around two upstream issues:
        //   1. 2-Story Ship inflates the trigger collider that drives PlayerControllerB.isInHangarShipRoom,
        //      so the vanilla flag flips true even outside the railing.
        //   2. Imperium teleports skip the trigger entirely until the player physically lands.
        // shipInnerRoomBounds is the vanilla box that defines the actual interior bounds; querying it by
        // position each frame is robust to both cases.
        public static bool IsInShip(PlayerControllerB player)
        {
            if (player == null) return false;
            var bounds = StartOfRound.Instance?.shipInnerRoomBounds;
            if (bounds != null)
                return bounds.bounds.Contains(player.transform.position);
            return player.isInHangarShipRoom;
        }

        public static void Tick(PlayerControllerB player, float deltaTime)
        {
            if (!_roundActive) return;

            bool isInShip  = IsInShip(player);
            bool isOutdoor = !player.isInsideFactory && !isInShip;
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

            // Underwater priority: while submerged outdoors, any outdoor decay is suppressed so the
            // UnderwaterBonus (+rate) dominates. Without this, decay (-0.8) partially cancels underwater
            // gain (+0.4), producing a net decrease in flooded weathers — the opposite of intended.
            if (isOutdoor && player.isUnderwater && outdoorRate < 0f)
                outdoorRate = 0f;

            float baseDelta = InsanityCalculator.TickDelta(
                player.isInsideFactory,
                isInShip,
                ModConfig.InsanityRateInFacility.Value,
                GetShipRate(),
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

        private static float GetShipRate()
        {
            if (_cachedShipLights == null)
                _cachedShipLights = Object.FindObjectOfType<ShipLights>();

            bool lightsOn = _cachedShipLights != null && _cachedShipLights.areLightsOn;
            return lightsOn
                ? ModConfig.RateOnShipLightsOn.Value
                : ModConfig.RateOnShipLightsOff.Value;
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
