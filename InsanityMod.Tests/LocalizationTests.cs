using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace InsanityMod.Tests
{
    public class LocalizationTests
    {
        private static Dictionary<string, string> LoadLang(string json, string lang)
        {
            var root    = JsonConvert.DeserializeObject<JObject>(json)!;
            var section = (JObject)(root[lang] ?? root["EN"]!);
            var result  = new Dictionary<string, string>();
            foreach (var kv in section)
                result[kv.Key] = kv.Value?.ToString() ?? kv.Key;
            return result;
        }

        private const string SampleJson = @"{
          ""KO"": { ""item.bread.name"": ""50원짜리 빵"", ""hud.max_insanity"": ""최대 광기"" },
          ""EN"": { ""item.bread.name"": ""Cheap Bread"",  ""hud.max_insanity"": ""Peak Insanity"" }
        }";

        [Fact]
        public void Korean_key_returns_korean_string()
        {
            var strings = LoadLang(SampleJson, "KO");
            Assert.Equal("50원짜리 빵", strings["item.bread.name"]);
        }

        [Fact]
        public void English_key_returns_english_string()
        {
            var strings = LoadLang(SampleJson, "EN");
            Assert.Equal("Cheap Bread", strings["item.bread.name"]);
        }

        [Fact]
        public void Unknown_lang_falls_back_to_EN()
        {
            var root    = JsonConvert.DeserializeObject<JObject>(SampleJson)!;
            var section = (JObject)(root["FR"] ?? root["EN"]!);
            var result  = new Dictionary<string, string>();
            foreach (var kv in section)
                result[kv.Key] = kv.Value?.ToString() ?? kv.Key;

            Assert.Equal("Cheap Bread", result["item.bread.name"]);
        }
    }
}
