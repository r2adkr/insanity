using Dissonance.Audio.Playback;
using HarmonyLib;
using InsanityMod.Patches;

namespace InsanityMod.Voice
{
    [HarmonyPatch(typeof(SamplePlaybackComponent))]
    internal static class VoiceCapturePatcher
    {
        // Guards run unwrapped so the closure isn't allocated on every audio callback
        // for non-tracked players (local player, or remotes before they're registered).
        // OnAudioFilterRead fires ~50-100 Hz per remote voice playback component.
        [HarmonyPatch("OnAudioFilterRead")]
        [HarmonyPostfix]
        private static void CaptureSamples(SamplePlaybackComponent __instance, float[] data, int channels)
        {
            var track = VoiceHaunt.GetTrackFor(__instance);
            if (track == null || channels <= 0 || data.Length == 0) return;

            SafePatch.Run(nameof(CaptureSamples), () =>
            {
                int frameCount = data.Length / channels;
                var mono = new float[frameCount];
                for (int i = 0; i < frameCount; i++)
                {
                    float sum = 0f;
                    for (int c = 0; c < channels; c++)
                        sum += data[i * channels + c];
                    mono[i] = sum / channels;
                }
                track.Buffer.Write(mono, 0, frameCount);
            });
        }
    }
}
