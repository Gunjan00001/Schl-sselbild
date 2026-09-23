# rules.md — version ledger

Every update to Schlüsselbild is recorded here as a tag in **`major.minor.bugs`** form.

## Tag semantics

| Field | Meaning | Bump when |
|-------|---------|-----------|
| `major` | Breaking change | Wire format, architecture, or public API changes break compatibility (e.g. QR envelope, crypto blob layout, save/export format). |
| `minor` | New feature | New capability added without breaking existing behaviour (e.g. a new screen, hand-tracking support, export format added). |
| `bugs` | Fix / polish | Bug fixes, accessibility tweaks, docs, performance, refactors — no new feature, no breaking change. |

Rules:
- The tag in this file is the single source of truth for a build.
- Bump on **every** committed update; never leave a commit without a matching ledger entry.
- The same version is exposed in-app (Settings → About) so a build maps to a tag.
- Git-tag each release to match this ledger.

## Ledger

| Tag | Date | Summary | Areas |
|-----|------|---------|-------|
| `0.1.0` | 2026-09-23 | Initial port scaffold: Unity 6 project at repo root, legacy Kitsune Python moved to `legacy/python/`, repo `.gitignore`, Meta Quest 3 target selected. | repo, scaffold, legacy |
| `0.1.1` | 2026-09-23 | Package setup (XR/OpenXR/Meta-OpenXR/Input System/Meta XR SDK Core/MRUK/mcp-unity) + Android Build Support. Crypto layer: `AesGcmCodec`, `KeyStore`, `KeyImporter` with pure-managed AES-256-GCM (`ManagedGcm`) replacing the Mono-unsupported `System.Security.Cryptography.AesGcm`; 28/28 EditMode tests, 212/212 OpenSSL interop vectors. | packages, android, crypto |
| `0.1.2` | 2026-09-23 | QR transport + Page layers (`QrEnvelope`, `IQrEncoder/Decoder`, `ZxingQrEncoder/Decoder`, `PageLayout`, `PageComposer`, `PageDecoder`) on ZXing.Net; Python fixtures; 80/80 EditMode tests. Bidirectional byte-compatibility verified: Python page → Quest decode (fixture tests) and Quest page → Python oracle decode. | qr, page, fixtures, interop |
| `0.2.0` | 2026-09-23 | Capture layer: `FrameDecoder` (downscale → grayscale → decode) and `PassthroughScanner` (MonoBehaviour over `Meta.XR.PassthroughCameraAccess`, throttled retry loop, Decoded/Invalid events). Runtime asmdef now references `meta.xr.mrutilitykit`. 87/87 EditMode tests. | capture, passthrough, mruk |
| `0.3.0` | 2026-09-23 | Phase-4 VR UI: code-built world-space uGUI + TextMeshPro (`UiTheme`, `UiFactory`, `ControllerRayPointer`, `Keypad` + `KeypadModel`), five screens (Home/Create/Scan/Decoded/Settings), `AppController`/`UiBootstrap`/`VersionInfo`, editor `SceneBuilder` writing `Assets/Schlusselbild/Scenes/Main.unity` at build index 0. Accessibility: S/M/L text scale, ICON+WORD+COLOUR status, WCAG-AA palettes, comfort settings (UI distance, seated mode, snap turn). 111/111 EditMode tests. | ui, screens, scene, accessibility |
| `0.4.0` | 2026-09-23 | Android build toolchain completed (Android Build Support + SDK/build-tools/platforms, NDK r27c, OpenJDK 17) and `DevTools.BuildAndroid` (IL2CPP, ARM64, Linear, min API 32, `com.schlusselbild.quest`). Produced `Builds/Schlusselbild.apk` (50.3 MB, arm64-v8a, libil2cpp + libOVRPlugin + libopenxr_loader + libmrutilitykitshared). On-device install pending headset. | android, build, apk |
| `0.4.1` | 2026-09-23 | Fix VR screen navigation: `AppController.Initialize` set `initialized` after `ShowHome()` (which early-returns when uninitialized), so `HideAll()` never ran and all five screens rendered stacked; `initialized` is now set before navigation. Cleaned 8 deprecation warnings (`FindFirstObjectByType`→`FindAnyObjectByType`, `PlayerSettings`→`NamedBuildTarget.Android`). Added Editor dev tools (`DevTools`: export interop page, build APK, capture Game view / all screens). Verified in-Editor: 0 errors/0 warnings, 111/111 tests, all five screens render correctly in Play mode. | ui, fix, editor-tools |
| `0.5.0` | 2026-09-23 | UI overhaul + input rebuild. Premium theme: procedurally-generated rounded 9-slice sprites (`UiSprites`), card/button radii, subtle borders and drop shadows, larger spacing, refined palette (`UiTheme`). `UiFactory` reworked (rounded panels/buttons/toggles/sliders/scroll). Input replaced: hand-rolled pointer removed; now uses the Meta-official `OVRInputModule` + `OVRRaycaster` with per-hand ray visuals and hand selection (`OvrUiInput`). Home key row fixed (fixed-width Enter key button, friendly "32-byte key loaded"). Verified in-Editor via MCP: 0 errors, all five screens captured. | ui, input, theme |
| `0.6.0` | 2026-09-23 | UI polish + documentation. Added accent title underlines, screen fade-in/scale transitions, a rounded scroll indicator, and preview placeholder polish. Full docs under `docs/`: decision log (`DECISIONS.md`, D1–D20), architecture, UI design system, build/deploy, testing, and a graphify knowledge graph of the codebase (`docs/graph/` — 731 nodes, 1210 edges, 43 communities). Root README links the docs. | docs, ui-polish, graphify |
| `0.6.1` | 2026-09-23 | Fix Editor mouse input. `OVRRaycaster.Raycast` only handles VR pointers, so switching the canvas to `OVRRaycaster` broke mouse clicks in the Editor. Added `MouseUiInput` (Editor-only) with its own plain `GraphicRaycaster` that raycasts and dispatches pointer events; the controller ray fallback is disabled when no controller is present. README updated (status, screens, controls). | ui, input, fix |
