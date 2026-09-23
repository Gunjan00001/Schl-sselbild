using System;
using UnityEngine;

namespace Schlusselbild.Qr
{
    internal static class TexturePixels
    {
        public static Color32[] ReadTopDown(Texture2D texture)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }

            Color32[] source = texture.GetPixels32();
            int width = texture.width;
            int height = texture.height;
            if (width <= 0 || height <= 0 || source.Length < width * height)
            {
                throw new ArgumentException("texture has no readable pixel data", nameof(texture));
            }

            Color32[] topDown = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                int sourceRow = (height - 1 - y) * width;
                int targetRow = y * width;
                for (int x = 0; x < width; x++)
                {
                    topDown[targetRow + x] = source[sourceRow + x];
                }
            }

            return topDown;
        }

        public static Texture2D CreateTopDown(int width, int height, Color32[] topDown)
        {
            if (topDown == null)
            {
                throw new ArgumentNullException(nameof(topDown));
            }

            if (width <= 0 || height <= 0 || topDown.Length != width * height)
            {
                throw new ArgumentException("pixel buffer does not match the requested size", nameof(topDown));
            }

            Color32[] pixels = new Color32[topDown.Length];
            for (int y = 0; y < height; y++)
            {
                int sourceRow = y * width;
                int targetRow = (height - 1 - y) * width;
                for (int x = 0; x < width; x++)
                {
                    pixels[targetRow + x] = topDown[sourceRow + x];
                }
            }

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }
    }
}
