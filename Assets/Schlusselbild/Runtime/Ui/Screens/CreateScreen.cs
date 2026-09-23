using System;
using System.IO;
using Schlusselbild.Page;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Schlusselbild.Ui.Screens
{
    public sealed class CreateScreen : ScreenBase
    {
        private TextMeshProUGUI secretLabel;
        private TextMeshProUGUI statusLabel;
        private TextMeshProUGUI previewHint;
        private RawImage preview;
        private AspectRatioFitter previewFitter;
        private Keypad keypad;
        private byte[] key;
        private Texture2D page;

        public event Action BackRequested;

        public string Secret
        {
            get { return keypad == null ? string.Empty : keypad.Text; }
        }

        public Texture2D Page
        {
            get { return page; }
        }

        public void Build(RectTransform parent)
        {
            UiPalette palette = UiTheme.Palette;

            BuildRoot(parent, "CreateScreen");
            RectTransform body = BuildBody("Create page");

            RectTransform entryRow = AddRow(body, "EntryRow", 176f, UiFactory.Gap);

            RectTransform secretPanel = UiFactory.Panel(entryRow, "SecretPanel", palette.Panel, true);
            UiFactory.SetFlexible(secretPanel, 2.2f, 1f);
            RectTransform secretColumn = UiFactory.VerticalLayout(secretPanel, "SecretColumn", 8f, new RectOffset(20, 20, 14, 14));
            UiFactory.Stretch(secretColumn, 0f, 0f, 0f, 0f);

            AddCaption(secretColumn, "SecretCaption", "Secret (keypad below, up to 1024 characters)", UiTheme.WithAlpha(palette.Text, 0.8f));
            secretLabel = UiFactory.Label(secretColumn, "SecretText", string.Empty, UiTheme.BodyFontSize, TextAlignmentOptions.TopLeft, palette.Text);
            UiFactory.SetFlexible((RectTransform)secretLabel.transform, 1f, 1f);
            secretLabel.overflowMode = TextOverflowModes.Ellipsis;

            RectTransform previewPanel = UiFactory.Panel(entryRow, "PreviewPanel", palette.Background, true);
            UiFactory.SetFlexible(previewPanel, 1f, 1f);

            preview = UiFactory.CreateRawImage(previewPanel, "PreviewImage", Color.white);
            UiFactory.Stretch((RectTransform)preview.transform, 10f, 10f, 10f, 10f);
            previewFitter = preview.gameObject.AddComponent<AspectRatioFitter>();
            previewFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            previewFitter.aspectRatio = 0.71f;
            preview.enabled = false;

            previewHint = UiFactory.Label(previewPanel, "PreviewHint", "Preview", UiTheme.CaptionFontSize, TextAlignmentOptions.Bottom, UiTheme.WithAlpha(palette.Text, 0.75f));
            UiFactory.Stretch((RectTransform)previewHint.transform, 12f, 12f, 0f, 0f);

            keypad = Keypad.Create(body, KeypadModel.DefaultMaxLength);
            keypad.TextChanged += HandleSecretChanged;

            RectTransform actions = AddRow(body, "Actions", UiFactory.ButtonHeight, UiFactory.Gap);

            UiButtonHandle compose = UiFactory.CreateButton(actions, "Compose", "Encrypt & preview", ComposePreview, UiTheme.ButtonFontSize, true);
            UiFactory.SetFlexible(compose.Rect, 1.5f, 1f);

            UiButtonHandle export = UiFactory.CreateButton(actions, "Export", "Export PNG", ExportPage, UiTheme.ButtonFontSize);
            UiFactory.SetFlexible(export.Rect, 1f, 1f);

            UiButtonHandle selfTest = UiFactory.CreateButton(actions, "SelfTest", "Self-test", RunSelfTest, UiTheme.ButtonFontSize);
            UiFactory.SetFlexible(selfTest.Rect, 1f, 1f);

            UiButtonHandle back = UiFactory.CreateButton(actions, "Back", "Back", RaiseBack, UiTheme.ButtonFontSize);
            UiFactory.SetFlexible(back.Rect, 0.8f, 1f);

            statusLabel = AddCaption(body, "Status", "Ready.", UiTheme.WithAlpha(palette.Text, 0.85f));
        }

        public void SetKey(byte[] value)
        {
            key = value;
        }

        public void ResetSecret()
        {
            if (keypad != null)
            {
                keypad.Clear();
            }
        }

        private void HandleSecretChanged(string value)
        {
            if (secretLabel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                secretLabel.text = "Enter the secret with the keypad below.";
                secretLabel.color = UiTheme.WithAlpha(UiTheme.Palette.Text, 0.6f);
            }
            else
            {
                secretLabel.text = value;
                secretLabel.color = UiTheme.Palette.Text;
            }
        }

        private void ComposePreview()
        {
            string secret = Secret;
            if (!ValidateSecret(secret))
            {
                return;
            }

            try
            {
                Texture2D composed = PageComposer.Compose(secret, key);
                ShowPage(composed);
                SetStatus(StatusKind.Ok, "Page composed: " + composed.width + " x " + composed.height + " pixels.");
            }
            catch (Exception error)
            {
                SetStatus(StatusKind.Invalid, "Compose failed: " + error.Message);
            }
        }

        private void RunSelfTest()
        {
            string secret = Secret;
            if (!ValidateSecret(secret))
            {
                return;
            }

            Texture2D composed = null;
            try
            {
                composed = PageComposer.Compose(secret, key);
                string decoded = PageDecoder.Decode(composed, key);
                ShowPage(composed);

                if (decoded == secret)
                {
                    SetStatus(StatusKind.Ok, "Self-test: the composed page decodes back to the same secret.");
                }
                else
                {
                    SetStatus(StatusKind.Invalid, "Self-test: the decoded text differs from the secret.");
                }
            }
            catch (Exception error)
            {
                if (composed != null)
                {
                    DestroySafely(composed);
                }

                SetStatus(StatusKind.Invalid, "Self-test: " + error.Message);
            }
        }

        private void ExportPage()
        {
            string secret = Secret;
            if (!ValidateSecret(secret))
            {
                return;
            }

            try
            {
                if (page == null)
                {
                    ShowPage(PageComposer.Compose(secret, key));
                }

                byte[] png = page.EncodeToPNG();
                string path = Path.Combine(Application.persistentDataPath, "page_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
                File.WriteAllBytes(path, png);
                SetStatus(StatusKind.Ok, "Exported " + png.Length + " bytes to " + path);
            }
            catch (Exception error)
            {
                SetStatus(StatusKind.Invalid, "Export failed: " + error.Message);
            }
        }

        private bool ValidateSecret(string secret)
        {
            if (key == null || key.Length == 0)
            {
                SetStatus(StatusKind.Invalid, "Encryption key unavailable.");
                return false;
            }

            if (string.IsNullOrEmpty(secret))
            {
                SetStatus(StatusKind.Invalid, "Enter a secret with the keypad first.");
                return false;
            }

            return true;
        }

        private void ShowPage(Texture2D composed)
        {
            if (page != composed)
            {
                DestroySafely(page);
                page = composed;
            }

            if (preview == null || page == null)
            {
                return;
            }

            preview.texture = page;
            preview.enabled = true;
            previewFitter.aspectRatio = (float)page.width / page.height;

            if (previewHint != null)
            {
                previewHint.gameObject.SetActive(false);
            }
        }

        private void SetStatus(StatusKind kind, string message)
        {
            if (statusLabel == null)
            {
                return;
            }

            StatusStyle style = UiTheme.ResolveStatus(kind, UiTheme.ColorblindSafe);
            statusLabel.text = style.Icon + " " + style.Word + " \u2014 " + message;
            statusLabel.color = style.Color;
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
            if (keypad != null)
            {
                keypad.TextChanged -= HandleSecretChanged;
            }

            DestroySafely(page);
            page = null;
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