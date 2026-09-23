# Graph Report - D:\Schl-sselbild\Assets\Schlusselbild  (2026-09-23)

## Corpus Check
- Corpus is ~20,362 words - fits in a single context window. You may not need a graph.

## Summary
- 731 nodes · 1210 edges · 43 communities
- Extraction: 99% EXTRACTED · 1% INFERRED · 0% AMBIGUOUS · INFERRED: 12 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_AppController|AppController]]
- [[_COMMUNITY_UiFactory|UiFactory]]
- [[_COMMUNITY_Keypad|Keypad]]
- [[_COMMUNITY_UiTheme|UiTheme]]
- [[_COMMUNITY_UiBootstrap|UiBootstrap]]
- [[_COMMUNITY_PassthroughScanner|PassthroughScanner]]
- [[_COMMUNITY_SettingsScreen|SettingsScreen]]
- [[_COMMUNITY_ManagedGcm|ManagedGcm]]
- [[_COMMUNITY_CreateScreen|CreateScreen]]
- [[_COMMUNITY_QrEnvelopeTests|QrEnvelopeTests]]
- [[_COMMUNITY_FrameDecoderTests|FrameDecoderTests]]
- [[_COMMUNITY_PageRoundTripTests|PageRoundTripTests]]
- [[_COMMUNITY_ScanScreen|ScanScreen]]
- [[_COMMUNITY_ZxingQrDecoder  IQrDecoder|ZxingQrDecoder / IQrDecoder]]
- [[_COMMUNITY_OvrUiInput|OvrUiInput]]
- [[_COMMUNITY_UiThemeTests|UiThemeTests]]
- [[_COMMUNITY_AppSettings|AppSettings]]
- [[_COMMUNITY_GcmVectorsTests|GcmVectorsTests]]
- [[_COMMUNITY_ZxingQrRoundTripTests|ZxingQrRoundTripTests]]
- [[_COMMUNITY_ZxingQrEncoder  IQrEncoder|ZxingQrEncoder / IQrEncoder]]
- [[_COMMUNITY_AesGcmCodecTests|AesGcmCodecTests]]
- [[_COMMUNITY_PageLayout|PageLayout]]
- [[_COMMUNITY_SceneBuilder|SceneBuilder]]
- [[_COMMUNITY_PageFixtureTests|PageFixtureTests]]
- [[_COMMUNITY_DecodedScreen|DecodedScreen]]
- [[_COMMUNITY_AppSettingsTests|AppSettingsTests]]
- [[_COMMUNITY_FrameDecoder|FrameDecoder]]
- [[_COMMUNITY_HomeScreen  CreateScreen|HomeScreen / CreateScreen]]
- [[_COMMUNITY_KeypadModelTests|KeypadModelTests]]
- [[_COMMUNITY_HomeScreen|HomeScreen]]
- [[_COMMUNITY_UiSprites|UiSprites]]
- [[_COMMUNITY_KeyImporterTests|KeyImporterTests]]
- [[_COMMUNITY_PageComposer|PageComposer]]
- [[_COMMUNITY_HomeScreen|HomeScreen]]
- [[_COMMUNITY_QrEnvelope|QrEnvelope]]
- [[_COMMUNITY_KeyStore|KeyStore]]
- [[_COMMUNITY_AesGcmCodec|AesGcmCodec]]
- [[_COMMUNITY_PageFont|PageFont]]
- [[_COMMUNITY_TexturePixels|TexturePixels]]
- [[_COMMUNITY_KeyImporter|KeyImporter]]
- [[_COMMUNITY_PageDecoder|PageDecoder]]
- [[_COMMUNITY_VersionInfo|VersionInfo]]

## God Nodes (most connected - your core abstractions)
1. `AppController` - 51 edges
2. `SettingsScreen` - 24 edges
3. `UiBootstrap` - 23 edges
4. `PassthroughScanner` - 21 edges
5. `UiTheme` - 21 edges
6. `CreateScreen` - 21 edges
7. `ManagedGcm` - 19 edges
8. `QrEnvelopeTests` - 19 edges
9. `Keypad` - 18 edges
10. `OvrUiInput` - 18 edges

## Surprising Connections (you probably didn't know these)
- `ZxingQrDecoder` --implements--> `IQrDecoder`  [EXTRACTED]
  Runtime/Qr/ZxingQrDecoder.cs → Runtime/Qr/IQrDecoder.cs
- `ZxingQrEncoder` --implements--> `IQrEncoder`  [EXTRACTED]
  Runtime/Qr/ZxingQrEncoder.cs → Runtime/Qr/IQrEncoder.cs

