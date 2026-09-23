# Decision log

Every significant decision for Schlüsselbild, with the reason and the consequence.
Format: **Decision — Reason — Consequence.** Newest sections are appended; the
`rules.md` ledger records the version each change shipped in.

## D1 — Target runtime: Unity 6 + Meta XR SDK

- **Decision:** Build the Quest 3 app as a native Unity 6 (6000.6.2f1) project using the
  Meta XR SDK.
- **Reason:** Reading QR codes through the headset requires the forward RGB **Passthrough
  Camera API**, which is only exposed to native apps (Unity/Unreal/native), not WebXR.
  Unity + Meta XR is the best-supported path and the SDK was already available.
- **Consequence:** We need the full Android build toolchain (SDK/NDK/OpenJDK) and cannot
  iterate in a browser. This ruled out WebXR (fast to deploy but no camera access) and
  Godot (weaker Meta camera support).

## D2 — Scope: full encode + passthrough scan + overlay

- **Decision:** Both directions in-headset — type a secret and compose the QR page, and
  scan a real page through passthrough and decrypt it.
- **Reason:** The whole point of the original project is the unreadable-artifact loop; a
  scan-only viewer would drop half of it.
- **Consequence:** More surface to build (keyboard, page composition, capture, decode).

## D3 — Byte-for-byte compatibility with the Python oracle

- **Decision:** Keep the exact wire format: AES-256-GCM blob `nonce(12)‖tag(16)‖ciphertext`,
  QR envelope `[index:1][total:1][len:2 BE][chunk]`, 256-byte chunks, ECC-H, page margins
  48 / gap 36 / 3 columns.
- **Reason:** Preserves all prior work, lets the proven Python implementation act as a test
  oracle, and makes cross-platform pages meaningful.
- **Consequence:** The C# layers must not "improve" the format. Verified both directions
  (see `TESTING.md`).

## D4 — Repository layout: Unity at root, Python under `legacy/python/`

- **Decision:** The Unity project lives at the repository root; the original Kitsune code is
  kept verbatim under `legacy/python/`.
- **Reason:** A Unity project must be openable directly at its root. Keeping the Python next
  to it gives one repository and a permanent, runnable oracle.
- **Consequence:** `.gitignore` must exclude Unity's `Library/`, `Logs/`, `Builds/`,
  `UserSettings/`, `.utmp/` and the key file.

## D5 — Key handling: import `key.bin` + hex entry

- **Decision:** Load a 32-byte key from `Application.persistentDataPath/key.bin`; allow the
  user to type a 64-char hex (or base64) key in-headset.
- **Reason:** Matches Kitsune's single-machine "local key is the trust boundary" model and
  stays byte-compatible without inventing a key-exchange protocol.
- **Consequence:** No key distribution; a page made on the PC needs the same key on the
  headset. There is no multi-user story (out of scope, as in the MVP).

## D6 — Crypto primitive: managed AES-256-GCM (`ManagedGcm`)

- **Decision:** Implement AES-256-GCM in pure C# (AES-CTR + GHASH) instead of using
  `System.Security.Cryptography.AesGcm`.
- **Reason:** `AesGcm` **compiles but throws `PlatformNotSupportedException` on Unity's Mono
  runtime**, so EditMode tests and the Editor fail. The managed implementation is
  IL2CPP-safe and has no dependency.
- **Consequence:** We own ~150 lines of crypto, mitigated by known-answer tests against
  NIST SP 800-38D vectors and 212/212 byte-identical interop checks against OpenSSL.

## D7 — QR library: ZXing.Net (managed, netstandard2.0)

- **Decision:** Use the `netstandard2.0` build of ZXing.Net dropped into
  `Assets/Plugins/ZXing/zxing.dll`, behind `IQrDecoder`/`IQrEncoder`.
- **Reason:** Pure managed => works in Editor, Mono and IL2CPP with no native toolchain.
  `IQrDecoder` isolates it so a native `zxing-cpp` backend can be swapped in later.
- **Consequence:** Must force byte mode (ISO-8859-1 mapping) so arbitrary ciphertext bytes
  survive; verified against the Python fixtures.

## D8 — Capture: MRUK `PassthroughCameraAccess`, throttled and downscaled

- **Decision:** Use `Meta.XR.PassthroughCameraAccess.GetTexture()` (from the installed MRUK),
  decode on every Nth frame, on a grayscale copy downscaled to a long edge of 900 px.
- **Reason:** The passthrough camera is the only way to see a physical page. Downscaling and
  throttling keep the frame budget healthy; zbar/ZXing cost scales with pixel count.
- **Consequence:** The runtime asmdef references `meta.xr.mrutilitykit`. The API is
  device-only, so scanning cannot be exercised in the Editor.

## D9 — UI built from code with uGUI + TextMeshPro

- **Decision:** Build the world-space UI entirely in C# (`UiFactory`/`UiBootstrap`) on a
  world-space Canvas with uGUI + TMP, rather than authoring prefabs or using UI Toolkit or
  the Interaction SDK prefabs.
- **Reason:** Code-built UI is reproducible, diff-friendly, testable in EditMode, and avoids
  fragile scene/prefab wiring. uGUI is the most robust world-space VR UI path.
- **Consequence:** We implement interaction and theming ourselves (see D10, D12).

## D10 — Input: Meta `OVRInputModule` + `OVRRaycaster`

