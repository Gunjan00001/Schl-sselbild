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
        public const float ButtonHeight = 84f;
        public const float ToggleHeight = 72f;
        public const float SliderHeight = 72f;
        public const float Pad = 28f;
        public const float Gap = 18f;

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

        public static RectTransform Panel(Transform parent, string name, Color color, bool raycastTarget)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
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
            label.richText = false;
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
            Color fill = emphasized ? palette.Accent : palette.Panel;
            Color labelColor = emphasized ? palette.Background : palette.Text;

            RectTransform rect = CreateRect(name, parent);
            rect.sizeDelta = new Vector2(0f, ButtonHeight);
            SetPreferredHeight(rect, ButtonHeight);

            Image background = rect.gameObject.AddComponent<Image>();
            background.color = fill;
            background.raycastTarget = true;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.6f, 1.6f, 1.6f, 1f);
            colors.pressedColor = new Color(2.4f, 2.4f, 2.4f, 1f);
            colors.selectedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.06f;
            button.colors = colors;

            if (onClick != null)
            {
                button.onClick.AddListener(new UnityAction(onClick));
            }

            TextMeshProUGUI label = Label(rect, name + "Label", text, fontSize, TextAlignmentOptions.Center, labelColor);
            Stretch((RectTransform)label.transform, 14f, 14f, 6f, 6f);
            label.overflowMode = TextOverflowModes.Ellipsis;

            return new UiButtonHandle(button, background, label);
        }

        public static Slider CreateSlider(Transform parent, string name, float min, float max, float value, Action<float> onValueChanged)
        {
            UiPalette palette = UiTheme.Palette;
            float trackHeight = 14f;
            float inset = trackHeight * 0.5f;

            RectTransform rect = CreateRect(name, parent);
            rect.sizeDelta = new Vector2(0f, SliderHeight);
            SetPreferredHeight(rect, SliderHeight);

            RectTransform background = Panel(rect, name + "Background", UiTheme.Mix(palette.Panel, palette.Text, 0.25f), true);
            Stretch(background, 0f, 0f, (SliderHeight - trackHeight) * 0.5f, (SliderHeight - trackHeight) * 0.5f);

            RectTransform fillArea = CreateRect(name + "FillArea", rect);
            Stretch(fillArea, inset, inset, (SliderHeight - trackHeight) * 0.5f, (SliderHeight - trackHeight) * 0.5f);

            RectTransform fill = Panel(fillArea, name + "Fill", palette.Accent, false);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(0f, 1f);
            fill.pivot = new Vector2(0f, 0.5f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            fill.sizeDelta = new Vector2(0f, 0f);

            RectTransform handleArea = CreateRect(name + "HandleArea", rect);
            Stretch(handleArea, inset, inset, 0f, 0f);

            RectTransform handle = Panel(handleArea, name + "Handle", palette.Accent, true);
            handle.anchorMin = new Vector2(0f, 0.5f);
            handle.anchorMax = new Vector2(0f, 0.5f);
            handle.pivot = new Vector2(0.5f, 0.5f);
            handle.anchoredPosition = Vector2.zero;
            handle.sizeDelta = new Vector2(44f, 44f);

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

            RectTransform box = Panel(rect, name + "Box", UiTheme.Mix(palette.Panel, palette.Text, 0.2f), true);
            box.anchorMin = new Vector2(0f, 0.5f);
            box.anchorMax = new Vector2(0f, 0.5f);
            box.pivot = new Vector2(0f, 0.5f);
            box.anchoredPosition = Vector2.zero;
            box.sizeDelta = new Vector2(48f, 48f);

            RectTransform check = Panel(box, name + "Check", palette.Accent, false);
            Stretch(check, 11f, 11f, 11f, 11f);

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
            labelRect.offsetMin = new Vector2(72f, 0f);
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
            root.gameObject.AddComponent<RectMask2D>();

            RectTransform viewport = CreateRect(name + "Viewport", root);
            Stretch(viewport, 0f, 0f, 0f, 0f);

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
