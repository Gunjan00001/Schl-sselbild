using System;
using UnityEngine;

namespace Schlusselbild.Page
{
    public static class PageLayout
    {
        public const int PageMargin = 48;
        public const int QrGap = 36;
        public const int TitleGap = 24;
        public const int GridCols = 3;
        public const string DefaultTitle = "Kitsune - encrypted page (unreadable without the key)";
        public const int MinTitleScale = 1;
        public const int MaxTitleScale = 3;

        public static Color32 Background
        {
            get { return new Color32(255, 255, 255, 255); }
        }

        public static Color32 Foreground
        {
            get { return new Color32(0, 0, 0, 255); }
        }

        public static void GridDimensions(int itemCount, out int cols, out int rows)
        {
            if (itemCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(itemCount));
            }

            cols = itemCount <= 0 ? 1 : Math.Min(GridCols, itemCount);
            if (cols < 1)
            {
                cols = 1;
            }

            rows = itemCount <= 0 ? 0 : (itemCount + cols - 1) / cols;
        }

        public static int GridWidth(int cols, int cellWidth)
        {
            return cols * cellWidth + (cols > 0 ? cols - 1 : 0) * QrGap;
        }

        public static int GridHeight(int rows, int cellHeight)
        {
            return rows * cellHeight + (rows > 0 ? rows - 1 : 0) * QrGap;
        }

        public static int PageWidth(int gridWidth)
        {
            return gridWidth + 2 * PageMargin;
        }

        public static int PageHeight(int titleHeight, int gridHeight)
        {
            return PageMargin + titleHeight + TitleGap + gridHeight + PageMargin;
        }

        public static int GridTop(int titleHeight)
        {
            return PageMargin + titleHeight + TitleGap;
        }

        public static int CellX(int column, int cellWidth)
        {
            return PageMargin + column * (cellWidth + QrGap);
        }

        public static int CellY(int row, int gridTop, int cellHeight)
        {
            return gridTop + row * (cellHeight + QrGap);
        }

        public static int CenteredOffset(int cellExtent, int contentExtent)
        {
            int offset = (cellExtent - contentExtent) / 2;
            return offset < 0 ? 0 : offset;
        }

        public static int TitleScaleFor(int availableWidth, int titleLength)
        {
            if (availableWidth <= 0 || titleLength <= 0)
            {
                return MinTitleScale;
            }

            int widthAtScaleOne = titleLength * PageFont.Advance;
            int scale = availableWidth / widthAtScaleOne;
            if (scale < MinTitleScale)
            {
                return MinTitleScale;
            }

            return scale > MaxTitleScale ? MaxTitleScale : scale;
        }

        public static int TitleHeight(int scale)
        {
            return PageFont.GlyphHeight * scale;
        }
    }
}
