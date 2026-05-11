using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using WeatherRegistry;

namespace InsanityMod.Managers
{
    internal static class BloodNightManager
    {
        public static bool IsActive { get; private set; }

        private static Weather?    _bloodNightWeather;
        private static GameObject? _rainEffectRef;
        private static Light?      _cachedSun;
        private static float       _lastSunSearch = -100f;
        private const  float       SunSearchCooldown = 5f;

        private static readonly HashSet<SelectableLevel> _paranoiaLevels = new();

        private static AmbientMode _savedAmbientMode;
        private static Color       _savedAmbientLight;
        private static Color       _savedSunColor;
        private static float       _savedSunIntensity;
        private static bool        _pendingSave;

        // Fade-out state — kept active after IsActive=false to smoothly tear down visuals
        private static bool  _fadingOut;

        // Sky/color volume — outdoor-gated, weight lerped 0↔1 in lockstep with fog
        private static GameObject?    _skyVolumeObj;
        private static Volume?        _skyVolume;
        private static VolumeProfile? _skyProfile;

        // Fog volume — outdoor-only, weight lerped 0↔1 based on player location
        private static GameObject?    _fogVolumeObj;
        private static Volume?        _fogVolume;
        private static VolumeProfile? _fogProfile;

        private static readonly Color      _ambientFlat = new Color(0.07f, 0.01f, 0.01f);
        private static readonly Color      _sunColor    = new Color(0.7f,  0.1f,  0.1f);
        private static readonly Quaternion _sunRot      = Quaternion.Euler(8f, 330f, 0f);

        public static void Initialize()
        {
            var effect = ScriptableObject.CreateInstance<ImprovedWeatherEffect>();

            _bloodNightWeather = new Weather(
                LocalizationManager.Get("weather.blood_night"),
                effect
            );

            _bloodNightWeather.Config.DefaultWeight = new IntegerConfigHandler(ModConfig.BloodNightSpawnWeight.Value, false);

            var gradient = ScriptableObject.CreateInstance<TMP_ColorGradient>();
            var c = new Color(0.55f, 0.04f, 0.04f);
            gradient.topLeft = gradient.topRight = gradient.bottomLeft = gradient.bottomRight = c;
            _bloodNightWeather.ColorGradient = gradient;

            WeatherManager.RegisterWeather(_bloodNightWeather);
            EventManager.WeatherChanged.AddListener(OnWeatherChanged);
        }

        public static void OnPluginDestroy()
        {
            _paranoiaLevels.Clear();
            if (_bloodNightWeather != null)
                EventManager.WeatherChanged.RemoveListener(OnWeatherChanged);
        }

        private static void OnWeatherChanged((SelectableLevel level, Weather weather) args) => InsanityMod.Patches.SafePatch.Run(nameof(OnWeatherChanged), () =>
        {
            if (_bloodNightWeather == null) return;
            if (args.weather == _bloodNightWeather) _paranoiaLevels.Add(args.level);
            else _paranoiaLevels.Remove(args.level);
        });

        public static void OnRoundStart()
        {
            var level = StartOfRound.Instance?.currentLevel;
            bool newActive = level != null && _paranoiaLevels.Contains(level);

            if (!newActive && IsActive)
                RestoreVisuals();

            IsActive     = newActive;
            _pendingSave = false;
        }

        public static void OnLevelLoaded()
        {
            if (!IsActive) return;
            EnableRain();
            EnableHDRPVolume();
            _pendingSave = true;
        }

        public static void OnRoundEnd()
        {
            if (!IsActive) return;
            IsActive     = false;
            _pendingSave = false;
            _fadingOut   = true;
            DisableRain();
            // Visuals fade out via TickFade() in LateUpdate
        }

        // Called from Plugin.LateUpdate every frame, even when !IsActive — handles fade-out tear-down
        public static void TickFade()
        {
            if (!_fadingOut) return;
            if (_skyVolume == null && _fogVolume == null) { _fadingOut = false; return; }

            float step = Time.deltaTime / 2.0f; // ~2 s fade-out (deliberately slower than EnforceVisuals' 0.5 s in-out)
            if (_skyVolume != null) _skyVolume.weight = Mathf.MoveTowards(_skyVolume.weight, 0f, step);
            if (_fogVolume != null) _fogVolume.weight = Mathf.MoveTowards(_fogVolume.weight, 0f, step);

            // Same lerp expression as EnforceVisuals: w = 0 → vanilla, w = 1 → full Paranoia.
            float w = _fogVolume?.weight ?? 0f;
            RenderSettings.ambientLight = Color.Lerp(_savedAmbientLight, _ambientFlat, w);
            var sun = GetSun();
            if (sun != null)
            {
                sun.color     = Color.Lerp(_savedSunColor, _sunColor, w);
                sun.intensity = Mathf.Lerp(_savedSunIntensity, 0.12f, w);
            }

            float skyW = _skyVolume?.weight ?? 0f;
            if (skyW <= 0f && w <= 0f)
            {
                RestoreVisuals();
                _fadingOut = false;
            }
        }

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

