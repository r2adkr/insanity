# Insanity Mod — 설계 문서

**날짜:** 2026-05-04  
**대상 게임:** Lethal Company V80-81  
**프레임워크:** BepInEx 5.4.23.5+  
**의존성:** WeatherRegistry (mrov), Coroner (EliteMasterEric, optional)

---

## 1. 프로젝트 개요

플레이어에게 선택의 권한을 주되 치명적인 책임을 묻는 광기(Insanity) 시스템 모드.  
함선 잠수 메타를 방지하고 아이템을 전략적으로 사용하게 만드는 것이 핵심.  
클라이언트 사이드 광기 추적 + 필요한 게임 상태 변화(HP 등)는 기존 LC 네트워크 시스템 활용.

---

## 2. 폴더 구조

```
InsanityMod/
├── InsanityMod.csproj
├── Plugin.cs                        # BepInEx 엔트리, Config 초기화, Manager 등록
├── Config.cs                        # 모든 ConfigEntry 선언
├── Managers/
│   ├── InsanityManager.cs           # 광기 수치 계산, 위치별 로직, 오디오 트리거
│   ├── BloodNightManager.cs         # WeatherRegistry 등록 및 이벤트 처리
│   ├── VFXManager.cs                # 터널 비전 Canvas 오버레이
│   └── LocalizationManager.cs      # Langs.json KO/EN 로드 및 조회
├── Patches/
│   ├── PlayerPatcher.cs             # Update 훅, 위치 감지 (시설/야외/함선)
│   ├── RoundResultsPatcher.cs       # 결과창 최대 광기 수치 표시 패치
│   └── ItemPatcher.cs               # ActivateItem 훅 (빵, 물약)
├── Items/
│   ├── ValueBread.cs                # 빵 GrabbableObject 구현
│   └── LucidDoom.cs                 # 빨간 물약 GrabbableObject 구현
├── Network/
│   └── InsanityNetworkHandler.cs    # 최대 광기 수치 ServerRpc/ClientRpc 동기화
├── Assets/
│   └── insanitymod.bundle           # Unity AssetBundle (아이템 프리팹, VFX 머티리얼)
└── Resources/
    └── Langs.json                   # 다국어 텍스트 리소스
```

---

## 3. 핵심 시스템: InsanityManager

### 광기 수치 규칙

- 범위: `0f ~ 100f` (float, 퍼센트 단위)
- 클라이언트 사이드 전용 (타인에게 수치 자체는 전송하지 않음)
- `maxInsanityThisRound`: 라운드 중 도달한 최대값 추적, 종료 시 공유

### 위치 감지 (LC V80+ PlayerControllerB 필드)

- 시설 내부: `player.isInsideFactory == true`
- 함선 내부: `player.isInHangarShipRoom == true`
- 야외: 위 둘 다 false

### 매 틱(Update) 계산 로직

```
if (player.isInsideFactory)
    delta = +Config.InsanityRateInFacility * BloodNightMultiplier * deltaTime
else if (!player.isInHangarShipRoom)   // 야외
    delta = -Config.InsanityDecayOutdoor * deltaTime
else                                    // 함선
    delta = +Config.InsanityRateOnShip * deltaTime   // 잠수 방지, 시설보다 낮음

insanity = Clamp(insanity + delta, 0f, 100f)
maxInsanityThisRound = Max(maxInsanityThisRound, insanity)
```

### 팀원 광기 오디오 단서 (네트워크 동기화)

`insanity >= Config.InsanityAudioThreshold` (기본 50%) 조건 충족 시 트리거.  
광기 수치 구간별로 타인에게 3D AudioSource 사운드 재생 (ClientRpc):

| 구간 | 효과 |
|------|------|
| 50~70% | 불규칙한 숨소리 (낮고 조용하게, 낮은 빈도) |
| 70~90% | 알아듣기 힘든 중얼거림 (중간 빈도) |
| 90~100% | 숨소리 + 중얼거림 동시, 높은 빈도 |

- 3D AudioSource → 거리 기반 자연 감쇠, 가까운 팀원만 들림
- 신호 전송: `PlayInsanityAudioServerRpc(playerId, clipIndex)` → 호스트 → 전원 ClientRpc

