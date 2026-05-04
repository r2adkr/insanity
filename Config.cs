using BepInEx.Configuration;

namespace InsanityMod
{
    internal static class ModConfig
    {
        public static ConfigEntry<float> InsanityRateInFacility  { get; private set; } = null!;
        public static ConfigEntry<float> InsanityRateOnShip      { get; private set; } = null!;
        public static ConfigEntry<float> InsanityDecayOutdoor    { get; private set; } = null!;
        public static ConfigEntry<float> BloodNightMultiplier    { get; private set; } = null!;
        public static ConfigEntry<int>   BloodNightSpawnWeight   { get; private set; } = null!;
        public static ConfigEntry<float> TunnelVisionThreshold   { get; private set; } = null!;

        public static void Initialize(ConfigFile cfg)
        {
            const string S_INS = "Insanity";
            const string S_BN  = "BloodNight";
            const string S_VFX = "VFX";

            InsanityRateInFacility = cfg.Bind(S_INS, "InsanityRateInFacility", 0.5f, "Insanity gained per second inside the facility.");
            InsanityRateOnShip     = cfg.Bind(S_INS, "InsanityRateOnShip",     0.1f, "Insanity gained per second on the ship (anti-camping).");
            InsanityDecayOutdoor   = cfg.Bind(S_INS, "InsanityDecayOutdoor",   0.8f, "Insanity lost per second outdoors.");
            BloodNightMultiplier   = cfg.Bind(S_BN,  "BloodNightMultiplier",   1.2f, "Insanity rate multiplier when Blood Night weather is active.");
            BloodNightSpawnWeight  = cfg.Bind(S_BN,  "BloodNightSpawnWeight",  20,   "Spawn weight for Blood Night weather (other weathers are 100, so ~3% per night at 20).");
            TunnelVisionThreshold  = cfg.Bind(S_VFX, "TunnelVisionThreshold",  80f,  "Insanity % at which the red tunnel-vision overlay starts.");
        }
    }
}
