using BepInEx.Configuration;

namespace InsanityMod
{
    internal static class ModConfig
    {
        public static ConfigEntry<string> Language               { get; private set; } = null!;
        public static ConfigEntry<float> InsanityRateInFacility  { get; private set; } = null!;
        public static ConfigEntry<float> InsanityRateOnShip      { get; private set; } = null!;
        public static ConfigEntry<float> InsanityDecayOutdoor    { get; private set; } = null!;
        public static ConfigEntry<float> NightOutdoorRate        { get; private set; } = null!;
        public static ConfigEntry<int>   NightStartHour          { get; private set; } = null!;
        public static ConfigEntry<float> EclipseOutdoorRate      { get; private set; } = null!;
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
        public static ConfigEntry<float> VoiceHauntThreshold     { get; private set; } = null!;
        public static ConfigEntry<float> LightProximityRange      { get; private set; } = null!;
        public static ConfigEntry<float> ApparatusSpike          { get; private set; } = null!;
        public static ConfigEntry<bool>  EnableMaskedTransform  { get; private set; } = null!;

        public static void Initialize(ConfigFile cfg)
        {
            const string S_INS = "Insanity";
            const string S_BN  = "Paranoia";
            const string S_VFX = "VFX";
            const string S_REA = "Reactions";

            Language               = cfg.Bind("General", "Language", "AUTO", "Display language. AUTO detects from system locale. Options: AUTO, EN, KO.");
            InsanityRateInFacility = cfg.Bind(S_INS, "InsanityRateInFacility", 0.167f, "Insanity gained per second inside the facility (baseline, ~10 min solo to 100%).");
            InsanityRateOnShip     = cfg.Bind(S_INS, "InsanityRateOnShip",     0f,   "Insanity gained per second on the ship.");
            InsanityDecayOutdoor   = cfg.Bind(S_INS, "InsanityDecayOutdoor",   0.8f, "Insanity lost per second outdoors (daytime).");
            NightOutdoorRate       = cfg.Bind(S_INS, "NightOutdoorRate",       0.05f, "Insanity gained per second outdoors at night. Set to 0 to disable.");
            NightStartHour         = cfg.Bind(S_INS, "NightStartHour",         19,    "Game hour (0-23) at which night begins outdoors.");
            EclipseOutdoorRate     = cfg.Bind(S_INS, "EclipseOutdoorRate",     0.1f,  "Insanity gained per second outdoors during Eclipse weather.");
            BloodNightMultiplier   = cfg.Bind(S_BN,  "ParanoiaMultiplier",   1.2f, "Insanity rate multiplier when Paranoia weather is active.");
            BloodNightSpawnWeight  = cfg.Bind(S_BN,  "ParanoiaSpawnWeight",  20,   "Spawn weight for Paranoia weather (other weathers are 100, so ~3% per night at 20).");
            TunnelVisionThreshold  = cfg.Bind(S_VFX, "TunnelVisionThreshold",  80f,  "Insanity % at which the red tunnel-vision overlay starts.");

            MobVisibilityScale      = cfg.Bind(S_REA, "MobVisibilityScale",      1f,    "Multiplier for insanity gained per second per visible enemy (per-type rates are scaled by this).");
            MobVisibilityRange      = cfg.Bind(S_REA, "MobVisibilityRange",      30f,   "Max distance (m) at which a visible enemy contributes to insanity.");
            TeammateBuffRate        = cfg.Bind(S_REA, "TeammateBuffRate",        0.15f, "Insanity-rate reduction per second when at least one living teammate is nearby.");
            TeammateBuffRange       = cfg.Bind(S_REA, "TeammateBuffRange",       6f,    "Max distance (m) for the teammate proximity buff.");
            LightBuffRate           = cfg.Bind(S_REA, "LightBuffRate",           0.1f,  "Insanity-rate reduction per second when player is illuminated (own flashlight on, or in ship).");
            DeathWitnessSpike       = cfg.Bind(S_REA, "DeathWitnessSpike",       25f,   "Instant insanity gain when a teammate dies in your line of sight.");
            DeathWitnessRange       = cfg.Bind(S_REA, "DeathWitnessRange",       40f,   "Max distance (m) for the death witness check.");
            GhostGirlBoostThreshold = cfg.Bind(S_REA, "GhostGirlBoostThreshold", 80f,   "Insanity % above which the host slightly raises Ghost Girl haunt chance (set high to disable).");
            VoiceHauntThreshold     = cfg.Bind(S_REA, "VoiceHauntThreshold",     70f,   "Insanity % at which voice distortion + recorded teammate voice playback start.");
            LightProximityRange  = cfg.Bind(S_REA, "LightProximityRange",  8f,   "Max distance (m) to a facility light for the light proximity buff.");
            ApparatusSpike        = cfg.Bind(S_REA, "ApparatusSpike",        15f,  "Instant insanity gain when the apparatus is removed. Also disables insanity-reduction buffs inside the facility for the rest of the round.");
            EnableMaskedTransform = cfg.Bind(S_REA, "EnableMaskedTransform", true, "If true, reaching 100% insanity transforms the player into a Masked enemy.");
        }
    }
}
