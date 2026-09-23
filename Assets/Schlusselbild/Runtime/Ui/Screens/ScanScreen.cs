using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Schlusselbild.Ui.Screens
{
    public sealed class ScanScreen : ScreenBase
    {
        private TextMeshProUGUI statusLabel;
        private TextMeshProUGUI hintLabel;
        private TextMeshProUGUI sizeLabel;
        private RawImage preview;
        private RectTransform reticle;
        private Texture2D frame;

        public event Action BackRequested;

        public void Build(RectTransform parent)
        {
            UiPalette palette = UiTheme.Palette;

            BuildRoot(parent, "ScanScreen");
            RectTransform body = BuildBody("Scan page");

            RectTransform banner = UiFactory.Panel(body, "StatusBanner", palette.Panel, true);
            UiFactory.SetPreferredHeight(banner, 76f);
            statusLabel = UiFactory.Label(banner, "StatusText", string.Empty, UiTheme.BodyFontSize, TextAlignmentOptions.Center, palette.Text);
            UiFactory.Stretch((RectTransform)statusLabel.transform, 16f, 16f, 8f, 8f);

            RectTransform previewPanel = UiFactory.Panel(body, "PreviewPanel", palette.Background, true);
            UiFactory.SetFlexible(previewPanel, 1f, 1f);

            preview = UiFactory.CreateRawImage(previewPanel, "PreviewImage", Color.white);
            RectTransform previewRect = (RectTransform)preview.transform;
            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewRect.pivot = new Vector2(0.5f, 0.5f);
            previewRect.anchoredPosition = Vector2.zero;
            previewRect.sizeDelta = new Vector2(380f, 380f);
            preview.enabled = false;

            reticle = UiFactory.CreateRect("Reticle", previewPanel);
            reticle.anchorMin = new Vector2(0.5f, 0.5f);
            reticle.anchorMax = new Vector2(0.5f, 0.5f);
            reticle.pivot = new Vector2(0.5f, 0.5f);
            reticle.anchoredPosition = Vector2.zero;
            reticle.sizeDelta = new Vector2(120f, 120f);
            BuildReticle(reticle, palette);

            sizeLabel = UiFactory.Label(previewPanel, "FrameSize", string.Empty, UiTheme.CaptionFontSize, TextAlignmentOptions.BottomLeft, UiTheme.WithAlpha(palette.Text, 0.8f));
            UiFactory.Stretch((RectTransform)sizeLabel.transform, 16f, 16f, 0f, 8f);

            hintLabel = AddCaption(body, "Hint", "Point the headset at the printed page.", UiTheme.WithAlpha(palette.Text, 0.8f));

            RectTransform actions = AddRow(body, "Actions", UiFactory.ButtonHeight, UiFactory.Gap);
            UiButtonHandle back = UiFactory.CreateButton(actions, "Back", "Back", RaiseBack, UiTheme.ButtonFontSize, true);
            UiFactory.SetFlexible(back.Rect, 1f, 1f);

            SetStatus(StatusKind.Scanning, "Waiting for the passthrough camera.");
        }

        public void SetStatus(StatusKind kind, string message)
        {
            UiPalette palette = UiTheme.Palette;
            StatusStyle style = UiTheme.ResolveStatus(kind, UiTheme.ColorblindSafe);

            if (statusLabel != null)
            {
                statusLabel.text = style.Icon + "  " + style.Word;
                statusLabel.color = style.Color;
            }

            if (hintLabel != null)
            {
                hintLabel.text = message;
                hintLabel.color = UiTheme.WithAlpha(palette.Text, 0.85f);
            }

            SetScanning(kind == StatusKind.Scanning);
        }

        public void SetScanning(bool scanning)
        {
            if (reticle != null)
            {
                reticle.gameObject.SetActive(scanning);
            }
        }

        public void ShowFrame(byte[] rgba, int width, int height)
        {
            if (rgba == null || width <= 0 || height <= 0 || rgba.Length < width * height * 4)
            {
                return;
            }

            if (frame == null || frame.width != width || frame.height != height)
            {
                DestroySafely(frame);
                frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
                frame.wrapMode = TextureWrapMode.Clamp;
                preview.texture = frame;
            }

            frame.LoadRawTextureData(rgba);
            frame.Apply(false, false);

            preview.enabled = true;
            preview.rectTransform.sizeDelta = new Vector2(420f, 420f * height / width);

            if (sizeLabel != null)
            {
                sizeLabel.text = width + " x " + height + " passthrough frame";
            }
        }

        public void ClearFrame()
        {
            if (preview != null)
            {
                preview.enabled = false;
                preview.texture = null;
            }

            DestroySafely(frame);
            frame = null;
        }

        protected override void OnHidden()
        {
            ClearFrame();
            SetScanning(false);
        }

        private void BuildReticle(RectTransform parent, UiPalette palette)
        {
            Color color = UiTheme.WithAlpha(palette.Accent, 0.9f);

            RectTransform horizontal = UiFactory.Panel(parent, "ReticleHorizontal", color, false);
            UiFactory.Stretch(horizontal, 0f, 0f, 58f, 58f);

            RectTransform vertical = UiFactory.Panel(parent, "ReticleVertical", color, false);
            UiFactory.Stretch(vertical, 58f, 58f, 0f, 0f);

            RectTransform center = UiFactory.Panel(parent, "ReticleCenter", color, false);
            center.anchorMin = new Vector2(0.5f, 0.5f);
            center.anchorMax = new Vector2(0.5f, 0.5f);
            center.pivot = new Vector2(0.5f, 0.5f);
            center.anchoredPosition = Vector2.zero;
            center.sizeDelta = new Vector2(10f, 10f);
        }

        private void RaiseBack()
        {
            Action handler = BackRequested;
            if (handler != null)
            {
                handler();
            }
        }

        private void OnDestroy()
        {
            ClearFrame();
        }

        private static void DestroySafely(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}