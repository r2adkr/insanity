using Dissonance.Audio.Playback;
using HarmonyLib;

namespace InsanityMod.Voice
{
    [HarmonyPatch(typeof(SamplePlaybackComponent))]
    internal static class VoiceCapturePatcher
    {
        [HarmonyPatch("OnAudioFilterRead")]
        [HarmonyPostfix]
        private static void CaptureSamples(SamplePlaybackComponent __instance, float[] data, int channels)
        {
            var track = VoiceHaunt.GetTrackFor(__instance);
            if (track == null || channels <= 0 || data.Length == 0) return;

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
        }
    }
}
