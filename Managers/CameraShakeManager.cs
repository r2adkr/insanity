using GameNetcodeStuff;
using UnityEngine;

namespace InsanityMod.Managers
{
    internal static class CameraShakeManager
    {
        private const float ShakeThreshold = 90f;
        private const float NoiseFrequency = 8f;
        private const float MaxPositionAmplitude = 0.02f;
        private const float MaxRotationAmplitude = 0.8f;

        private static Vector3    _prevPosShake = Vector3.zero;
        private static Quaternion _prevRotShake = Quaternion.identity;

        public static void ApplyShake(PlayerControllerB local, float insanity)
        {
            var cam = local.gameplayCamera;
            if (cam == null) return;

            // Undo last frame's shake first so offsets never accumulate
            cam.transform.localPosition -= _prevPosShake;
            cam.transform.localRotation  = cam.transform.localRotation * Quaternion.Inverse(_prevRotShake);

            if (insanity < ShakeThreshold)
            {
                _prevPosShake = Vector3.zero;
                _prevRotShake = Quaternion.identity;
                return;
            }

            float t = Mathf.Clamp01((insanity - ShakeThreshold) / (100f - ShakeThreshold));

            float time = Time.time * NoiseFrequency;
            float xn = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f;
            float yn = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f;
            float zn = (Mathf.PerlinNoise(time + 7f, time) - 0.5f) * 2f;

            _prevPosShake = new Vector3(xn, yn, zn) * MaxPositionAmplitude * t;
            _prevRotShake = Quaternion.Euler(
                yn * MaxRotationAmplitude * t,
                xn * MaxRotationAmplitude * t,
                zn * MaxRotationAmplitude * 0.6f * t);

            cam.transform.localPosition += _prevPosShake;
            cam.transform.localRotation *= _prevRotShake;
        }
    }
}
