using Xunit;

namespace InsanityMod.Tests
{
    public class InsanityCalculatorTests
    {
        [Fact]
        public void InFacility_increases_insanity()
        {
            float delta = InsanityCalculator.TickDelta(
                isInFacility: true, isInShip: false,
                rateInFacility: 0.5f, rateOnShip: 0.1f, decayOutdoor: 0.8f,
                multiplier: 1.0f, deltaTime: 1.0f);
            Assert.Equal(0.5f, delta);
        }

        [Fact]
        public void Outdoor_decreases_insanity()
        {
            float delta = InsanityCalculator.TickDelta(
                isInFacility: false, isInShip: false,
                rateInFacility: 0.5f, rateOnShip: 0.1f, decayOutdoor: 0.8f,
                multiplier: 1.0f, deltaTime: 1.0f);
            Assert.Equal(-0.8f, delta);
        }

        [Fact]
        public void OnShip_increases_slowly()
        {
            float delta = InsanityCalculator.TickDelta(
                isInFacility: false, isInShip: true,
                rateInFacility: 0.5f, rateOnShip: 0.1f, decayOutdoor: 0.8f,
                multiplier: 1.0f, deltaTime: 1.0f);
            Assert.Equal(0.1f, delta);
        }

        [Fact]
        public void BloodNight_multiplier_applied_in_facility()
        {
            float delta = InsanityCalculator.TickDelta(
                isInFacility: true, isInShip: false,
                rateInFacility: 0.5f, rateOnShip: 0.1f, decayOutdoor: 0.8f,
                multiplier: 1.2f, deltaTime: 1.0f);
            Assert.Equal(0.6f, delta, precision: 4);
        }

        [Fact]
        public void Clamp_caps_at_100()
        {
            Assert.Equal(100f, InsanityCalculator.Clamp(150f));
        }

        [Fact]
        public void Clamp_floor_at_0()
        {
            Assert.Equal(0f, InsanityCalculator.Clamp(-10f));
        }

        [Fact]
        public void ChokeChance_accumulates_correctly()
        {
            float chance = InsanityCalculator.ChokeChance(0.2f, 0.05f, 3);
            Assert.Equal(0.35f, chance, precision: 4);
        }

        [Fact]
        public void TunnelVisionAlpha_zero_below_threshold()
        {
            float alpha = InsanityCalculator.TunnelVisionAlpha(75f, 80f);
            Assert.Equal(0f, alpha);
        }

        [Fact]
        public void TunnelVisionAlpha_one_at_max()
        {
            float alpha = InsanityCalculator.TunnelVisionAlpha(100f, 80f);
            Assert.Equal(1f, alpha, precision: 4);
        }
    }
}
