using System;

namespace InsanityMod
{
    internal static class InsanityCalculator
    {
        public static float TickDelta(
            bool isInFacility, bool isInShip,
            float rateInFacility, float rateOnShip, float decayOutdoor,
            float multiplier, float deltaTime)
        {
            float rate;
            if (isInFacility)
                rate = rateInFacility * multiplier;
            else if (!isInShip)   // 야외
                rate = -decayOutdoor;
            else                  // 함선
                rate = rateOnShip;

            return rate * deltaTime;
        }

        public static float Clamp(float value) => Math.Clamp(value, 0f, 100f);

        public static float ChokeChance(float baseChance, float stackIncrement, int consecutiveUses) =>
            baseChance + stackIncrement * consecutiveUses;

        public static float TunnelVisionAlpha(float insanity, float threshold) =>
            insanity >= threshold
                ? (insanity - threshold) / (100f - threshold)
                : 0f;
    }
}