---

## 4. 피의 밤 날씨 (BloodNightManager)

### WeatherRegistry 등록

```csharp
// Plugin.Awake()에서 호출
WeatherRegistry.API.RegisterWeather(new WeatherDefinition {
    Name = LocalizationManager.Get("weather.blood_night"),
    Color = new Color(0.8f, 0.1f, 0.1f),
    SpawnWeight = Config.BloodNightSpawnWeight
});
```

### 이벤트 훅

```
WeatherRegistry.OnWeatherChanged += (weather) => {
    BloodNightManager.IsActive = weather.Name == "Blood Night";
    InsanityManager.CurrentMultiplier = IsActive ? Config.BloodNightMultiplier : 1.0f;
}
```

날씨 비주얼(붉은 안개 등)은 WeatherRegistry가 처리. 모드는 광기 배율만 관여.

---

## 5. 터널 비전 VFX (VFXManager)

Unity UI Canvas 오버레이 방식 (별도 포스트프로세싱 없음):

```
씬 로드 시:
  Canvas (Screen Space - Overlay, SortOrder 999) 생성
  Image 컴포넌트: 전체화면, 방사형 그라디언트 머티리얼 (중앙 투명 → 테두리 붉은색)
  기본 alpha = 0, DontDestroyOnLoad

매 프레임:
  if (insanity >= Config.TunnelVisionThreshold)
      targetAlpha = (insanity - threshold) / (100 - threshold)
      image.color.a = Lerp(현재, targetAlpha, Time.deltaTime * 3f)
      점멸: brightness *= 0.8f + 0.2f * Mathf.Sin(Time.time * insanity * 0.1f)
  else
      image.color.a = Lerp(현재, 0f, Time.deltaTime * 3f)

ClearEffect():
  image.color.a = 0f  // 물약 사용 시 즉시 초기화
```

머티리얼은 Unity AssetBundle에 포함 (빵/물약 에셋 작업 시 동시 제작).

---

## 6. 아이템

### ValueBread (50원짜리 빵)

**기본 정보**
- 상점 판매 가격: `Config.BreadShopPrice` (기본 10크레딧)
- 인벤토리 스택: 슬롯 1개에 최대 10개

**ActivateItem() 로직**

```
1. InsanityManager.AddInsanity(-Config.BreadInsanityReduction)  // -15%
2. 질식 판정:
     currentChance = Config.BreadChokeBaseChance + (consecutiveUses * Config.BreadChokeStack)
     if (Random.value < currentChance)
         playerController.DamagePlayer(Mathf.RoundToInt(playerController.health * 0.2f))
         Coroner (설치된 경우): SetCauseOfDeath("insanitymod.choke_bread")
         consecutiveUses++
     else
         consecutiveUses = 0
3. stackCount--
   if (stackCount <= 0) Destroy(gameObject)
```

**스택 합치기**
- 같은 아이템 줍기 시 기존 스택 stackCount++ (최대 10), 새 오브젝트 Destroy

---

### LucidDoom (빨간 물약)

**기본 정보**
- 상점 판매 가격: `Config.PotionShopPrice` (기본 80크레딧)
- 시설 내 랜덤 스폰: 라운드당 `Config.PotionFacilitySpawnCount`개 (기본 2개)

**ActivateItem() 로직**

```
1. VFXManager.ClearEffect()                    // 터널 비전 즉시 제거
2. int dmg = playerController.health - 1
   playerController.DamagePlayer(dmg)          // HP → 1 (이후 어떤 공격이든 즉사)
3. InsanityManager 수치 유지 (초기화 없음)      // 광기는 계속 상승
4. Coroner (설치된 경우): 이후 사망 시 "insanitymod.lucid_doom_death" 등록
5. Destroy(gameObject)
```

**랜덤 스폰 (호스트만 실행)**

```
RoundManager.OnLevelLoaded 이벤트 →
  시설 내 랜덤 스폰 포인트 N개 선택
  SpawnItemServerRpc(LucidDoomPrefab, position)  // 모든 클라이언트에 동기화
```

