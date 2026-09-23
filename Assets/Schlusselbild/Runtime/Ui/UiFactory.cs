using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Schlusselbild.Ui
{
    public sealed class UiButtonHandle
    {
        private readonly Button buttonComponent;
        private readonly TextMeshProUGUI label;

        public UiButtonHandle(Button button, Image background, TextMeshProUGUI label)
        {
            buttonComponent = button;
            Background = background;
            this.label = label;
        }

        public Button ButtonComponent
        {
            get { return buttonComponent; }
        }

        public Image Background { get; private set; }

        public RectTransform Rect
        {
            get { return buttonComponent == null ? null : buttonComponent.transform as RectTransform; }
        }

        public TextMeshProUGUI Label
        {
            get { return label; }
        }

        public bool Interactable
        {
            get { return buttonComponent != null && buttonComponent.interactable; }
            set
            {
                if (buttonComponent != null)
                {
                    buttonComponent.interactable = value;
                }
            }
        }

        public void SetLabel(string text)
        {
            if (label != null)
            {
                label.text = text;
            }
        }
    }

    public static class UiFactory
    {
        public const float ButtonHeight = 96f;
        public const float ToggleHeight = 80f;
        public const float SliderHeight = 80f;
        public const float Pad = 44f;
        public const float Gap = 26f;

        public static GameObject CreateObject(string name, Transform parent)
        {
            GameObject created = new GameObject(name, typeof(RectTransform));
            RectTransform rect = (RectTransform)created.transform;
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.localPosition = Vector3.zero;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(0f, 0f);
            rect.offsetMax = new Vector2(0f, 0f);
            return created;
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            return (RectTransform)CreateObject(name, parent).transform;
        }

        public static RectTransform Stretch(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        public static LayoutElement SetPreferredHeight(RectTransform rect, float height)
        {
            LayoutElement element = rect.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = rect.gameObject.AddComponent<LayoutElement>();
            }

            element.minHeight = height;
            element.preferredHeight = height;
            return element;
        }

        public static LayoutElement SetFlexible(RectTransform rect, float width, float height)
        {
            LayoutElement element = rect.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = rect.gameObject.AddComponent<LayoutElement>();
            }

            element.flexibleWidth = width;
            element.flexibleHeight = height;
            return element;
        }

        public static LayoutElement SetPreferredWidth(RectTransform rect, float width)
        {
            LayoutElement element = rect.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = rect.gameObject.AddComponent<LayoutElement>();
            }

            element.minWidth = width;
            element.preferredWidth = width;
            element.flexibleWidth = 0f;
            return element;
        }

        public static RectTransform Panel(Transform parent, string name, Color color, bool raycastTarget)
        {
            return Panel(parent, name, color, raycastTarget, UiTheme.CardRadius, true);
        }

        public static RectTransform Panel(Transform parent, string name, Color color, bool raycastTarget, int radius, bool border)
        {
            RectTransform rect = CreateRect(name, parent);

            if (radius <= 0)
            {
                Image flat = rect.gameObject.AddComponent<Image>();
                flat.color = color;
                flat.raycastTarget = raycastTarget;
                return rect;
            }

            if (border)
            {
                if (radius >= 32)
                {
                    RectTransform shadowRect = CreateRect(name + "Shadow", rect);
                    shadowRect.anchorMin = Vector2.zero;
                    shadowRect.anchorMax = Vector2.one;
                    shadowRect.pivot = new Vector2(0.5f, 0.5f);
                    shadowRect.offsetMin = new Vector2(-4f, -12f);
                    shadowRect.offsetMax = new Vector2(4f, 6f);
                    Image shadow = shadowRect.gameObject.AddComponent<Image>();
                    UiSprites.ApplyRounded(shadow, radius, true);
                    shadow.color = new Color(0f, 0f, 0f, 0.35f);
                    shadow.raycastTarget = false;
                }

                RectTransform strokeRect = CreateRect(name + "Border", rect);
                Stretch(strokeRect, 0f, 0f, 0f, 0f);
                Image stroke = strokeRect.gameObject.AddComponent<Image>();
                UiSprites.ApplyStroke(stroke, radius, UiTheme.BorderThickness);
                stroke.color = UiTheme.Palette.Border;
            }

            RectTransform surfaceRect = CreateRect(name + "Surface", rect);
            float inset = border ? UiTheme.BorderThickness : 0f;
            Stretch(surfaceRect, inset, inset, inset, inset);
            Image surface = surfaceRect.gameObject.AddComponent<Image>();
            UiSprites.ApplyRounded(surface, Mathf.Max(2, radius - (int)inset), true);
            surface.color = color;
            surface.raycastTarget = raycastTarget;

            return rect;
        }

        public static RectTransform Surface(Transform parent, string name, Color color, int radius, bool raycastTarget)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            UiSprites.ApplyRounded(image, radius, true);
            image.color = color;
            image.raycastTarget = raycastTarget;
            return rect;
        }

        public static RectTransform VerticalLayout(Transform parent, string name, float spacing, RectOffset padding)
        {
            RectTransform rect = CreateRect(name, parent);
            VerticalLayoutGroup group = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            group.spacing = spacing;
            group.padding = padding == null ? new RectOffset() : padding;
            group.childAlignment = TextAnchor.UpperCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            return rect;
        }

        public static RectTransform Row(Transform parent, string name, float spacing, RectOffset padding)
        {
            RectTransform rect = CreateRect(name, parent);
            HorizontalLayoutGroup group = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.spacing = spacing;
            group.padding = padding == null ? new RectOffset() : padding;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = true;
            return rect;
        }

        public static TextMeshProUGUI Label(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = UiTheme.FontAsset;
            label.fontSize = fontSize;
            label.text = text;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            label.richText = true;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            return label;
        }

        public static UiButtonHandle CreateButton(Transform parent, string name, string text, Action onClick, float fontSize)
        {
            return CreateButton(parent, name, text, onClick, fontSize, false);
        }

        public static UiButtonHandle CreateButton(Transform parent, string name, string text, Action onClick, float fontSize, bool emphasized)
        {
            UiPalette palette = UiTheme.Palette;
            Color fill = emphasized ? palette.Accent : palette.PanelRaised;
            Color labelColor = emphasized ? palette.Background : palette.Text;

            RectTransform rect = CreateRect(name, parent);
            rect.sizeDelta = new Vector2(0f, ButtonHeight);
            SetPreferredHeight(rect, ButtonHeight);

            Image background = rect.gameObject.AddComponent<Image>();
            UiSprites.ApplyRounded(background, UiTheme.ButtonRadius, true);
            background.color = fill;
            background.raycastTarget = true;

            if (!emphasized)
            {
                RectTransform border = Surface(rect, name + "Border", palette.Border, UiTheme.ButtonRadius, false);
                Stretch(border, 0f, 0f, 0f, 0f);
            }

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            colors.pressedColor = new Color(0.86f, 0.86f, 0.86f, 1f);
            colors.selectedColor = new Color(1.06f, 1.06f, 1.06f, 1f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.35f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            if (onClick != null)
            {
                button.onClick.AddListener(new UnityAction(onClick));
            }

            TextMeshProUGUI label = Label(rect, name + "Label", text, fontSize, TextAlignmentOptions.Center, labelColor);
            Stretch((RectTransform)label.transform, 18f, 18f, 8f, 8f);
            label.overflowMode = TextOverflowModes.Ellipsis;

            return new UiButtonHandle(button, background, label);
        }

        public static Slider CreateSlider(Transform parent, string name, float min, float max, float value, Action<float> onValueChanged)
        {
            UiPalette palette = UiTheme.Palette;
            float trackHeight = 16f;
            float inset = trackHeight * 0.5f;

            RectTransform rect = CreateRect(name, parent);
            rect.sizeDelta = new Vector2(0f, SliderHeight);
            SetPreferredHeight(rect, SliderHeight);

            RectTransform background = Surface(rect, name + "Background", UiTheme.Mix(palette.Panel, palette.Text, 0.22f), 8, true);
            Stretch(background, 0f, 0f, (SliderHeight - trackHeight) * 0.5f, (SliderHeight - trackHeight) * 0.5f);

            RectTransform fillArea = CreateRect(name + "FillArea", rect);
            Stretch(fillArea, inset, inset, (SliderHeight - trackHeight) * 0.5f, (SliderHeight - trackHeight) * 0.5f);

            RectTransform fill = Surface(fillArea, name + "Fill", palette.Accent, 8, false);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(0f, 1f);
            fill.pivot = new Vector2(0f, 0.5f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            fill.sizeDelta = new Vector2(0f, 0f);

            RectTransform handleArea = CreateRect(name + "HandleArea", rect);
            Stretch(handleArea, inset, inset, 0f, 0f);

            RectTransform handle = Surface(handleArea, name + "Handle", palette.Text, 20, true);
            handle.anchorMin = new Vector2(0f, 0.5f);
            handle.anchorMax = new Vector2(0f, 0.5f);
            handle.pivot = new Vector2(0.5f, 0.5f);
            handle.anchoredPosition = Vector2.zero;
            handle.sizeDelta = new Vector2(46f, 46f);

            Slider slider = rect.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
            slider.value = Mathf.Clamp(value, min, max);

            if (onValueChanged != null)
            {
                slider.onValueChanged.AddListener(new UnityAction<float>(onValueChanged));
            }

            return slider;
        }

        public static Toggle CreateToggle(Transform parent, string name, string text, bool value, Action<bool> onValueChanged)
        {
            UiPalette palette = UiTheme.Palette;

            RectTransform rect = CreateRect(name, parent);
            rect.sizeDelta = new Vector2(0f, ToggleHeight);
            SetPreferredHeight(rect, ToggleHeight);

            RectTransform box = Surface(rect, name + "Box", UiTheme.Mix(palette.Panel, palette.Text, 0.16f), 12, true);
            box.anchorMin = new Vector2(0f, 0.5f);
            box.anchorMax = new Vector2(0f, 0.5f);
            box.pivot = new Vector2(0f, 0.5f);
            box.anchoredPosition = Vector2.zero;
            box.sizeDelta = new Vector2(52f, 52f);

            RectTransform check = Surface(box, name + "Check", palette.Accent, 8, false);
            Stretch(check, 12f, 12f, 12f, 12f);

            Toggle toggle = rect.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = box.GetComponent<Image>();
            toggle.graphic = check.GetComponent<Image>();
            toggle.toggleTransition = Toggle.ToggleTransition.Fade;
            toggle.isOn = value;

            TextMeshProUGUI label = Label(rect, name + "Label", text, UiTheme.BodyFontSize, TextAlignmentOptions.MidlineLeft, palette.Text);
            RectTransform labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.offsetMin = new Vector2(80f, 0f);
            labelRect.offsetMax = new Vector2(0f, 0f);
            label.overflowMode = TextOverflowModes.Ellipsis;

            if (onValueChanged != null)
            {
                toggle.onValueChanged.AddListener(new UnityAction<bool>(onValueChanged));
            }

            return toggle;
        }

        public static ScrollRect ScrollView(Transform parent, string name, out RectTransform content, out TextMeshProUGUI text)
        {
            UiPalette palette = UiTheme.Palette;

            RectTransform root = Panel(parent, name, palette.Background, true);
            SetFlexible(root, 1f, 1f);

            RectTransform viewport = CreateRect(name + "Viewport", root);
            Stretch(viewport, 8f, 34f, 8f, 8f);
            viewport.gameObject.AddComponent<RectMask2D>();

            content = CreateRect(name + "Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup group = content.gameObject.AddComponent<VerticalLayoutGroup>();
            group.spacing = Gap;
            group.padding = new RectOffset((int)Pad, (int)Pad, (int)Pad, (int)Pad);
            group.childAlignment = TextAnchor.UpperLeft;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            text = Label(content, name + "Text", string.Empty, UiTheme.BodyFontSize, TextAlignmentOptions.TopLeft, palette.Text);
            text.overflowMode = TextOverflowModes.Overflow;

            ScrollRect scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            scroll.inertia = true;

            RectTransform track = Surface(root, name + "Scrollbar", UiTheme.Mix(palette.Background, palette.Border, 0.7f), 8, false);
            track.anchorMin = new Vector2(1f, 0f);
            track.anchorMax = new Vector2(1f, 1f);
            track.pivot = new Vector2(1f, 0.5f);
            track.offsetMin = new Vector2(-24f, 12f);
            track.offsetMax = new Vector2(-12f, -12f);

            RectTransform handle = Surface(track, name + "Handle", UiTheme.Mix(palette.Panel, palette.Text, 0.4f), 8, true);
            handle.anchorMin = Vector2.zero;
            handle.anchorMax = Vector2.one;
            handle.offsetMin = Vector2.zero;
            handle.offsetMax = Vector2.zero;

            Scrollbar scrollbar = track.gameObject.AddComponent<Scrollbar>();
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handle.GetComponent<Image>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            return scroll;
        }

        public static RawImage CreateRawImage(Transform parent, string name, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            RawImage image = rect.gameObject.AddComponent<RawImage>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
    }
}
