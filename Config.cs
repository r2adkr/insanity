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
        public static ConfigEntry<float> BreadInsanityReduction  { get; private set; } = null!;
        public static ConfigEntry<float> BreadChokeBaseChance    { get; private set; } = null!;
        public static ConfigEntry<float> BreadChokeStack         { get; private set; } = null!;
        public static ConfigEntry<int>   BreadShopPrice          { get; private set; } = null!;
        public static ConfigEntry<int>   PotionShopPrice         { get; private set; } = null!;
        public static ConfigEntry<int>   PotionFacilitySpawnCount{ get; private set; } = null!;
        public static ConfigEntry<float> TunnelVisionThreshold   { get; private set; } = null!;
        public static ConfigEntry<float> InsanityAudioThreshold  { get; private set; } = null!;

        public static void Initialize(ConfigFile cfg)
        {
            const string S_INS  = "Insanity";
            const string S_BN   = "BloodNight";
            const string S_ITEM = "Items";
            const string S_VFX  = "VFX";
            const string S_AUD  = "Audio";

            InsanityRateInFacility   = cfg.Bind(S_INS,  "InsanityRateInFacility",   0.5f,  "시설 내 광기 상승/초");
            InsanityRateOnShip       = cfg.Bind(S_INS,  "InsanityRateOnShip",        0.1f,  "함선 내 광기 상승/초");
            InsanityDecayOutdoor     = cfg.Bind(S_INS,  "InsanityDecayOutdoor",      0.8f,  "야외 광기 감소/초");
            BloodNightMultiplier     = cfg.Bind(S_BN,   "BloodNightMultiplier",      1.2f,  "피의 밤 광기 배율");
            BloodNightSpawnWeight    = cfg.Bind(S_BN,   "BloodNightSpawnWeight",     1,     "피의 밤 날씨 발생 가중치");
            BreadInsanityReduction   = cfg.Bind(S_ITEM, "BreadInsanityReduction",    15f,   "빵 광기 감소%");
            BreadChokeBaseChance     = cfg.Bind(S_ITEM, "BreadChokeBaseChance",      0.2f,  "빵 기본 질식 확률 (0~1)");
            BreadChokeStack          = cfg.Bind(S_ITEM, "BreadChokeStack",           0.05f, "연속 섭취 질식 확률 누적치");
            BreadShopPrice           = cfg.Bind(S_ITEM, "BreadShopPrice",            10,    "빵 상점 가격 (크레딧)");
            PotionShopPrice          = cfg.Bind(S_ITEM, "PotionShopPrice",           80,    "물약 상점 가격 (크레딧)");
            PotionFacilitySpawnCount = cfg.Bind(S_ITEM, "PotionFacilitySpawnCount",  2,     "라운드당 시설 내 물약 스폰 수");
            TunnelVisionThreshold    = cfg.Bind(S_VFX,  "TunnelVisionThreshold",     80f,   "터널 비전 발동 광기%");
            InsanityAudioThreshold   = cfg.Bind(S_AUD,  "InsanityAudioThreshold",    50f,   "광기 오디오 발동 광기%");
        }
    }
}