## Communities (43 total, 0 thin omitted)

### Community 0 - "AppController"
Cohesion: 0.06
Nodes (23): AppController, Schlusselbild.App, CreateScreen, DecodedScreen, HomeScreen, Action, AppSettings, bool (+15 more)

### Community 1 - "UiFactory"
Cohesion: 0.15
Nodes (18): Button, LayoutElement, RectOffset, Action, Color, float, GameObject, RawImage (+10 more)

### Community 2 - "Keypad"
Cohesion: 0.11
Nodes (12): KeypadMode, MonoBehaviour, Action, int, List, RectTransform, string, UiButtonHandle (+4 more)

### Community 3 - "UiTheme"
Cohesion: 0.11
Nodes (12): bool, Color, float, int, StatusKind, string, TextScale, UiPalette (+4 more)

### Community 4 - "UiBootstrap"
Cohesion: 0.13
Nodes (12): Schlusselbild.App, UiBootstrap, AppController, AppSettings, bool, Camera, Canvas, float (+4 more)

### Community 5 - "PassthroughScanner"
Cohesion: 0.13
Nodes (11): PassthroughScanner, Schlusselbild.Capture, PassthroughCameraAccess, RenderTexture, byte, Color32, int, Object (+3 more)

### Community 6 - "SettingsScreen"
Cohesion: 0.11
Nodes (9): AppSettings, bool, Slider, TextMeshProUGUI, TextScale, Toggle, UiButtonHandle, Schlusselbild.Ui.Screens (+1 more)

### Community 7 - "ManagedGcm"
Cohesion: 0.19
Nodes (6): Aes, ManagedGcm, Schlusselbild.Crypto, ICryptoTransform, int, ulong

### Community 8 - "CreateScreen"
Cohesion: 0.14
Nodes (10): AspectRatioFitter, Keypad, byte, Object, RawImage, StatusKind, TextMeshProUGUI, Texture2D (+2 more)

### Community 9 - "QrEnvelopeTests"
Cohesion: 0.19
Nodes (4): QrEnvelopeTests, Schlusselbild.Tests, Test, TestCase

### Community 10 - "FrameDecoderTests"
Cohesion: 0.22
Nodes (8): FrameDecoderTests, Schlusselbild.Tests, Color32, List, string, TearDown, Test, Texture2D

### Community 11 - "PageRoundTripTests"
Cohesion: 0.19
Nodes (7): PageRoundTripTests, Schlusselbild.Tests, byte, List, TearDown, Test, Texture2D

### Community 12 - "ScanScreen"
Cohesion: 0.15
Nodes (9): Object, RawImage, RectTransform, StatusKind, TextMeshProUGUI, Texture2D, UiPalette, ScanScreen (+1 more)

### Community 13 - "ZxingQrDecoder / IQrDecoder"
Cohesion: 0.15
Nodes (11): LuminanceSource, IQrDecoder, Schlusselbild.Qr, Schlusselbild.Qr, ZxingQrDecoder, Result, List, Texture2D (+3 more)

### Community 14 - "OvrUiInput"
Cohesion: 0.21
Nodes (8): LineRenderer, Material, OVRInputModule, Camera, Canvas, Transform, OvrUiInput, Schlusselbild.Ui

### Community 15 - "UiThemeTests"
Cohesion: 0.16
Nodes (6): SetUp, TearDown, Test, TextScale, Schlusselbild.Tests, UiThemeTests

### Community 16 - "AppSettings"
Cohesion: 0.18
Nodes (6): AppSettings, Schlusselbild.App, bool, float, string, TextScale

### Community 17 - "GcmVectorsTests"
Cohesion: 0.25
Nodes (4): GcmVectorsTests, Schlusselbild.Tests.Crypto, string, Test

### Community 18 - "ZxingQrRoundTripTests"
Cohesion: 0.18
Nodes (7): Schlusselbild.Tests, ZxingQrRoundTripTests, List, TearDown, Test, TestCase, Texture2D

### Community 19 - "ZxingQrEncoder / IQrEncoder"
Cohesion: 0.15
Nodes (10): BitMatrix, IQrEncoder, Schlusselbild.Qr, Schlusselbild.Qr, ZxingQrEncoder, Texture2D, Color32, int (+2 more)

### Community 20 - "AesGcmCodecTests"
Cohesion: 0.18
Nodes (5): AesGcmCodecTests, Schlusselbild.Tests.Crypto, byte, int, Test

