using System.Collections.Generic;
using UnityEngine;

namespace Schlusselbild.Ui
{
    public static class UiSprites
    {
        private static readonly Dictionary<int, Sprite> FillCache = new Dictionary<int, Sprite>();
        private static readonly Dictionary<int, Sprite> StrokeCache = new Dictionary<int, Sprite>();
        private static Sprite gradient;

        public const float GradientBottomAlpha = 0.55f;

        /// <summary>White vertical sheen, opaque at the top and fading towards the bottom.
        /// Tint it to give surfaces the soft-glass highlight.</summary>
        public static Sprite VerticalGradient()
        {
            if (gradient != null)
            {
                return gradient;
            }

            const int height = 128;
            Texture2D texture = new Texture2D(1, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;

            Color32[] pixels = new Color32[height];
            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                float alpha = Mathf.Lerp(GradientBottomAlpha, 1f, t);
                pixels[y] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);

            gradient = Sprite.Create(texture, new Rect(0f, 0f, 1f, height), new Vector2(0.5f, 0.5f), 100f);
            gradient.hideFlags = HideFlags.HideAndDontSave;
            return gradient;
        }

        public static void ApplyGradient(UnityEngine.UI.Image image)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = VerticalGradient();
            image.type = UnityEngine.UI.Image.Type.Simple;
        }

        public static Sprite RoundedFill(int radius)
        {
            radius = Mathf.Max(2, radius);
            if (FillCache.TryGetValue(radius, out Sprite cached) && cached != null)
            {
                return cached;
            }

            int border = radius + 1;
            int size = border * 2 + 2;
            Sprite sprite = Build(size, radius, 1f, 0f);
            FillCache[radius] = sprite;
            return sprite;
        }

        public static Sprite RoundedStroke(int radius, int thickness)
        {
            radius = Mathf.Max(2, radius);
            thickness = Mathf.Clamp(thickness, 1, radius);
            int key = radius * 100 + thickness;
            if (StrokeCache.TryGetValue(key, out Sprite cached) && cached != null)
            {
                return cached;
            }

            int border = radius + 1;
            int size = border * 2 + 2;
            Sprite sprite = Build(size, radius, thickness, size);
            StrokeCache[key] = sprite;
            return sprite;
        }

        private static Sprite Build(int size, int radius, float innerInset, float innerSize)
        {
            float innerHeight = innerSize > 0f ? innerSize : size * 0.5f;
            float innerRadius = Mathf.Max(0f, radius - innerInset);

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;

            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = RoundedAlpha(x + 0.5f, y + 0.5f, size, radius);

                    if (innerInset > 0f && innerSize > 0f)
                    {
                        float inner = RoundedAlpha(x + 0.5f - innerInset, y + 0.5f - innerInset, innerHeight, innerRadius);
                        alpha = Mathf.Clamp01(alpha - inner);
                    }

                    byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);

            int border = radius + 1;
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
        }

        private static float RoundedAlpha(float px, float py, float size, float radius)
        {
            float half = size * 0.5f;
            float qx = Mathf.Max(Mathf.Abs(px - half) - (half - radius), 0f);
            float qy = Mathf.Max(Mathf.Abs(py - half) - (half - radius), 0f);
            float distance = Mathf.Sqrt(qx * qx + qy * qy);
            return Mathf.Clamp01(radius - distance + 0.5f);
        }

        public static void ApplyRounded(UnityEngine.UI.Image image, int radius, bool sliced)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = RoundedFill(radius);
            image.type = sliced ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
        }

        public static void ApplyStroke(UnityEngine.UI.Image image, int radius, int thickness)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = RoundedStroke(radius, thickness);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.raycastTarget = false;
        }
    }
}
