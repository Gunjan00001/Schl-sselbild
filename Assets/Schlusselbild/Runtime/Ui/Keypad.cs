using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Schlusselbild.Ui
{
    public enum KeypadMode
    {
        Letters,
        Digits
    }

    public sealed class KeypadModel
    {
        public const int DefaultMaxLength = 1024;
        public const string SpaceKey = " ";

        public static readonly string[] LetterKeys =
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I",
            "J", "K", "L", "M", "N", "O", "P", "Q", "R",
            "S", "T", "U", "V", "W", "X", "Y", "Z"
        };

        public static readonly string[] DigitKeys =
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        };

        public static readonly string[] SymbolKeys =
        {
            ".", ",", "-", "!", "?", "@", ":", "/"
        };

        private readonly StringBuilder buffer = new StringBuilder();
        private readonly int maxLength;
        private KeypadMode mode;

        public KeypadModel()
            : this(DefaultMaxLength)
        {
        }

        public KeypadModel(int maxLength)
        {
            this.maxLength = maxLength < 1 ? 1 : maxLength;
            mode = KeypadMode.Letters;
        }

        public event Action<string> TextChanged;

        public KeypadMode Mode
        {
            get { return mode; }
        }

        public int MaxLength
        {
            get { return maxLength; }
        }

        public string Text
        {
            get { return buffer.ToString(); }
        }

        public int Length
        {
            get { return buffer.Length; }
        }

        public bool IsFull
        {
            get { return buffer.Length >= maxLength; }
        }

        public string[] CurrentKeys()
        {
            string[] modeKeys = mode == KeypadMode.Letters ? LetterKeys : DigitKeys;
            string[] keys = new string[modeKeys.Length + SymbolKeys.Length];
            Array.Copy(modeKeys, 0, keys, 0, modeKeys.Length);
            Array.Copy(SymbolKeys, 0, keys, modeKeys.Length, SymbolKeys.Length);
            return keys;
        }

        public bool Append(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            int room = maxLength - buffer.Length;
            if (room <= 0)
            {
                return false;
            }

            if (value.Length > room)
            {
                buffer.Append(value.Substring(0, room));
            }
            else
            {
                buffer.Append(value);
            }

            RaiseChanged();
            return true;
        }

        public bool Append(char value)
        {
            return Append(value.ToString());
        }

        public bool Backspace()
        {
            if (buffer.Length == 0)
            {
                return false;
            }

            buffer.Length = buffer.Length - 1;
            RaiseChanged();
            return true;
        }

        public bool Clear()
        {
            if (buffer.Length == 0)
            {
                return false;
            }

            buffer.Length = 0;
            RaiseChanged();
            return true;
        }

        public bool SetText(string value)
        {
            string next = value == null ? string.Empty : value;
            if (next.Length > maxLength)
            {
                next = next.Substring(0, maxLength);
            }

            if (buffer.ToString() == next)
            {
                return false;
            }

            buffer.Length = 0;
            buffer.Append(next);
            RaiseChanged();
            return true;
        }

        public KeypadMode ToggleMode()
        {
            mode = mode == KeypadMode.Letters ? KeypadMode.Digits : KeypadMode.Letters;
            return mode;
        }

        private void RaiseChanged()
        {
            Action<string> handler = TextChanged;
            if (handler != null)
            {
                handler(buffer.ToString());
            }
        }
    }

    public sealed class Keypad : MonoBehaviour
    {
        private const int Columns = 9;

        private readonly List<UiButtonHandle> keyButtons = new List<UiButtonHandle>();

        private KeypadModel model;
        private RectTransform gridRect;
        private UiButtonHandle modeButton;

        public event Action<string> TextChanged;

        public string Text
        {
            get { return model == null ? string.Empty : model.Text; }
        }

        public static Keypad Create(RectTransform parent, int maxLength)
        {
            GameObject created = new GameObject("Keypad", typeof(RectTransform));
            RectTransform rect = (RectTransform)created.transform;
            rect.SetParent(parent, false);
            Keypad keypad = created.AddComponent<Keypad>();
            keypad.Build(maxLength);
            return keypad;
        }

        public void Clear()
        {
            if (model != null)
            {
                model.Clear();
            }
        }

        private void Build(int maxLength)
        {
            model = new KeypadModel(maxLength);
            model.TextChanged += HandleTextChanged;

            RectTransform root = (RectTransform)transform;
            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            gridRect = UiFactory.CreateRect("Keys", root);
            GridLayoutGroup grid = gridRect.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(112f, 74f);
            grid.spacing = new Vector2(8f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Columns;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;

            RectTransform controls = UiFactory.Row(root, "Controls", 12f, new RectOffset());
            UiFactory.SetPreferredHeight(controls, UiFactory.ButtonHeight);
            float controlFont = UiTheme.FontSize(UiTheme.CurrentScale, UiTheme.ButtonBasePixels);

            modeButton = UiFactory.CreateButton(controls, "Mode", ModeButtonText(), ToggleMode, controlFont);
            UiFactory.SetFlexible(modeButton.Rect, 0.9f, 0f);

            UiButtonHandle space = UiFactory.CreateButton(controls, "Space", "Space", AppendSpace, controlFont);
            UiFactory.SetFlexible(space.Rect, 1f, 0f);

            UiButtonHandle backspace = UiFactory.CreateButton(controls, "Backspace", "Backspace", Backspace, controlFont);
            UiFactory.SetFlexible(backspace.Rect, 1.5f, 0f);

            UiButtonHandle clear = UiFactory.CreateButton(controls, "Clear", "Clear", Clear, controlFont);
            UiFactory.SetFlexible(clear.Rect, 1f, 0f);

            BuildKeys();
        }

        private void ToggleMode()
        {
            model.ToggleMode();
            modeButton.SetLabel(ModeButtonText());
            BuildKeys();
        }

        private string ModeButtonText()
        {
            return model != null && model.Mode == KeypadMode.Digits ? "ABC" : "123";
        }

        private void AppendSpace()
        {
            model.Append(KeypadModel.SpaceKey);
        }

        private void Backspace()
        {
            model.Backspace();
        }

        private void BuildKeys()
        {
            for (int i = 0; i < keyButtons.Count; i++)
            {
                UiButtonHandle handle = keyButtons[i];
                if (handle == null || handle.Rect == null)
                {
                    continue;
                }

                RectTransform rect = handle.Rect;
                rect.SetParent(null, false);
                Destroy(rect.gameObject);
            }

            keyButtons.Clear();

            string[] keys = model.CurrentKeys();
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                UiButtonHandle handle = UiFactory.CreateButton(gridRect, "Key " + key, key, MakeAppend(key), UiTheme.KeyFontSize);
                keyButtons.Add(handle);
            }
        }

        private Action MakeAppend(string key)
        {
            return delegate { model.Append(key); };
        }

        private void HandleTextChanged(string value)
        {
            Action<string> handler = TextChanged;
            if (handler != null)
            {
                handler(value);
            }
        }

        private void OnDestroy()
        {
            if (model != null)
            {
                model.TextChanged -= HandleTextChanged;
            }
        }
    }
}
