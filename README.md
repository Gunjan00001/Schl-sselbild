# Schlüsselbild — Meta Quest 3

An encrypted-page privacy demo, ported to **Meta Quest 3**. A secret is encrypted
(AES-256-GCM) and printed onto an ordinary-looking page as one or more QR codes.
Anyone glancing at the page — or scanning a QR with a phone — recovers only random
ciphertext. An authorized viewer wearing the headset points the **passthrough camera**
at the page and the plaintext is recovered and shown in a spatial AR overlay.

This is a college / proof-of-concept project, **not** a production security product.

## Status

**0.7.0** — feature-complete for the MVP scope, verified in the Editor and building a Quest 3 APK.
The UI uses the Soft Glass direction (navy backdrop, sheen panels, violet accent).

- **Encode:** type a secret (in-world keypad) → AES-256-GCM → segmented QR page, viewable and exportable as PNG.
- **Scan:** point the passthrough camera at a page → decode every QR → decrypt → show the plaintext.
- **UI:** five screens (Home, Create, Scan, Decoded, Settings) with a premium rounded theme and accessibility options.
- **Tests:** 111/111 EditMode tests, byte-compatible with the Python oracle in both directions.

The original Python implementation (Kitsune) is preserved under
[`legacy/python/`](legacy/python/) as the reference implementation and test oracle.

Still needs hardware to confirm: real passthrough scanning and controller clicks (see
[`docs/TESTING.md`](docs/TESTING.md)).

## Target device — Meta Quest 3

- Processor: Qualcomm Snapdragon XR2 Gen 2
- Memory: 8 GB RAM, 128/512 GB storage
- Display: 2,064 × 2,208 per eye, up to 120 Hz
- Passthrough cameras: forward-facing RGB (Passthrough Camera API, Horizon OS v74+)
- Input: Meta Quest Touch Plus controllers + hand tracking

## Compatibility (hard requirement)

The Quest app is **byte-compatible** with the Python format:

- Crypto blob: `nonce(12) ‖ tag(16) ‖ ciphertext` (AES-256-GCM, UTF-8 plaintext)
- QR envelope: `[index:1][total:1][chunk_len:2 BE][chunk bytes]`
- Chunks: 256 bytes each, ECC level H, byte mode
- Page layout: title band + 3-column QR grid, margins 48 px, gap 36 px

A page composed by the Python tool decrypts in the Quest app and vice-versa.

## Repository layout

```text
Schl-sselbild/
├── Assets/Schlusselbild/     # C# runtime, UI, tests (Quest app)
├── Packages/manifest.json    # UPM packages (Unity, Meta XR, mcp-unity)
├── ProjectSettings/          # Unity 6 (6000.6.2f1) project settings
├── legacy/python/            # original Kitsune implementation (reference + oracle)
├── docs/                     # design docs
├── rules.md                  # version ledger (major.minor.bugs)
└── README.md
```

## Toolchain

- Unity 6 LTS (6000.6.2f1) + Meta XR SDK + Mixed Reality Utility Kit (MRUK)
- All-managed C#: pure-managed AES-256-GCM (`ManagedGcm`) + ZXing.Net (behind `IQrDecoder`)
- Input: Meta `OVRInputModule` + `OVRRaycaster` (controllers/hands); Editor mouse handler
- Unity MCP (`com.gamelovers.mcp-unity`) for Editor automation
- mcode (MiniMax Code, DeepSeek V4.1 Flash) for delegated implementation

## Screens

| Screen | Purpose |
|--------|---------|
| **Home** | Create page / Scan page, key status, Settings, Help. |
| **Create** | Secret entry (keypad), compose/preview, Export PNG, Self-test. |
| **Scan** | Passthrough retry loop; shows status and a live preview. |
| **Decoded** | The recovered plaintext, with Clear. |
| **Settings** | Text size S/M/L, UI distance, seated mode, snap turn, colorblind-safe. |

## Controls and input

- **Headset:** `OVRInputModule` + `OVRRaycaster` drive the UI from the Quest Touch Plus
  controllers or hand tracking; the pointing hand's ray is highlighted. Trigger = click.
- **Editor:** a dedicated mouse handler (`MouseUiInput`) with its own `GraphicRaycaster`
  lets you click the UI with the mouse. Use **Meta → Meta XR Simulator → Activate** to test
  controller input without a headset.
- Settings are persisted (`PlayerPrefs`) and applied live.

## Accessibility

Controllers **and** hand tracking for every action, comfort/motion options
(seated/standing, snap turn, adjustable UI distance), and vision options
(S/M/L text scaling, WCAG-AA contrast, colorblind-safe status states). Status is always
shown as **icon + word + colour**, never colour alone.

## Documentation

Detailed docs live in [`docs/`](docs/):

- [`DECISIONS.md`](docs/DECISIONS.md) — every design decision with its reason (D1–D20).
- [`ARCHITECTURE.md`](docs/ARCHITECTURE.md) — layers, data flow, file map, runtime wiring.
- [`UI-DESIGN.md`](docs/UI-DESIGN.md) — design tokens, components, screens, accessibility.
- [`BUILD-AND-DEPLOY.md`](docs/BUILD-AND-DEPLOY.md) — setup, Editor/Simulator, Android build, adb.
- [`TESTING.md`](docs/TESTING.md) — test strategy, coverage, device-only gaps.
- [`docs/graph/graph.html`](docs/graph/graph.html) — generated knowledge graph of the codebase.

## Versioning

See [`rules.md`](rules.md). Every update is tagged `major.minor.bugs`.
