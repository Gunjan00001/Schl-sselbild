# Graph Report - Schl-sselbild  (2026-11-27)

## Corpus Check
- 72 files · ~184,861 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1312 nodes · 2630 edges · 62 communities (59 shown, 3 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 205 edges (avg confidence: 0.83)
- Token cost: 11,800 input · 7,200 output

## Community Hubs (Navigation)
- Python compose/decode pipeline
- Runtime bootstrap & passthrough
- C# namespaces: Crypto, UI, App
- ZXing QR round trip
- Settings & text scale
- Theme tokens & status system
- Page compose & Create screen
- Keypad model & UI
- Core package manifest
- Editor/test package manifest
- QR envelope chunking
- Managed AES-GCM
- Passthrough frame decoding
- Page decode & fixtures
- UI factory widgets
- Core Unity modules
- Unity module manifests A
- Screen base & layout
- XR / Meta SDK packages
- Scan screen & status
- AES-GCM codec
- UI / IMGUI modules
- App controller & navigation
- Networking modules
- OVR UI ray input
- Scene builder
- 2D physics modules
- Physics modules
- Image/capture modules
- UI system (docs)
- Runtime rounded sprites
- App pipeline (docs)
- Decoded screen
- Buttons & handles
- IDE / test packages
- XR legacy input helpers
- Modals & key dialog
- Layout primitives
- Project overview (docs)
- Python oracle (docs)
- Math / particles modules
- OpenXR / Meta XR packages
- Screen lifecycle & captures
- MCP tooling config
- Input System packages
- Animation modules
- Terrain modules
- uGUI & TextMeshPro packages
- Editor dev tools
- Home screen
- Keys & Android build (docs)
- Unity setup (docs)
- Codec & pipeline concepts (docs)
- Multi-QR page fixture
- Screen names (docs)
- Composition layers package
- OpenXR plugin package
- Project bootstrap
- App controller key flow
- Toolchain (docs)
- Android JNI module

## God Nodes (most connected - your core abstractions)
1. `AppController` - 52 edges
2. `AppSettings` - 28 edges
3. `UiTheme` - 27 edges
4. `PassthroughScanner` - 26 edges
5. `KeypadModel` - 26 edges
6. `ScreenBase` - 26 edges
7. `SettingsScreen` - 25 edges
8. `UiBootstrap` - 24 edges
9. `CreateScreen` - 23 edges
10. `OvrUiInput` - 19 edges

## Surprising Connections (you probably didn't know these)
- `Python AES-256-GCM crypto layer` --semantically_similar_to--> `AesGcmCodec (crypto layer)`  [INFERRED] [semantically similar]
  legacy/python/README.md → docs/ARCHITECTURE.md
- `Python QR transport format (256-byte chunk envelope)` --semantically_similar_to--> `QrEnvelope (chunk split/reassemble format)`  [INFERRED] [semantically similar]
  legacy/python/README.md → docs/ARCHITECTURE.md
- `Python page compose/decode layer` --semantically_similar_to--> `PageComposer (secret to page Texture2D)`  [INFERRED] [semantically similar]
  legacy/python/README.md → docs/ARCHITECTURE.md
- `Python live webcam decode loop` --semantically_similar_to--> `PassthroughScanner retry loop`  [INFERRED] [semantically similar]
  legacy/python/README.md → docs/ARCHITECTURE.md
- `Python local key policy (key.bin, 0600, gitignored)` --semantically_similar_to--> `KeyStore and KeyImporter (key handling)`  [INFERRED] [semantically similar]
  legacy/python/README.md → docs/ARCHITECTURE.md

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Encode flow: secret to QR page** — docs_architecture_aesgcmcodec, docs_architecture_qrenvelope, docs_architecture_zxing_backend, docs_architecture_pagecomposer [EXTRACTED 1.00]
- **Decode flow: passthrough frame to plaintext** — docs_architecture_passthroughscanner, docs_architecture_framedecoder, docs_architecture_pagedecoder, docs_architecture_aesgcmcodec [EXTRACTED 1.00]
- **Accessibility program across design, input and status** — ui_accessibility, readme_accessibility, docs_ui_design_tokens, docs_decisions_d10_ovr_input [INFERRED 0.80]

## Communities (62 total, 3 thin omitted)

### Community 0 - "Python compose/decode pipeline"
Cohesion: 0.07
Nodes (44): Action, Color, Transform, Image, VerticalLayoutGroup, RectTransform, RectTransform, Color (+36 more)

### Community 1 - "Runtime bootstrap & passthrough"
Cohesion: 0.07
Nodes (17): Aes, ArgumentNullException, AesGcmCodec, KeyStore, DefaultPath, ManagedGcm, ArgumentException, CryptographicException (+9 more)

### Community 2 - "C# namespaces: Crypto, UI, App"
Cohesion: 0.08
Nodes (62): compose_page(), _decode_all_qr_bytes(), decode_page(), decode_page_image(), _grid_dims(), Image, PathLike, _reassemble() (+54 more)

### Community 3 - "ZXing QR round trip"
Cohesion: 0.06
Nodes (26): List, Texture2D, IQrDecoder, Texture2D, IQrEncoder, Color32, Texture2D, TexturePixels (+18 more)

### Community 4 - "Settings & text scale"
Cohesion: 0.07
Nodes (28): Camera, Canvas, EventSystem, OVRManager, RectTransform, Transform, UiBootstrap, Controller (+20 more)

### Community 5 - "Theme tokens & status system"
Cohesion: 0.07
Nodes (25): TextScale, Color, StatusKind, Invalid, Ok, Scanning, StatusStyle, UiPalette (+17 more)

### Community 6 - "Page compose & Create screen"
Cohesion: 0.08
Nodes (15): AppSettings, ColorblindSafe, ScaleFactor, SeatedMode, SnapTurn, UiDistance, Slider, TextMeshProUGUI (+7 more)

### Community 7 - "Keypad model & UI"
Cohesion: 0.07
Nodes (18): AspectRatioFitter, Color32, Texture2D, PageComposer, PageFont, Color32, PageLayout, Background (+10 more)

### Community 8 - "Core package manifest"
Cohesion: 0.04
Nodes (46): dependencies, com.gamelovers.mcp-unity, com.meta.xr.mrutilitykit, com.unity.ide.rider, com.unity.ide.visualstudio, com.unity.inputsystem, com.unity.modules.accessibility, com.unity.modules.adaptiveperformance (+38 more)

### Community 9 - "Editor/test package manifest"
Cohesion: 0.10
Nodes (16): Action, List, RectTransform, Keypad, Text, KeypadMode, Digits, Letters (+8 more)

### Community 10 - "QR envelope chunking"
Cohesion: 0.07
Nodes (14): TextScale, Large, Medium, Small, VersionInfo, PageDecoder, Test, UiSpritesTests (+6 more)

### Community 11 - "Managed AES-GCM"
Cohesion: 0.10
Nodes (18): Camera, GameObject, MenuItem, OVRManager, Transform, SceneBuilder, Color32, Object (+10 more)

### Community 12 - "Passthrough frame decoding"
Cohesion: 0.13
Nodes (13): Texture2D, CryptographicException, List, TearDown, Test, Texture2D, PageFixtureTests, List (+5 more)

### Community 13 - "Page decode & fixtures"
Cohesion: 0.17
Nodes (10): Color32, Texture2D, FrameDecoder, Color32, CryptographicException, List, TearDown, Test (+2 more)

### Community 14 - "UI factory widgets"
Cohesion: 0.12
Nodes (7): RectTransform, TextMeshProUGUI, AppController, HasKey, Settings, StatusKind, UiScreen

### Community 15 - "Core Unity modules"
Cohesion: 0.19
Nodes (8): List, QrEnvelope, ArgumentException, Test, TestCase, QrEnvelopeTests, IEnumerable, InvalidDataException

### Community 16 - "Unity module manifests A"
Cohesion: 0.09
Nodes (26): dependencies, dependencies, depth, source, version, dependencies, depth, source (+18 more)

### Community 17 - "Screen base & layout"
Cohesion: 0.09
Nodes (21): dependencies, depth, source, version, dependencies, depth, source, version (+13 more)

### Community 18 - "XR / Meta SDK packages"
Cohesion: 0.10
Nodes (22): dependencies, depth, source, version, dependencies, dependencies, depth, source (+14 more)

### Community 19 - "Scan screen & status"
Cohesion: 0.11
Nodes (22): depth, source, version, depth, source, version, dependencies, depth (+14 more)

### Community 20 - "AES-GCM codec"
Cohesion: 0.10
Nodes (22): dependencies, depth, source, url, version, depth, source, url (+14 more)

### Community 21 - "UI / IMGUI modules"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, version, dependencies, depth, source, version (+13 more)

### Community 22 - "App controller & navigation"
Cohesion: 0.10
Nodes (20): dependencies, depth, source, url, version, depth, source, url (+12 more)

### Community 23 - "Networking modules"
Cohesion: 0.10
Nodes (20): dependencies, depth, source, url, version, dependencies, depth, source (+12 more)

### Community 24 - "OVR UI ray input"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 25 - "Scene builder"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 26 - "2D physics modules"
Cohesion: 0.18
Nodes (6): CanvasGroup, ScreenBase, IsVisible, Root, Coroutine, IEnumerator

### Community 27 - "Physics modules"
Cohesion: 0.13
Nodes (15): depth, source, version, dependencies, depth, source, version, dependencies (+7 more)

### Community 28 - "Image/capture modules"
Cohesion: 0.14
Nodes (15): dependencies, depth, source, version, dependencies, depth, source, version (+7 more)

### Community 29 - "UI system (docs)"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, version, dependencies, depth, source, version (+7 more)

### Community 30 - "Runtime rounded sprites"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, version, dependencies, depth, source, version (+7 more)

### Community 31 - "App pipeline (docs)"
Cohesion: 0.19
Nodes (5): ScrollRect, StatusKind, TextMeshProUGUI, DecodedScreen, Plaintext

### Community 32 - "Decoded screen"
Cohesion: 0.27
Nodes (3): KeyImporter, Test, KeyImporterTests

### Community 33 - "Buttons & handles"
Cohesion: 0.20
Nodes (14): UiBootstrap (runtime UI construction), D10 - Input via OVRInputModule + OVRRaycaster, D12 - Procedural rounded 9-slice sprites, D21 - Soft Glass visual direction, D9 - Code-built world-space uGUI + TextMeshPro UI, UiTheme design tokens (palette, radii, spacing), Accessibility guarantees (icon + word + colour, comfort options), Soft Glass UI direction (+6 more)

### Community 34 - "IDE / test packages"
Cohesion: 0.23
Nodes (5): Object, RawImage, TextMeshProUGUI, Texture2D, ScanScreen

### Community 35 - "XR legacy input helpers"
Cohesion: 0.19
Nodes (13): AppController (screens, key, navigation, scanner wiring), FrameDecoder (downscale, grayscale, decode), PassthroughScanner retry loop, Schlusselbild.Runtime assembly definition, Main scene runtime wiring (OVRCameraRig + AppRoot), D11 - Custom in-world keypad, D13 - One screen at a time, initialized before navigation, D14 - Three-tier verification strategy (+5 more)

### Community 36 - "Modals & key dialog"
Cohesion: 0.25
Nodes (11): D15 - major.minor.bugs version ledger, D20 - Documentation set and architecture graph, D2 - Full encode + passthrough scan + overlay scope, D4 - Unity at repo root, Python under legacy/python, Schlüsselbild documentation index, Passthrough scan-and-decrypt pipeline, In-headset encode pipeline (AES-256-GCM to QR page), Legacy Kitsune Python implementation as reference and test oracle (+3 more)

### Community 37 - "Layout primitives"
Cohesion: 0.31
Nodes (11): D3 - Byte-for-byte compatibility with the Python oracle, Cross-platform byte compatibility verification, Python-generated fixtures, Python AES-256-GCM crypto layer, Kitsune encrypted-page privacy demo, Python live webcam decode loop, Python page compose/decode layer, Python QR transport format (256-byte chunk envelope) (+3 more)

### Community 38 - "Project overview (docs)"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 39 - "Python oracle (docs)"
Cohesion: 0.20
Nodes (9): MCP_UNITY_AUTH_TOKEN_PATH, MCP_UNITY_SETTINGS_PATH, mcp, mcp-unity, command, enabled, environment, type (+1 more)

### Community 40 - "Math / particles modules"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 41 - "OpenXR / Meta XR packages"
Cohesion: 0.36
Nodes (4): Canvas, CanvasGroup, MenuItem, DevTools

### Community 42 - "Screen lifecycle & captures"
Cohesion: 0.28
Nodes (9): ADB install and key push to Quest 3, Android APK build (IL2CPP, ARM64, Linear), DevTools.BuildAndroid, KeyStore and KeyImporter (key handling), D16 - Unity-bundled Android SDK/NDK/OpenJDK, D5 - Key handling: key.bin import + hex entry, Future work: AR / live overlay, Python local key policy (key.bin, 0600, gitignored) (+1 more)

### Community 43 - "MCP tooling config"
Cohesion: 0.22
Nodes (9): Meta XR Simulator controller testing setup, First-time Unity project setup, Editor DevTools (build, export, captures), D18 - Do not commit large local installers, D19 - Meta XR Simulator for controller testing, D1 - Target runtime: Unity 6 + Meta XR SDK, In-Editor verification through Unity MCP, Unity Editor 6000.6.2f1 (+1 more)

### Community 44 - "Input System packages"
Cohesion: 0.39
Nodes (9): AesGcmCodec (crypto layer), Data-flow guarantees (structural validation, order-independent reassembly), Layered encode/decode pipeline, PageComposer (secret to page Texture2D), PageDecoder (page Texture2D to plaintext), PageLayout constants, QrEnvelope (chunk split/reassemble format), ZXing.Net QR backend behind IQrEncoder/IQrDecoder (+1 more)

### Community 46 - "Terrain modules"
Cohesion: 0.50
Nodes (8): Decryption Key (Implied by Caption), Multi-QR Encrypted Payload, Page Multi Test Fixture (Composed Encrypted QR Page), Multi-QR Page Layout (Payload Split Across Codes), Large QR Code Chunk 1 (Left, High Density), Large QR Code Chunk 2 (Middle, High Density), Small QR Code Chunk 3 (Right, Lower Density), Title Band: "Kitsune - encrypted page (unreadable without the key)"

### Community 47 - "uGUI & TextMeshPro packages"
Cohesion: 0.33
Nodes (6): UiScreen, Create, Decoded, Home, Scan, Settings

### Community 48 - "Editor dev tools"
Cohesion: 0.33
Nodes (6): dependencies, depth, hash, source, version, com.gamelovers.mcp-unity

### Community 49 - "Home screen"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, version, dependencies, com.unity.burst

### Community 50 - "Keys & Android build (docs)"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, version, dependencies, com.unity.ext.nunit

### Community 51 - "Unity setup (docs)"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.ide.rider

### Community 52 - "Codec & pipeline concepts (docs)"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.ide.visualstudio

### Community 53 - "Multi-QR page fixture"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.nuget.mono-cecil

### Community 54 - "Screen names (docs)"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.nuget.newtonsoft-json

### Community 56 - "OpenXR plugin package"
Cohesion: 0.50
Nodes (5): Build troubleshooting table, ManagedGcm (pure C# AES-256-GCM), D17 - Delegation policy for the mcode harness, D6 - Managed AES-256-GCM (ManagedGcm), Unity 6 / Meta XR / all-managed C# toolchain

### Community 57 - "Project bootstrap"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.test-framework.performance

### Community 58 - "App controller key flow"
Cohesion: 0.83
Nodes (4): Kitsune-Encrypted QR Page (Short Fixture), Encrypted Payload Requiring a Key to Decode, High-Density QR Code Grid with Three Finder Patterns, Title Band: 'Kitsune-encrypted page (unreadable without the key)'

### Community 59 - "Toolchain (docs)"
Cohesion: 0.50
Nodes (4): depth, source, version, com.unity.test-framework

## Knowledge Gaps
- **352 isolated node(s):** `Home`, `Create`, `Scan`, `Decoded`, `Settings` (+347 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `AppController` connect `UI factory widgets` to `Python compose/decode pipeline`, `Decoded screen`, `IDE / test packages`, `Settings & text scale`, `Page compose & Create screen`, `Keypad model & UI`, `QR envelope chunking`, `Managed AES-GCM`, `Animation modules`, `uGUI & TextMeshPro packages`, `2D physics modules`, `App pipeline (docs)`?**
  _High betweenness centrality (0.092) - this node is a cross-community bridge._
- **Why does `Schlusselbild.Tests` connect `QR envelope chunking` to `ZXing QR round trip`?**
  _High betweenness centrality (0.053) - this node is a cross-community bridge._
- **Are the 5 inferred relationships involving `AppSettings` (e.g. with `.Changed_FiresOncePerRealChange()` and `.NewInstance_UsesDocumentedDefaults()`) actually correct?**
  _`AppSettings` has 5 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Home`, `Create`, `Scan` to the rest of the system?**
  _352 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Python compose/decode pipeline` be split into smaller, more focused modules?**
  _Cohesion score 0.07381748686096512 - nodes in this community are weakly interconnected._
- **Should `Runtime bootstrap & passthrough` be split into smaller, more focused modules?**
  _Cohesion score 0.07122736418511066 - nodes in this community are weakly interconnected._
- **Should `C# namespaces: Crypto, UI, App` be split into smaller, more focused modules?**
  _Cohesion score 0.07502131287297528 - nodes in this community are weakly interconnected._