using Dissonance.Audio.Playback;
using UnityEngine;

namespace InsanityMod.Voice
{
    internal sealed class VoiceTrack
    {
        public string PlayerName { get; }
        public VoicePlayback Playback { get; }
        public AudioSource Source { get; }
        public CircularSampleBuffer Buffer { get; }

        public AudioLowPassFilter LowPass { get; }
        public AudioDistortionFilter Distortion { get; }
        public AudioReverbFilter Reverb { get; }
        public AudioEchoFilter Echo { get; }

        public VoiceTrack(VoicePlayback playback, int bufferSamples)
        {
            PlayerName = playback.PlayerName;
            Playback   = playback;
            Source     = playback.AudioSource;
            Buffer     = new CircularSampleBuffer(bufferSamples);

            var go = playback.gameObject;
            LowPass    = GetOrAdd<AudioLowPassFilter>(go);
            Distortion = GetOrAdd<AudioDistortionFilter>(go);
            Reverb     = GetOrAdd<AudioReverbFilter>(go);
            Echo       = GetOrAdd<AudioEchoFilter>(go);

            ResetFilters();
        }

        public void ApplyLiveDistortion(float insanityNorm)
        {
            float t = Mathf.Clamp01(insanityNorm);

            LowPass.cutoffFrequency    = Mathf.Lerp(22000f, 1500f, t);
            Distortion.distortionLevel = Mathf.Lerp(0f,     0.55f, t);
            Reverb.reverbLevel         = Mathf.Lerp(-10000f, 800f,  t);
            Reverb.decayTime           = Mathf.Lerp(0.1f,   3.5f,  t);
            Echo.delay                 = Mathf.Lerp(0f,     180f,  t);
            Echo.wetMix                = Mathf.Lerp(0f,     0.45f, t);
            Echo.dryMix                = 1f;
            Echo.decayRatio            = Mathf.Lerp(0f,     0.55f, t);

            LowPass.enabled    = t > 0.01f;
            Distortion.enabled = t > 0.2f;
            Reverb.enabled     = t > 0.01f;
            Echo.enabled       = t > 0.4f;
        }

        public void ResetFilters()
        {
            LowPass.cutoffFrequency = 22000f;
            LowPass.enabled = false;
            Distortion.distortionLevel = 0f;
            Distortion.enabled = false;
            Reverb.reverbLevel = -10000f;
            Reverb.decayTime   = 0.1f;
            Reverb.enabled = false;
            Echo.delay = 0f;
            Echo.wetMix = 0f;
            Echo.dryMix = 1f;
            Echo.decayRatio = 0f;
            Echo.enabled = false;
        }

        public void ClearBuffer() => Buffer.Clear();

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }
    }
}