---

## 7. 네트워크: InsanityNetworkHandler

라운드 종료 시 최대 광기 수치 공유:

```
클라이언트 → SubmitMaxInsanityServerRpc(float maxInsanity)
호스트     → 전원 maxInsanity 수집 후 BroadcastInsanityResultsClientRpc(PlayerInsanityResult[])
전원       → 결과 데이터 로컬 저장 (결과창 패치에서 참조)
```

광기 오디오 트리거:
```
클라이언트 → PlayInsanityAudioServerRpc(ulong playerId, int clipIndex)
호스트     → PlayInsanityAudioClientRpc(ulong playerId, int clipIndex)
전원       → 해당 플레이어 위치에서 AudioSource.PlayOneShot(clips[clipIndex])
```

---

## 8. 결과창

### Coroner 연동 (optional 의존성)

```csharp
[BepInDependency("EliteMasterEric.Coroner", BepInDependency.DependencyFlags.SoftDependency)]
```

등록할 사망 원인:
| 키 | 상황 |
|----|------|
| `insanitymod.choke_bread` | 빵 질식으로 사망 |
| `insanitymod.lucid_doom_death` | 물약 복용 후 HP 1 상태로 사망 |

### 자체 HUD 패치 (RoundResultsPatcher)

결과창 플레이어 목록 하단에 광기 통계 행 추가:
```
[플레이어명]  최대 광기: 73%
```
- 100% 도달자: 붉은 색상 강조
- 데이터 소스: `InsanityNetworkHandler`가 수집한 `PlayerInsanityResult[]`

---

## 9. 다국어 (Langs.json)

```json
{
  "KO": {
    "item.bread.name": "50원짜리 빵",
    "item.bread.choke": "빵에 걸렸다!",
    "item.potion.name": "루시드 둠",
    "item.potion.use": "시야가 맑아졌지만...",
    "hud.max_insanity": "최대 광기",
    "weather.blood_night": "피의 밤"
  },
  "EN": {
    "item.bread.name": "Cheap Bread",
    "item.bread.choke": "Choking on Bread!",
    "item.potion.name": "Lucid Doom",
    "item.potion.use": "Vision cleared, but...",
    "hud.max_insanity": "Peak Insanity",
    "weather.blood_night": "Blood Night"
  }
}
```

`GameNetworkManager.preferredLang` 감지 → KO/EN 자동 선택, 미지원 언어 시 EN 폴백.

---

## 10. Config 전체 목록

| 키 | 기본값 | 설명 |
|----|--------|------|
| `InsanityRateInFacility` | 0.5f | 시설 내 광기 상승/초 |
| `InsanityRateOnShip` | 0.1f | 함선 내 광기 상승/초 |
| `InsanityDecayOutdoor` | 0.8f | 야외 광기 감소/초 |
| `BloodNightMultiplier` | 1.2f | 피의 밤 광기 배율 |
| `BloodNightSpawnWeight` | 1 | 피의 밤 날씨 발생 가중치 |
| `BreadInsanityReduction` | 15f | 빵 광기 감소% |
| `BreadChokeBaseChance` | 0.2f | 빵 기본 질식 확률 |
| `BreadChokeStack` | 0.05f | 연속 섭취 질식 확률 누적치 |
| `BreadShopPrice` | 10 | 빵 상점 가격 (크레딧) |
| `PotionShopPrice` | 80 | 물약 상점 가격 (크레딧) |
| `PotionFacilitySpawnCount` | 2 | 라운드당 시설 내 물약 스폰 수 |
| `TunnelVisionThreshold` | 80f | 터널 비전 발동 광기% |
| `InsanityAudioThreshold` | 50f | 광기 오디오 발동 광기% |

---

## 11. 레퍼런스 경로

- 게임 DLL: `C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\`
- WeatherRegistry DLL: `...\plugins\mrov-WeatherRegistry\WeatherRegistry.dll`
- MrovLib DLL: `...\plugins\mrov-MrovLib\MrovLib.dll`
- BepInEx core: `...\BepInEx\core\`
