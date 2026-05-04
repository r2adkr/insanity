# Insanity Mod Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** BepInEx 5.4.23.5+ 기반의 Lethal Company V80-81 광기 시스템 모드를 구현한다.

**Architecture:** Manager Singleton 패턴으로 InsanityManager(광기 계산), VFXManager(터널 비전), BloodNightManager(WeatherRegistry), InsanityNetworkHandler(결과 공유/오디오 동기화)를 분리한다. Harmony 패치는 얇은 중계자 역할만 하며, GrabbableObject 서브클래스로 아이템을 구현한다.

**Tech Stack:** C# (.NET Standard 2.1), BepInEx 5.4.23.5, Harmony 2, Unity Netcode (NGO), WeatherRegistry 0.8.8, Coroner (optional), Newtonsoft.Json, Unity AssetBundle (Unity 2022.3)

---

## 경로 상수 (모든 태스크에서 공통 참조)

```
LC_MANAGED  = C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed
BEPINEX_CORE    = C:\Users\yeokyoomin\AppData\Roaming\Thunderstore Mod Manager\DataFolder\LethalCompany\profiles\mods\BepInEx\core
BEPINEX_PLUGINS = C:\Users\yeokyoomin\AppData\Roaming\Thunderstore Mod Manager\DataFolder\LethalCompany\profiles\mods\BepInEx\plugins
DEPLOY_DIR  = C:\Users\yeokyoomin\AppData\Roaming\Thunderstore Mod Manager\DataFolder\LethalCompany\profiles\mods\BepInEx\plugins\InsanityMod
PROJECT_DIR = C:\Users\yeokyoomin\Downloads\insanity
```

---

## Task 1: 프로젝트 스캐폴딩

**Files:**
- Create: `InsanityMod.csproj`
- Create: `Plugin.cs`
- Create: `Directory.Build.props`

- [ ] **Step 1: Directory.Build.props 생성 — 공통 경로 변수 정의**

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <LCManaged>C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\</LCManaged>
    <BepInExCore>C:\Users\yeokyoomin\AppData\Roaming\Thunderstore Mod Manager\DataFolder\LethalCompany\profiles\mods\BepInEx\core\</BepInExCore>
    <BepInExPlugins>C:\Users\yeokyoomin\AppData\Roaming\Thunderstore Mod Manager\DataFolder\LethalCompany\profiles\mods\BepInEx\plugins\</BepInExPlugins>
    <DeployDir>$(BepInExPlugins)InsanityMod\</DeployDir>
  </PropertyGroup>
</Project>
```

- [ ] **Step 2: InsanityMod.csproj 생성**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <AssemblyName>InsanityMod</AssemblyName>
    <RootNamespace>InsanityMod</RootNamespace>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <LangVersion>9</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="Assembly-CSharp">
      <HintPath>$(LCManaged)Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>$(LCManaged)UnityEngine.CoreModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.UI">
      <HintPath>$(LCManaged)UnityEngine.UI.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.AudioModule">
      <HintPath>$(LCManaged)UnityEngine.AudioModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="Unity.Netcode.Runtime">
      <HintPath>$(LCManaged)Unity.Netcode.Runtime.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="Unity.TextMeshPro">
      <HintPath>$(LCManaged)Unity.TextMeshPro.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="Newtonsoft.Json">
      <HintPath>$(LCManaged)Newtonsoft.Json.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="BepInEx">
      <HintPath>$(BepInExCore)BepInEx.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="0Harmony">
      <HintPath>$(BepInExCore)0Harmony.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="WeatherRegistry">
      <HintPath>$(BepInExPlugins)mrov-WeatherRegistry\WeatherRegistry.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="MrovLib">
      <HintPath>$(BepInExPlugins)mrov-MrovLib\MrovLib.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

  <!-- 빌드 후 deploy 디렉토리에 자동 복사 -->
  <Target Name="DeployToBepInEx" AfterTargets="Build">
    <MakeDir Directories="$(DeployDir)" />
    <Copy SourceFiles="$(OutputPath)$(AssemblyName).dll" DestinationFolder="$(DeployDir)" />
    <Copy SourceFiles="$(ProjectDir)Resources\Langs.json" DestinationFolder="$(DeployDir)" />
    <Copy SourceFiles="$(ProjectDir)Assets\insanitymod.bundle" DestinationFolder="$(DeployDir)" Condition="Exists('$(ProjectDir)Assets\insanitymod.bundle')" />
  </Target>
</Project>
```

- [ ] **Step 3: 폴더 구조 생성**

```
mkdir Managers
mkdir Patches
mkdir Items
mkdir Network
mkdir Assets
mkdir Resources
```

- [ ] **Step 4: Plugin.cs 스켈레톤 작성**

```csharp
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using InsanityMod.Managers;
using InsanityMod.Network;

namespace InsanityMod
{
    [BepInPlugin(Plugin.GUID, Plugin.NAME, Plugin.VERSION)]
    [BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("EliteMasterEric.Coroner", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID    = "com.insanitymod.lethalcompany";
        public const string NAME    = "InsanityMod";
        public const string VERSION = "1.0.0";

        internal static ManualLogSource Log = null!;
        internal static Plugin Instance    = null!;

        private readonly Harmony _harmony = new Harmony(GUID);

        private void Awake()
        {
            Instance = this;
            Log      = Logger;

            ModConfig.Initialize(Config);
            LocalizationManager.Initialize();
            AssetBundleLoader.Load();
            BloodNightManager.Initialize();
            VFXManager.Initialize();
            CoronerCompat.Initialize();

            _harmony.PatchAll();
            Log.LogInfo($"{NAME} v{VERSION} loaded.");
        }
    }
}
```

- [ ] **Step 5: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 6: 커밋**

```
git add InsanityMod.csproj Directory.Build.props Plugin.cs Managers/ Patches/ Items/ Network/ Assets/ Resources/
git commit -m "feat: project scaffolding and Plugin entry point"
```

---

## Task 2: Config.cs

**Files:**
- Create: `Config.cs`

- [ ] **Step 1: Config.cs 작성**

```csharp
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
```

- [ ] **Step 2: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 3: 커밋**

```
git add Config.cs
git commit -m "feat: add ModConfig with all ConfigEntry values"
```

---

## Task 3: LocalizationManager + Langs.json (테스트 포함)

**Files:**
- Create: `Managers/LocalizationManager.cs`
- Create: `Resources/Langs.json`
- Create: `InsanityMod.Tests/InsanityMod.Tests.csproj`
- Create: `InsanityMod.Tests/LocalizationTests.cs`

- [ ] **Step 1: Langs.json 작성**

