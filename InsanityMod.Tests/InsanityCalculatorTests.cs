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
                rateInFacility: 0.5f, rateOnShip: 0.1f, outdoorRate: -0.8f,
                multiplier: 1.0f, deltaTime: 1.0f);
            Assert.Equal(0.5f, delta);
        }

        [Fact]
        public void Outdoor_daytime_decreases_insanity()
        {
            float delta = InsanityCalculator.TickDelta(
                isInFacility: false, isInShip: false,
                rateInFacility: 0.5f, rateOnShip: 0.1f, outdoorRate: -0.8f,
                multiplier: 1.0f, deltaTime: 1.0f);
            Assert.Equal(-0.8f, delta);
        }

        [Fact]
        public void Outdoor_night_increases_insanity()
        {
            float delta = InsanityCalculator.TickDelta(
                isInFacility: false, isInShip: false,
                rateInFacility: 0.5f, rateOnShip: 0.1f, outdoorRate: 0.05f,
                multiplier: 1.0f, deltaTime: 1.0f);
            Assert.Equal(0.05f, delta, precision: 4);
        }

        [Fact]
        public void Outdoor_eclipse_increases_insanity_faster()
        {
            float delta = InsanityCalculator.TickDelta(
                isInFacility: false, isInShip: false,
                rateInFacility: 0.5f, rateOnShip: 0.1f, outdoorRate: 0.1f,
                multiplier: 1.0f, deltaTime: 1.0f);
            Assert.Equal(0.1f, delta, precision: 4);
        }

        [Fact]
        public void OnShip_increases_slowly()
        {
            float delta = InsanityCalculator.TickDelta(
                isInFacility: false, isInShip: true,
                rateInFacility: 0.5f, rateOnShip: 0.1f, outdoorRate: -0.8f,
                multiplier: 1.0f, deltaTime: 1.0f);
            Assert.Equal(0.1f, delta);
        }

        [Fact]
        public void Paranoia_multiplier_applied_in_facility()
        {
            float delta = InsanityCalculator.TickDelta(
                isInFacility: true, isInShip: false,
                rateInFacility: 0.5f, rateOnShip: 0.1f, outdoorRate: -0.8f,
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
