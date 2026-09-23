using Schlusselbild.App;
using TMPro;
using UnityEngine;

namespace Schlusselbild.Ui
{
    public enum StatusKind
    {
        Ok,
        Invalid,
        Scanning
    }

    public struct StatusStyle
    {
        public string Icon;
        public string Word;
        public Color Color;
    }

    public struct UiPalette
    {
        public Color Background;
        public Color Panel;
        public Color Text;
        public Color Accent;
        public Color Success;
        public Color Warning;
        public Color Error;
    }

    public static class UiTheme
    {
        public const float PixelsPerMeter = 1000f;
        public const float MinComfortableAngularSizeDegrees = 1.4f;
        public const float MinimumTextContrastRatio = 4.5f;

        public const string OkIcon = "\u2022";
        public const string InvalidIcon = "\u00d7";
        public const string ScanningIcon = "\u2026";

        public const string OkWord = "OK";
        public const string InvalidWord = "INVALID";
        public const string ScanningWord = "SCANNING";

        public const float TitleBasePixels = 64f;
        public const float BodyBasePixels = 40f;
        public const float ButtonBasePixels = 40f;
        public const float CaptionBasePixels = 32f;
        public const float KeyBasePixels = 40f;

        private static readonly UiPalette DefaultPaletteValue = new UiPalette
        {
            Background = Rgb(0x10, 0x15, 0x1B),
            Panel = Rgb(0x1B, 0x24, 0x30),
            Text = Rgb(0xF3, 0xF6, 0xFA),
            Accent = Rgb(0x4F, 0xA3, 0xFF),
            Success = Rgb(0x4A, 0xDE, 0x80),
            Warning = Rgb(0xFB, 0xBF, 0x24),
            Error = Rgb(0xF8, 0x71, 0x71)
        };

        private static readonly UiPalette ColorblindPaletteValue = new UiPalette
        {
            Background = Rgb(0x10, 0x15, 0x1B),
            Panel = Rgb(0x1B, 0x24, 0x30),
            Text = Rgb(0xF3, 0xF6, 0xFA),
            Accent = Rgb(0x4F, 0xA3, 0xFF),
            Success = Rgb(0x58, 0xA6, 0xFF),
            Warning = Rgb(0xD0, 0xD7, 0xE2),
            Error = Rgb(0xFF, 0x8A, 0x3D)
        };

        private static TMP_FontAsset fontAsset;
        private static TextScale currentScale = TextScale.Medium;
        private static bool colorblindSafe;

        public static UiPalette DefaultPalette
        {
            get { return DefaultPaletteValue; }
        }

        public static UiPalette ColorblindSafePalette
        {
            get { return ColorblindPaletteValue; }
        }

        public static bool ColorblindSafe
        {
            get { return colorblindSafe; }
        }

        public static TextScale CurrentScale
        {
            get { return currentScale; }
        }

        public static UiPalette Palette
        {
            get { return PaletteFor(colorblindSafe); }
        }

        public static UiPalette PaletteFor(bool useColorblindSafePalette)
        {
            return useColorblindSafePalette ? ColorblindPaletteValue : DefaultPaletteValue;
        }

        public static void SetColorblindSafe(bool value)
        {
            colorblindSafe = value;
        }

        public static void SetTextScale(TextScale scale)
        {
            currentScale = scale;
        }

        public static StatusStyle ResolveStatus(StatusKind kind, bool useColorblindSafePalette)
        {
            UiPalette palette = PaletteFor(useColorblindSafePalette);
            StatusStyle style = new StatusStyle();

            switch (kind)
            {
                case StatusKind.Ok:
                    style.Icon = OkIcon;
                    style.Word = OkWord;
                    style.Color = palette.Success;
                    break;
                case StatusKind.Invalid:
                    style.Icon = InvalidIcon;
                    style.Word = InvalidWord;
                    style.Color = palette.Error;
                    break;
                default:
                    style.Icon = ScanningIcon;
                    style.Word = ScanningWord;
                    style.Color = palette.Warning;
                    break;
            }

            return style;
        }

        public static float ScaleFactor
        {
            get { return AppSettings.ScaleFactorFor(currentScale); }
        }

        public static float FontSize(TextScale scale, float basePixels)
        {
            return basePixels * AppSettings.ScaleFactorFor(scale);
        }

        public static float TitleFontSize
        {
            get { return FontSize(currentScale, TitleBasePixels); }
        }

        public static float BodyFontSize
        {
            get { return FontSize(currentScale, BodyBasePixels); }
        }

        public static float ButtonFontSize
        {
            get { return FontSize(currentScale, ButtonBasePixels); }
        }

        public static float CaptionFontSize
        {
            get { return FontSize(currentScale, CaptionBasePixels); }
        }

        public static float KeyFontSize
        {
            get { return FontSize(currentScale, KeyBasePixels); }
        }

        public static float AngularSizeDegrees(float sizePixels, float distanceMeters)
        {
            if (distanceMeters <= 0f)
            {
                return 180f;
            }

            float sizeMeters = sizePixels / PixelsPerMeter;
            return 2f * Mathf.Atan(sizeMeters * 0.5f / distanceMeters) * Mathf.Rad2Deg;
        }

        public static float ContrastRatio(Color first, Color second)
        {
            float firstLuminance = RelativeLuminance(first);
            float secondLuminance = RelativeLuminance(second);
            float lighter = Mathf.Max(firstLuminance, secondLuminance);
            float darker = Mathf.Min(firstLuminance, secondLuminance);
            return (lighter + 0.05f) / (darker + 0.05f);
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        public static Color Mix(Color from, Color to, float amount)
        {
            return Color.Lerp(from, to, Mathf.Clamp01(amount));
        }

        public static TMP_FontAsset FontAsset
        {
            get
            {
                if (fontAsset == null)
                {
                    fontAsset = ResolveFontAsset();
                }

                return fontAsset;
            }
        }

        private static TMP_FontAsset ResolveFontAsset()
        {
            if (TMP_Settings.instance != null)
            {
                TMP_FontAsset preferred = TMP_Settings.defaultFontAsset;
                if (preferred != null)
                {
                    return preferred;
                }
            }

            Font builtin = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (builtin == null)
            {
                return null;
            }

            return TMP_FontAsset.CreateFontAsset(builtin);
        }

        private static float RelativeLuminance(Color color)
        {
            return 0.2126f * LinearChannel(color.r) +
                   0.7152f * LinearChannel(color.g) +
                   0.0722f * LinearChannel(color.b);
        }

        private static float LinearChannel(float value)
        {
            if (value <= 0.03928f)
            {
                return value / 12.92f;
            }

            return Mathf.Pow((value + 0.055f) / 1.055f, 2.4f);
        }

        private static Color Rgb(int red, int green, int blue)
        {
            return new Color(red / 255f, green / 255f, blue / 255f, 1f);
        }
    }
}