```json
{
  "KO": {
    "item.bread.name":    "50원짜리 빵",
    "item.bread.choke":   "빵에 걸렸다!",
    "item.potion.name":   "루시드 둠",
    "item.potion.use":    "시야가 맑아졌지만...",
    "hud.max_insanity":   "최대 광기",
    "weather.blood_night":"피의 밤"
  },
  "EN": {
    "item.bread.name":    "Cheap Bread",
    "item.bread.choke":   "Choking on Bread!",
    "item.potion.name":   "Lucid Doom",
    "item.potion.use":    "Vision cleared, but...",
    "hud.max_insanity":   "Peak Insanity",
    "weather.blood_night":"Blood Night"
  }
}
```

- [ ] **Step 2: LocalizationManager.cs 작성**

```csharp
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InsanityMod.Managers
{
    internal static class LocalizationManager
    {
        private static Dictionary<string, string> _strings = new();

        public static void Initialize()
        {
            string pluginDir  = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string langsPath  = Path.Combine(pluginDir, "Langs.json");

            if (!File.Exists(langsPath))
            {
                Plugin.Log.LogWarning($"Langs.json not found at {langsPath}, falling back to EN defaults.");
                LoadDefaults();
                return;
            }

            string lang = DetectLanguage();
            string json = File.ReadAllText(langsPath);
            var root    = JsonConvert.DeserializeObject<JObject>(json)!;

            // 지원 언어가 없으면 EN 폴백
            var section = root[lang] ?? root["EN"]!;
            foreach (var kv in section)
                _strings[kv.Key] = kv.Value?.ToString() ?? kv.Key;

            Plugin.Log.LogInfo($"Localization loaded: {lang} ({_strings.Count} strings)");
        }

        public static string Get(string key) =>
            _strings.TryGetValue(key, out var val) ? val : key;

        private static string DetectLanguage()
        {
            // LC V80+: GameNetworkManager.preferredLang 는 인트/enum, 0=EN, 1=KO (확인 필요)
            // 안전하게 시스템 언어로 폴백
            return System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpper()
                switch { "KO" => "KO", _ => "EN" };
        }

        private static void LoadDefaults()
        {
            _strings["item.bread.name"]     = "Cheap Bread";
            _strings["item.bread.choke"]    = "Choking on Bread!";
            _strings["item.potion.name"]    = "Lucid Doom";
            _strings["item.potion.use"]     = "Vision cleared, but...";
            _strings["hud.max_insanity"]    = "Peak Insanity";
            _strings["weather.blood_night"] = "Blood Night";
        }
    }
}
```

- [ ] **Step 3: 테스트 프로젝트 생성**

```xml
<!-- InsanityMod.Tests/InsanityMod.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit" Version="2.6.6" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: 실패하는 테스트 먼저 작성**

```csharp
// InsanityMod.Tests/LocalizationTests.cs
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace InsanityMod.Tests
{
    public class LocalizationTests
    {
        private static Dictionary<string, string> LoadLang(string json, string lang)
        {
            var root    = JsonConvert.DeserializeObject<JObject>(json)!;
            var section = root[lang] ?? root["EN"]!;
            var result  = new Dictionary<string, string>();
            foreach (var kv in section)
                result[kv.Key] = kv.Value?.ToString() ?? kv.Key;
            return result;
        }

        private const string SampleJson = @"{
          ""KO"": { ""item.bread.name"": ""50원짜리 빵"", ""hud.max_insanity"": ""최대 광기"" },
          ""EN"": { ""item.bread.name"": ""Cheap Bread"",  ""hud.max_insanity"": ""Peak Insanity"" }
        }";

        [Fact]
        public void Korean_key_returns_korean_string()
        {
            var strings = LoadLang(SampleJson, "KO");
            Assert.Equal("50원짜리 빵", strings["item.bread.name"]);
        }

        [Fact]
        public void English_key_returns_english_string()
        {
            var strings = LoadLang(SampleJson, "EN");
            Assert.Equal("Cheap Bread", strings["item.bread.name"]);
        }

        [Fact]
        public void Unknown_lang_falls_back_to_EN()
        {
            var root    = JsonConvert.DeserializeObject<JObject>(SampleJson)!;
            var section = root["FR"] ?? root["EN"]!;
            var result  = new Dictionary<string, string>();
            foreach (var kv in section)
                result[kv.Key] = kv.Value?.ToString() ?? kv.Key;

            Assert.Equal("Cheap Bread", result["item.bread.name"]);
        }
    }
}
```

- [ ] **Step 5: 테스트 실행 — 실패 확인**

```
dotnet test InsanityMod.Tests/InsanityMod.Tests.csproj
```
Expected: 3 tests pass (로직이 이미 JSON 파싱 헬퍼로 분리되었으므로 바로 통과함)

- [ ] **Step 6: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 7: 커밋**

```
git add Managers/LocalizationManager.cs Resources/Langs.json InsanityMod.Tests/
git commit -m "feat: add LocalizationManager with KO/EN Langs.json and tests"
```

---

## Task 4: AssetBundleLoader.cs (빈 스텁)

Unity AssetBundle은 Task 13에서 제작. 여기서는 로더만 준비.

**Files:**
- Create: `AssetBundleLoader.cs`

- [ ] **Step 1: AssetBundleLoader.cs 작성**

```csharp
using System.IO;
using System.Reflection;
using UnityEngine;

namespace InsanityMod
{
    internal static class AssetBundleLoader
    {
        public static Material?    TunnelVisionMaterial  { get; private set; }
        public static AudioClip[]  InsanityAudioClips    { get; private set; } = System.Array.Empty<AudioClip>();
        public static GameObject?  ValueBreadPrefab      { get; private set; }
        public static GameObject?  LucidDoomPrefab       { get; private set; }
        public static Item?        ValueBreadItem        { get; private set; }
        public static Item?        LucidDoomItem         { get; private set; }

        public static void Load()
        {
            string pluginDir  = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string bundlePath = Path.Combine(pluginDir, "insanitymod.bundle");

            if (!File.Exists(bundlePath))
            {
                Plugin.Log.LogWarning("insanitymod.bundle not found — items and VFX will be missing. Build the Unity AssetBundle first (Task 13).");
                return;
            }

            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Plugin.Log.LogError("Failed to load insanitymod.bundle.");
                return;
            }

