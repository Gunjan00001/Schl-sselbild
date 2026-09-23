using System;
using System.Collections.Generic;
using UnityEngine;
using Schlusselbild.Crypto;
using Schlusselbild.Qr;

namespace Schlusselbild.Page
{
    public static class PageComposer
    {
        public static Texture2D Compose(string secret, byte[] key, string title = PageLayout.DefaultTitle)
        {
            if (secret == null)
            {
                throw new ArgumentNullException(nameof(secret));
            }

            if (title == null)
            {
                title = string.Empty;
            }

            byte[] blob = AesGcmCodec.Encrypt(secret, key);
            List<byte[]> envelopes = QrEnvelope.Split(blob);
            int count = envelopes.Count;

            IQrEncoder encoder = new ZxingQrEncoder();
            Color32[][] tiles = new Color32[count][];
            int[] tileWidths = new int[count];
            int[] tileHeights = new int[count];
            int cellWidth = 0;
            int cellHeight = 0;

            for (int i = 0; i < count; i++)
            {
                Texture2D tile = encoder.Encode(envelopes[i]);
                tiles[i] = TexturePixels.ReadTopDown(tile);
                tileWidths[i] = tile.width;
                tileHeights[i] = tile.height;
                if (tileWidths[i] > cellWidth)
                {
                    cellWidth = tileWidths[i];
                }

                if (tileHeights[i] > cellHeight)
                {
                    cellHeight = tileHeights[i];
                }

                Release(tile);
            }

            int cols;
            int rows;
            PageLayout.GridDimensions(count, out cols, out rows);

            int gridWidth = PageLayout.GridWidth(cols, cellWidth);
            int gridHeight = PageLayout.GridHeight(rows, cellHeight);
            int pageWidth = PageLayout.PageWidth(gridWidth);
            int titleScale = PageLayout.TitleScaleFor(gridWidth, title.Length);
            int titleHeight = PageLayout.TitleHeight(titleScale);
            int pageHeight = PageLayout.PageHeight(titleHeight, gridHeight);

            Color32 background = PageLayout.Background;
            Color32[] page = new Color32[pageWidth * pageHeight];
            for (int i = 0; i < page.Length; i++)
            {
                page[i] = background;
            }

            DrawTitle(page, pageWidth, pageHeight, title, titleScale);

            int gridTop = PageLayout.GridTop(titleHeight);
            for (int i = 0; i < count; i++)
            {
                int row = i / cols;
                int column = i % cols;
                int cellX = PageLayout.CellX(column, cellWidth);
                int cellY = PageLayout.CellY(row, gridTop, cellHeight);
                int offsetX = cellX + PageLayout.CenteredOffset(cellWidth, tileWidths[i]);
                int offsetY = cellY + PageLayout.CenteredOffset(cellHeight, tileHeights[i]);
                Blit(page, pageWidth, pageHeight, tiles[i], tileWidths[i], tileHeights[i], offsetX, offsetY);
            }

            return TexturePixels.CreateTopDown(pageWidth, pageHeight, page);
        }

        private static void DrawTitle(Color32[] page, int pageWidth, int pageHeight, string title, int scale)
        {
            if (string.IsNullOrEmpty(title) || scale < 1)
            {
                return;
            }

            Color32 foreground = PageLayout.Foreground;
            int penX = PageLayout.PageMargin;
            int penY = PageLayout.PageMargin;

            for (int i = 0; i < title.Length; i++)
            {
                char character = title[i];
                for (int glyphY = 0; glyphY < PageFont.GlyphHeight; glyphY++)
                {
                    for (int glyphX = 0; glyphX < PageFont.GlyphWidth; glyphX++)
                    {
                        if (!PageFont.IsSet(character, glyphX, glyphY))
                        {
                            continue;
                        }

                        for (int stepY = 0; stepY < scale; stepY++)
                        {
                            for (int stepX = 0; stepX < scale; stepX++)
                            {
                                SetPixel(
                                    page,
                                    pageWidth,
                                    pageHeight,
                                    penX + glyphX * scale + stepX,
                                    penY + glyphY * scale + stepY,
                                    foreground);
                            }
                        }
                    }
                }

                penX += PageFont.Advance * scale;
            }
        }

        private static void Blit(
            Color32[] target,
            int targetWidth,
            int targetHeight,
            Color32[] source,
            int sourceWidth,
            int sourceHeight,
            int offsetX,
            int offsetY)
        {
            for (int y = 0; y < sourceHeight; y++)
            {
                int targetY = offsetY + y;
                if (targetY < 0 || targetY >= targetHeight)
                {
                    continue;
                }

                int sourceRow = y * sourceWidth;
                int targetRow = targetY * targetWidth;
                for (int x = 0; x < sourceWidth; x++)
                {
                    int targetX = offsetX + x;
                    if (targetX < 0 || targetX >= targetWidth)
                    {
                        continue;
                    }

                    target[targetRow + targetX] = source[sourceRow + x];
                }
            }
        }

        private static void SetPixel(Color32[] page, int pageWidth, int pageHeight, int x, int y, Color32 color)
        {
            if (x < 0 || y < 0 || x >= pageWidth || y >= pageHeight)
            {
                return;
            }

            page[y * pageWidth + x] = color;
        }

        private static void Release(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(texture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
