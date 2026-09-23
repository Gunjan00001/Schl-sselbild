using System;
using UnityEngine;
using Schlusselbild.Page;

namespace Schlusselbild.Capture
{
    public static class FrameDecoder
    {
        public static string Decode(Texture2D frame, byte[] key, int maxLongEdge = 900)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            return DecodeTopDown(ReadTopDown(frame), frame.width, frame.height, key, maxLongEdge);
        }

        public static string Decode(Color32[] pixels, int width, int height, byte[] key, int maxLongEdge = 900)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("frame dimensions must be positive", nameof(width));
            }

            if (pixels.Length != width * height)
            {
                throw new ArgumentException("pixel buffer does not match the frame dimensions", nameof(pixels));
            }

            Color32[] topDown = FlipVertical(pixels, width, height);
            try
            {
                return DecodeTopDown(topDown, width, height, key, maxLongEdge);
            }
            catch (ArgumentException)
            {
                return DecodeTopDown(FlipVertical(topDown, width, height), width, height, key, maxLongEdge);
            }
        }

        private static string DecodeTopDown(Color32[] topDown, int width, int height, byte[] key, int maxLongEdge)
        {
            int targetWidth = width;
            int targetHeight = height;
            if (maxLongEdge > 0)
            {
                int longEdge = width > height ? width : height;
                if (longEdge > maxLongEdge)
                {
                    float scale = (float)maxLongEdge / longEdge;
                    targetWidth = Mathf.Max(1, Mathf.RoundToInt(width * scale));
                    targetHeight = Mathf.Max(1, Mathf.RoundToInt(height * scale));
                }
            }

            Color32[] scaled = targetWidth == width && targetHeight == height
                ? topDown
                : ResampleBilinear(topDown, width, height, targetWidth, targetHeight);

            Texture2D page = CreateTopDownTexture(targetWidth, targetHeight, ToGrayscale(scaled));
            try
            {
                return PageDecoder.Decode(page, key);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(page);
            }
        }

        private static Color32[] ReadTopDown(Texture2D frame)
        {
            if (frame.isReadable)
            {
                return FlipVertical(frame.GetPixels32(), frame.width, frame.height);
            }

            RenderTexture previous = RenderTexture.active;
            RenderTexture temporary = RenderTexture.GetTemporary(
                frame.width, frame.height, 0, RenderTextureFormat.ARGB32);
            Texture2D readable = new Texture2D(frame.width, frame.height, TextureFormat.RGBA32, false);
            try
            {
                Graphics.Blit(frame, temporary);
                RenderTexture.active = temporary;
                readable.ReadPixels(new Rect(0, 0, frame.width, frame.height), 0, 0);
                readable.Apply(false, false);
                return FlipVertical(readable.GetPixels32(), frame.width, frame.height);
            }
            finally
            {
                RenderTexture.active = previous;
                UnityEngine.Object.DestroyImmediate(readable);
                RenderTexture.ReleaseTemporary(temporary);
            }
        }

        private static Texture2D CreateTopDownTexture(int width, int height, Color32[] topDown)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(FlipVertical(topDown, width, height));
            texture.Apply(false, false);
            return texture;
        }

        private static Color32[] FlipVertical(Color32[] pixels, int width, int height)
        {
            Color32[] flipped = new Color32[pixels.Length];
            for (int y = 0; y < height; y++)
            {
                int sourceRow = (height - 1 - y) * width;
                int targetRow = y * width;
                for (int x = 0; x < width; x++)
                {
                    flipped[targetRow + x] = pixels[sourceRow + x];
                }
            }

            return flipped;
        }

        private static Color32[] ResampleBilinear(Color32[] source, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            Color32[] result = new Color32[targetWidth * targetHeight];
            float xScale = (float)sourceWidth / targetWidth;
            float yScale = (float)sourceHeight / targetHeight;

            for (int y = 0; y < targetHeight; y++)
            {
                float sourceY = (y + 0.5f) * yScale - 0.5f;
                if (sourceY < 0f)
                {
                    sourceY = 0f;
                }

                int y0 = (int)sourceY;
                if (y0 > sourceHeight - 1)
                {
                    y0 = sourceHeight - 1;
                }

                int y1 = y0 + 1 < sourceHeight ? y0 + 1 : y0;
                float fy = sourceY - y0;
                if (fy < 0f)
                {
                    fy = 0f;
                }

                for (int x = 0; x < targetWidth; x++)
                {
                    float sourceX = (x + 0.5f) * xScale - 0.5f;
                    if (sourceX < 0f)
                    {
                        sourceX = 0f;
                    }

                    int x0 = (int)sourceX;
                    if (x0 > sourceWidth - 1)
                    {
                        x0 = sourceWidth - 1;
                    }

                    int x1 = x0 + 1 < sourceWidth ? x0 + 1 : x0;
                    float fx = sourceX - x0;
                    if (fx < 0f)
                    {
                        fx = 0f;
                    }

                    Color32 topLeft = source[y0 * sourceWidth + x0];
                    Color32 topRight = source[y0 * sourceWidth + x1];
                    Color32 bottomLeft = source[y1 * sourceWidth + x0];
                    Color32 bottomRight = source[y1 * sourceWidth + x1];

                    float red = Lerp(Lerp(topLeft.r, topRight.r, fx), Lerp(bottomLeft.r, bottomRight.r, fx), fy);
                    float green = Lerp(Lerp(topLeft.g, topRight.g, fx), Lerp(bottomLeft.g, bottomRight.g, fx), fy);
                    float blue = Lerp(Lerp(topLeft.b, topRight.b, fx), Lerp(bottomLeft.b, bottomRight.b, fx), fy);
                    float alpha = Lerp(Lerp(topLeft.a, topRight.a, fx), Lerp(bottomLeft.a, bottomRight.a, fx), fy);

                    result[y * targetWidth + x] = new Color32(ToByte(red), ToByte(green), ToByte(blue), ToByte(alpha));
                }
            }

            return result;
        }

        private static Color32[] ToGrayscale(Color32[] pixels)
        {
            Color32[] gray = new Color32[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                byte luminance = (byte)((pixel.r * 77 + pixel.g * 150 + pixel.b * 29) >> 8);
                gray[i] = new Color32(luminance, luminance, luminance, 255);
            }

            return gray;
        }

        private static float Lerp(float from, float to, float amount)
        {
            return from + (to - from) * amount;
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.Clamp(Mathf.RoundToInt(value), 0, 255);
        }
    }
}
