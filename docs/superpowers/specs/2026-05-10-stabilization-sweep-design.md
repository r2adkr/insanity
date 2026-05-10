# InsanityMod Stabilization & Targeted Optimization Sweep

**Date:** 2026-05-10
**Status:** Approved
**Scope:** Defensive sweep across the mod with targeted fixes for identified hot spots. No behavior changes for end users.

## Goal

Improve the mod's resilience without altering player-facing behavior:

- **Crash prevention** — wrap every Harmony postfix so a single thrown exception does not break gameplay for the whole session
- **Memory leak prevention** — clean up `DontDestroyOnLoad` GameObjects and stale state at session boundaries
- **Performance** — fix the one path that can degrade into a per-frame `FindObjectsOfType` call

## Approach

Approach **B (chosen)**: introduce one `SafePatch` helper plus targeted fixes for specific identified issues. DRY, consistent logging, and minimal boilerplate at each call site.

## Identified Issues

| # | Issue | File | Severity |
|---|-------|------|----------|
| 1 | Harmony postfix bodies have no exception handling | All `Patches/*.cs` (8 patchers) | High — one bad frame can disable the patch silently or cascade |
| 2 | `RoundResultsPatcher.ShowInsanityStats` appends text on every `ApplyPenalty` call (no idempotency) | `Patches/RoundResultsPatcher.cs` | Med — duplicate text if `ApplyPenalty` fires twice |
| 3 | `VoiceHaunt.PlayHauntClip` GameObjects use `DontDestroyOnLoad` but aren't tracked for cleanup if round ends early | `Voice/VoiceHaunt.cs` | Med — lingering audio sources across scenes |
| 4 | `BloodNightManager.GetSun()` fallback runs `FindObjectsOfType<Light>()` every frame when `_cachedSun` is null/missing | `Managers/BloodNightManager.cs` | Med — per-frame allocation under specific timing |
| 5 | `BloodNightManager._paranoiaLevels` HashSet retains entries across game sessions | `Managers/BloodNightManager.cs` | Low — stale references to old `SelectableLevel` objects |
| 6 | `InsanityNetworkHandler` message handlers don't catch exceptions | `Network/InsanityNetworkHandler.cs` | Med — a malformed message could crash the handler |
| 7 | `InsanityHud overlay created.` log fires on every level load | `Managers/InsanityHud.cs` | Low — log spam |

Excluded by YAGNI: physics linecast mask tuning in `MobVisibilityBonus` (no measured perf issue), new abstractions, integration test framework.

## Design

### SafePatch helper (`Patches/SafePatch.cs`, new file)

```csharp
using System;
using System.Collections.Generic;

namespace InsanityMod.Patches
{
    internal static class SafePatch
    {
        private static readonly HashSet<string> _loggedKeys = new();

        public static void Run(string label, Action body)
        {
            try { body(); }
            catch (Exception ex)
            {
                string key = $"{label}:{ex.GetType().FullName}:{ex.Message}";
                lock (_loggedKeys)
                {
                    if (!_loggedKeys.Add(key)) return;
                }
                Plugin.Log.LogError($"[{label}] {ex}");
            }
        }
    }
}
```

**Behavior:** Catches any exception thrown by `body`, logs it once per unique `(label, exception type, exception message)` combination per session. Subsequent identical exceptions are silently swallowed (no log spam).

The `lock` on `_loggedKeys` is to be safe — `InsanityNetworkHandler` callbacks can fire from message threads (Netcode) and audio capture postfix runs on the audio thread.

### Patch call site shape

Existing pattern (vulnerable):

```csharp
[HarmonyPostfix]
private static void StartGamePostfix()
{
    InsanityManager.StartRound();
    // ...more calls
}
```

New pattern:

```csharp
[HarmonyPostfix]
private static void StartGamePostfix() => SafePatch.Run(nameof(StartGamePostfix), () =>
{
    InsanityManager.StartRound();
    // ...more calls
});
```

Applied to every postfix (and prefix if any) in:

