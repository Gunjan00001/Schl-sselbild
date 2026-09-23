using NUnit.Framework;
using Schlusselbild.App;
using Schlusselbild.Ui;
using UnityEngine;

namespace Schlusselbild.Tests
{
    public class UiThemeTests
    {
        private static readonly TextScale[] Scales =
        {
            TextScale.Small,
            TextScale.Medium,
            TextScale.Large
        };

        [SetUp]
        public void SetUp()
        {
            UiTheme.SetTextScale(TextScale.Medium);
            UiTheme.SetColorblindSafe(false);
        }

        [TearDown]
        public void TearDown()
        {
            UiTheme.SetTextScale(TextScale.Medium);
            UiTheme.SetColorblindSafe(false);
        }

        [Test]
        public void ResolveStatus_ReturnsIconWordAndColourForEveryState()
        {
            StatusStyle ok = UiTheme.ResolveStatus(StatusKind.Ok, false);
            StatusStyle invalid = UiTheme.ResolveStatus(StatusKind.Invalid, false);
            StatusStyle scanning = UiTheme.ResolveStatus(StatusKind.Scanning, false);

            Assert.That(ok.Icon, Is.Not.Null.And.Not.Empty);
            Assert.That(invalid.Icon, Is.Not.Null.And.Not.Empty);
            Assert.That(scanning.Icon, Is.Not.Null.And.Not.Empty);
            Assert.That(ok.Word, Is.EqualTo("OK"));
            Assert.That(invalid.Word, Is.EqualTo("INVALID"));
            Assert.That(scanning.Word, Is.EqualTo("SCANNING"));
        }

        [Test]
        public void ResolveStatus_StatesDifferInIconWordAndColour()
        {
            AssertStatesDiffer(false);
            AssertStatesDiffer(true);
        }

        [Test]
        public void ColorblindSafePalette_ChangesTheStatusColours()
        {
            StatusStyle defaultOk = UiTheme.ResolveStatus(StatusKind.Ok, false);
            StatusStyle safeOk = UiTheme.ResolveStatus(StatusKind.Ok, true);
            StatusStyle defaultInvalid = UiTheme.ResolveStatus(StatusKind.Invalid, false);
            StatusStyle safeInvalid = UiTheme.ResolveStatus(StatusKind.Invalid, true);

            Assert.That(safeOk.Color, Is.Not.EqualTo(defaultOk.Color));
            Assert.That(safeInvalid.Color, Is.Not.EqualTo(defaultInvalid.Color));
            Assert.That(safeOk.Icon, Is.EqualTo(defaultOk.Icon));
            Assert.That(safeOk.Word, Is.EqualTo(defaultOk.Word));
        }

        [Test]
        public void Palettes_KeepTheSharedSurfaceColours()
        {
            UiPalette fallback = UiTheme.PaletteFor(false);
            UiPalette safe = UiTheme.PaletteFor(true);

            Assert.That(safe.Background, Is.EqualTo(fallback.Background));
            Assert.That(safe.Panel, Is.EqualTo(fallback.Panel));
            Assert.That(safe.Text, Is.EqualTo(fallback.Text));
            Assert.That(safe.Accent, Is.EqualTo(fallback.Accent));
        }

        [Test]
        public void Palettes_MeetWcagAaContrastForTextAndStatuses()
        {
            AssertContrast(false);
            AssertContrast(true);
        }

        [Test]
        public void FontSizes_FollowTheTextScale()
        {
            Assert.That(UiTheme.FontSize(TextScale.Small, UiTheme.BodyBasePixels), Is.EqualTo(34f).Within(0.001f));
            Assert.That(UiTheme.FontSize(TextScale.Medium, UiTheme.BodyBasePixels), Is.EqualTo(40f).Within(0.001f));
            Assert.That(UiTheme.FontSize(TextScale.Large, UiTheme.BodyBasePixels), Is.EqualTo(50f).Within(0.001f));

            UiTheme.SetTextScale(TextScale.Large);
            Assert.That(UiTheme.CurrentScale, Is.EqualTo(TextScale.Large));
            Assert.That(UiTheme.ScaleFactor, Is.EqualTo(1.25f).Within(0.001f));
            Assert.That(UiTheme.BodyFontSize, Is.EqualTo(50f).Within(0.001f));
            Assert.That(UiTheme.TitleFontSize, Is.EqualTo(UiTheme.TitleBasePixels * 1.25f).Within(0.001f));

            UiTheme.SetTextScale(TextScale.Small);
            Assert.That(UiTheme.KeyFontSize, Is.EqualTo(UiTheme.KeyBasePixels * 0.85f).Within(0.001f));
        }