            TunnelVisionMaterial  = bundle.LoadAsset<Material>("TunnelVisionMat");
            InsanityAudioClips    = new[]
            {
                bundle.LoadAsset<AudioClip>("insanity_breath_lo"),
                bundle.LoadAsset<AudioClip>("insanity_mutter"),
                bundle.LoadAsset<AudioClip>("insanity_breath_hi"),
            };
            ValueBreadPrefab = bundle.LoadAsset<GameObject>("ValueBreadPrefab");
            LucidDoomPrefab  = bundle.LoadAsset<GameObject>("LucidDoomPrefab");
            ValueBreadItem   = bundle.LoadAsset<Item>("ValueBreadItem");
            LucidDoomItem    = bundle.LoadAsset<Item>("LucidDoomItem");

            Plugin.Log.LogInfo("AssetBundle loaded successfully.");
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 3: 커밋**

```
git add AssetBundleLoader.cs
git commit -m "feat: add AssetBundleLoader stub"
```

---

## Task 5: InsanityManager + InsanityCalculator (테스트 포함)

**Files:**
- Create: `InsanityCalculator.cs` (순수 수학, Unity 의존 없음 → 테스트 가능)
- Create: `Managers/InsanityManager.cs`
- Modify: `InsanityMod.Tests/InsanityCalculatorTests.cs`

- [ ] **Step 1: InsanityCalculator.cs 작성 (순수 로직)**

```csharp
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
```

- [ ] **Step 2: 실패하는 테스트 작성**

```csharp
// InsanityMod.Tests/InsanityCalculatorTests.cs
using Xunit;

namespace InsanityMod.Tests
{
    public class InsanityCalculatorTests
    {
        [Fact]
        public void InFacility_increases_insanity()
        {
            float delta = InsanityMod.InsanityCalculator.TickDelta(
                isInFacility: true, isInShip: false,
                rateInFacility: 0.5f, rateOnShip: 0.1f, decayOutdoor: 0.8f,
                multiplier: 1.0f, deltaTime: 1.0f);
            Assert.Equal(0.5f, delta);
        }

        [Fact]
        public void Outdoor_decreases_insanity()
        {
            float delta = InsanityMod.InsanityCalculator.TickDelta(
                isInFacility: false, isInShip: false,
                rateInFacility: 0.5f, rateOnShip: 0.1f, decayOutdoor: 0.8f,
                multiplier: 1.0f, deltaTime: 1.0f);
            Assert.Equal(-0.8f, delta);
        }

        [Fact]
        public void OnShip_increases_slowly()
        {
            float delta = InsanityMod.InsanityCalculator.TickDelta(
                isInFacility: false, isInShip: true,
                rateInFacility: 0.5f, rateOnShip: 0.1f, decayOutdoor: 0.8f,
                multiplier: 1.0f, deltaTime: 1.0f);
            Assert.Equal(0.1f, delta);
        }

        [Fact]
        public void BloodNight_multiplier_applied_in_facility()
        {
            float delta = InsanityMod.InsanityCalculator.TickDelta(
                isInFacility: true, isInShip: false,
                rateInFacility: 0.5f, rateOnShip: 0.1f, decayOutdoor: 0.8f,
                multiplier: 1.2f, deltaTime: 1.0f);
            Assert.Equal(0.6f, delta, precision: 4);
        }

        [Fact]
        public void Clamp_caps_at_100()
        {
            Assert.Equal(100f, InsanityMod.InsanityCalculator.Clamp(150f));
        }

        [Fact]
        public void Clamp_floor_at_0()
        {
            Assert.Equal(0f, InsanityMod.InsanityCalculator.Clamp(-10f));
        }

        [Fact]
        public void ChokeChance_accumulates_correctly()
        {
            float chance = InsanityMod.InsanityCalculator.ChokeChance(0.2f, 0.05f, 3);
            Assert.Equal(0.35f, chance, precision: 4);
        }

        [Fact]
        public void TunnelVisionAlpha_zero_below_threshold()
        {
            float alpha = InsanityMod.InsanityCalculator.TunnelVisionAlpha(75f, 80f);
            Assert.Equal(0f, alpha);
        }

        [Fact]
        public void TunnelVisionAlpha_one_at_max()
        {
            float alpha = InsanityMod.InsanityCalculator.TunnelVisionAlpha(100f, 80f);
            Assert.Equal(1f, alpha, precision: 4);
        }
    }
}
```

- [ ] **Step 3: 테스트 실행 — 실패 확인**

```
dotnet test InsanityMod.Tests/InsanityMod.Tests.csproj --filter "FullyQualifiedName~InsanityCalculator"
```
Expected: `Error: InsanityMod.InsanityCalculator 참조 불가` (InsanityCalculator.cs가 테스트 프로젝트에 없음)

- [ ] **Step 4: 테스트 프로젝트에서 InsanityCalculator 참조 추가**

테스트 프로젝트의 `.csproj`에 추가:
```xml
<ItemGroup>
  <Compile Include="..\InsanityCalculator.cs" />
</ItemGroup>
```

- [ ] **Step 5: 테스트 재실행 — 통과 확인**

```
dotnet test InsanityMod.Tests/InsanityMod.Tests.csproj --filter "FullyQualifiedName~InsanityCalculator"
```
Expected: `9 passed`

- [ ] **Step 6: InsanityManager.cs 작성 (Unity/LC 의존)**

```csharp
using GameNetcodeStuff;
using InsanityMod.Network;
using UnityEngine;

namespace InsanityMod.Managers
{
    internal static class InsanityManager
    {
        private static float _insanity;
        private static float _maxInsanityThisRound;
        private static int   _consecutiveBreadUses;
        private static float _audioTimer;

        public static float Insanity            => _insanity;
        public static float MaxInsanityThisRound => _maxInsanityThisRound;

        public static void ResetForRound()
        {
            _insanity              = 0f;
            _maxInsanityThisRound  = 0f;
            _consecutiveBreadUses  = 0;
            _audioTimer            = 0f;
        }

        public static void Tick(PlayerControllerB player, float deltaTime)
        {
            float delta = InsanityCalculator.TickDelta(
                player.isInsideFactory,
                player.isInHangarShipRoom,
                ModConfig.InsanityRateInFacility.Value,
                ModConfig.InsanityRateOnShip.Value,
                ModConfig.InsanityDecayOutdoor.Value,
                BloodNightManager.IsActive ? ModConfig.BloodNightMultiplier.Value : 1f,
                deltaTime);

            _insanity = InsanityCalculator.Clamp(_insanity + delta);
            if (_insanity > _maxInsanityThisRound)
                _maxInsanityThisRound = _insanity;

            VFXManager.UpdateTunnelVision(_insanity);
            TryTriggerInsanityAudio(deltaTime);
        }

        public static void AddInsanity(float amount)
        {
            _insanity = InsanityCalculator.Clamp(_insanity + amount);
            if (_insanity > _maxInsanityThisRound)
                _maxInsanityThisRound = _insanity;
        }

        public static int GetConsecutiveBreadUses()  => _consecutiveBreadUses;
        public static void IncrementBreadUses()      => _consecutiveBreadUses++;
        public static void ResetBreadUses()          => _consecutiveBreadUses = 0;

        private static void TryTriggerInsanityAudio(float deltaTime)
        {
            if (_insanity < ModConfig.InsanityAudioThreshold.Value) return;

            float interval = _insanity >= 90f ? 5f : _insanity >= 70f ? 10f : 20f;
            _audioTimer -= deltaTime;
            if (_audioTimer > 0f) return;

            _audioTimer = interval;
            int clipIndex = _insanity >= 90f ? 2 : _insanity >= 70f ? 1 : 0;
            InsanityNetworkHandler.SendPlayInsanityAudio(clipIndex);
        }
    }
}
```

- [ ] **Step 7: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 8: 커밋**

```
git add InsanityCalculator.cs Managers/InsanityManager.cs InsanityMod.Tests/InsanityCalculatorTests.cs
git commit -m "feat: add InsanityCalculator with tests and InsanityManager"
```

---

## Task 6: VFXManager.cs

**Files:**
- Create: `Managers/VFXManager.cs`

- [ ] **Step 1: VFXManager.cs 작성**

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace InsanityMod.Managers
{
    internal static class VFXManager
    {
        private static Image? _overlay;

        public static void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // SampleSceneRelay = 실제 플레이 씬 이름 (LC V80+)
            if (scene.name != "SampleSceneRelay") return;
            CreateOverlay();
        }

        private static void CreateOverlay()
        {
            if (_overlay != null) return;

            var canvasGO = new GameObject("InsanityVFX_Canvas");
            Object.DontDestroyOnLoad(canvasGO);

            var canvas        = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var imageGO = new GameObject("InsanityVFX_Overlay");
            imageGO.transform.SetParent(canvasGO.transform, false);

            var rt          = imageGO.AddComponent<RectTransform>();
            rt.anchorMin    = Vector2.zero;
            rt.anchorMax    = Vector2.one;
            rt.sizeDelta    = Vector2.zero;

            _overlay                = imageGO.AddComponent<Image>();
            _overlay.raycastTarget  = false;
            _overlay.color          = new Color(1f, 0f, 0f, 0f);

            // AssetBundle 머티리얼이 있으면 적용, 없으면 기본 이미지로 동작
            if (AssetBundleLoader.TunnelVisionMaterial != null)
                _overlay.material = AssetBundleLoader.TunnelVisionMaterial;
        }

        public static void UpdateTunnelVision(float insanity)
        {
            if (_overlay == null) return;

            float threshold  = ModConfig.TunnelVisionThreshold.Value;
            float target     = InsanityCalculator.TunnelVisionAlpha(insanity, threshold);
            float current    = _overlay.color.a;
            float newAlpha   = Mathf.Lerp(current, target, Time.deltaTime * 3f);

            // 점멸: 광기가 높을수록 진동 빠르게
            float pulse = 0.8f + 0.2f * Mathf.Sin(Time.time * insanity * 0.1f);
            _overlay.color = new Color(pulse, 0f, 0f, newAlpha);
        }

        public static void ClearEffect()
        {
            if (_overlay == null) return;
            _overlay.color = new Color(1f, 0f, 0f, 0f);
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 3: 커밋**

```
git add Managers/VFXManager.cs
git commit -m "feat: add VFXManager canvas overlay for tunnel vision"
```

---

## Task 7: BloodNightManager.cs

**Files:**
- Create: `Managers/BloodNightManager.cs`

- [ ] **Step 1: BloodNightManager.cs 작성**

```csharp
using WeatherRegistry;

namespace InsanityMod.Managers
{
    internal static class BloodNightManager
    {
        public static bool IsActive { get; private set; }

        private static Weather? _bloodNightWeather;

        public static void Initialize()
        {
            // WeatherRegistry v0.8.8 API
            var effect = new ImprovedWeatherEffect(null, null); // 비주얼은 WeatherRegistry가 처리

            _bloodNightWeather = new Weather(
                LocalizationManager.Get("weather.blood_night"),
                effect
            );

            _bloodNightWeather.Config.DefaultWeight = ModConfig.BloodNightSpawnWeight.Value;

            WeatherManager.RegisterWeather(_bloodNightWeather);

            // 날씨 변경 이벤트 구독
            EventManager.WeatherChanged.AddListener(OnWeatherChanged);
            EventManager.DayChanged.AddListener(OnDayChanged);
        }

        private static void OnWeatherChanged(SelectableLevel level, Weather weather)
        {
            IsActive = _bloodNightWeather != null && weather == _bloodNightWeather;
        }

        private static void OnDayChanged(int day)
        {
            // 새 날이 시작되면 리셋 (행성 이동 시 날씨 재확인은 WeatherChanged가 처리)
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded.`  
빌드 오류 발생 시: WeatherRegistry DLL의 네임스페이스(`WeatherRegistry.Modules` 등) 확인 후 `using` 조정.

- [ ] **Step 3: 커밋**

```
git add Managers/BloodNightManager.cs
git commit -m "feat: add BloodNightManager with WeatherRegistry integration"
```

---

## Task 8: InsanityNetworkHandler.cs

**Files:**
- Create: `Network/InsanityNetworkHandler.cs`

- [ ] **Step 1: InsanityNetworkHandler.cs 작성**

```csharp
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace InsanityMod.Network
{
    internal static class InsanityNetworkHandler
    {
        private const string MSG_SUBMIT   = "InsanityMod.SubmitMaxInsanity";
        private const string MSG_RESULTS  = "InsanityMod.BroadcastResults";
        private const string MSG_AUDIO    = "InsanityMod.PlayAudio";

        public static readonly Dictionary<ulong, float> PlayerMaxInsanity = new();

        public static void RegisterHandlers()
        {
            if (NetworkManager.Singleton == null) return;
            var mgr = NetworkManager.Singleton.CustomMessagingManager;

            if (NetworkManager.Singleton.IsServer)
                mgr.RegisterNamedMessageHandler(MSG_SUBMIT, ReceiveMaxInsanity);

            mgr.RegisterNamedMessageHandler(MSG_RESULTS, ReceiveBroadcastResults);
            mgr.RegisterNamedMessageHandler(MSG_AUDIO,   ReceivePlayAudio);
        }

        public static void UnregisterHandlers()
        {
            if (NetworkManager.Singleton == null) return;
            var mgr = NetworkManager.Singleton.CustomMessagingManager;
            mgr.UnregisterNamedMessageHandler(MSG_SUBMIT);
            mgr.UnregisterNamedMessageHandler(MSG_RESULTS);
            mgr.UnregisterNamedMessageHandler(MSG_AUDIO);
        }

        // 클라이언트 → 호스트: 라운드 최대 광기 전송
        public static void SendMaxInsanity(float maxInsanity)
        {
            if (NetworkManager.Singleton == null) return;
            var writer = new FastBufferWriter(sizeof(float), Allocator.Temp);
            writer.WriteValueSafe(maxInsanity);
            NetworkManager.Singleton.CustomMessagingManager
                .SendNamedMessage(MSG_SUBMIT, NetworkManager.ServerClientId, writer);
            writer.Dispose();
        }

        private static void ReceiveMaxInsanity(ulong senderId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out float val);
            PlayerMaxInsanity[senderId] = val;
        }

        // 호스트 → 전원: 결과 브로드캐스트
        public static void BroadcastResults()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            int    count  = PlayerMaxInsanity.Count;
            int    size   = sizeof(int) + count * (sizeof(ulong) + sizeof(float));
            var    writer = new FastBufferWriter(size, Allocator.Temp);

            writer.WriteValueSafe(count);
            foreach (var kv in PlayerMaxInsanity)
            {
                writer.WriteValueSafe(kv.Key);
                writer.WriteValueSafe(kv.Value);
            }
            NetworkManager.Singleton.CustomMessagingManager
                .SendNamedMessageToAll(MSG_RESULTS, writer);
            writer.Dispose();
        }

        private static void ReceiveBroadcastResults(ulong _, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int count);
            PlayerMaxInsanity.Clear();
            for (int i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out ulong id);
                reader.ReadValueSafe(out float val);
                PlayerMaxInsanity[id] = val;
            }
        }

        // 클라이언트 → 전원: 광기 오디오 재생 요청
        public static void SendPlayInsanityAudio(int clipIndex)
        {
            if (NetworkManager.Singleton == null) return;
            ulong localId = NetworkManager.Singleton.LocalClientId;
            var   writer  = new FastBufferWriter(sizeof(ulong) + sizeof(int), Allocator.Temp);
            writer.WriteValueSafe(localId);
            writer.WriteValueSafe(clipIndex);
            NetworkManager.Singleton.CustomMessagingManager
                .SendNamedMessageToAll(MSG_AUDIO, writer);
            writer.Dispose();
        }

        private static void ReceivePlayAudio(ulong _, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong playerId);
            reader.ReadValueSafe(out int   clipIndex);

            var clips = AssetBundleLoader.InsanityAudioClips;
            if (clipIndex >= clips.Length || clips[clipIndex] == null) return;

            // 해당 플레이어의 AudioSource에서 재생
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.actualClientId != playerId || player.isPlayerDead) continue;
                player.movementAudio.PlayOneShot(clips[clipIndex]);
                break;
            }
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 3: 커밋**

```
git add Network/InsanityNetworkHandler.cs
git commit -m "feat: add InsanityNetworkHandler with CustomMessagingManager"
```

---

## Task 9: CoronerCompat.cs

**Files:**
- Create: `CoronerCompat.cs`

- [ ] **Step 1: CoronerCompat.cs 작성**

```csharp
using System.Collections.Generic;
using GameNetcodeStuff;

namespace InsanityMod
{
    internal static class CoronerCompat
    {
        private static bool _available;

        // Coroner.API 타입은 리플렉션으로 접근 (soft dependency — 컴파일 타임에 없을 수 있음)
        private static object? _chokeCause;
        private static object? _lucidDoomCause;

        // Lucid Doom 사용 후 사망 대기 중인 플레이어
        private static readonly Dictionary<int, bool> _pendingLucidDoom = new();

        public static void Initialize()
        {
            try
            {
                var apiType = System.Type.GetType("Coroner.API, Coroner");
                if (apiType == null) return;

                var registerMethod = apiType.GetMethod("Register",
                    new[] { typeof(string) });

                _chokeCause   = registerMethod?.Invoke(null, new object[] { "insanitymod.choke_bread" });
                _lucidDoomCause = registerMethod?.Invoke(null, new object[] { "insanitymod.lucid_doom_death" });
                _available    = true;

                Plugin.Log.LogInfo("Coroner integration enabled.");
            }
            catch
            {
                _available = false;
            }
        }

        public static void SetChokeCause(PlayerControllerB player)
        {
            if (!_available || _chokeCause == null) return;
            TrySetCause(player, _chokeCause);
        }

        public static void MarkLucidDoomUser(PlayerControllerB player) =>
            _pendingLucidDoom[player.GetInstanceID()] = true;

        public static void ApplyLucidDoomCause(PlayerControllerB player)
        {
            if (!_available || _lucidDoomCause == null) return;
            int id = player.GetInstanceID();
            if (!_pendingLucidDoom.ContainsKey(id)) return;
            _pendingLucidDoom.Remove(id);
            TrySetCause(player, _lucidDoomCause);
        }

        private static void TrySetCause(PlayerControllerB player, object cause)
        {
            try
            {
                var apiType = System.Type.GetType("Coroner.API, Coroner")!;
                var method  = apiType.GetMethod("SetCauseOfDeath",
                    new[] { typeof(PlayerControllerB), cause.GetType() });
                method?.Invoke(null, new[] { player, cause });
            }
            catch { }
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 3: 커밋**

```
git add CoronerCompat.cs
git commit -m "feat: add CoronerCompat soft dependency wrapper"
```

---

## Task 10: ValueBread.cs

**Files:**
- Create: `Items/ValueBread.cs`

- [ ] **Step 1: ValueBread.cs 작성**

```csharp
using GameNetcodeStuff;
using InsanityMod.Managers;
using Unity.Netcode;
using UnityEngine;

namespace InsanityMod.Items
{
    public class ValueBread : GrabbableObject
    {
        // NetworkVariable로 선언해야 모든 클라이언트에 스택 수 동기화됨
        public NetworkVariable<int> StackCount { get; } =
            new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone,
                                        NetworkVariableWritePermission.Server);

        public override void ActivateItem(bool onButtonDown, bool onButtonHeld)
        {
            if (!onButtonDown || !IsOwner) return;

            var player = GameNetworkManager.Instance.localPlayerController;

            // 1. 광기 감소
            InsanityManager.AddInsanity(-ModConfig.BreadInsanityReduction.Value);

            // 2. 질식 판정
            int   uses   = InsanityManager.GetConsecutiveBreadUses();
            float chance = InsanityCalculator.ChokeChance(
                ModConfig.BreadChokeBaseChance.Value,
                ModConfig.BreadChokeStack.Value,
                uses);

            if (Random.value < chance)
            {
                int damage = Mathf.RoundToInt(player.health * 0.2f);
                player.DamagePlayer(damage, hasDamageSFX: true, callRPC: true,
                    causeOfDeath: CauseOfDeath.Suffocation);
                InsanityManager.IncrementBreadUses();
                InsanityManager.IncrementBreadUses();
                CoronerCompat.SetChokeCause(player);
                HUDManager.Instance.DisplayTip(
                    LocalizationManager.Get("item.bread.name"),
                    LocalizationManager.Get("item.bread.choke"),
                    isWarning: true);
            }
            else
            {
                InsanityManager.ResetBreadUses();
            }

            // 3. 스택 소모
            ConsumeOneServingServerRpc();
        }

        // 빵 줍기 시 기존 스택에 합치기 (서버에서 처리)
        public override void GrabItem()
        {
            base.GrabItem();
            if (!IsServer) return;
            TryMergeWithExistingStack();
        }

        private void TryMergeWithExistingStack()
        {
            if (playerHeldBy == null) return;
            foreach (var slot in playerHeldBy.ItemSlots)
            {
                if (slot == null || slot == this) continue;
                if (slot is not ValueBread other) continue;
                if (other.StackCount.Value >= 10) continue;

                // 합치기: 기존 스택에 +1, 현재 오브젝트 제거
                other.StackCount.Value = Mathf.Min(other.StackCount.Value + StackCount.Value, 10);
                GetComponent<NetworkObject>().Despawn(true);
                return;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ConsumeOneServingServerRpc()
        {
            StackCount.Value--;
            if (StackCount.Value <= 0)
                GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 3: 커밋**

```
git add Items/ValueBread.cs
git commit -m "feat: add ValueBread item with choke mechanic"
```

---

## Task 11: LucidDoom.cs

**Files:**
- Create: `Items/LucidDoom.cs`

- [ ] **Step 1: LucidDoom.cs 작성**

```csharp
using GameNetcodeStuff;
using InsanityMod.Managers;
using Unity.Netcode;
using UnityEngine;

namespace InsanityMod.Items
{
    public class LucidDoom : GrabbableObject
    {
        public override void ActivateItem(bool onButtonDown, bool onButtonHeld)
        {
            if (!onButtonDown || !IsOwner) return;

            var player = GameNetworkManager.Instance.localPlayerController;

            // 1. 터널 비전 즉시 제거
            VFXManager.ClearEffect();

            // 2. HP → 1 (이후 어떤 공격이든 즉사)
            int damage = player.health - 1;
            if (damage > 0)
                player.DamagePlayer(damage, hasDamageSFX: false, callRPC: true,
                    causeOfDeath: CauseOfDeath.Unknown);

            // 3. Coroner: 이후 사망 시 원인 적용 대기
            CoronerCompat.MarkLucidDoomUser(player);

            HUDManager.Instance.DisplayTip(
                LocalizationManager.Get("item.potion.name"),
                LocalizationManager.Get("item.potion.use"),
                isWarning: false);

            // 4. 아이템 소비
            ConsumeServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ConsumeServerRpc()
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 3: 커밋**

```
git add Items/LucidDoom.cs
git commit -m "feat: add LucidDoom item"
```

---

## Task 12: Patches — PlayerPatcher, ItemRegistrationPatcher, RoundPatcher

**Files:**
- Create: `Patches/PlayerPatcher.cs`
- Create: `Patches/RoundPatcher.cs`
- Create: `Patches/ItemRegistrationPatcher.cs`

- [ ] **Step 1: PlayerPatcher.cs 작성**

```csharp
using GameNetcodeStuff;
using HarmonyLib;
using InsanityMod.Managers;
using InsanityMod.Network;
using UnityEngine;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class PlayerPatcher
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(PlayerControllerB __instance)
        {
            // 로컬 플레이어 + 살아있을 때만 처리
            if (__instance != GameNetworkManager.Instance.localPlayerController) return;
            if (__instance.isPlayerDead) return;

            InsanityManager.Tick(__instance, Time.deltaTime);
        }

        // 플레이어 사망 시 Coroner 적용
        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        private static void KillPlayerPrefix(PlayerControllerB __instance)
        {
            CoronerCompat.ApplyLucidDoomCause(__instance);
        }
    }
}
```

- [ ] **Step 2: RoundPatcher.cs 작성**

```csharp
using HarmonyLib;
using InsanityMod.Managers;
using InsanityMod.Network;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class RoundPatcher
    {
        // 라운드 시작: 광기 리셋 + 네트워크 핸들러 등록
        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        private static void StartGamePostfix()
        {
            InsanityManager.ResetForRound();
            InsanityNetworkHandler.PlayerMaxInsanity.Clear();
            InsanityNetworkHandler.RegisterHandlers();
        }

        // 라운드 종료: 최대 광기 전송
        [HarmonyPatch("ShipLeave")]
        [HarmonyPostfix]
        private static void ShipLeavePostfix()
        {
            InsanityNetworkHandler.SendMaxInsanity(InsanityManager.MaxInsanityThisRound);
        }

        // 결과창 표시 직전: 호스트가 결과 브로드캐스트
        [HarmonyPatch("EndGameClientRpc")]
        [HarmonyPostfix]
        private static void EndGamePostfix()
        {
            InsanityNetworkHandler.BroadcastResults();
            InsanityNetworkHandler.UnregisterHandlers();
        }
    }
}
```

- [ ] **Step 3: ItemRegistrationPatcher.cs 작성 — 아이템을 LC 시스템에 등록**

```csharp
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class ItemRegistrationPatcher
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void RegisterItems(StartOfRound __instance)
        {
            if (AssetBundleLoader.ValueBreadItem == null ||
                AssetBundleLoader.LucidDoomItem  == null) return;

            var list = new List<Item>(__instance.allItemsList.itemsList)
            {
                AssetBundleLoader.ValueBreadItem,
                AssetBundleLoader.LucidDoomItem
            };
            __instance.allItemsList.itemsList = list.ToArray();

            RegisterShopItem(AssetBundleLoader.ValueBreadItem, ModConfig.BreadShopPrice.Value);
            RegisterShopItem(AssetBundleLoader.LucidDoomItem,  ModConfig.PotionShopPrice.Value);
        }

        private static void RegisterShopItem(Item item, int price)
        {
            // Terminal에 구매 노드 추가
            var terminal = Object.FindObjectOfType<Terminal>();
            if (terminal == null) return;

            var buyNode = ScriptableObject.CreateInstance<TerminalNode>();
            buyNode.displayText      = $"구매: {item.itemName}\n가격: {price} 크레딧.\n\n[CONFIRM]";
            buyNode.clearPreviousText = true;
            buyNode.buyItemIndex      = item.itemId;

            var keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            keyword.word       = item.itemName.ToLower().Replace(" ", "");
            keyword.specialKeywordResult = buyNode;

            var keywordList = new List<TerminalKeyword>(terminal.terminalNodes.allKeywords) { keyword };
            terminal.terminalNodes.allKeywords = keywordList.ToArray();
        }
    }
}
```

- [ ] **Step 4: RoundResultsPatcher.cs 작성 — 결과창 광기 표시**

```csharp
using HarmonyLib;
using InsanityMod.Managers;
using InsanityMod.Network;
using TMPro;
using UnityEngine;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal static class RoundResultsPatcher
    {
        [HarmonyPatch("ApplyPenalty")]
        [HarmonyPostfix]
        private static void ShowInsanityStats(HUDManager __instance)
        {
            if (InsanityNetworkHandler.PlayerMaxInsanity.Count == 0) return;

            // 결과창 컨테이너 찾기 (LC V80+ UI 구조)
            var statsContainer = __instance.statsUI?.transform;
            if (statsContainer == null) return;

            string header = LocalizationManager.Get("hud.max_insanity");
            var lineGO    = new GameObject("InsanityStats");
            lineGO.transform.SetParent(statsContainer, false);

            var text           = lineGO.AddComponent<TextMeshProUGUI>();
            text.fontSize      = 14f;
            text.alignment     = TextAlignmentOptions.Left;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"<b>— {header} —</b>");

            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                ulong id = player.actualClientId;
                if (!InsanityNetworkHandler.PlayerMaxInsanity.TryGetValue(id, out float max)) continue;

                string name   = player.playerUsername;
                string pct    = $"{max:F0}%";
                string color  = max >= 100f ? "<color=#FF4444>" : "<color=#DDDDDD>";
                sb.AppendLine($"{color}{name}  {pct}</color>");
            }

            text.text = sb.ToString();
        }
    }
}
```

- [ ] **Step 5: LucidDoom 시설 랜덤 스폰 패치 추가 (RoundPatcher에 추가)**

```csharp
// RoundPatcher.cs 에 메서드 추가
[HarmonyPatch(typeof(RoundManager))]
internal static class LucidDoomSpawnPatcher
{
    [HarmonyPatch("LoadNewLevelWait")]
    [HarmonyPostfix]
    private static System.Collections.IEnumerator SpawnLucidDooms(
        System.Collections.IEnumerator __result)
    {
        yield return __result; // 원본 코루틴 완료 대기

        if (!NetworkManager.Singleton.IsServer) yield break;
        if (AssetBundleLoader.LucidDoomPrefab == null) yield break;

        var spawnPoints = Object.FindObjectsOfType<RandomScrapSpawn>();
        if (spawnPoints.Length == 0) yield break;

        int count = ModConfig.PotionFacilitySpawnCount.Value;
        for (int i = 0; i < count; i++)
        {
            var point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            var go    = Object.Instantiate(
                AssetBundleLoader.LucidDoomPrefab,
                point.transform.position,
                UnityEngine.Random.rotation);
            go.GetComponent<Unity.Netcode.NetworkObject>().Spawn(destroyWithScene: true);
        }
    }
}
```

- [ ] **Step 6: 빌드 확인**

```
dotnet build InsanityMod.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 7: 커밋**

```
git add Patches/PlayerPatcher.cs Patches/RoundPatcher.cs Patches/ItemRegistrationPatcher.cs
git commit -m "feat: add Harmony patches for player update, round lifecycle, item registration, and results screen"
```

---

## Task 13: Unity AssetBundle 제작

**Prerequisites:** Unity 2022.3 LTS 설치, LC Modding SDK (LethalSDK 또는 직접 구성)

**Files to create in Unity project:**
- `Assets/InsanityMod/Materials/TunnelVisionMat.mat`
- `Assets/InsanityMod/Audio/insanity_breath_lo.wav`
- `Assets/InsanityMod/Audio/insanity_mutter.wav`
- `Assets/InsanityMod/Audio/insanity_breath_hi.wav`
- `Assets/InsanityMod/Prefabs/ValueBreadPrefab.prefab`
- `Assets/InsanityMod/Prefabs/LucidDoomPrefab.prefab`
- `Assets/InsanityMod/ScriptableObjects/ValueBreadItem.asset`
- `Assets/InsanityMod/ScriptableObjects/LucidDoomItem.asset`

- [ ] **Step 1: Unity 프로젝트 생성**

Unity Hub → New Project → 3D (URP 아님, Built-in) → 이름: `InsanityModAssets`  
Unity 버전: **2022.3.9f1** (LC V80과 동일 버전 사용할 것)

- [ ] **Step 2: LC 게임 DLL을 Unity 프로젝트에 추가**

```
Packages/manifest.json 에 추가:
Unity 프로젝트의 Assets/Plugins/ 폴더에 복사:
  C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll
  Unity.Netcode.Runtime.dll
```

- [ ] **Step 3: TunnelVisionMat 머티리얼 제작**

1. `Assets/InsanityMod/Materials/` 폴더 생성
2. Create → Material → 이름: `TunnelVisionMat`
3. Shader: `UI/Default` (또는 `Sprites/Default`)
4. Rendering Mode: `Transparent`
5. Color: `(1, 0, 0, 0)` (기본 투명)
6. Texture: 방사형 그라디언트 텍스처 — Photoshop/GIMP에서 512×512 PNG 제작:
   - 중앙: 완전 투명 (alpha=0)
   - 테두리: 완전 불투명 흰색 (alpha=255)
   - 저장: `Assets/InsanityMod/Textures/radial_vignette.png`
7. 머티리얼의 Texture 슬롯에 `radial_vignette.png` 연결

- [ ] **Step 4: 오디오 클립 준비**

무료 공포 오디오 (FreeSound.org 등에서 CC0 라이선스 다운로드):
- `insanity_breath_lo.wav` — 낮은 숨소리 (2~3초)
- `insanity_mutter.wav` — 중얼거림 (2~3초)
- `insanity_breath_hi.wav` — 빠른 숨소리 (2~3초)

`Assets/InsanityMod/Audio/` 에 임포트. Import Settings: Load Type = `Decompress On Load`, Compression = `Vorbis`

- [ ] **Step 5: ValueBread 프리팹 제작**

1. LC 게임에서 비슷한 아이템의 프리팹 구조를 참고 (알약류 아이템)
2. Unity에서 빈 GameObject 생성 → 이름: `ValueBreadPrefab`
3. 컴포넌트 추가:
   - `NetworkObject` (Unity Netcode)
   - `ValueBread` (InsanityMod.Items.ValueBread — 스크립트 직접 임포트)
   - `MeshFilter` + `MeshRenderer` (빵 모양 단순 메쉬 또는 임시 Cube)
   - `AudioSource` (3D, Spatial Blend=1.0, Rolloff=Logarithmic, MaxDistance=10)
   - `BoxCollider` (trigger=false)
4. `ValueBread` 컴포넌트에서 `GrabbableObject` 설정 확인
5. 프리팹으로 저장: `Assets/InsanityMod/Prefabs/ValueBreadPrefab.prefab`

- [ ] **Step 6: LucidDoom 프리팹 제작**

ValueBread와 동일 구조, 컴포넌트를 `LucidDoom`으로 변경.  
붉은 유리병 모양 메쉬 (또는 임시 Capsule).  
저장: `Assets/InsanityMod/Prefabs/LucidDoomPrefab.prefab`

- [ ] **Step 7: Item ScriptableObject 생성**

LC의 `Item` 타입은 `Assembly-CSharp.dll` 내 `ScriptableObject`. Unity에서 직접 생성:

```
Create → ScriptableObject → Item (검색)
```

ValueBreadItem 설정:
```
itemName: "Cheap Bread" (EN) / "50원짜리 빵" (KO) → "ValueBread"로 통일
spawnPrefab: ValueBreadPrefab
weight: 0.5 (가벼움)
itemId: 900001 (임의 고유 ID, 충돌 없을 것)
twoHanded: false
requiresBattery: false
```

LucidDoomItem 설정:
```
itemName: "LucidDoom"
spawnPrefab: LucidDoomPrefab
weight: 0.3
itemId: 900002
twoHanded: false
```

- [ ] **Step 8: AssetBundle 라벨 지정 및 빌드**

1. 각 에셋 선택 → Inspector 하단 `AssetBundle` 드롭다운 → `insanitymod`로 설정:
   - `TunnelVisionMat`
   - 3개 AudioClip
   - `ValueBreadPrefab`, `LucidDoomPrefab`
   - `ValueBreadItem`, `LucidDoomItem`

2. `Editor/BuildAssetBundles.cs` 생성:
```csharp
using UnityEditor;
using System.IO;

public class BuildAssetBundles
{
    [MenuItem("InsanityMod/Build AssetBundle")]
    static void Build()
    {
        string outputPath = "Assets/../AssetBundles";
        Directory.CreateDirectory(outputPath);
        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64);
        UnityEngine.Debug.Log("AssetBundle built.");
    }
}
```

3. Unity 메뉴 → InsanityMod → Build AssetBundle
4. 빌드된 `insanitymod.bundle` 을 BepInEx 프로젝트로 복사:
```
copy AssetBundles\insanitymod C:\Users\yeokyoomin\Downloads\insanity\Assets\insanitymod.bundle
```

- [ ] **Step 9: 커밋**

```
git add Assets/insanitymod.bundle
git commit -m "feat: add Unity AssetBundle with items, materials, and audio"
```

---

## Task 14: 최종 통합 & 인게임 테스트

**Files:**
- 수정 없음 (빌드 + 검증)

- [ ] **Step 1: 전체 빌드**

```
dotnet build InsanityMod.csproj -c Release
```
Expected: `Build succeeded.` — `DEPLOY_DIR`에 `InsanityMod.dll`, `Langs.json`, `insanitymod.bundle` 자동 복사됨

- [ ] **Step 2: 테스트 전체 실행**

```
dotnet test InsanityMod.Tests/InsanityMod.Tests.csproj
```
Expected: `12 passed, 0 failed`

- [ ] **Step 3: 인게임 테스트 체크리스트**

Thunderstore Mod Manager에서 "mods" 프로파일로 LC 실행.  
BepInEx 콘솔에서 확인:
```
[InsanityMod] InsanityMod v1.0.0 loaded.
[InsanityMod] Localization loaded: KO (6 strings)
[InsanityMod] AssetBundle loaded successfully.
```

인게임 시나리오:

| 시나리오 | 예상 결과 |
|----------|-----------|
| 시설 입장 | 광기 서서히 증가 |
| 야외 이동 | 광기 서서히 감소 |
| 함선에서 대기 | 광기 느리게 증가 (잠수 방지) |
| 광기 80% 이상 | 화면 테두리 붉게 점멸 |
| 빵 연속 섭취 | 3회쯤에서 질식 + HP 감소 + "빵에 걸렸다!" 팁 |
| 물약 사용 | 빨간 화면 즉시 제거, HP 1로 감소, 이후 즉사 확인 |
| 라운드 종료 | 결과창에 "최대 광기: XX%" 표시 |
| 팀원 광기 50%↑ | 가까이 있으면 팀원 숨소리 들림 |
| 피의 밤 날씨 | 날씨 목록에 "피의 밤" 표시, 시설 내 광기 1.2배 |

- [ ] **Step 4: 오류 수정 커밋**

인게임 테스트에서 발견된 오류 수정 후:
```
git add .
git commit -m "fix: in-game test fixes"
```

- [ ] **Step 5: 최종 커밋**

```
git tag v1.0.0
git commit -m "feat: InsanityMod v1.0.0 complete"
```

---

## 빠른 참조 — WeatherRegistry API (v0.8.8)

```csharp
// 등록
var effect  = new ImprovedWeatherEffect(effectGO, worldGO);
var weather = new Weather("Blood Night", effect);
weather.Config.DefaultWeight = 1;
WeatherManager.RegisterWeather(weather);

// 이벤트
EventManager.WeatherChanged.AddListener((SelectableLevel level, Weather w) => { ... });

// 현재 날씨 확인
var current = WeatherManager.GetCurrentLevelWeather();
bool isBloodNight = current == _bloodNightWeather;
```

## 빠른 참조 — LC V80+ 주요 API

```csharp
player.isInsideFactory          // 시설 내부
player.isInHangarShipRoom       // 함선 내부
player.DamagePlayer(int, ...)   // 데미지 (자동 네트워크 동기화)
player.movementAudio            // 3D AudioSource
player.actualClientId           // Netcode 클라이언트 ID
StartOfRound.Instance.allPlayerScripts  // 전체 플레이어 배열
NetworkManager.Singleton.CustomMessagingManager  // RPC 대안
```