- `Patches/RoundPatcher.cs`
- `Patches/LevelGenerationPatcher.cs`
- `Patches/PlayerPatcher.cs`
- `Patches/RoundResultsPatcher.cs`
- `Patches/DeathWitnessPatcher.cs`
- `Patches/GhostGirlBoostPatcher.cs`
- `Patches/LungPropPatcher.cs`
- `Voice/VoiceCapturePatcher.cs`

### Targeted fixes

**Fix #2 — RoundResultsPatcher idempotency**

Add a flag tracking whether stats have been appended this round:

```csharp
private static bool _appendedThisRound;

[HarmonyPostfix]
private static void ShowInsanityStats(HUDManager __instance) => SafePatch.Run(..., () =>
{
    if (_appendedThisRound) return;
    // ...append logic
    _appendedThisRound = true;
});

public static void Reset() => _appendedThisRound = false;
```

Call `RoundResultsPatcher.Reset()` from `RoundPatcher.StartGamePostfix`.

**Fix #3 — VoiceHaunt clip cleanup**

Track spawned haunt GameObjects in a list. Clean them up in `ResetForRound`:

```csharp
private static readonly List<GameObject> _activeClips = new();

private static void PlayHauntClip(...)
{
    var go = new GameObject("InsanityHauntPlayer");
    UnityEngine.Object.DontDestroyOnLoad(go);
    _activeClips.Add(go);
    // ...
    UnityEngine.Object.Destroy(go, samples.Length / (float)SampleRate + 1f);
}

public static void ResetForRound()
{
    // ...existing
    for (int i = _activeClips.Count - 1; i >= 0; i--)
    {
        if (_activeClips[i] != null) UnityEngine.Object.Destroy(_activeClips[i]);
    }
    _activeClips.Clear();
}
```

**Fix #4 — GetSun() throttling**

Add a timestamp:

```csharp
private static float _lastSunSearch = -100f;
private const float SunSearchCooldown = 5f;

private static Light? GetSun()
{
    if (_cachedSun != null) return _cachedSun;
    if (RenderSettings.sun != null) return _cachedSun = RenderSettings.sun;

    if (Time.time - _lastSunSearch < SunSearchCooldown) return null;
    _lastSunSearch = Time.time;

    foreach (var l in Object.FindObjectsOfType<Light>())
        if (l.type == LightType.Directional) return _cachedSun = l;
    return null;
}
```

**Fix #5 — `_paranoiaLevels` cleanup**

Hook into `Plugin.OnDestroy` (BepInEx tears the plugin down on game exit). Add public `BloodNightManager.OnPluginDestroy()` that clears the HashSet, and call it from `Plugin.OnDestroy`.

This is conservative — running this once at game exit prevents leaks across long Steam sessions where the player launches/exits LC multiple times without restarting Steam.

**Fix #6 — Network handler safety**

Wrap each `Receive*` method body in `SafePatch.Run`.

**Fix #7 — Remove log spam**

Delete the `Plugin.Log.LogInfo("InsanityHud overlay created.")` line in `InsanityHud.cs:112`.

## Error Handling Behavior

When a patch body throws:
1. The exception is caught by `SafePatch.Run`
2. A single `LogError` is emitted with full stack trace, prefixed by the patch name
3. Subsequent identical exceptions in the same session are silently swallowed
4. The Harmony patch returns normally, so the original game method's execution is unaffected (postfixes only)

## Testing

- **Build:** `dotnet build InsanityMod.csproj -c Release` → 0 errors
- **Manual:** Run one normal round (host a lobby, land on a moon, complete a round). Verify no regressions:
  - Insanity HUD appears and updates
  - Tunnel vision triggers at threshold
  - End-of-round stats display correctly (and not duplicated)
  - Paranoia visuals work as before
  - Voice haunt fires when above threshold
- **No formal unit tests** for the wrappers themselves — they're trivial and side-effecting.

## Out of Scope

- Touch the actual logic of any patched method (the wrappers preserve all existing behavior)
- Add new features
- Refactor file structure
- Replace any third-party calls

## Files Touched

- **New:** `Patches/SafePatch.cs`
- **Modified:** All 8 patcher files; `Voice/VoiceHaunt.cs`; `Managers/BloodNightManager.cs`; `Managers/InsanityHud.cs`; `Network/InsanityNetworkHandler.cs`; `Plugin.cs`