- **Decision:** Drive UI with the SDK's `OVRInputModule` (on the EventSystem) and
  `OVRRaycaster` (on the Canvas), with per-hand ray lines and automatic hand selection.
- **Reason:** The first version used a hand-rolled pointer that bypassed `EventSystem`,
  which did not reliably fire buttons. The official Meta stack is what Meta's own samples
  use and handles hover/press/click correctly.
- **Consequence:** Removed `ControllerRayPointer` entirely. Trigger is
  `OVRInput.Button.PrimaryIndexTrigger`. Confirming real button presses still requires a
  device or the Meta XR Simulator.

## D11 — Text entry: custom in-world keypad

- **Decision:** A keypad built with the UI factory (A–Z, 0–9, symbols, space, backspace,
  clear, 123/ABC toggle, 1024-char cap).
- **Reason:** Self-contained, fully controllable for accessibility (large keys), and avoids
  pulling in the Interaction SDK keyboard prefab.
- **Consequence:** We maintain `Keypad`/`KeypadModel` ourselves; the model is unit-tested
  without any scene.

## D12 — Visual system: procedural rounded sprites

- **Decision:** Generate rounded-rect **9-slice** sprites at runtime (`UiSprites`) rather
  than shipping art. Cards/buttons get radii, a subtle 2 px border, and a soft drop shadow.
- **Reason:** Delivers a "premium" look with zero external assets and keeps the whole visual
  system in version-controlled code.
- **Consequence:** Sprites are cached per radius; the palette, radii and spacing live in
  `UiTheme` as a small design-token set (see `UI-DESIGN.md`).

## D13 — Navigation: one screen at a time, `initialized` before first navigation

- **Decision:** `AppController.Initialize` sets `initialized = true` **before** calling
  `ShowHome()`.
- **Reason:** `ShowHome()` early-returns when `!initialized`, so the original ordering meant
  `HideAll()` never ran and **all five screens rendered stacked**. Found by running the app
  and reading a screenshot.
- **Consequence:** Only the active screen is visible; `Show()` does `HideAll()` then shows the
  target. Regressions are guarded by keeping navigation logic simple.

## D14 — Verification strategy: EditMode fixtures + Play mode + on-device

- **Decision:** Three tiers — (1) EditMode tests including Python-generated fixtures,
  (2) Play-mode smoke checks and screenshots through Unity MCP, (3) on-device APK.
- **Reason:** The crypto/QR/page format can be proven deterministically off-device; the
  optical and interaction paths genuinely need hardware.
- **Consequence:** `TESTING.md` states exactly what is and is not proven. Device-only gaps
  (real scanning, controller clicks) are called out rather than implied.

## D15 — Versioning: `major.minor.bugs` ledger

- **Decision:** `rules.md` is the source of truth; every committed update adds a row tagged
  `major.minor.bugs` (major = breaking format/architecture, minor = feature, bugs = fix/polish).
- **Reason:** The user's explicit requirement; the in-app `VersionInfo` mirrors the tag so a
  build maps to a ledger entry.
- **Consequence:** Each release also gets a matching git tag.

## D16 — Android toolchain: use Unity's bundled SDK/NDK/OpenJDK

- **Decision:** Install Android Build Support + SDK/NDK r27c + OpenJDK 17 and set the paths
  from code via `UnityEditor.Android.AndroidExternalToolsSettings`.
- **Reason:** The Meta SDK failed to compile without Android Build Support
  (`AndroidExternalToolsSettings` missing), and Unity did not read the registry tool paths.
  Setting them programmatically in the build method is deterministic.
- **Consequence:** `DevTools.BuildAndroid` configures IL2CPP/ARM64/Linear/min API 32 and
  produces `Builds/Schlusselbild.apk`.

## D17 — Delegation policy for the `mcode` harness

- **Decision:** Use the external `mcode` (DeepSeek V4.1 Flash) agent only when a task is
  large, isolated in scope, cheaply verifiable, and genuinely parallel or too big for one
  context; otherwise do the work inline. Ask before any large delegation.
- **Reason:** Delegation kept the main context small and caught real issues (the `AesGcm`
  Mono failure), but raised total token spend and incurred timeouts/coordination overhead.
- **Consequence:** Bulk layers (crypto, QR/page, capture) were delegated; UI design and
  fixes were done inline.

## D18 — Do not commit large local installers

- **Decision:** Ignore `/Meta/` and `*.msi` in `.gitignore`.
- **Reason:** The Meta Quest Developer Hub and XR Simulator installers (200 MB and 279 MB)
  exceeded GitHub's 100 MB limit and blocked a push.
- **Consequence:** They stay local-only; install them manually when needed.

## D19 — Meta XR Simulator for controller testing

- **Decision:** Use the supplied Meta XR Simulator (and Quest Developer Hub) for
  without-headset development.
- **Reason:** Controller/hand interaction cannot be exercised by a mouse or by MCP; the
  simulator is the intended local substitute.
- **Consequence:** The MSI needs admin elevation to install; until then, interaction is
  verified by code inspection only, not by an actual click.

## D20 — Documentation and architecture graph

- **Decision:** Keep a decision log (`DECISIONS.md`), architecture, UI design, build and
  testing docs under `docs/`, and generate a knowledge graph of the codebase for the
  architecture view.
- **Reason:** The user asked for detailed docs recording every decision and reason, and for
  graphify to be used where it adds value.
- **Consequence:** Docs are versioned with the code; the graph is a generated artefact under
  `docs/graph/`.
