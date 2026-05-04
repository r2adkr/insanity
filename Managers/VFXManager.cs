using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace InsanityMod.Managers
{
    internal static class VFXManager
    {
        private static Image? _overlay;
        private static Sprite? _vignetteSprite;

        public static void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "SampleSceneRelay") return;
            CreateOverlay();
        }

        private static void CreateOverlay()
        {
            if (_overlay != null) return;

            var canvasGO = new GameObject("InsanityVFX_Canvas");
            Object.DontDestroyOnLoad(canvasGO);

            var canvas          = canvasGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var imageGO = new GameObject("InsanityVFX_Overlay");
            imageGO.transform.SetParent(canvasGO.transform, false);

            var rt       = imageGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            _overlay               = imageGO.AddComponent<Image>();
            _overlay.raycastTarget = false;
            _overlay.sprite        = GetVignetteSprite();
            _overlay.color         = new Color(1f, 0f, 0f, 0f);
        }

        private static Sprite GetVignetteSprite()
        {
            if (_vignetteSprite != null) return _vignetteSprite;

            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.Alpha8, false)
            {
                wrapMode   = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            var center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float t = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    float alpha = Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(t), 2.5f));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            _vignetteSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _vignetteSprite;
        }

        public static void UpdateTunnelVision(float insanity)
        {
            if (_overlay == null) return;

            float threshold = ModConfig.TunnelVisionThreshold.Value;
            float target    = InsanityCalculator.TunnelVisionAlpha(insanity, threshold);
            float current   = _overlay.color.a;
            float newAlpha  = Mathf.Lerp(current, target, Time.deltaTime * 3f);

            float pulse = 0.8f + 0.2f * Mathf.Sin(Time.time * insanity * 0.1f);
            _overlay.color = new Color(pulse, 0f, 0f, newAlpha);
        }

        public static void ClearEffect()
        {
            if (_overlay == null) return;
            _overlay.color = new Color(1f, 0f, 0f, 0f);
        }
    }
}
