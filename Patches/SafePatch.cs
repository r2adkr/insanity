using System;
using System.Collections.Generic;

namespace InsanityMod.Patches
{
    // Catches exceptions thrown inside Harmony postfix/prefix bodies and logs them
    // once per (label, exception type, exception message) per session, so a buggy
    // patch can't kill another patch's behavior or spam the console.
    //
    // The lock serializes both the dedup HashSet write AND the log emission. This
    // matters because some patches (e.g. VoiceCapturePatcher) run on the audio
    // thread, and BepInEx's ManualLogSource is not guaranteed thread-safe when
    // hit concurrently from main-thread patches.
    internal static class SafePatch
    {
        private static readonly HashSet<string> _loggedKeys = new();
        private static readonly object _lock = new();

        public static void Run(string label, Action body)
        {
            try { body(); }
            catch (Exception ex)
            {
                string key = $"{label}:{ex.GetType().FullName}:{ex.Message}";
                lock (_lock)
                {
                    if (!_loggedKeys.Add(key)) return;
                    Plugin.Log.LogError($"[{label}] {ex}");
                }
            }
        }
    }
}
