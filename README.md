# Schlüsselbild — Meta Quest 3

An encrypted-page privacy demo, ported to **Meta Quest 3**. A secret is encrypted
(AES-256-GCM) and printed onto an ordinary-looking page as one or more QR codes.
Anyone glancing at the page — or scanning a QR with a phone — recovers only random
ciphertext. An authorized viewer wearing the headset points the **passthrough camera**
at the page and the plaintext is recovered and shown in a spatial AR overlay.

This is a college / proof-of-concept project, **not** a production security product.

## Status

Early port. The original Python implementation (Kitsune) is preserved under
[`legacy/python/`](legacy/python/) as the reference implementation and test oracle.

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
- All-managed C#: `System.Security.Cryptography.AesGcm` + ZXing.Net (behind `IQrDecoder`)
- Unity MCP (`com.gamelovers.mcp-unity`) for Editor automation
- mcode (MiniMax Code, DeepSeek V4.1 Flash) for delegated implementation

## Accessibility

Controllers **and** hand tracking for every action, comfort/motion options
(seated/standing, snap turn, adjustable UI distance), and vision options
(S/M/L text scaling, WCAG-AA contrast, colorblind-safe status states).

## Versioning

See [`rules.md`](rules.md). Every update is tagged `major.minor.bugs`.
