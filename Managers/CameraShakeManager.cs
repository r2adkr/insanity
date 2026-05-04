using GameNetcodeStuff;
using UnityEngine;

namespace InsanityMod.Managers
{
    internal static class CameraShakeManager
    {
        private const float ShakeThreshold = 90f;
        private const float NoiseFrequency = 14f;
        private const float MaxPositionAmplitude = 0.03f;
        private const float MaxRotationAmplitude = 1.5f;

        public static void ApplyShake(PlayerControllerB local, float insanity)
        {
            var cam = local.gameplayCamera;
            if (cam == null) return;

            if (insanity < ShakeThreshold) return;

            float t = Mathf.Clamp01((insanity - ShakeThreshold) / (100f - ShakeThreshold));

            float time = Time.time * NoiseFrequency;
            float xn = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f;
            float yn = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f;
            float zn = (Mathf.PerlinNoise(time + 7f, time) - 0.5f) * 2f;

            Vector3    posShake = new Vector3(xn, yn, zn) * MaxPositionAmplitude * t;
            Quaternion rotShake = Quaternion.Euler(
                yn * MaxRotationAmplitude * t,
                xn * MaxRotationAmplitude * t,
                zn * MaxRotationAmplitude * 0.6f * t);

            cam.transform.localPosition += posShake;
            cam.transform.localRotation *= rotShake;
        }
    }
}
