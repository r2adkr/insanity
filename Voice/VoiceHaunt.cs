using System;
using System.Collections.Generic;
using Dissonance;
using Dissonance.Audio.Playback;
using UnityEngine;

namespace InsanityMod.Voice
{
    internal static class VoiceHaunt
    {
        private const int   SampleRate          = 48000;
        private const int   BufferSeconds       = 30;
        private const int   SnippetMinMs        = 600;
        private const int   SnippetMaxMs        = 2000;

        private static DissonanceComms? _comms;
        private static readonly Dictionary<string, VoiceTrack> _tracks = new();
        private static readonly Dictionary<int, VoiceTrack>    _tracksByPlayback = new();
        private static readonly System.Random _rng = new();
        private static float _hauntCooldown = 5f;
        private static bool  _subscribed;

        public static void Tick(float localInsanity, float deltaTime)
        {
            EnsureComms();
            if (_comms == null) return;

            RefreshTracks();

            float threshold = InsanityMod.ModConfig.VoiceHauntThreshold.Value;
            float t = Mathf.Clamp01((localInsanity - threshold) / Mathf.Max(0.01f, 100f - threshold));
            foreach (var track in _tracks.Values) track.ApplyLiveDistortion(t);

            if (localInsanity < threshold)
            {
                _hauntCooldown = NextInterval(localInsanity);
                return;
            }

            _hauntCooldown -= deltaTime;
            if (_hauntCooldown > 0f) return;

            _hauntCooldown = NextInterval(localInsanity);
            TryHauntPlayback(t);
        }

        public static void ResetForRound()
        {
            foreach (var track in _tracks.Values)
            {
                track.ClearBuffer();
                track.ResetFilters();
            }
            _hauntCooldown = 5f;
        }

        public static VoiceTrack? GetTrackFor(SamplePlaybackComponent component)
        {
            if (component == null) return null;
            int id = component.gameObject.GetInstanceID();
            _tracksByPlayback.TryGetValue(id, out var track);
            return track;
        }

        private static void EnsureComms()
        {
            if (_comms != null) return;
            _comms = UnityEngine.Object.FindObjectOfType<DissonanceComms>();
            if (_comms == null) return;

            if (!_subscribed)
            {
                _comms.OnPlayerLeftSession += OnPlayerLeft;
                _subscribed = true;
            }
        }

        private static void OnPlayerLeft(VoicePlayerState state)
        {
            if (state == null) return;
            if (!_tracks.TryGetValue(state.Name, out var track)) return;
            _tracks.Remove(state.Name);
            _tracksByPlayback.Remove(track.Playback.gameObject.GetInstanceID());
        }

        private static void RefreshTracks()
        {
            if (_comms == null) return;

            foreach (var player in _comms.Players)
            {
                if (player.IsLocalPlayer) continue;
                if (player.Playback is not VoicePlayback playback) continue;
                if (_tracks.ContainsKey(player.Name)) continue;

                int bufferSamples = SampleRate * BufferSeconds;
                var track = new VoiceTrack(playback, bufferSamples);
                _tracks[player.Name] = track;
                _tracksByPlayback[playback.gameObject.GetInstanceID()] = track;
            }
        }

        private static float NextInterval(float insanity)
        {
            if (insanity >= 90f) return UnityEngine.Random.Range(3f, 8f);
            return UnityEngine.Random.Range(12f, 25f);
        }

        private static void TryHauntPlayback(float intensity)
        {
            var candidates = new List<VoiceTrack>();
            foreach (var track in _tracks.Values)
                if (track.Buffer.FilledSamples > SampleRate / 2) candidates.Add(track);

            if (candidates.Count == 0) return;

            var pick = candidates[_rng.Next(candidates.Count)];
            int snippetMs = _rng.Next(SnippetMinMs, SnippetMaxMs + 1);
            int snippetSamples = SampleRate * snippetMs / 1000;

            float[]? samples = pick.Buffer.ExtractSnippet(snippetSamples, _rng);
            if (samples == null) return;

            PlayHauntClip(samples, pick.Playback.transform.position, intensity);
        }

        private static void PlayHauntClip(float[] samples, Vector3 position, float intensity)
        {
            var clip = AudioClip.Create("InsanityHaunt", samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);

            var go = new GameObject("InsanityHauntPlayer");
            go.transform.position = position;
            UnityEngine.Object.DontDestroyOnLoad(go);

            var src = go.AddComponent<AudioSource>();
            src.clip            = clip;
            src.spatialBlend    = 1f;
            src.minDistance     = 1f;
            src.maxDistance     = 25f;
            src.rolloffMode     = AudioRolloffMode.Logarithmic;
            src.volume          = Mathf.Lerp(0.4f, 0.85f, intensity);
            src.pitch           = Mathf.Lerp(0.95f, 0.55f, intensity);

            var lp = go.AddComponent<AudioLowPassFilter>();
            lp.cutoffFrequency  = Mathf.Lerp(6000f, 1200f, intensity);

            var dist = go.AddComponent<AudioDistortionFilter>();
            dist.distortionLevel = Mathf.Lerp(0.15f, 0.65f, intensity);

            var reverb = go.AddComponent<AudioReverbFilter>();
            reverb.reverbPreset = AudioReverbPreset.PaddedCell;

            if (intensity > 0.6f)
            {
                var echo = go.AddComponent<AudioEchoFilter>();
                echo.delay = Mathf.Lerp(120f, 280f, intensity);
                echo.wetMix = 0.5f;
                echo.dryMix = 1f;
                echo.decayRatio = Mathf.Lerp(0.4f, 0.7f, intensity);
            }

            src.Play();
            UnityEngine.Object.Destroy(go, samples.Length / (float)SampleRate + 1f);
        }
    }
}
