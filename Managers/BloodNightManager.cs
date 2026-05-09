using UnityEngine;
using UnityEngine.SceneManagement;
using WeatherRegistry;

namespace InsanityMod.Managers
{
    internal static class BloodNightManager
    {
        public static bool IsActive { get; private set; }

        private static Weather?     _bloodNightWeather;
        private static GameObject?  _rainInstance;

        private static Color _savedFogColor;
        private static float _savedFogDensity;
        private static bool  _savedFogEnabled;
        private static Color _savedAmbientSky;
        private static Color _savedAmbientEquator;
        private static Color _savedAmbientGround;

        public static void Initialize()
        {
            var effect = ScriptableObject.CreateInstance<ImprovedWeatherEffect>();

            _bloodNightWeather = new Weather(
                LocalizationManager.Get("weather.blood_night"),
                effect
            );

            _bloodNightWeather.Config.DefaultWeight = new IntegerConfigHandler(ModConfig.BloodNightSpawnWeight.Value, false);

            WeatherManager.RegisterWeather(_bloodNightWeather);

            EventManager.WeatherChanged.AddListener(OnWeatherChanged);
            EventManager.DayChanged.AddListener(OnDayChanged);
        }

        private static void OnWeatherChanged((SelectableLevel level, Weather weather) args)
        {
            bool wasActive = IsActive;
            IsActive = _bloodNightWeather != null && args.weather == _bloodNightWeather;

            if (IsActive && !wasActive)
                ApplyVisuals();
            else if (!IsActive && wasActive)
                RestoreVisuals();
        }

        private static void ApplyVisuals()
        {
            _savedFogColor       = RenderSettings.fogColor;
            _savedFogDensity     = RenderSettings.fogDensity;
            _savedFogEnabled     = RenderSettings.fog;
            _savedAmbientSky     = RenderSettings.ambientSkyColor;
            _savedAmbientEquator = RenderSettings.ambientEquatorColor;
            _savedAmbientGround  = RenderSettings.ambientGroundColor;

            // 붉은 안개
            RenderSettings.fog            = true;
            RenderSettings.fogMode        = FogMode.Exponential;
            RenderSettings.fogColor       = new Color(0.3f, 0.01f, 0.01f);
            RenderSettings.fogDensity     = 0.05f;

            // 불그스름한 검은 하늘
            RenderSettings.ambientSkyColor     = new Color(0.12f, 0.01f, 0.01f);
            RenderSettings.ambientEquatorColor = new Color(0.07f, 0.01f, 0.01f);
            RenderSettings.ambientGroundColor  = new Color(0.02f, 0f,    0f);

            // 기본 비 파티클 재사용
            SpawnRainParticles();
        }

        private static void RestoreVisuals()
        {
            RenderSettings.fog                 = _savedFogEnabled;
            RenderSettings.fogColor            = _savedFogColor;
            RenderSettings.fogDensity          = _savedFogDensity;
            RenderSettings.ambientSkyColor     = _savedAmbientSky;
            RenderSettings.ambientEquatorColor = _savedAmbientEquator;
            RenderSettings.ambientGroundColor  = _savedAmbientGround;

            if (_rainInstance != null)
            {
                Object.Destroy(_rainInstance);
                _rainInstance = null;
            }
        }

        private static void SpawnRainParticles()
        {
            var tod = TimeOfDay.Instance;
            if (tod == null || tod.effects == null) return;

            // LevelWeatherType.Rainy = 1, effects 배열은 0부터 시작
            int idx = (int)LevelWeatherType.Rainy;
            if (idx < 0 || idx >= tod.effects.Length) return;

            var prefab = tod.effects[idx].effectObject;
            if (prefab == null) return;

            _rainInstance = Object.Instantiate(prefab);
        }

        private static void OnDayChanged(int day)
        {
        }
    }
}
