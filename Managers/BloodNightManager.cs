using WeatherRegistry;

namespace InsanityMod.Managers
{
    internal static class BloodNightManager
    {
        public static bool IsActive { get; private set; }

        private static Weather? _bloodNightWeather;

        public static void Initialize()
        {
            var effect = new ImprovedWeatherEffect(null, null);

            _bloodNightWeather = new Weather(
                LocalizationManager.Get("weather.blood_night"),
                effect
            );

            _bloodNightWeather.Config.DefaultWeight = new IntegerConfigHandler(ModConfig.BloodNightSpawnWeight.Value, true);

            WeatherManager.RegisterWeather(_bloodNightWeather);

            EventManager.WeatherChanged.AddListener(OnWeatherChanged);
            EventManager.DayChanged.AddListener(OnDayChanged);
        }

        private static void OnWeatherChanged((SelectableLevel level, Weather weather) args)
        {
            IsActive = _bloodNightWeather != null && args.weather == _bloodNightWeather;
        }

        private static void OnDayChanged(int day)
        {
        }
    }
}
