using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Schlusselbild.Ui.Screens
{
    public abstract class ScreenBase : MonoBehaviour
    {
        private RectTransform root;
        private CanvasGroup canvasGroup;
        private Coroutine fadeRoutine;

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
            canvasGroup = root.gameObject.AddComponent<CanvasGroup>();
            return root;
        }

        protected RectTransform BuildBody(string title)
        {
            RectTransform body = UiFactory.VerticalLayout(root, "Body", UiFactory.Gap, new RectOffset(44, 44, 36, 36));
            UiFactory.Stretch(body, 0f, 0f, 0f, 0f);

            if (!string.IsNullOrEmpty(title))
            {
                TextMeshProUGUI titleLabel = UiFactory.Label(body, "Title", title, UiTheme.TitleFontSize, TextAlignmentOptions.Center, UiTheme.Palette.Text);
                UiFactory.SetPreferredHeight((RectTransform)titleLabel.transform, UiTheme.TitleFontSize * 1.5f);

                RectTransform underline = UiFactory.Surface((RectTransform)titleLabel.transform, "TitleAccent", UiTheme.Palette.Accent, 3, false);
                underline.anchorMin = new Vector2(0.5f, 0f);
                underline.anchorMax = new Vector2(0.5f, 0f);
                underline.pivot = new Vector2(0.5f, 1f);
                underline.anchoredPosition = new Vector2(0f, -6f);
                underline.sizeDelta = new Vector2(132f, 6f);
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

            if (canvasGroup != null && isActiveAndEnabled)
            {
                if (fadeRoutine != null)
                {
                    StopCoroutine(fadeRoutine);
                }

                fadeRoutine = StartCoroutine(FadeIn());
            }
        }

        public virtual void Hide()
        {
            if (root == null)
            {
                return;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            root.gameObject.SetActive(false);
            OnHidden();
        }

        private IEnumerator FadeIn()
        {
            const float duration = 0.16f;
            float elapsed = 0f;
            canvasGroup.alpha = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                root.localScale = Vector3.one * Mathf.Lerp(0.985f, 1f, canvasGroup.alpha);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            root.localScale = Vector3.one;
            fadeRoutine = null;
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

        public event Action CreateRequested;
        public event Action ScanRequested;
        public event Action SettingsRequested;
        public event Action HelpRequested;
        public event Action EnterKeyRequested;

        public void Build(RectTransform parent)
        {
            BuildRoot(parent, "HomeScreen");
            RectTransform body = BuildBody("Schl\u00fcsselbild");

            AddCaption(body, "Subtitle", "Your secret, invisible on paper.", UiTheme.WithAlpha(UiTheme.Palette.Text, 0.8f));
            AddSpacer(body, "Spacer");

            UiButtonHandle createButton = UiFactory.CreateButton(body, "Create", "Create page", RaiseCreate, UiTheme.ButtonFontSize * 1.15f, true);
            UiFactory.SetPreferredHeight(createButton.Rect, 116f);

            UiButtonHandle scanButton = UiFactory.CreateButton(body, "Scan", "Scan page", RaiseScan, UiTheme.ButtonFontSize * 1.15f, true);
            UiFactory.SetPreferredHeight(scanButton.Rect, 116f);

            RectTransform keyRow = AddRow(body, "KeyRow", UiFactory.ButtonHeight, UiFactory.Gap);
            keyStatus = UiFactory.Label(keyRow, "KeyStatus", string.Empty, UiTheme.BodyFontSize, TextAlignmentOptions.MidlineLeft, UiTheme.Palette.Text);
            UiFactory.SetFlexible((RectTransform)keyStatus.transform, 1f, 1f);
            keyStatus.overflowMode = TextOverflowModes.Ellipsis;

            UiButtonHandle enterKey = UiFactory.CreateButton(keyRow, "EnterKey", "Enter key", RaiseEnterKey, UiTheme.BodyFontSize);
            UiFactory.SetPreferredWidth(enterKey.Rect, 320f);

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