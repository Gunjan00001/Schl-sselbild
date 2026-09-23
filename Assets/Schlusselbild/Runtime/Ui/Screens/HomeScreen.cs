using System;
using TMPro;
using UnityEngine;

namespace Schlusselbild.Ui.Screens
{
    public abstract class ScreenBase : MonoBehaviour
    {
        private RectTransform root;

        public RectTransform Root
        {
            get { return root; }
        }

        public bool IsVisible
        {
            get { return root != null && root.gameObject.activeSelf; }
        }

        protected RectTransform BuildRoot(RectTransform parent, string name)
        {
            root = UiFactory.CreateRect(name, parent);
            UiFactory.Stretch(root, 0f, 0f, 0f, 0f);
            return root;
        }

        protected RectTransform BuildBody(string title)
        {
            RectTransform body = UiFactory.VerticalLayout(root, "Body", UiFactory.Gap, new RectOffset(40, 40, 32, 32));
            UiFactory.Stretch(body, 0f, 0f, 0f, 0f);

            if (!string.IsNullOrEmpty(title))
            {
                TextMeshProUGUI titleLabel = UiFactory.Label(body, "Title", title, UiTheme.TitleFontSize, TextAlignmentOptions.Center, UiTheme.Palette.Text);
                UiFactory.SetPreferredHeight((RectTransform)titleLabel.transform, UiTheme.TitleFontSize * 1.45f);
            }

            return body;
        }

        protected RectTransform AddRow(Transform parent, string name, float height, float spacing)
        {
            RectTransform row = UiFactory.Row(parent, name, spacing, new RectOffset());
            UiFactory.SetPreferredHeight(row, height);
            return row;
        }

        protected TextMeshProUGUI AddCaption(Transform parent, string name, string text, Color color)
        {
            TextMeshProUGUI caption = UiFactory.Label(parent, name, text, UiTheme.CaptionFontSize, TextAlignmentOptions.MidlineLeft, color);
            UiFactory.SetPreferredHeight((RectTransform)caption.transform, UiTheme.CaptionFontSize * 1.6f);
            return caption;
        }

        protected RectTransform AddSpacer(Transform parent, string name)
        {
            RectTransform spacer = UiFactory.CreateRect(name, parent);
            UiFactory.SetFlexible(spacer, 1f, 1f);
            UiFactory.SetPreferredHeight(spacer, 0f);
            return spacer;
        }

        public virtual void Show()
        {
            if (root == null)
            {
                return;
            }

            root.gameObject.SetActive(true);
            OnShown();
        }

        public virtual void Hide()
        {
            if (root == null)
            {
                return;
            }

            root.gameObject.SetActive(false);
            OnHidden();
        }

        public virtual void Teardown()
        {
        }

        protected virtual void OnShown()
        {
        }

        protected virtual void OnHidden()
        {
        }
    }

    public sealed class HomeScreen : ScreenBase
    {
        private TextMeshProUGUI keyStatus;
        private UiButtonHandle createButton;
        private UiButtonHandle scanButton;

        public event Action CreateRequested;
        public event Action ScanRequested;
        public event Action SettingsRequested;
        public event Action HelpRequested;
        public event Action EnterKeyRequested;

        public void Build(RectTransform parent)
        {
            BuildRoot(parent, "HomeScreen");
            RectTransform body = BuildBody("Schl\u00fcsselbild");

            AddCaption(body, "Subtitle", "Encrypted page on a Meta Quest 3", UiTheme.WithAlpha(UiTheme.Palette.Text, 0.8f));
            AddSpacer(body, "Spacer");

            createButton = UiFactory.CreateButton(body, "Create", "Create page", RaiseCreate, UiTheme.ButtonFontSize * 1.15f, true);
            UiFactory.SetPreferredHeight(createButton.Rect, 116f);

            scanButton = UiFactory.CreateButton(body, "Scan", "Scan page", RaiseScan, UiTheme.ButtonFontSize * 1.15f, true);
            UiFactory.SetPreferredHeight(scanButton.Rect, 116f);

            RectTransform keyRow = AddRow(body, "KeyRow", UiFactory.ButtonHeight, UiFactory.Gap);
            keyStatus = UiFactory.Label(keyRow, "KeyStatus", string.Empty, UiTheme.BodyFontSize, TextAlignmentOptions.MidlineLeft, UiTheme.Palette.Text);
            UiFactory.SetFlexible((RectTransform)keyStatus.transform, 2.4f, 1f);
            keyStatus.overflowMode = TextOverflowModes.Ellipsis;

            UiButtonHandle enterKey = UiFactory.CreateButton(keyRow, "EnterKey", "Enter key", RaiseEnterKey, UiTheme.BodyFontSize);
            UiFactory.SetFlexible(enterKey.Rect, 1.2f, 1f);

            RectTransform optionsRow = AddRow(body, "OptionsRow", UiFactory.ButtonHeight, UiFactory.Gap);
            UiButtonHandle settings = UiFactory.CreateButton(optionsRow, "Settings", "Settings", RaiseSettings, UiTheme.BodyFontSize);
            UiFactory.SetFlexible(settings.Rect, 1f, 1f);
            UiButtonHandle help = UiFactory.CreateButton(optionsRow, "Help", "Help", RaiseHelp, UiTheme.BodyFontSize);
            UiFactory.SetFlexible(help.Rect, 1f, 1f);

            AddSpacer(body, "SpacerBottom");
            AddCaption(body, "Version", "Version " + Schlusselbild.App.VersionInfo.Version, UiTheme.WithAlpha(UiTheme.Palette.Text, 0.8f));
        }

        public void SetKeyStatus(bool loaded, string detail)
        {
            if (keyStatus == null)
            {
                return;
            }

            UiPalette palette = UiTheme.Palette;
            StatusStyle style = UiTheme.ResolveStatus(loaded ? StatusKind.Ok : StatusKind.Invalid, UiTheme.ColorblindSafe);
            keyStatus.text = style.Icon + " Key " + style.Word + (string.IsNullOrEmpty(detail) ? string.Empty : " \u2014 " + detail);
            keyStatus.color = loaded ? palette.Text : style.Color;
        }

        public void SetBusy(bool busy)
        {
            if (createButton != null)
            {
                createButton.Interactable = !busy;
            }

            if (scanButton != null)
            {
                scanButton.Interactable = !busy;
            }
        }

        private void RaiseCreate()
        {
            Action handler = CreateRequested;
            if (handler != null)
            {
                handler();
            }
        }

        private void RaiseScan()
        {
            Action handler = ScanRequested;
            if (handler != null)
            {
                handler();
            }
        }

        private void RaiseSettings()
        {
            Action handler = SettingsRequested;
            if (handler != null)
            {
                handler();
            }
        }

        private void RaiseHelp()
        {
            Action handler = HelpRequested;
            if (handler != null)
            {
                handler();
            }
        }

        private void RaiseEnterKey()
        {
            Action handler = EnterKeyRequested;
            if (handler != null)
            {
                handler();
            }
        }
    }
}