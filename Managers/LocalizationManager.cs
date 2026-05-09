using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace InsanityMod.Managers
{
    internal static class LocalizationManager
    {
        private static Dictionary<string, string> _strings = new();

        public static void Initialize()
        {
            string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string langsPath = Path.Combine(pluginDir, "Langs.json");

            if (!File.Exists(langsPath))
            {
                Plugin.Log.LogWarning($"Langs.json not found at {langsPath}, falling back to EN defaults.");
                LoadDefaults();
                return;
            }

            string lang = DetectLanguage();
            string json = File.ReadAllText(langsPath);

            var sections = ParseLangSections(json);
            if (!sections.TryGetValue(lang, out var section))
                sections.TryGetValue("EN", out section);

            if (section != null)
            {
                foreach (var kv in section) _strings[kv.Key] = kv.Value;
            }
            else
            {
                LoadDefaults();
            }

            Plugin.Log.LogInfo($"Localization loaded: {lang} ({_strings.Count} strings)");
        }

        public static string Get(string key) =>
            _strings.TryGetValue(key, out var val) ? val : key;

        private static string DetectLanguage()
        {
            var configured = ModConfig.Language.Value?.Trim().ToUpper();
            if (!string.IsNullOrEmpty(configured) && configured != "AUTO")
                return configured;

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
            _strings["weather.blood_night"] = "Paranoia";
        }

        private static Dictionary<string, Dictionary<string, string>> ParseLangSections(string json)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            int i = 0;
            SkipWhitespace(json, ref i);
            if (i >= json.Length || json[i] != '{') return result;
            i++;

            while (i < json.Length)
            {
                SkipWhitespace(json, ref i);
                if (i < json.Length && json[i] == '}') break;

                string sectionName = ReadString(json, ref i);
                SkipWhitespace(json, ref i);
                if (i >= json.Length || json[i] != ':') break;
                i++;
                SkipWhitespace(json, ref i);
                if (i >= json.Length || json[i] != '{') break;
                i++;

                var section = new Dictionary<string, string>();
                while (i < json.Length)
                {
                    SkipWhitespace(json, ref i);
                    if (i < json.Length && json[i] == '}') { i++; break; }

                    string key = ReadString(json, ref i);
                    SkipWhitespace(json, ref i);
                    if (i >= json.Length || json[i] != ':') break;
                    i++;
                    SkipWhitespace(json, ref i);
                    string value = ReadString(json, ref i);
                    section[key] = value;

                    SkipWhitespace(json, ref i);
                    if (i < json.Length && json[i] == ',') i++;
                }
                result[sectionName] = section;

                SkipWhitespace(json, ref i);
                if (i < json.Length && json[i] == ',') i++;
            }
            return result;
        }

        private static void SkipWhitespace(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        }

        private static string ReadString(string s, ref int i)
        {
            if (i >= s.Length || s[i] != '"') return "";
            i++;
            var sb = new StringBuilder();
            while (i < s.Length && s[i] != '"')
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    char next = s[i + 1];
                    if (next == 'n') sb.Append('\n');
                    else if (next == 't') sb.Append('\t');
                    else if (next == 'r') sb.Append('\r');
                    else sb.Append(next);
                    i += 2;
                }
                else
                {
                    sb.Append(s[i]);
                    i++;
                }
            }
            if (i < s.Length) i++;
            return sb.ToString();
        }
    }
}
