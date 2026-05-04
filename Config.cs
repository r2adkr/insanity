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

        public static ConfigEntry<float> MobVisibilityScale      { get; private set; } = null!;
        public static ConfigEntry<float> MobVisibilityRange      { get; private set; } = null!;
        public static ConfigEntry<float> TeammateBuffRate        { get; private set; } = null!;
        public static ConfigEntry<float> TeammateBuffRange       { get; private set; } = null!;
        public static ConfigEntry<float> LightBuffRate           { get; private set; } = null!;
        public static ConfigEntry<float> DeathWitnessSpike       { get; private set; } = null!;
        public static ConfigEntry<float> DeathWitnessRange       { get; private set; } = null!;
        public static ConfigEntry<float> GhostGirlBoostThreshold { get; private set; } = null!;

        public static void Initialize(ConfigFile cfg)
        {
            const string S_INS = "Insanity";
            const string S_BN  = "BloodNight";
            const string S_VFX = "VFX";
            const string S_REA = "Reactions";

            InsanityRateInFacility = cfg.Bind(S_INS, "InsanityRateInFacility", 0.2f, "Insanity gained per second inside the facility (baseline).");
            InsanityRateOnShip     = cfg.Bind(S_INS, "InsanityRateOnShip",     0f,   "Insanity gained per second on the ship.");
            InsanityDecayOutdoor   = cfg.Bind(S_INS, "InsanityDecayOutdoor",   0.8f, "Insanity lost per second outdoors.");
            BloodNightMultiplier   = cfg.Bind(S_BN,  "BloodNightMultiplier",   1.2f, "Insanity rate multiplier when Blood Night weather is active.");
            BloodNightSpawnWeight  = cfg.Bind(S_BN,  "BloodNightSpawnWeight",  20,   "Spawn weight for Blood Night weather (other weathers are 100, so ~3% per night at 20).");
            TunnelVisionThreshold  = cfg.Bind(S_VFX, "TunnelVisionThreshold",  80f,  "Insanity % at which the red tunnel-vision overlay starts.");

            MobVisibilityScale      = cfg.Bind(S_REA, "MobVisibilityScale",      1f,    "Multiplier for insanity gained per second per visible enemy (per-type rates are scaled by this).");
            MobVisibilityRange      = cfg.Bind(S_REA, "MobVisibilityRange",      30f,   "Max distance (m) at which a visible enemy contributes to insanity.");
            TeammateBuffRate        = cfg.Bind(S_REA, "TeammateBuffRate",        0.15f, "Insanity-rate reduction per second when at least one living teammate is nearby.");
            TeammateBuffRange       = cfg.Bind(S_REA, "TeammateBuffRange",       6f,    "Max distance (m) for the teammate proximity buff.");
            LightBuffRate           = cfg.Bind(S_REA, "LightBuffRate",           0.1f,  "Insanity-rate reduction per second when player is illuminated (own flashlight on, or in ship).");
            DeathWitnessSpike       = cfg.Bind(S_REA, "DeathWitnessSpike",       25f,   "Instant insanity gain when a teammate dies in your line of sight.");
            DeathWitnessRange       = cfg.Bind(S_REA, "DeathWitnessRange",       40f,   "Max distance (m) for the death witness check.");
            GhostGirlBoostThreshold = cfg.Bind(S_REA, "GhostGirlBoostThreshold", 80f,   "Insanity % above which the host slightly raises Ghost Girl haunt chance (set high to disable).");
        }
    }
}
