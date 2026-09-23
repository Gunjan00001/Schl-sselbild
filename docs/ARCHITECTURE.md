# Architecture

Schlüsselbild is a layered pipeline, mirrored from the original Kitsune Python implementation
so the two stay byte-compatible. Each layer has one responsibility and passes a well-defined
value to the next.

## Layers

```
ENCODE
  secret string
      │  AesGcmCodec.Encrypt          (Crypto)
      ▼
  nonce(12) ‖ tag(16) ‖ ciphertext
      │  QrEnvelope.Split             (Qr)
      ▼
  [index:1][total:1][len:2 BE][chunk] per QR
      │  ZxingQrEncoder.Encode        (Qr)
      ▼
  QR textures  ──  PageComposer.Compose  ──▶  page Texture2D  ──▶  PNG

DECODE
  passthrough frame (Texture)
      │  PassthroughScanner (throttled) + FrameDecoder (downscale, grayscale)
      ▼
  page Texture2D
      │  ZxingQrDecoder.DecodeAll     (Qr)
      ▼
  QR envelopes (any order)
      │  QrEnvelope.Reassemble        (Qr)
      ▼
  ciphertext blob
      │  AesGcmCodec.Decrypt          (Crypto)
      ▼
  recovered plaintext  ──▶  AppController.ShowDecoded  ──▶  Decoded screen
```

## Assemblies and file map

`Assets/Schlusselbild/Runtime/Schlusselbild.Runtime.asmdef` (namespace root `Schlusselbild`)
references `meta.xr.mrutilitykit`, `Oculus.VR`, `UnityEngine.UI`, `Unity.TextMeshPro`.

| Path | Responsibility | Notes |
|------|----------------|-------|
| `Crypto/AesGcmCodec.cs` | `Encrypt(string,byte[]) -> blob`, `Decrypt(blob,byte[]) -> string`. | Frozen blob layout. |
| `Crypto/ManagedGcm.cs` | Pure C# AES-256-GCM (CTR + GHASH). | Replaces the Mono-unsupported `AesGcm`. |
| `Crypto/KeyStore.cs` | Load/create the 32-byte `key.bin`. | `persistentDataPath`. |
| `Crypto/KeyImporter.cs` | Parse a 64-char hex or base64 key. | Used by the in-VR key dialog. |
| `Qr/QrEnvelope.cs` | Split/reassemble the chunk envelope; validates indices. | Order-independent. |
| `Qr/ZxingQrEncoder.cs` | Envelope bytes -> QR `Texture2D` (byte mode, ECC-H). | ISO-8859-1 mapping. |
| `Qr/ZxingQrDecoder.cs` | Image -> all QR raw bytes (`Result.RawBytes`). | Multi-symbol. |
| `Qr/IQrEncoder.cs`, `Qr/IQrDecoder.cs` | Backend abstraction. | Swap for native later. |
| `Page/PageLayout.cs` | Page constants (margins, gap, columns, title). | Matches Python. |
| `Page/PageComposer.cs` | Secret -> composed page `Texture2D`. | Uses Crypto + Qr. |
| `Page/PageDecoder.cs` | Page `Texture2D` -> plaintext. | Uses Qr + Crypto. |
| `Page/PageFont.cs` | Tiny bitmap font for the page title band. | No external font. |
| `Capture/FrameDecoder.cs` | Camera frame -> downscale -> grayscale -> decode. | Testable with fixtures. |
| `Capture/PassthroughScanner.cs` | MonoBehaviour retry loop over `Meta.XR.PassthroughCameraAccess`. | Emits status/decoded/invalid/frame events. |
| `App/AppSettings.cs` | Persisted comfort/accessibility settings. | `PlayerPrefs`. |
| `App/AppController.cs` | Owns screens, key, navigation, scanner wiring, modals. | Single source of UI truth. |
| `App/UiBootstrap.cs` | Builds canvas, screens, pointers at runtime. | Attached to `AppRoot`. |
| `App/VersionInfo.cs` | In-app version string. | Mirrors `rules.md`. |
| `Ui/UiTheme.cs` | Palette, radii, spacing, font sizes, status mapping. | Design tokens. |
| `Ui/UiSprites.cs` | Procedural rounded 9-slice sprites (fill + stroke). | Cached. |
| `Ui/UiFactory.cs` | Builders for panels, buttons, sliders, toggles, scroll views. | Rounded + border + shadow. |
| `Ui/OvrUiInput.cs` | `OVRInputModule` setup, hand selection, ray visuals. | Official Meta input. |
| `Ui/Keypad.cs` | In-world keypad widget + `KeypadModel`. | Model is unit-tested. |
| `Ui/Screens/*.cs` | Home, Create, Scan, Decoded, Settings. | `ScreenBase` + fade-in. |

`Assets/Editor/ProjectBootstrap.cs` installs UPM packages once.
`Assets/Editor/SceneBuilder.cs` builds `Scenes/Main.unity` reproducibly.
`Assets/Editor/DevTools.cs` provides dev utilities: export interop page, build APK,
capture Game view / all screens.

## Runtime wiring

`Scenes/Main.unity` contains two roots: `OVRCameraRig` (OVRManager, OVRPassthroughLayer,
OVRHeadsetEmulator) and `AppRoot` (`UiBootstrap` + `PassthroughScanner`).

`UiBootstrap.Awake` loads settings, builds the event system, canvas and `Screens` container,
creates the `AppController`, wires `OvrUiInput` to the hand anchors, and applies the tracking
origin. `AppController.Initialize` loads the key, builds the five screens, binds the scanner,
sets `initialized = true`, then `ShowHome()`.

Only one screen is active at a time; `Show()` calls `HideAll()` then activates the target and
fades it in.

## Data-flow guarantees

- The crypto blob and QR envelope are validated structurally before any decrypt; a wrong key
  or a tampered byte raises `CryptographicException` and is never shown as plaintext.
- Reassembly is order-independent and rejects missing, duplicate-conflicting, out-of-range or
  inconsistent chunks.
- The scanner never throws: absent camera, incomplete pages and auth failures are reported as
  status and the loop continues.
