using System;
using System.IO;
using Schlusselbild.Capture;
using Schlusselbild.Crypto;
using Schlusselbild.Ui;
using Schlusselbild.Ui.Screens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Schlusselbild.App
{
    public sealed class AppController : MonoBehaviour
    {
        private const int KeyHexLength = 64;
        private const string KeyFileName = "key.bin";

        private AppSettings settings;
        private RectTransform canvasRoot;
        private PassthroughScanner scanner;

        private HomeScreen homeScreen;
        private CreateScreen createScreen;
        private ScanScreen scanScreen;
        private DecodedScreen decodedScreen;
        private SettingsScreen settingsScreen;

        private RectTransform modalLayer;
        private RectTransform activeModal;
        private TextMeshProUGUI modalStatus;
        private TextMeshProUGUI keyValueLabel;
        private UiButtonHandle modalImport;
        private string pendingKeyText = string.Empty;

        private byte[] key;
        private string keyPath;
        private string keyError;
        private UiScreen currentScreen;
        private bool initialized;

        private enum UiScreen
        {
            Home,
            Create,
            Scan,
            Decoded,
            Settings
        }

        public AppSettings Settings
        {
            get { return settings; }
        }

        public bool HasKey
        {
            get { return key != null && key.Length == KeyStore.KeySize; }
        }

        public void Initialize(AppSettings appSettings, RectTransform screensRoot, PassthroughScanner passthroughScanner)
        {
            settings = appSettings == null ? AppSettings.Load() : appSettings;
            canvasRoot = screensRoot;
            scanner = passthroughScanner;

            UiTheme.SetTextScale(settings.TextScale);
            UiTheme.SetColorblindSafe(settings.ColorblindSafe);

            settings.Changed += HandleSettingsChanged;

            LoadKey();
            BuildUi();
            BindScanner();

            initialized = true;
            ShowHome();
        }

        public void ShowHome()
        {
            if (!initialized)
            {
                return;
            }

            Show(homeScreen, UiScreen.Home);
            homeScreen.SetKeyStatus(HasKey, KeyDetail());
        }

        public void ShowCreate()
        {
            if (!initialized)
            {
                return;
            }

            createScreen.SetKey(key);
            Show(createScreen, UiScreen.Create);
        }

        public void ShowScan()
        {
            if (!initialized)
            {
                return;
            }

            Show(scanScreen, UiScreen.Scan);

            if (scanner == null)
            {
                scanScreen.SetStatus(StatusKind.Invalid, "No passthrough scanner in the scene.");
                return;
            }

            scanner.BeginScan();
        }

        public void ShowDecoded(string plaintext)
        {
            if (!initialized)
            {
                return;
            }

            decodedScreen.SetPlaintext(plaintext);
            decodedScreen.SetCharacterCount(plaintext == null ? 0 : plaintext.Length);
            Show(decodedScreen, UiScreen.Decoded);
        }

        public void ShowSettings()
        {
            if (!initialized)
            {
                return;
            }

            Show(settingsScreen, UiScreen.Settings);
        }

        public void ShowHelp()
        {
            RectTransform column = BeginModal("Help", 800f);
            if (column == null)
            {
                return;
            }

            AddModalCaption(column, "Schl\u00fcsselbild \u2014 version " + VersionInfo.Version, UiTheme.Palette.Text);

            UiFactory.ScrollView(column, "HelpText", out _, out TextMeshProUGUI text);
            text.text =
                "Create page\n" +
                "Type a secret with the keypad, then Encrypt & preview to compose an encrypted page " +
                "(AES-256-GCM) printed as QR codes. Export PNG writes the page to the device storage.\n\n" +
                "Scan page\n" +
                "Point the headset at the printed page. The passthrough camera decodes the QR codes " +
                "and decrypts them with the local key. Nothing leaves the device.\n\n" +
                "Keys\n" +
                "The app creates a 32-byte key on first launch. Enter key lets you type the 64 " +
                "hexadecimal characters of another key.\n\n" +
                "Accessibility\n" +
                "Status is always shown as icon, word and colour together. Text scales in three " +
                "steps, and the settings screen carries the comfort options: UI distance, seated " +
                "mode and snap turn.";

            AddModalButtons(column, "Close", CloseModal, null, null);
        }

        public void ShowKeyDialog()
        {
            RectTransform column = BeginModal("Enter key", 800f);
            if (column == null)
            {
                return;
            }

            modalStatus = AddModalCaption(column, "0 of " + KeyHexLength + " hexadecimal characters.", UiTheme.WithAlpha(UiTheme.Palette.Text, 0.85f));

            TextMeshProUGUI value = UiFactory.Label(column, "KeyValue", string.Empty, UiTheme.BodyFontSize, TextAlignmentOptions.TopLeft, UiTheme.Palette.Text);
            UiFactory.SetPreferredHeight((RectTransform)value.transform, 84f);
            value.overflowMode = TextOverflowModes.Ellipsis;
            keyValueLabel = value;

            pendingKeyText = string.Empty;
            Keypad keypad = Keypad.Create(column, KeyHexLength);
            keypad.TextChanged += HandleKeyTextChanged;

            RectTransform modalButtons = AddRow(column, "ModalButtons", UiFactory.ButtonHeight, UiFactory.Gap);

            modalImport = UiFactory.CreateButton(modalButtons, "Import", "Import", ImportKey, UiTheme.ButtonFontSize, true);
            UiFactory.SetFlexible(modalImport.Rect, 1f, 1f);
            modalImport.Interactable = false;

            UiButtonHandle cancel = UiFactory.CreateButton(modalButtons, "Cancel", "Cancel", CloseModal, UiTheme.ButtonFontSize);
            UiFactory.SetFlexible(cancel.Rect, 1f, 1f);
        }

        private void HandleKeyTextChanged(string value)
        {
            pendingKeyText = value;

            if (keyValueLabel != null)
            {
                keyValueLabel.text = value;
            }

            bool parsed = KeyImporter.TryParse(value, out byte[] parsedKey, out string error);

            if (modalImport != null)
            {
                modalImport.Interactable = parsed && parsedKey != null;
            }

            if (modalStatus != null)
            {
                string state = parsed ? "\u2014 valid key." : "\u2014 " + (error == null ? "not a valid key yet." : error);
                modalStatus.text = value.Length + " of " + KeyHexLength + " characters " + state;
                modalStatus.color = parsed ? UiTheme.ResolveStatus(StatusKind.Ok, UiTheme.ColorblindSafe).Color : UiTheme.WithAlpha(UiTheme.Palette.Text, 0.85f);
            }
        }

        private void ImportKey()
        {
            if (!KeyImporter.TryParse(pendingKeyText, out byte[] parsed, out string error) || parsed == null)
            {
                if (modalStatus != null)
                {
                    modalStatus.text = error == null ? "Not a valid key." : error;
                    modalStatus.color = UiTheme.ResolveStatus(StatusKind.Invalid, UiTheme.ColorblindSafe).Color;
                }

                return;
            }

            try
            {
                KeyStore.Save(parsed, keyPath);
            }
            catch (Exception saveError)
            {
                if (modalStatus != null)
                {
                    modalStatus.text = "Could not store the key: " + saveError.Message;
                    modalStatus.color = UiTheme.ResolveStatus(StatusKind.Invalid, UiTheme.ColorblindSafe).Color;
                }

                return;
            }

            key = parsed;
            keyError = null;
            ApplyKeyToScanner();
            CloseModal();

            if (createScreen != null)
            {
                createScreen.SetKey(key);
            }

            if (homeScreen != null)
            {
                homeScreen.SetKeyStatus(HasKey, KeyDetail());
            }
        }

        private void BuildUi()
        {
            homeScreen = CreateScreenComponent<HomeScreen>("HomeScreen");
            homeScreen.Build(canvasRoot);
            homeScreen.CreateRequested += ShowCreate;
            homeScreen.ScanRequested += ShowScan;
            homeScreen.SettingsRequested += ShowSettings;
            homeScreen.HelpRequested += ShowHelp;
            homeScreen.EnterKeyRequested += ShowKeyDialog;

            createScreen = CreateScreenComponent<CreateScreen>("CreateScreen");
            createScreen.Build(canvasRoot);
            createScreen.BackRequested += ShowHome;
            createScreen.SetKey(key);

            scanScreen = CreateScreenComponent<ScanScreen>("ScanScreen");
            scanScreen.Build(canvasRoot);
            scanScreen.BackRequested += ShowHome;

            decodedScreen = CreateScreenComponent<DecodedScreen>("DecodedScreen");
            decodedScreen.Build(canvasRoot);
            decodedScreen.BackRequested += ShowHome;
            decodedScreen.ClearRequested += HandleClearDecoded;

            settingsScreen = CreateScreenComponent<SettingsScreen>("SettingsScreen");
            settingsScreen.Build(canvasRoot, settings);
            settingsScreen.BackRequested += ShowHome;

            modalLayer = UiFactory.CreateRect("Modals", canvasRoot);
            UiFactory.Stretch(modalLayer, 0f, 0f, 0f, 0f);
        }

        private void DestroyUi()
        {
            CloseModal();
            DestroyScreen(homeScreen);
            DestroyScreen(createScreen);
            DestroyScreen(scanScreen);
            DestroyScreen(decodedScreen);
            DestroyScreen(settingsScreen);

            homeScreen = null;
            createScreen = null;
            scanScreen = null;
            decodedScreen = null;
            settingsScreen = null;

            if (modalLayer != null)
            {
                modalLayer.SetParent(null, false);
                Destroy(modalLayer.gameObject);
                modalLayer = null;
            }
        }

        private void DestroyScreen(ScreenBase screen)
        {
            if (screen == null)
            {
                return;
            }

            screen.Teardown();
            screen.gameObject.SetActive(false);
            screen.transform.SetParent(null, false);
            Destroy(screen.gameObject);
        }

        private T CreateScreenComponent<T>(string name) where T : ScreenBase
        {
            GameObject created = new GameObject(name, typeof(RectTransform));
            RectTransform rect = (RectTransform)created.transform;
            rect.SetParent(canvasRoot, false);
            UiFactory.Stretch(rect, 0f, 0f, 0f, 0f);
            return created.AddComponent<T>();
        }

        private void Show(ScreenBase target, UiScreen kind)
        {
            HideAll();
            currentScreen = kind;
            target.Show();

            if (kind != UiScreen.Scan && scanner != null && scanner.IsScanning)
            {
                scanner.StopScan();
            }
        }

        private void HideAll()
        {
            Hide(homeScreen);
            Hide(createScreen);
            Hide(scanScreen);
            Hide(decodedScreen);
            Hide(settingsScreen);
        }

        private static void Hide(ScreenBase screen)
        {
            if (screen != null && screen.IsVisible)
            {
                screen.Hide();
            }
        }

        private void BindScanner()
        {
            if (scanner == null)
            {
                return;
            }

            scanner.Key = key;
            scanner.StatusChanged += HandleScannerStatus;
            scanner.Decoded += HandleScannerDecoded;
            scanner.Invalid += HandleScannerInvalid;
            scanner.FrameReady += HandleScannerFrame;
        }

        private void HandleScannerStatus(string message)
        {
            if (scanScreen == null || currentScreen != UiScreen.Scan)
            {
                return;
            }

            scanScreen.SetStatus(ClassifyStatus(message), message);
        }

        private void HandleScannerInvalid()
        {
            if (scanScreen == null)
            {
                return;
            }

            scanScreen.SetStatus(StatusKind.Invalid, "Page found but the key does not match.");
        }

        private void HandleScannerDecoded(string plaintext)
        {
            ShowDecoded(plaintext);
        }

        private void HandleScannerFrame(byte[] rgba)
        {
            if (scanScreen == null || currentScreen != UiScreen.Scan || scanner == null)
            {
                return;
            }

            Vector2Int resolution = scanner.CameraAccess != null ? scanner.CameraAccess.CurrentResolution : Vector2Int.zero;
            if (resolution.x <= 0 || resolution.y <= 0)
            {
                return;
            }

            scanScreen.ShowFrame(rgba, resolution.x, resolution.y);
        }

        private void HandleClearDecoded()
        {
            if (decodedScreen != null)
            {
                decodedScreen.Clear();
            }
        }

        private void HandleSettingsChanged()
        {
            if (!initialized || settings == null)
            {
                return;
            }

            bool rebuild = UiTheme.CurrentScale != settings.TextScale || UiTheme.ColorblindSafe != settings.ColorblindSafe;

            UiTheme.SetTextScale(settings.TextScale);
            UiTheme.SetColorblindSafe(settings.ColorblindSafe);

            if (!rebuild)
            {
                return;
            }

            UiScreen restoring = currentScreen;
            string decodedText = decodedScreen == null ? string.Empty : decodedScreen.Plaintext;
            DestroyUi();
            BuildUi();

            switch (restoring)
            {
                case UiScreen.Create:
                    ShowCreate();
                    break;
                case UiScreen.Scan:
                    ShowScan();
                    break;
                case UiScreen.Decoded:
                    ShowDecoded(decodedText);
                    break;
                case UiScreen.Settings:
                    ShowSettings();
                    break;
                default:
                    ShowHome();
                    break;
            }
        }

        private void LoadKey()
        {
            keyPath = Path.Combine(Application.persistentDataPath, KeyFileName);

            try
            {
                key = KeyStore.LoadOrCreate(keyPath);
                keyError = null;
            }
            catch (Exception error)
            {
                key = null;
                keyError = error.Message;
            }
        }

        private void ApplyKeyToScanner()
        {
            if (scanner != null)
            {
                scanner.Key = key;
            }
        }

        private string KeyDetail()
        {
            if (key == null)
            {
                return keyError == null ? "not loaded" : keyError;
            }

            return KeyStore.KeySize + "-byte key loaded";
        }

        private static StatusKind ClassifyStatus(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return StatusKind.Scanning;
            }

            string lower = message.ToLowerInvariant();

            if (lower.Contains("decoded"))
            {
                return StatusKind.Ok;
            }

            if (lower.Contains("invalid") || lower.Contains("wrong") || lower.Contains("not available") || lower.Contains("unavailable"))
            {
                return StatusKind.Invalid;
            }

            return StatusKind.Scanning;
        }

        private RectTransform BeginModal(string title, float height)
        {
            CloseModal();

            if (modalLayer == null)
            {
                return null;
            }

            UiPalette palette = UiTheme.Palette;

            RectTransform shade = UiFactory.Panel(modalLayer, "Modal", new Color(0f, 0f, 0f, 0.7f), true, 0, false);
            UiFactory.Stretch(shade, 0f, 0f, 0f, 0f);

            RectTransform panel = UiFactory.Panel(shade, "ModalPanel", palette.Panel, true);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = new Vector2(1150f, height);

            RectTransform column = UiFactory.VerticalLayout(panel, "ModalColumn", 12f, new RectOffset(28, 28, 24, 24));
            UiFactory.Stretch(column, 0f, 0f, 0f, 0f);

            TextMeshProUGUI titleLabel = UiFactory.Label(column, "ModalTitle", title, UiTheme.TitleFontSize * 0.8f, TextAlignmentOptions.Center, palette.Text);
            UiFactory.SetPreferredHeight((RectTransform)titleLabel.transform, UiTheme.TitleFontSize);

            activeModal = shade;
            return column;
        }

        private static TextMeshProUGUI AddModalCaption(Transform parent, string text, Color color)
        {
            TextMeshProUGUI caption = UiFactory.Label(parent, "Caption", text, UiTheme.BodyFontSize * 0.85f, TextAlignmentOptions.MidlineLeft, color);
            UiFactory.SetPreferredHeight((RectTransform)caption.transform, UiTheme.BodyFontSize * 1.3f);
            return caption;
        }

        private static void AddModalButtons(Transform parent, string confirmText, Action onConfirm, string cancelText, Action onCancel)
        {
            RectTransform row = UiFactory.Row(parent, "ModalButtons", UiFactory.Gap, new RectOffset());
            UiFactory.SetPreferredHeight(row, UiFactory.ButtonHeight);

            UiButtonHandle confirm = UiFactory.CreateButton(row, "Confirm", confirmText, onConfirm, UiTheme.ButtonFontSize, true);
            UiFactory.SetFlexible(confirm.Rect, 1f, 1f);

            if (cancelText == null)
            {
                return;
            }

            UiButtonHandle cancel = UiFactory.CreateButton(row, "Cancel", cancelText, onCancel, UiTheme.ButtonFontSize);
            UiFactory.SetFlexible(cancel.Rect, 1f, 1f);
        }

        private static RectTransform AddRow(Transform parent, string name, float height, float spacing)
        {
            RectTransform row = UiFactory.Row(parent, name, spacing, new RectOffset());
            UiFactory.SetPreferredHeight(row, height);
            return row;
        }

        private void CloseModal()
        {
            if (activeModal == null)
            {
                return;
            }

            activeModal.SetParent(null, false);
            Destroy(activeModal.gameObject);
            activeModal = null;
            keyValueLabel = null;
            modalImport = null;
            modalStatus = null;
        }

        private void OnDestroy()
        {
            if (settings != null)
            {
                settings.Changed -= HandleSettingsChanged;
            }

            if (scanner != null)
            {
                scanner.StatusChanged -= HandleScannerStatus;
                scanner.Decoded -= HandleScannerDecoded;
                scanner.Invalid -= HandleScannerInvalid;
                scanner.FrameReady -= HandleScannerFrame;
            }
        }
    }
}