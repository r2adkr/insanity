using System;
using System.Collections.Generic;

namespace InsanityMod.Patches
{
    // Catches exceptions thrown inside Harmony postfix/prefix bodies and logs them
    // once per (label, exception type, exception message) per session, so a buggy
    // patch can't kill another patch's behavior or spam the console.
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
                }
                Plugin.Log.LogError($"[{label}] {ex}");
            }
        }
    }
}
