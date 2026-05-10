# Changelog

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