        public static void EnforceVisuals()
        {
            if (_pendingSave)
            {
                _savedAmbientMode  = RenderSettings.ambientMode;
                _savedAmbientLight = RenderSettings.ambientLight;
                var s = GetSun();
                if (s != null) { _savedSunColor = s.color; _savedSunIntensity = s.intensity; }
                _pendingSave = false;
            }

            var player = GameNetworkManager.Instance?.localPlayerController;
            bool outdoors = player != null && !player.isInsideFactory && !player.isInHangarShipRoom;

            float target = outdoors ? 1f : 0f;
            float step   = Time.deltaTime * 2f; // ~0.5 s fade

            if (_skyVolume != null)
                _skyVolume.weight = Mathf.MoveTowards(_skyVolume.weight, target, step);
            if (_fogVolume != null)
                _fogVolume.weight = Mathf.MoveTowards(_fogVolume.weight, target, step);

            // Drive ambient/sun off the same lerp factor (fog weight) so all four channels move in lockstep.
            // 0 = vanilla saved values, 1 = full Paranoia. ambientMode stays at the saved vanilla mode.
            float w = _fogVolume?.weight ?? 0f;
            RenderSettings.ambientMode  = _savedAmbientMode;
            RenderSettings.ambientLight = Color.Lerp(_savedAmbientLight, _ambientFlat, w);

            var sun = GetSun();
            if (sun != null)
            {
                sun.color     = Color.Lerp(_savedSunColor, _sunColor, w);
                sun.intensity = Mathf.Lerp(_savedSunIntensity, 0.12f, w);
                if (outdoors)
                    sun.transform.rotation = _sunRot; // rotation forced only outdoors; not lerped (would visibly swing the shadow)
            }

            if (_rainEffectRef != null && !_rainEffectRef.activeSelf)
                _rainEffectRef.SetActive(true);
        }

        private static void EnableHDRPVolume()
        {
            if (_skyVolumeObj != null) return;

            // Sky volume — outdoor-gated like fog. Starts at 0 and lerps to 1 only while the local player is outdoors.
            _skyVolumeObj = new GameObject("InsanityMod_ParanoiaSky");
            _skyVolume          = _skyVolumeObj.AddComponent<Volume>();
            _skyVolume.isGlobal = true;
            _skyVolume.priority = 9999f;
            _skyVolume.weight   = 0f;

            _skyProfile        = ScriptableObject.CreateInstance<VolumeProfile>();
            _skyVolume.profile = _skyProfile;

            var skyAdj = _skyProfile.Add<ColorAdjustments>(true);
            skyAdj.postExposure.Override(-0.4f); // subtle, doesn't crush ship interior
            skyAdj.colorFilter.Override(new Color(0.95f, 0.82f, 0.82f));

            // Fog volume — outdoor only, contains the heavy darkening + fog + saturation cut
            _fogVolumeObj = new GameObject("InsanityMod_ParanoiaFog");
            _fogVolume          = _fogVolumeObj.AddComponent<Volume>();
            _fogVolume.isGlobal = true;
            _fogVolume.priority = 10000f; // higher than sky volume
            _fogVolume.weight   = 0f;

            _fogProfile        = ScriptableObject.CreateInstance<VolumeProfile>();
            _fogVolume.profile = _fogProfile;

            var fog = _fogProfile.Add<Fog>(true);
            fog.enabled.Override(true);
            fog.albedo.Override(new Color(0.18f, 0.02f, 0.02f));
            fog.meanFreePath.Override(120f);
            fog.baseHeight.Override(0f);
            fog.maximumHeight.Override(150f);
            fog.tint.Override(new Color(0.5f, 0.1f, 0.1f));

            var fogAdj = _fogProfile.Add<ColorAdjustments>(true);
            fogAdj.postExposure.Override(-1.3f); // outdoor only — strong darkening
            fogAdj.colorFilter.Override(new Color(0.88f, 0.58f, 0.58f));
            fogAdj.saturation.Override(-22f);
        }

        private static void DisableHDRPVolume()
        {
            if (_skyVolumeObj != null) Object.Destroy(_skyVolumeObj);
            if (_skyProfile != null) Object.Destroy(_skyProfile);
            if (_fogVolumeObj != null) Object.Destroy(_fogVolumeObj);
            if (_fogProfile != null) Object.Destroy(_fogProfile);
            _skyVolumeObj = null; _skyVolume = null; _skyProfile = null;
            _fogVolumeObj = null; _fogVolume = null; _fogProfile = null;
        }

        private static void RestoreVisuals()
        {
            RenderSettings.ambientMode  = _savedAmbientMode;
            RenderSettings.ambientLight = _savedAmbientLight;

            var sun = GetSun();
            if (sun != null)
            {
                sun.color     = _savedSunColor;
                sun.intensity = _savedSunIntensity;
            }
            _cachedSun = null;

            DisableHDRPVolume();
            DisableRain();
        }

        private static void EnableRain()
        {
            var tod = TimeOfDay.Instance;
            if (tod == null || tod.effects == null) return;

            int idx = (int)LevelWeatherType.Rainy;
            if (idx < 0 || idx >= tod.effects.Length) return;

            var obj = tod.effects[idx].effectObject;
            if (obj == null) return;

            _rainEffectRef = obj;
            ClearStopActions(_rainEffectRef);
            _rainEffectRef.SetActive(true);
        }

        private static void DisableRain()
        {
            if (_rainEffectRef == null) return;
            _rainEffectRef.SetActive(false);
            _rainEffectRef = null;
        }

        private static void ClearStopActions(GameObject root)
        {
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                if (main.stopAction != ParticleSystemStopAction.None)
                    main.stopAction = ParticleSystemStopAction.None;
            }
        }
    }
}
