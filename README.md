# InsanityMod

A Lethal Company mod that tracks player sanity and makes high-insanity runs genuinely terrifying.

**Game version:** V80–81 | **BepInEx:** 5.4.23.5+

---

## Dependencies

- [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry/) — required for Paranoia weather

---

## What it does

### Insanity meter

Insanity (0–100%) accumulates while you're inside the facility and drains while you're outdoors during the day. On the ship it stays flat.

- **Solo in the facility:** ~10 minutes to 100%
- **Outdoors (daytime):** slowly decays back toward 0
- **Outdoors (night / Eclipse):** insanity rises instead of falling
- **Resets each round** — every new expedition starts fresh

### Things that make it worse

| Trigger | Effect |
|---------|--------|
| Visible enemy nearby | +rate per second (scales by enemy type) |
| Watching a teammate die | instant spike |
| Paranoia weather | rate multiplier |
| Night outdoors | slow rise instead of decay |
| Eclipse weather outdoors | faster rise than night |

Enemy threat scale (examples): Bracken / Ghost Girl = 2.0×, Jester / Coilhead = 1.5×, Forest Giant / Masked = 1.4×, Sand Worm = 1.8×, Thumper = 0.8×

### Things that make it better

| Condition | Effect |
|-----------|--------|
| Teammate within 6m | −0.15/s |
| Flashlight on / in ship | −0.1/s |
| Near a facility light | −0.1/s |

Being with a teammate near a light source in the facility effectively keeps insanity stable — unless the apparatus has been removed.

---

## Effects at high insanity

Things start happening as insanity climbs. Find out for yourself.

<details>
<summary>Spoilers — click to reveal</summary>

### 70% — Voice distortion
Nearby teammates' voices begin to sound subtly warped. The mod also captures a rolling 30-second buffer of teammate voice chat and starts playing back distorted snippets spatially in 3D — you'll hear phantom voices from positions that seem to move.

### 80% — Tunnel vision
A deep red vignette slowly pulses in from the edges of the screen. The pulse and color intensity increase toward 100%.

Also at 80%+: the Ghost Girl (DressGirl) slightly increases her haunt speed while targeting you.

### 90% — Camera shake
Perlin-noise position and rotation jitter on the camera. Starts subtle, gets pronounced at 100%.

### 100% — Transformation
Insanity peaks. Movement gradually slows to a halt, the screen fades to black, and a Masked enemy spawns at your position wearing your suit. You die.

The transformation can be disabled in config (`EnableMaskedTransform = false`).

### Apparatus removal
Pulling the apparatus triggers an immediate insanity spike for everyone in the round. For the rest of the round, insanity-reduction buffs (teammate proximity, flashlight, facility lights) no longer work inside the facility — insanity can only rise in there. Outdoors is unaffected.

</details>

---

## Paranoia weather

A custom weather event. During Paranoia, insanity accumulates faster in the facility and enemies are more active. Appears at roughly 3% chance per night (configurable).

---

## HUD

A small ring meter in the bottom-right corner of the screen shows your current insanity percentage. Color shifts white → yellow → red as it fills.

---

## End-of-round results

After the ship leaves, the host broadcasts each player's peak insanity for the round to all clients.

---

## Configuration

All values are in `BepInEx/config/com.insanitymod.lethalcompany.cfg`.

> **Language note:** The `Language` setting only accepts values defined in `Langs.json` (`AUTO`, `EN`, `KO`). Setting an unsupported language code will cause UI strings to display as raw keys (e.g. `hud.max_insanity` instead of `Peak Insanity`).

