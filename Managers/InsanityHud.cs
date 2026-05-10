using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace InsanityMod.Managers
{
    internal static class InsanityHud
    {
        private const int   RingSize      = 110;
        private const int   RingThickness = 7;
        private const float ScreenMarginX = 35f;
        private const float ScreenMarginY = 130f;

        private static GameObject? _canvasGO;
        private static Image? _ringFill;
        private static Image? _ringBg;
        private static TextMeshProUGUI? _label;
        private static Sprite? _ringSprite;

        public static void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InsanityMod.Patches.SafePatch.Run(nameof(OnSceneLoaded), () =>
        {
            if (scene.name != "SampleSceneRelay") return;
            CreateOverlay();
            SetVisible(false);
        });

        public static void SetVisible(bool visible)
        {
            if (_canvasGO == null) return;
            _canvasGO.SetActive(visible);
        }

        public static void UpdateValue(float insanity)
        {
            if (_canvasGO != null && ModConfig.HideHudAtZero.Value)
            {
                bool shouldShow = insanity > 0.5f;
                if (_canvasGO.activeSelf != shouldShow)
                    _canvasGO.SetActive(shouldShow);
            }

            if (_ringFill == null || _ringBg == null || _label == null) return;

            float t = Mathf.Clamp01(insanity / 100f);
            _ringFill.fillAmount = t;
            _ringFill.color      = ColorForInsanity(t);
            _label.text          = $"{Mathf.FloorToInt(insanity)}%";
            _label.color         = ColorForInsanity(t);
        }

        private static void CreateOverlay()
        {
            if (_canvasGO != null) return;

            _canvasGO = new GameObject("InsanityHud_Canvas");
            Object.DontDestroyOnLoad(_canvasGO);

            var canvas          = _canvasGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            _canvasGO.AddComponent<CanvasScaler>();
            _canvasGO.AddComponent<GraphicRaycaster>();

            var sprite = GetRingSprite();

            var rootGO = new GameObject("InsanityHud_Root");
            var rootRT = rootGO.AddComponent<RectTransform>();
            rootRT.SetParent(_canvasGO.transform, false);
            rootRT.anchorMin        = new Vector2(1f, 0f);
            rootRT.anchorMax        = new Vector2(1f, 0f);
            rootRT.pivot            = new Vector2(1f, 0f);
            rootRT.anchoredPosition = new Vector2(-ScreenMarginX, ScreenMarginY);
            rootRT.sizeDelta        = new Vector2(RingSize, RingSize);

            var bgGO = new GameObject("InsanityHud_RingBg");
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.SetParent(rootRT, false);
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            _ringBg = bgGO.AddComponent<Image>();
            _ringBg.sprite        = sprite;
            _ringBg.color         = new Color(0.1f, 0.1f, 0.1f, 0.75f);
            _ringBg.raycastTarget = false;

            var fillGO = new GameObject("InsanityHud_RingFill");
            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.SetParent(rootRT, false);
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;
            _ringFill = fillGO.AddComponent<Image>();
            _ringFill.sprite        = sprite;
            _ringFill.type          = Image.Type.Filled;
            _ringFill.fillMethod    = Image.FillMethod.Radial360;
            _ringFill.fillOrigin    = (int)Image.Origin360.Top;
            _ringFill.fillClockwise = true;
            _ringFill.fillAmount    = 0f;
            _ringFill.color         = ColorForInsanity(0f);
            _ringFill.raycastTarget = false;

            var labelGO = new GameObject("InsanityHud_Label");
            var labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.SetParent(rootRT, false);
            labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
            labelRT.sizeDelta = Vector2.zero;
            _label = labelGO.AddComponent<TextMeshProUGUI>();
            _label.alignment      = TextAlignmentOptions.Center;
            _label.fontSize       = 22f;
            _label.fontStyle      = FontStyles.Bold;
            _label.text           = "0%";
            _label.color          = ColorForInsanity(0f);
            _label.raycastTarget  = false;
        }

        private static Color ColorForInsanity(float t)
        {
            if (t < 0.5f)
                return Color.Lerp(new Color(0.85f, 0.85f, 0.85f), new Color(1f, 0.85f, 0.2f), t / 0.5f);
            return Color.Lerp(new Color(1f, 0.85f, 0.2f), new Color(1f, 0.15f, 0.15f), (t - 0.5f) / 0.5f);
        }

        private static Sprite GetRingSprite()
        {
            if (_ringSprite != null) return _ringSprite;

            int size = RingSize;
            var tex = new Texture2D(size, size, TextureFormat.Alpha8, false)
            {
                wrapMode   = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            var center  = new Vector2(size / 2f, size / 2f);
            float outer = size / 2f - 1.5f;
            float inner = outer - RingThickness;

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    float a = 0f;
                    if (d >= inner && d <= outer)        a = 1f;
                    else if (d > outer && d < outer + 1) a = 1f - (d - outer);
                    else if (d < inner && d > inner - 1) a = 1f - (inner - d);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            _ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _ringSprite;
        }
    }
}