        [Test]
        public void EveryTextRole_KeepsAComfortableAngularSizeAtTheDefaultDistance()
        {
            float[] bases =
            {
                UiTheme.TitleBasePixels,
                UiTheme.BodyBasePixels,
                UiTheme.ButtonBasePixels,
                UiTheme.CaptionBasePixels,
                UiTheme.KeyBasePixels
            };

            for (int s = 0; s < Scales.Length; s++)
            {
                for (int b = 0; b < bases.Length; b++)
                {
                    float pixels = UiTheme.FontSize(Scales[s], bases[b]);
                    float degrees = UiTheme.AngularSizeDegrees(pixels, AppSettings.DefaultUiDistance);
                    Assert.That(degrees, Is.GreaterThanOrEqualTo(UiTheme.MinComfortableAngularSizeDegrees),
                        "scale " + Scales[s] + " base " + bases[b] + " is only " + degrees + " degrees");
                }
            }
        }

        [Test]
        public void AngularSize_GrowsWithFontSizeAndShrinksWithDistance()
        {
            Assert.That(UiTheme.AngularSizeDegrees(100f, 1f), Is.GreaterThan(UiTheme.AngularSizeDegrees(50f, 1f)));
            Assert.That(UiTheme.AngularSizeDegrees(100f, 2f), Is.LessThan(UiTheme.AngularSizeDegrees(100f, 1f)));
        }

        private static void AssertStatesDiffer(bool colorblindSafe)
        {
            StatusStyle ok = UiTheme.ResolveStatus(StatusKind.Ok, colorblindSafe);
            StatusStyle invalid = UiTheme.ResolveStatus(StatusKind.Invalid, colorblindSafe);
            StatusStyle scanning = UiTheme.ResolveStatus(StatusKind.Scanning, colorblindSafe);

            Assert.That(ok.Icon, Is.Not.EqualTo(invalid.Icon));
            Assert.That(ok.Icon, Is.Not.EqualTo(scanning.Icon));
            Assert.That(invalid.Icon, Is.Not.EqualTo(scanning.Icon));

            Assert.That(ok.Word, Is.Not.EqualTo(invalid.Word));
            Assert.That(ok.Word, Is.Not.EqualTo(scanning.Word));
            Assert.That(invalid.Word, Is.Not.EqualTo(scanning.Word));

            Assert.That(ok.Color, Is.Not.EqualTo(invalid.Color));
            Assert.That(ok.Color, Is.Not.EqualTo(scanning.Color));
            Assert.That(invalid.Color, Is.Not.EqualTo(scanning.Color));
        }

        private static void AssertContrast(bool colorblindSafe)
        {
            UiPalette palette = UiTheme.PaletteFor(colorblindSafe);

            float textContrast = UiTheme.ContrastRatio(palette.Text, palette.Panel);
            Assert.That(textContrast, Is.GreaterThanOrEqualTo(UiTheme.MinimumTextContrastRatio), "body text contrast");

            StatusKind[] kinds = { StatusKind.Ok, StatusKind.Invalid, StatusKind.Scanning };
            for (int i = 0; i < kinds.Length; i++)
            {
                StatusStyle style = UiTheme.ResolveStatus(kinds[i], colorblindSafe);
                float contrast = UiTheme.ContrastRatio(style.Color, palette.Panel);
                Assert.That(contrast, Is.GreaterThanOrEqualTo(UiTheme.MinimumTextContrastRatio), kinds[i] + " status contrast");
            }
        }
    }
}