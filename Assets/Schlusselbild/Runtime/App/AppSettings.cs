using System;
using UnityEngine;

namespace Schlusselbild.App
{
    public enum TextScale
    {
        Small = 0,
        Medium = 1,
        Large = 2
    }

    public sealed class AppSettings
    {
        public const string TextScaleKey = "schlusselbild.textScale";
        public const string UiDistanceKey = "schlusselbild.uiDistance";
        public const string SeatedModeKey = "schlusselbild.seatedMode";
        public const string SnapTurnKey = "schlusselbild.snapTurn";
        public const string ColorblindSafeKey = "schlusselbild.colorblindSafe";

        public const float MinUiDistance = 0.6f;
        public const float MaxUiDistance = 1.6f;
        public const float DefaultUiDistance = 1.0f;

        public const float SmallScaleFactor = 0.85f;
        public const float MediumScaleFactor = 1.0f;
        public const float LargeScaleFactor = 1.25f;

        private TextScale textScale;
        private float uiDistance;
        private bool seatedMode;
        private bool snapTurn;
        private bool colorblindSafe;

        public AppSettings()
        {
            textScale = TextScale.Medium;
            uiDistance = DefaultUiDistance;
            seatedMode = false;
            snapTurn = true;
            colorblindSafe = false;
        }

        public event Action Changed;

        public TextScale TextScale
        {
            get { return textScale; }
            set { SetTextScale(value); }
        }

        public float UiDistance
        {
            get { return uiDistance; }
            set { SetUiDistance(value); }
        }

        public bool SeatedMode
        {
            get { return seatedMode; }
            set { SetSeatedMode(value); }
        }

        public bool SnapTurn
        {
            get { return snapTurn; }
            set { SetSnapTurn(value); }
        }

        public bool ColorblindSafe
        {
            get { return colorblindSafe; }
            set { SetColorblindSafe(value); }
        }

        public float ScaleFactor
        {
            get { return ScaleFactorFor(textScale); }
        }

        public static float ScaleFactorFor(TextScale scale)
        {
            switch (scale)
            {
                case TextScale.Small:
                    return SmallScaleFactor;
                case TextScale.Large:
                    return LargeScaleFactor;
                default:
                    return MediumScaleFactor;
            }
        }

        public static float ClampUiDistance(float value)
        {
            if (float.IsNaN(value))
            {
                return DefaultUiDistance;
            }

            return Mathf.Clamp(value, MinUiDistance, MaxUiDistance);
        }

        public static AppSettings Load()
        {
            AppSettings settings = new AppSettings();
            settings.textScale = ReadScale();
            settings.uiDistance = ClampUiDistance(PlayerPrefs.GetFloat(UiDistanceKey, DefaultUiDistance));
            settings.seatedMode = PlayerPrefs.GetInt(SeatedModeKey, 0) != 0;
            settings.snapTurn = PlayerPrefs.GetInt(SnapTurnKey, 1) != 0;
            settings.colorblindSafe = PlayerPrefs.GetInt(ColorblindSafeKey, 0) != 0;
            return settings;
        }

        public void Save()
        {
            PlayerPrefs.SetInt(TextScaleKey, (int)textScale);
            PlayerPrefs.SetFloat(UiDistanceKey, uiDistance);
            PlayerPrefs.SetInt(SeatedModeKey, seatedMode ? 1 : 0);
            PlayerPrefs.SetInt(SnapTurnKey, snapTurn ? 1 : 0);
            PlayerPrefs.SetInt(ColorblindSafeKey, colorblindSafe ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetTextScale(TextScale scale)
        {
            if (!Enum.IsDefined(typeof(TextScale), scale))
            {
                scale = TextScale.Medium;
            }

            if (textScale == scale)
            {
                return;
            }

            textScale = scale;
            RaiseChanged();
        }

        public void SetUiDistance(float value)
        {
            float clamped = ClampUiDistance(value);
            if (Mathf.Approximately(uiDistance, clamped))
            {
                return;
            }

            uiDistance = clamped;
            RaiseChanged();
        }

        public void SetSeatedMode(bool value)
        {
            if (seatedMode == value)
            {
                return;
            }

            seatedMode = value;
            RaiseChanged();
        }

        public void SetSnapTurn(bool value)
        {
            if (snapTurn == value)
            {
                return;
            }

            snapTurn = value;
            RaiseChanged();
        }

        public void SetColorblindSafe(bool value)
        {
            if (colorblindSafe == value)
            {
                return;
            }

            colorblindSafe = value;
            RaiseChanged();
        }

        private static TextScale ReadScale()
        {
            int stored = PlayerPrefs.GetInt(TextScaleKey, (int)TextScale.Medium);
            if (stored < (int)TextScale.Small || stored > (int)TextScale.Large)
            {
                return TextScale.Medium;
            }

            return (TextScale)stored;
        }

        private void RaiseChanged()
        {
            Action handler = Changed;
            if (handler != null)
            {
                handler();
            }
        }
    }
}
