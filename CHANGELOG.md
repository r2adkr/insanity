# Changelog

## v1.0.5

Playtest-driven additions on top of v1.0.4.

- **Fix:** Paranoia weather visuals are now strictly outdoor-only. The red sky tint and exposure cut no longer leak into the ship or facility interior. Indoor↔outdoor transitions fade fog, sky tint, ambient light, and sun together over ~0.5 s — eliminating the bright→red flash that appeared when stepping off the ship ramp.
- **New:** `TZPInsanityDrainRate` config — sustained per-second insanity reduction while the local player has the TZP-Inhalant effect active (`drunkness > 0.05`). Default `1.0`/s. One inhale (~17 s effect window) recovers ~17%; stack three for ~50% — a strong safety valve, particularly useful on apparatus-removed rounds where in-facility buffs are disabled.
- **Change:** `InsanityRateOnShip` is replaced by `RateOnShipLightsOn` (default `-0.3`/s) and `RateOnShipLightsOff` (default `+0.15`/s). Ship lights become a tactical lever: lit ship is a passive recovery zone, dark ship is a mild threat (still slower than the facility). Old config files will retain the now-unread `InsanityRateOnShip` line as a cosmetic orphan — BepInEx does not auto-clean it. Safe to delete by hand.

## v1.0.4

Playtest-driven additions on top of the v1.0.3 stabilization release.

- **New:** HUD ring now fades in/out (≈0.5 s) when crossing the 0% threshold — replaces the abrupt toggle from v1.0.3.
- **New:** `UnderwaterRate` config — sustained per-second insanity gain while the local player is underwater (default `0.4`).
- **New:** `CompanyMoonDecayRate` config — sustained per-second insanity reduction while on the Company building moon (71 Gordion). Default `0.5`. The Company is treated as a thematic refuge.
- **New:** `MaskedTransformOnlyDuringParanoia` config (default `true`) — restricts the 100% Masked transformation to Paranoia weather rounds. Set `false` to restore the old behavior.
- **New:** `EnableHud` config (default `true`) — master switch for the insanity HUD ring. Set `false` to disable it entirely (no canvas is created).

## v1.0.3

Stabilization & defensive sweep — no player-facing behavior changes.

- **New:** `SafePatch` helper wraps every Harmony patch, Unity callback, and event listener with deduplicated exception logging. A single throw in any patch can no longer silently disable other patches or spam the log.
- **Perf:** Per-frame hot paths (`PlayerControllerB.Update`, `DressGirlAI.Update`, `OnAudioFilterRead`) lift their early-exit guards above the wrapper to avoid closure allocation when nothing needs to run.
- **Perf:** `BloodNightManager.GetSun()` fallback `FindObjectsOfType<Light>()` is throttled to once every 5 seconds when the cached reference is missing.
- **Fix:** End-of-round insanity stats no longer get appended twice if the game's `ApplyPenalty` fires more than once per round.
- **Fix:** `VoiceHaunt` haunt-clip GameObjects (which use `DontDestroyOnLoad`) are tracked and cleaned up on round end so they can't pile up across long sessions.
- **Fix:** Plugin teardown now clears the Paranoia weather level set and removes the `WeatherChanged` listener.
- **Fix:** Network message handlers wrapped — a malformed packet can no longer break the handler registration.
- **Chore:** Dropped a per-level "InsanityHud overlay created" log line.

## v1.0.2

*Most of this release is driven by feedback from **vDolo** — thanks!*

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

## v1.0.1

- **Fix:** Insanity HUD no longer visible on the main menu after exiting a save
- **Fix:** Tunnel vision and other VFX now properly clear when returning to the main menu mid-round
- **New:** Insanity rises outdoors at night instead of decaying (configurable, default 0.05/s after hour 19)
- **New:** Eclipse weather also raises insanity outdoors at a higher rate (default 0.1/s)
- **Change:** Renamed "Blood Night" weather to **Paranoia** to avoid confusion with the existing Blood Moon event
- **Change:** Apparatus removal reworked — now triggers an immediate insanity spike (+15) and disables all insanity-reduction buffs inside the facility for the rest of the round (replaced the ×2.0 rate multiplier)

## v1.0.0

- Initial release
