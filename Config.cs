using BepInEx.Configuration;

namespace InsanityMod
{
    internal static class ModConfig
    {
        public static ConfigEntry<string> Language               { get; private set; } = null!;
        public static ConfigEntry<float> InsanityRateInFacility  { get; private set; } = null!;
        public static ConfigEntry<float> InsanityDecayOutdoor    { get; private set; } = null!;
        public static ConfigEntry<float> NightOutdoorRate        { get; private set; } = null!;
        public static ConfigEntry<int>   NightStartHour          { get; private set; } = null!;
        public static ConfigEntry<float> EclipseOutdoorRate      { get; private set; } = null!;
        public static ConfigEntry<float> ParanoiaOutdoorRate     { get; private set; } = null!;
        public static ConfigEntry<float> BloodNightMultiplier    { get; private set; } = null!;
        public static ConfigEntry<int>   BloodNightSpawnWeight   { get; private set; } = null!;
        public static ConfigEntry<float>  TunnelVisionThreshold  { get; private set; } = null!;
        public static ConfigEntry<string> TunnelVisionColor      { get; private set; } = null!;
        public static ConfigEntry<bool>   HideHudAtZero          { get; private set; } = null!;
        public static ConfigEntry<bool>   EnableHud              { get; private set; } = null!;

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
        public static ConfigEntry<bool>  MaskedTransformOnlyDuringParanoia { get; private set; } = null!;
        public static ConfigEntry<float> UnderwaterRate          { get; private set; } = null!;
        public static ConfigEntry<float> CompanyMoonDecayRate    { get; private set; } = null!;
        public static ConfigEntry<float> TZPInsanityDrainRate    { get; private set; } = null!;
        public static ConfigEntry<float> RateOnShipLightsOn      { get; private set; } = null!;
        public static ConfigEntry<float> RateOnShipLightsOff     { get; private set; } = null!;

        public static void Initialize(ConfigFile cfg)
        {
            const string S_INS = "Insanity";
            const string S_BN  = "Paranoia";
            const string S_VFX = "VFX";
            const string S_REA = "Reactions";

            Language               = cfg.Bind("General", "Language", "AUTO", "Display language. AUTO detects from system locale. Options: AUTO, EN, KO.");
            InsanityRateInFacility = cfg.Bind(S_INS, "InsanityRateInFacility", 0.167f, "Insanity gained per second inside the facility (baseline, ~10 min solo to 100%).");
            InsanityDecayOutdoor   = cfg.Bind(S_INS, "InsanityDecayOutdoor",   0.8f, "Insanity lost per second outdoors (daytime).");
            NightOutdoorRate       = cfg.Bind(S_INS, "NightOutdoorRate",       0.05f, "Insanity gained per second outdoors at night. Set to 0 to disable.");
            NightStartHour         = cfg.Bind(S_INS, "NightStartHour",         19,    "Game hour (0-23) at which night begins outdoors.");
            EclipseOutdoorRate     = cfg.Bind(S_INS, "EclipseOutdoorRate",     0.1f,  "Insanity gained per second outdoors during Eclipse weather.");
            ParanoiaOutdoorRate    = cfg.Bind(S_INS, "ParanoiaOutdoorRate",    0.1f,  "Insanity gained per second outdoors during Paranoia weather.");
            BloodNightMultiplier   = cfg.Bind(S_BN,  "ParanoiaMultiplier",   1.2f, "Insanity rate multiplier when Paranoia weather is active.");
            BloodNightSpawnWeight  = cfg.Bind(S_BN,  "ParanoiaSpawnWeight",  20,   "Spawn weight for Paranoia weather (other weathers are 100, so ~3% per night at 20).");
            TunnelVisionThreshold = cfg.Bind(S_VFX, "TunnelVisionThreshold", 80f,        "Insanity % at which the tunnel-vision overlay starts.");
            TunnelVisionColor     = cfg.Bind(S_VFX, "TunnelVisionColor",     "#180202", "Tunnel vision overlay color (hex). Default is very dark red. Use #000000 for pure black.");
            HideHudAtZero         = cfg.Bind(S_VFX, "HideHudAtZero",         false,     "If true, the insanity HUD ring is hidden while insanity is 0%.");
            EnableHud             = cfg.Bind(S_VFX, "EnableHud",             true,      "Master switch for the insanity HUD ring. Set to false to disable it entirely (no canvas/sprite created).");

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
            MaskedTransformOnlyDuringParanoia = cfg.Bind(S_REA, "MaskedTransformOnlyDuringParanoia", true,  "If true, the 100% Masked transformation only triggers when Paranoia weather is active.");
            UnderwaterRate         = cfg.Bind(S_REA, "UnderwaterRate",         0.4f,  "Insanity gained per second while the local player is underwater.");
            CompanyMoonDecayRate   = cfg.Bind(S_INS, "CompanyMoonDecayRate",   0.5f,  "Insanity reduced per second while on the Company building moon (71 Gordion).");
            TZPInsanityDrainRate   = cfg.Bind(S_REA, "TZPInsanityDrainRate",   1.0f,  "Insanity reduced per second while the local player has an active TZP-Inhalant effect.");
            RateOnShipLightsOn     = cfg.Bind(S_INS, "RateOnShipLightsOn",     -0.3f, "Insanity per second on the ship while ship lights are ON. Negative reduces, positive increases.");
            RateOnShipLightsOff    = cfg.Bind(S_INS, "RateOnShipLightsOff",    0.15f, "Insanity per second on the ship while ship lights are OFF. Negative reduces, positive increases.");
        }
    }
}
