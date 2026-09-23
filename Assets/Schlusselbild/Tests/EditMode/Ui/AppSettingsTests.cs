using NUnit.Framework;
using Schlusselbild.App;
using UnityEngine;

namespace Schlusselbild.Tests
{
    public class AppSettingsTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        [Test]
        public void NewInstance_UsesDocumentedDefaults()
        {
            AppSettings settings = new AppSettings();

            Assert.That(settings.TextScale, Is.EqualTo(TextScale.Medium));
            Assert.That(settings.UiDistance, Is.EqualTo(AppSettings.DefaultUiDistance).Within(0.0001f));
            Assert.That(settings.SeatedMode, Is.False);
            Assert.That(settings.SnapTurn, Is.True);
            Assert.That(settings.ColorblindSafe, Is.False);
            Assert.That(settings.ScaleFactor, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Load_WithNothingStored_ReturnsDefaults()
        {
            AppSettings loaded = AppSettings.Load();

            Assert.That(loaded.TextScale, Is.EqualTo(TextScale.Medium));
            Assert.That(loaded.UiDistance, Is.EqualTo(AppSettings.DefaultUiDistance).Within(0.0001f));
            Assert.That(loaded.SeatedMode, Is.False);
            Assert.That(loaded.SnapTurn, Is.True);
            Assert.That(loaded.ColorblindSafe, Is.False);
        }

        [Test]
        public void SaveAndLoad_RoundTripsEveryValue()
        {
            AppSettings saved = new AppSettings();
            saved.SetTextScale(TextScale.Large);
            saved.SetUiDistance(1.35f);
            saved.SetSeatedMode(true);
            saved.SetSnapTurn(false);
            saved.SetColorblindSafe(true);
            saved.Save();

            AppSettings loaded = AppSettings.Load();

            Assert.That(loaded.TextScale, Is.EqualTo(TextScale.Large));
            Assert.That(loaded.UiDistance, Is.EqualTo(1.35f).Within(0.0001f));
            Assert.That(loaded.SeatedMode, Is.True);
            Assert.That(loaded.SnapTurn, Is.False);
            Assert.That(loaded.ColorblindSafe, Is.True);
            Assert.That(loaded.ScaleFactor, Is.EqualTo(AppSettings.LargeScaleFactor).Within(0.0001f));
        }

        [Test]
        public void ScaleFactor_MapsSmallMediumAndLarge()
        {
            Assert.That(AppSettings.ScaleFactorFor(TextScale.Small), Is.EqualTo(0.85f).Within(0.0001f));
            Assert.That(AppSettings.ScaleFactorFor(TextScale.Medium), Is.EqualTo(1.0f).Within(0.0001f));
            Assert.That(AppSettings.ScaleFactorFor(TextScale.Large), Is.EqualTo(1.25f).Within(0.0001f));
        }

        [Test]
        public void UiDistance_IsClampedToTheComfortRange()
        {
            AppSettings settings = new AppSettings();

            settings.SetUiDistance(0.1f);
            Assert.That(settings.UiDistance, Is.EqualTo(AppSettings.MinUiDistance).Within(0.0001f));

            settings.SetUiDistance(9f);
            Assert.That(settings.UiDistance, Is.EqualTo(AppSettings.MaxUiDistance).Within(0.0001f));
        }

        [Test]
        public void Changed_FiresOncePerRealChange()
        {
            AppSettings settings = new AppSettings();
            int changes = 0;
            settings.Changed += delegate { changes++; };

            settings.SetTextScale(TextScale.Small);
            Assert.That(changes, Is.EqualTo(1));

            settings.SetTextScale(TextScale.Small);
            Assert.That(changes, Is.EqualTo(1));

            settings.SetUiDistance(AppSettings.MaxUiDistance);
            Assert.That(changes, Is.EqualTo(2));

            settings.SetSeatedMode(false);
            Assert.That(changes, Is.EqualTo(2));

            settings.SetSeatedMode(true);
            settings.SetSnapTurn(false);
            settings.SetColorblindSafe(true);
            Assert.That(changes, Is.EqualTo(5));
        }

        [Test]
        public void TextScaleProperty_AssignsAndReportsScaleFactor()
        {
            AppSettings settings = new AppSettings();
            settings.TextScale = Schlusselbild.App.TextScale.Large;

            Assert.That(settings.TextScale, Is.EqualTo(TextScale.Large));
            Assert.That(settings.ScaleFactor, Is.EqualTo(1.25f).Within(0.0001f));
        }
    }
}