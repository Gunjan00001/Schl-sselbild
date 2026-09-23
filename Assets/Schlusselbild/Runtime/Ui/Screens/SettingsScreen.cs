using System;
using Schlusselbild.App;
using TMPro;
using UnityEngine;

namespace Schlusselbild.Ui.Screens
{
    public sealed class SettingsScreen : ScreenBase
    {
        private AppSettings settings;
        private TextMeshProUGUI textScaleLabel;
        private TextMeshProUGUI distanceLabel;
        private TextMeshProUGUI comfortLabel;
        private UiButtonHandle smallButton;
        private UiButtonHandle mediumButton;
        private UiButtonHandle largeButton;
        private UnityEngine.UI.Slider distanceSlider;
        private UnityEngine.UI.Toggle seatedToggle;
        private UnityEngine.UI.Toggle snapTurnToggle;
        private UnityEngine.UI.Toggle colorblindToggle;
        private bool suppressCallbacks;

        public event Action BackRequested;

        public void Build(RectTransform parent, AppSettings appSettings)
        {
            settings = appSettings;

            UiPalette palette = UiTheme.Palette;
            if (settings != null)
            {
                UiTheme.SetTextScale(settings.TextScale);
                UiTheme.SetColorblindSafe(settings.ColorblindSafe);
            }

            BuildRoot(parent, "SettingsScreen");
            RectTransform body = BuildBody("Settings & comfort");

            textScaleLabel = AddCaption(body, "TextScaleLabel", string.Empty, palette.Text);

            RectTransform scaleRow = AddRow(body, "TextScaleRow", UiFactory.ButtonHeight, UiFactory.Gap);
            smallButton = UiFactory.CreateButton(scaleRow, "Small", "Small", SelectSmall, UiTheme.ButtonFontSize);
            UiFactory.SetFlexible(smallButton.Rect, 1f, 1f);
            mediumButton = UiFactory.CreateButton(scaleRow, "Medium", "Medium", SelectMedium, UiTheme.ButtonFontSize);
            UiFactory.SetFlexible(mediumButton.Rect, 1f, 1f);
            largeButton = UiFactory.CreateButton(scaleRow, "Large", "Large", SelectLarge, UiTheme.ButtonFontSize);
            UiFactory.SetFlexible(largeButton.Rect, 1f, 1f);

            distanceLabel = AddCaption(body, "DistanceLabel", string.Empty, palette.Text);
            distanceSlider = UiFactory.CreateSlider(body, "UiDistance", AppSettings.MinUiDistance, AppSettings.MaxUiDistance,
                settings == null ? AppSettings.DefaultUiDistance : settings.UiDistance, ApplyDistance);

            seatedToggle = UiFactory.CreateToggle(body, "SeatedMode", "Seated mode (eye-level tracking origin)",
                settings != null && settings.SeatedMode, ApplySeated);
            snapTurnToggle = UiFactory.CreateToggle(body, "SnapTurn", "Snap turn (for future locomotion)",
                settings == null || settings.SnapTurn, ApplySnapTurn);
            colorblindToggle = UiFactory.CreateToggle(body, "ColorblindSafe", "Colorblind-safe status colours",
                settings != null && settings.ColorblindSafe, ApplyColorblindSafe);

            comfortLabel = AddCaption(body, "Comfort", "Large targets, controller-only input, no forced locomotion.", UiTheme.WithAlpha(palette.Text, 0.8f));

            AddSpacer(body, "Spacer");

            UiButtonHandle back = UiFactory.CreateButton(body, "Back", "Back", RaiseBack, UiTheme.ButtonFontSize, true);
            UiFactory.SetPreferredHeight(back.Rect, UiFactory.ButtonHeight);

            if (settings != null)
            {
                settings.Changed += HandleSettingsChanged;
            }

            Refresh();
        }

        private void SelectSmall()
        {
            SetScale(TextScale.Small);
        }

        private void SelectMedium()
        {
            SetScale(TextScale.Medium);
        }

        private void SelectLarge()
        {
            SetScale(TextScale.Large);
        }

        private void SetScale(TextScale scale)
        {
            if (settings == null)
            {
                return;
            }

            settings.SetTextScale(scale);
            settings.Save();
        }

        private void ApplyDistance(float value)
        {
            if (suppressCallbacks || settings == null)
            {
                return;
            }

            settings.SetUiDistance(value);
            settings.Save();
            Refresh();
        }

        private void ApplySeated(bool value)
        {
            if (suppressCallbacks || settings == null)
            {
                return;
            }

            settings.SetSeatedMode(value);
            settings.Save();
        }

        private void ApplySnapTurn(bool value)
        {
            if (suppressCallbacks || settings == null)
            {
                return;
            }

            settings.SetSnapTurn(value);
            settings.Save();
        }

        private void ApplyColorblindSafe(bool value)
        {
            if (suppressCallbacks || settings == null)
            {
                return;
            }

            settings.SetColorblindSafe(value);
            settings.Save();
        }

        private void HandleSettingsChanged()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (settings == null)
            {
                return;
            }

            suppressCallbacks = true;

            if (textScaleLabel != null)
            {
                textScaleLabel.text = "Text size: " + settings.TextScale + " (" + settings.ScaleFactor.ToString("0.00") + "x)";
            }

            if (distanceLabel != null)
            {
                distanceLabel.text = "UI distance: " + settings.UiDistance.ToString("0.00") + " m";
            }

            if (distanceSlider != null)
            {
                distanceSlider.value = settings.UiDistance;
            }

            if (seatedToggle != null)
            {
                seatedToggle.isOn = settings.SeatedMode;
            }

            if (snapTurnToggle != null)
            {
                snapTurnToggle.isOn = settings.SnapTurn;
            }

            if (colorblindToggle != null)
            {
                colorblindToggle.isOn = settings.ColorblindSafe;
            }

            if (comfortLabel != null)
            {
                comfortLabel.text = settings.SeatedMode
                    ? "Seated mode on: eye-level origin, no forced locomotion."
                    : "Standing mode: floor-level origin, no forced locomotion.";
            }

            MarkScale(settings.TextScale);
            suppressCallbacks = false;
        }

        private void MarkScale(TextScale active)
        {
            MarkButton(smallButton, "Small", active == TextScale.Small);
            MarkButton(mediumButton, "Medium", active == TextScale.Medium);
            MarkButton(largeButton, "Large", active == TextScale.Large);
        }

        private static void MarkButton(UiButtonHandle handle, string text, bool active)
        {
            if (handle == null)
            {
                return;
            }

            handle.SetLabel(active ? UiTheme.OkIcon + " " + text : text);
        }

        private void RaiseBack()
        {
            Action handler = BackRequested;
            if (handler != null)
            {
                handler();
            }
        }

        public override void Teardown()
        {
            if (settings != null)
            {
                settings.Changed -= HandleSettingsChanged;
            }
        }

        private void OnDestroy()
        {
            if (settings != null)
            {
                settings.Changed -= HandleSettingsChanged;
            }
        }
    }
}