| Key | Default | Description |
|-----|---------|-------------|
| `Language` | `AUTO` | Display language. `AUTO` detects from system locale. Supported: `EN`, `KO` |
| `InsanityRateInFacility` | `0.167` | Insanity/s inside facility |
| `InsanityRateOnShip` | `0` | Insanity/s on ship |
| `InsanityDecayOutdoor` | `0.8` | Insanity/s lost outdoors (daytime) |
| `NightOutdoorRate` | `0.05` | Insanity/s gained outdoors at night (0 = disabled) |
| `NightStartHour` | `19` | Game hour at which night begins (0–23) |
| `EclipseOutdoorRate` | `0.1` | Insanity/s gained outdoors during Eclipse |
| `ParanoiaMultiplier` | `1.2` | Rate multiplier during Paranoia weather |
| `ParanoiaSpawnWeight` | `20` | Spawn weight (other weathers: 100) |
| `TunnelVisionThreshold` | `80` | % at which vignette begins |
| `TunnelVisionColor` | `#180202` | Tunnel vision overlay color (hex). Use `#000000` for pure black |
| `HideHudAtZero` | `false` | If true, hides the insanity HUD ring while at 0% |
| `MobVisibilityScale` | `1.0` | Global multiplier for enemy-visibility rate |
| `MobVisibilityRange` | `30` | Max distance (m) for enemy detection |
| `TeammateBuffRate` | `0.15` | Rate reduction near a teammate |
| `TeammateBuffRange` | `6` | Range (m) for teammate buff |
| `LightBuffRate` | `0.1` | Rate reduction when illuminated |
| `LightProximityRange` | `8` | Range (m) to facility light |
| `DeathWitnessSpike` | `25` | Insanity spike when witnessing a death |
| `DeathWitnessRange` | `40` | Max distance (m) for death witness check |
| `GhostGirlBoostThreshold` | `80` | % above which Ghost Girl haunt speed increases |
| `VoiceHauntThreshold` | `70` | % at which voice distortion + haunting begins |
| `ApparatusSpike` | `15` | *(spoiler)* Instant insanity gain when apparatus is removed |
| `EnableMaskedTransform` | `true` | *(spoiler)* If false, the 100% effect is skipped entirely |

---

## Installation

1. Install BepInEx 5.4.23.5+
2. Install WeatherRegistry
3. Drop `InsanityMod.dll` into `BepInEx/plugins/InsanityMod/`

---

## Changelog

### v1.0.2
- **New:** Paranoia weather now has proper visual atmosphere — dark moody sky, dim red sun, light fog, and red color grading (HDRP volumes)
- **New:** Rain particles activate during Paranoia weather
- **New:** Custom terminal color for Paranoia weather (dark crimson)
- **New:** `TunnelVisionColor` config — change overlay color via hex (default darker than before; less eye-strain)
- **New:** `HideHudAtZero` config — option to hide the insanity HUD ring at 0%
- **Fix:** Paranoia visuals now apply at the correct moment (after level finishes loading) instead of leaking into the ship-departure transition
- **Fix:** Ship interior no longer pitch-black during Paranoia weather (fog/heavy darkening only applied outdoors)
- **Fix:** Smooth fade-out of Paranoia visuals when the ship departs, instead of an abrupt pop
- **Fix:** Voice haunt buffer is now cleared immediately on ship departure as well as round start
- **Change:** End-of-round insanity stats now appended to the existing penalty breakdown text for cleaner integration with the evaluation screen

### v1.0.1
- **Fix:** Insanity HUD no longer visible on the main menu after exiting a save
- **Fix:** Tunnel vision and other VFX now properly clear when returning to the main menu mid-round
- **New:** Insanity rises outdoors at night instead of decaying (configurable, default 0.05/s after hour 19)
- **New:** Eclipse weather also raises insanity outdoors at a higher rate (default 0.1/s)
- **Change:** Renamed "Blood Night" weather to **Paranoia** to avoid confusion with the existing Blood Moon event
- **Change:** Apparatus removal reworked — now triggers an immediate insanity spike (+15) and disables all insanity-reduction buffs inside the facility for the rest of the round (replaced the ×2.0 rate multiplier)

### v1.0.0
- Initial release

---

## Credits

Built for Lethal Company V80–81. Uses [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry/) by mrov and [Dissonance Voice Chat](https://dissonance.readthedocs.io/) for VOIP integration.

Special thanks to **vDolo** for extensive playtesting and feedback that drove most of the v1.0.1 and v1.0.2 improvements.