### Community 21 - "PageLayout"
Cohesion: 0.12
Nodes (4): PageLayout, Schlusselbild.Page, int, string

### Community 22 - "SceneBuilder"
Cohesion: 0.27
Nodes (5): GameObject, string, Transform, SceneBuilder, MenuItem

### Community 23 - "PageFixtureTests"
Cohesion: 0.25
Nodes (7): PageFixtureTests, Schlusselbild.Tests, List, string, TearDown, Test, Texture2D

### Community 24 - "DecodedScreen"
Cohesion: 0.17
Nodes (7): RectTransform, ScrollRect, StatusKind, TextMeshProUGUI, ScreenBase, DecodedScreen, Schlusselbild.Ui.Screens

### Community 25 - "AppSettingsTests"
Cohesion: 0.19
Nodes (5): SetUp, TearDown, Test, AppSettingsTests, Schlusselbild.Tests

### Community 26 - "FrameDecoder"
Cohesion: 0.34
Nodes (4): FrameDecoder, Schlusselbild.Capture, Color32, Texture2D

### Community 27 - "HomeScreen / CreateScreen"
Cohesion: 0.29
Nodes (5): RectTransform, Color, RectTransform, Transform, RectTransform

### Community 28 - "KeypadModelTests"
Cohesion: 0.26
Nodes (3): Test, KeypadModelTests, Schlusselbild.Tests

### Community 29 - "HomeScreen"
Cohesion: 0.17
Nodes (4): TextMeshProUGUI, UiButtonHandle, HomeScreen, Schlusselbild.Ui.Screens

### Community 30 - "UiSprites"
Cohesion: 0.29
Nodes (5): Dictionary, Image, Sprite, Schlusselbild.Ui, UiSprites

### Community 31 - "KeyImporterTests"
Cohesion: 0.27
Nodes (4): KeyImporterTests, Schlusselbild.Tests.Crypto, string, Test

### Community 32 - "PageComposer"
Cohesion: 0.36
Nodes (4): PageComposer, Schlusselbild.Page, Color32, Texture2D

### Community 33 - "HomeScreen"
Cohesion: 0.27
Nodes (4): CanvasGroup, Coroutine, IEnumerator, ScreenBase

### Community 34 - "QrEnvelope"
Cohesion: 0.25
Nodes (5): IEnumerable, QrEnvelope, Schlusselbild.Qr, int, List

### Community 35 - "KeyStore"
Cohesion: 0.32
Nodes (4): KeyStore, Schlusselbild.Crypto, int, string

### Community 36 - "AesGcmCodec"
Cohesion: 0.36
Nodes (3): AesGcmCodec, Schlusselbild.Crypto, int

### Community 37 - "PageFont"
Cohesion: 0.29
Nodes (4): PageFont, Schlusselbild.Page, byte, int

### Community 38 - "TexturePixels"
Cohesion: 0.38
Nodes (4): Schlusselbild.Qr, TexturePixels, Color32, Texture2D

### Community 39 - "KeyImporter"
Cohesion: 0.38
Nodes (3): KeyImporter, Schlusselbild.Crypto, int

### Community 40 - "PageDecoder"
Cohesion: 0.40
Nodes (3): PageDecoder, Schlusselbild.Page, Texture2D

### Community 41 - "VersionInfo"
Cohesion: 0.50
Nodes (3): Schlusselbild.App, VersionInfo, string

## Knowledge Gaps
- **176 isolated node(s):** `string`, `MenuItem`, `Schlusselbild.App`, `int`, `string` (+171 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ScreenBase` connect `HomeScreen` to `Keypad`, `HomeScreen / CreateScreen`, `HomeScreen`?**
  _High betweenness centrality (0.076) - this node is a cross-community bridge._
- **Why does `AppController` connect `AppController` to `Keypad`?**
  _High betweenness centrality (0.052) - this node is a cross-community bridge._
- **What connects `string`, `MenuItem`, `Schlusselbild.App` to the rest of the system?**
  _176 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `AppController` be split into smaller, more focused modules?**
  _Cohesion score 0.06110102843315184 - nodes in this community are weakly interconnected._
- **Should `UiFactory` be split into smaller, more focused modules?**
  _Cohesion score 0.1492063492063492 - nodes in this community are weakly interconnected._
- **Should `Keypad` be split into smaller, more focused modules?**
  _Cohesion score 0.1053763440860215 - nodes in this community are weakly interconnected._
- **Should `UiTheme` be split into smaller, more focused modules?**
  _Cohesion score 0.11076923076923077 - nodes in this community are weakly interconnected._