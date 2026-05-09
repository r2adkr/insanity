# InsanityMod

A Lethal Company mod that tracks player sanity and makes high-insanity runs genuinely terrifying.

**Game version:** V80–81 | **BepInEx:** 5.4.23.5+

---

## Dependencies

- [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry/) — required for Paranoia weather

---

## What it does

### Insanity meter

Insanity (0–100%) accumulates while you're inside the facility and drains while you're outdoors. On the ship it stays flat.

- **Solo in the facility:** ~10 minutes to 100%
- **Outdoors:** slowly decays back toward 0
- **Resets each round** — every new expedition starts fresh

### Things that make it worse

| Trigger | Effect |
|---------|--------|
| Visible enemy nearby | +rate per second (scales by enemy type) |
| Watching a teammate die | instant spike |
| Paranoia weather | rate multiplier |
| Certain in-game events | additional multipliers |

Enemy threat scale (examples): Bracken / Ghost Girl = 2.0×, Jester / Coilhead = 1.5×, Forest Giant / Masked = 1.4×, Sand Worm = 1.8×, Thumper = 0.8×

### Things that make it better

| Condition | Effect |
|-----------|--------|
| Teammate within 6m | −0.15/s |
| Flashlight on / in ship | −0.1/s |
| Near a facility light | −0.1/s |

Being with a teammate near a light source in the facility effectively keeps insanity stable.

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
Pulling the apparatus from the facility cuts the power — and doubles the insanity rate inside for the rest of the round. The effect is round-wide; any player entering the facility after the pull is also affected.

</details>

---

## Paranoia weather

A new custom weather event. During Paranoia the sky turns red, insanity accumulates 20% faster in the facility, and enemies are more active. Appears at roughly 3% chance per night (configurable).

---

## HUD

A small ring meter in the bottom-right corner of the screen shows your current insanity percentage. Color shifts white → yellow → red as it fills.

---

## End-of-round results

After the ship leaves, the host broadcasts each player's peak insanity for the round to all clients.

---

## Configuration

All values are in `BepInEx/config/com.insanitymod.lethalcompany.cfg`.

| Key | Default | Description |
|-----|---------|-------------|
| `InsanityRateInFacility` | `0.167` | Insanity/s inside facility |
| `InsanityRateOnShip` | `0` | Insanity/s on ship |
| `InsanityDecayOutdoor` | `0.8` | Insanity/s lost outdoors |
| `ParanoiaMultiplier` | `1.2` | Rate multiplier during Paranoia weather |
| `ParanoiaSpawnWeight` | `20` | Spawn weight (other weathers: 100) |
| `TunnelVisionThreshold` | `80` | % at which vignette begins |
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
| `ApparatusMultiplier` | `2.0` | *(spoiler)* Rate multiplier inside facility after apparatus is removed |
| `EnableMaskedTransform` | `true` | *(spoiler)* If false, the 100% effect is skipped entirely |

---

## Installation

1. Install BepInEx 5.4.23.5+
2. Install WeatherRegistry
3. Drop `InsanityMod.dll` into `BepInEx/plugins/InsanityMod/`

---

## Credits

Built for Lethal Company V80–81. Uses [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry/) by mrov and [Dissonance Voice Chat](https://dissonance.readthedocs.io/) for VOIP integration.
