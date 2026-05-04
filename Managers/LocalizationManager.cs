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

            var section = (JObject)(root[lang] ?? root["EN"]!);
            foreach (var kv in section)
                _strings[kv.Key] = kv.Value?.ToString() ?? kv.Key;

            Plugin.Log.LogInfo($"Localization loaded: {lang} ({_strings.Count} strings)");
        }

        public static string Get(string key) =>
            _strings.TryGetValue(key, out var val) ? val : key;

        private static string DetectLanguage()
        {
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
