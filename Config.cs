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

            InsanityRateInFacility = cfg.Bind(S_INS, "InsanityRateInFacility", 0.5f, "시설 내 광기 상승/초");
            InsanityRateOnShip     = cfg.Bind(S_INS, "InsanityRateOnShip",     0.1f, "함선 내 광기 상승/초");
            InsanityDecayOutdoor   = cfg.Bind(S_INS, "InsanityDecayOutdoor",   0.8f, "야외 광기 감소/초");
            BloodNightMultiplier   = cfg.Bind(S_BN,  "BloodNightMultiplier",   1.2f, "피의 밤 광기 배율");
            BloodNightSpawnWeight  = cfg.Bind(S_BN,  "BloodNightSpawnWeight",  1,    "피의 밤 날씨 발생 가중치");
            TunnelVisionThreshold  = cfg.Bind(S_VFX, "TunnelVisionThreshold",  80f,  "터널 비전 발동 광기%");
        }
    }
}
