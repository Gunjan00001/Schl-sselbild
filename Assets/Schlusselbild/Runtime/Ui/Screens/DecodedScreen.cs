using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Schlusselbild.Ui.Screens
{
    public sealed class DecodedScreen : ScreenBase
    {
        private TextMeshProUGUI banner;
        private TextMeshProUGUI plaintext;
        private ScrollRect scroll;

        public event Action BackRequested;
        public event Action ClearRequested;

        public string Plaintext
        {
            get { return plaintext == null ? string.Empty : plaintext.text; }
        }

        public void Build(RectTransform parent)
        {
            UiPalette palette = UiTheme.Palette;

            BuildRoot(parent, "DecodedScreen");
            RectTransform body = BuildBody("Decoded");

            RectTransform bannerPanel = UiFactory.Panel(body, "Banner", palette.Panel, true);
            UiFactory.SetPreferredHeight(bannerPanel, 76f);
            banner = UiFactory.Label(bannerPanel, "BannerText", string.Empty, UiTheme.BodyFontSize, TextAlignmentOptions.Center, palette.Text);
            UiFactory.Stretch((RectTransform)banner.transform, 16f, 16f, 8f, 8f);

            scroll = UiFactory.ScrollView(body, "Plaintext", out RectTransform content, out TextMeshProUGUI text);
            plaintext = text;

            RectTransform actions = AddRow(body, "Actions", UiFactory.ButtonHeight, UiFactory.Gap);

            UiButtonHandle back = UiFactory.CreateButton(actions, "Back", "Back", RaiseBack, UiTheme.ButtonFontSize, true);
            UiFactory.SetFlexible(back.Rect, 1f, 1f);

            UiButtonHandle clear = UiFactory.CreateButton(actions, "Clear", "Clear", RaiseClear, UiTheme.ButtonFontSize);
            UiFactory.SetFlexible(clear.Rect, 1f, 1f);

            SetStatus(StatusKind.Ok, "Nothing decoded yet.");
        }

        public void SetPlaintext(string value)
        {
            if (plaintext != null)
            {
                plaintext.text = value == null ? string.Empty : value;
            }

            if (scroll != null)
            {
                scroll.verticalNormalizedPosition = 1f;
            }
        }

        public void Clear()
        {
            SetPlaintext(string.Empty);
            SetStatus(StatusKind.Ok, "Cleared.");
        }

        public void SetStatus(StatusKind kind, string message)
        {
            if (banner == null)
            {
                return;
            }

            StatusStyle style = UiTheme.ResolveStatus(kind, UiTheme.ColorblindSafe);
            banner.text = style.Icon + "  " + style.Word + "  \u2014  " + message;
            banner.color = style.Color;
        }

        public void SetCharacterCount(int count)
        {
            SetStatus(StatusKind.Ok, count + " characters decrypted");
        }

        private void RaiseBack()
        {
            Action handler = BackRequested;
            if (handler != null)
            {
                handler();
            }
        }

        private void RaiseClear()
        {
            Action handler = ClearRequested;
            if (handler != null)
            {
                handler();
            }
        }
    }
}