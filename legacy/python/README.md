# Kitsune -- encrypted-page privacy demo

## What it is

Kitsune is a privacy MVP: a secret is encrypted and printed onto an
ordinary-looking page as one or more QR codes. To anyone glancing at the page -
or even cleanly scanning a QR with a phone - it is meaningless; they recover only
random ciphertext bytes. An authorized viewer who holds the same secret key
points a webcam at the page and the plaintext is recovered live on screen. This
is a college / proof-of-concept project, **not** a production security product:
it demonstrates the "unreadable artifact, recovered through a camera" idea end to
end, and deliberately leaves the hard real-world machinery (key distribution,
multi-user auth, hardened key storage, AR overlays) out of scope.

## How it works

Kitsune is two mirror-image pipelines - one to build the page, one to read it
back through a camera:

```
ENCODE
  plaintext ──AES-256-GCM──▶ ciphertext bytes ──segment──▶ QR code(s) ──compose──▶ printable page (PNG)

DECODE
  webcam ──capture frames──▶ locate & decode ALL QRs ──reassemble──▶ ciphertext bytes ──AES-256-GCM──▶ plaintext on screen
```

**Encoding.** The plaintext is encrypted with AES-256-GCM. The stored blob is
`nonce(12) ‖ tag(16) ‖ ciphertext` - a fresh random nonce every time, plus the
GCM authentication tag. That blob is sliced into 256-byte chunks, and each chunk
becomes one QR code. Before it goes into its QR, each chunk is wrapped in a
4-byte envelope header:

```
[ index : 1 ][ total : 1 ][ chunk_len : 2 (big-endian) ][ chunk bytes … ]
```

`index` is which chunk this is, `total` is how many chunks there are, and
`chunk_len` is how many payload bytes follow. Those three fields are all the
decoder needs to collect the QRs **in any order**, confirm none are missing, and
concatenate the chunks back into the exact original ciphertext. A short secret
fits in a single QR; a ~1 KB secret spans about five. The QRs are laid out on a
titled white page and saved as one PNG.

**Decoding.** The webcam loop grabs frames continuously. Each frame is converted
to grayscale and handed to zbar, which locates and decodes **every** QR in the
image at once - run in *binary* mode so it returns the exact raw bytes rather
than a text interpretation. The envelopes are reassembled by `index`; once a
complete set is present, the ciphertext is rebuilt and AES-256-GCM-decrypted.
Most frames are blurred, glared, or mid-focus, so the loop simply keeps trying -
out of many frames per second, one clean frame wins. On the first frame that both
decrypts *and* authenticates, the plaintext is frozen on screen and printed to
the terminal.

## System design

Kitsune is organised as a layered pipeline. Each layer has one responsibility
and passes a well-defined value to the next layer:

```text
                           ENCODE
  secret string
       |
       v
  AES-256-GCM + local key
       |
       v
  nonce || tag || ciphertext
       |
       v
  256-byte chunker
       |
       v
  [index || total || chunk_len || chunk] per chunk
       |
       v
  QR encoder, byte mode, ECC level H
       |
       v
  QR PNG files -> printable page PNG

                           DECODE
  webcam frame / page PNG
       |
       v
  grayscale image -> zbar QR-only binary scan
       |
       v
  QR envelopes, in any discovery order
       |
       v
  validate headers -> sort by index -> concatenate
       |
       v
  AES-256-GCM authentication and decryption
       |
       v
  recovered UTF-8 secret
```

### Architectural layers

| Layer | Implementation | Responsibility | Boundary |
|-------|----------------|----------------|----------|
| Secret and key | `aes_encrypt`, `aes_decrypt`, `load_or_create_key` in `qr_crypto.py` | Convert a Python string to authenticated ciphertext and back; validate the 32-byte AES key. | Receives/returns strings and raw bytes; never handles images. |
| QR transport | `bytes_to_qr`, `_decode_qr_bytes`, `qr_to_bytes` in `qr_crypto.py` | Carry arbitrary ciphertext bytes through one or more QR symbols and restore the original byte stream. | Does not know the plaintext or page layout. |
| Page | `compose_page`, `_decode_all_qr_bytes`, `_reassemble` in `compose_page.py` | Create one printable image and decode every QR found in a photographed or reopened page. | Reuses the crypto and QR envelope formats without redefining them. |
| Live capture | `run_live`, `decode_frame` in `live_decode.py` | Convert camera frames to the page-layer input, retry through optical noise, and render status/results. | Does not implement encryption or a second QR decoder. |
| Verification | `test_*.py`, `gate_page.py` | Prove the layers independently and measure the margin under simulated capture conditions. | Uses fixed in-memory test keys and temporary files; it does not modify `key.bin`. |

### Cryptographic subsystem

The cryptographic boundary uses AES-256-GCM with a 32-byte key and a fresh,
random 12-byte nonce for every encryption operation. GCM produces ciphertext
and a 16-byte authentication tag. Kitsune stores them in a self-contained blob:

```text
blob = nonce[12] || tag[16] || ciphertext[n]
```

The nonce is not secret; storing it beside the ciphertext lets the decoder
recover all inputs needed for decryption. The tag is checked by GCM during
`finalize()`. A wrong key, modified byte, or corrupted QR therefore raises
`InvalidTag` instead of returning plausible plaintext. Plaintext is encoded as
UTF-8 before encryption and decoded as UTF-8 only after authentication succeeds.

The local key policy is deliberately small for this MVP:

1. `load_or_create_key()` reads `key.bin` when it exists.
2. If the file is absent, it creates 32 random bytes with `os.urandom`.
3. The file is best-effort restricted to owner access with `chmod(..., 0o600)`.
4. `key.bin` is ignored by Git and is never part of the page artifact.

The key is the trust boundary. A recipient needs the same key file or the
plaintext cannot be recovered. There is no key exchange, password derivation,
key wrapping, or multi-recipient key management in this branch.

### QR transport format

The QR layer transports raw bytes, not text and not base64. This matters because
encrypted output has no linguistic structure, and converting it through a text
codec can alter bytes above `0x7f`. Each ciphertext blob is split into chunks of
at most `DATA_PER_CHUNK = 256` bytes. Every chunk is prefixed with a four-byte
big-endian envelope:

```text
offset  size  field
------  ----  ----------------------------------------
0       1     chunk index, starting at 0
1       1     total number of chunks
2       2     payload length, unsigned big-endian
4       n     raw ciphertext chunk
```

The envelope is encoded by `qrcode` in byte mode using error-correction level H.
The one-byte index and total fields support up to 255 chunks. QR files are
named `qr_000.png`, `qr_001.png`, and so on, but filenames are not authoritative:
the decoder reassembles by the embedded index. This permits the page decoder
and the QR-file decoder to accept symbols in any order.

Before concatenating data, the decoder enforces these invariants:

- Every symbol contains at least the four-byte header.
- All symbols declare the same total chunk count.
- The declared index is within the declared total.
- The declared payload length equals the bytes actually present.
- No required index is missing.
- A duplicate index is rejected by the file decoder; conflicting duplicate data
  is rejected by the page decoder.

Only after these structural checks pass is the reconstructed byte stream sent to
AES-GCM. QR error correction can recover damaged modules, but it is not a
replacement for GCM authentication: a structurally valid but altered stream is
still rejected at the cryptographic boundary.

### Page composition and decoding

`compose_page()` is an orchestration layer rather than a new encoding format.
It encrypts the secret, asks `qr_crypto.bytes_to_qr()` to create the segmented
QR images, and places those images on a white grayscale page with a title,
margin, and gutters. The default layout uses three QR symbols per row. The
temporary QR directory is removed after the final page is written, so the
deliverable is one PNG rather than a directory of implementation artifacts.

`decode_page_image()` performs the inverse operation on one in-memory image:

1. Convert the image to grayscale.
2. Configure zbar to scan QR symbols only.
3. Enable zbar binary mode so decoded values remain exact raw bytes.
4. Walk every symbol returned by zbar, not just the first one.
5. Validate and reassemble the envelopes by index.
6. Pass the reconstructed blob to AES-GCM decryption.

`decode_page()` is the file adapter around this path. Keeping the in-memory
function separate lets `gate_page.py` apply simulated camera effects before
decoding and lets `live_decode.py` use the identical page decoder for real
frames.

### Live capture subsystem

The webcam path deliberately separates preview rendering from QR decoding:

```text
camera capture
      |
      +--> display every raw BGR frame --------------------> OpenCV window
      |
      +--> every Nth frame: BGR -> grayscale -> downscale -> zbar -> decrypt
```

`open_camera()` probes a preferred camera or indices 0, 1, and 2 and confirms
that a frame can actually be read. `decode_frame()` accepts the BGR NumPy array
returned by OpenCV, converts it to a grayscale PIL image, optionally limits the
long edge to 900 pixels, and calls `decode_page_image()`.

The default loop decodes every second frame while displaying every frame. The
downscale is applied only to the live path because zbar's work grows with image
area; the clean and photograph-simulation gates continue to use full-resolution
images. This makes the preview responsive without changing the proven crypto or
page formats.

The loop treats failures by type and state:

- An incomplete, blurred, or unreadable page causes the frame attempt to fail
  and scanning continues.
- A structurally complete page with a wrong key or altered ciphertext raises
  `InvalidTag`, displays `INVALID - auth failed`, and continues scanning.
- The first authenticated plaintext is printed to the terminal, rendered in a
  success overlay, and freezes the displayed frame.
- Pressing `Esc` releases the camera and destroys the OpenCV window.

`--selfcheck` bypasses the physical camera but still exercises the real
`decode_frame()` path by loading a composed page as a BGR frame. It is useful
for separating software plumbing failures from camera focus, distance,
lighting, and resolution problems.

### Verification strategy

The tests form progressively broader gates:

1. `test_roundtrip.py` checks AES/QR losslessness across empty, Unicode,
   boundary, and multi-chunk messages. It also checks key-file creation,
   order-independent reassembly, and GCM tamper detection.
2. `test_page.py` checks compose-save-reopen-decode for short, Unicode, and
   multi-QR pages, plus wrong-key authentication failure.
3. `gate_page.py` applies downscale/upscale, JPEG compression, rotation, and
   brightness gradients to reopened pages. Small payloads are the release gate;
   the larger payload result is reported as a reliability range.
4. `test_live.py` checks the camera-free BGR frame path, including the live
   downscale setting and wrong-key handling.
5. `live_decode.py --selfcheck` runs the complete decode-from-frame flow without
   requiring a webcam.

The result is a deterministic software boundary with a deliberately separate
optical boundary. If clean QR or self-check tests fail, investigate the Python
dependencies or format implementation. If they pass but live capture fails,
investigate camera distance, focus, glare, page size in frame, and lighting
before changing the crypto or QR format.

## Design decisions

The reasoning behind each choice - this is the part worth reading if you only
read one:

- **AES-256-GCM, not CBC.** GCM is *authenticated* encryption: the blob is
  `nonce(12) ‖ tag(16) ‖ ciphertext`, and decrypt recomputes the tag over the
  ciphertext. Any altered or garbled byte - a misread QR module, a wrong key, a
  tampered page - fails the tag and raises `InvalidTag` instead of returning
  plausible-looking garbage. An earlier CBC layer had no such check: a bad
  decrypt just produced junk you could not distinguish from a real secret. For a
  webcam loop decoding noisy frames continuously, GCM is exactly what lets the
  loop treat "did it decode?" as a trustworthy yes/no - a real secret is shown
  only when it authenticates, and a bad read shows **INVALID** while the loop
  keeps scanning, never freezing a false positive.

- **QR codes, not the original visible-glyph cipher.** The artifact carries
  ciphertext, which is **uniformly random bytes** with no linguistic structure.
  The project's first idea was a visible-glyph cipher read back with generative
  OCR - but that is the wrong tool for random bytes. OCR and vision-language
  models are trained to resolve ambiguity toward *likely language*, so on
  high-entropy input they hallucinate plausible characters and cannot faithfully
  reproduce arbitrary bytes; and because GCM authenticates, a single wrong byte
  fails the entire decrypt, so "almost right" is a total failure. QR is the
  opposite: a deterministic byte channel with built-in **Reed–Solomon error
  correction (ECC level H)** and hard geometry (finder patterns, quiet zones), so
  it recovers the *exact* bytes off a photographed page under blur, mild rotation,
  and uneven lighting. The photograph-simulation gate (`gate_page.py`) is what
  proved this holds under capture-like degradation.

- **GLM-OCR was dropped from the pipeline.** GLM-OCR is a ~2.6 GB document
  *language* model. It is **not** part of the decode path on the `mu` branch, for
  the reason above - it cannot faithfully read random ciphertext - and it added a
  heavy dependency and a slow cold start for zero benefit. A separate webcam +
  GLM-OCR document scanner exists only on the `gs` branch; it is unrelated to this
  project and is **not on `mu`**. GLM-OCR survives here only as an optional "what
  an outsider's OCR sees" screenshot for the report (it reads the page as noise).

- **The live loop decouples display from decode.** Earlier the loop decoded every
  frame at full resolution on the display thread, so the preview stalled to the
  decode rate (13–70 ms/frame) and felt laggy. Now every raw frame is shown for a
  smooth preview, while decode runs only on every 2nd frame and on a grayscale
  copy downscaled to a 900 px long edge - QR detection does not need 720p, and
  zbar's cost scales with the pixel count it walks. The preview tracks the
  camera's native rate; numbers are in [Running the demo](#running-the-demo).

## Project structure

Every file tracked on `mu` (there is no `requirements.txt`, no `scanner.py`, and
no model weights on this branch):

| File | What it does | What it proves / its role |
|------|--------------|---------------------------|
| `qr_crypto.py`      | AES-256-GCM encrypt/decrypt; slices the blob into segmented QR PNGs and reads a single QR back (zbar binary mode). | The crypto + QR-channel core. Its format (envelope, `DATA_PER_CHUNK=256`, ECC-H) is frozen - everything else builds on top. |
| `compose_page.py`   | `compose_page(secret)` lays the QR PNGs onto one printable page; `decode_page_image(img)` finds **all** QRs in one image, reassembles the envelopes, and decrypts. | The page layer, built strictly on top of `qr_crypto` without touching its format. |
| `live_decode.py`    | The live webcam loop; `decode_frame(frame, max_dim=…)` is the per-frame path; display is decoupled from decode; `--selfcheck` runs the decode path headless. | Turns a photographed page into recovered plaintext, live. |
| `test_roundtrip.py` | Prompt 1 gate: encrypt → QR → decode → decrypt on clean digital QRs. | The crypto + QR channel is lossless. **17/17.** |
| `test_page.py`      | Prompt 2: compose a page → save → reopen → decode (short, unicode, multi-QR, 256 B, wrong-key). | The page layer round-trips exactly, and a wrong key fails. **5/5.** |
| `gate_page.py`      | Prompt 2 gate: degrade a reopened page with **simulated** photo effects (downscale, JPEG, ±5° rotation, brightness gradient) and measure the decode rate. | The page survives capture-like degradation *before* any real camera. |
| `test_live.py`      | Prompt 3/4: feed a saved page as a BGR frame through `decode_frame` (including the downscaled live path and a wrong key). | The per-frame plumbing works with no camera. **4/4.** |
| `key.bin`           | The local AES-256 key, auto-created on first use. | **Gitignored - never committed.** |

## Setup

Requires **Python 3.10+** (developed on **3.13.3**) and the native **zbar**
library. `pyzbar` binds to zbar but **`pip` cannot install it for you** - you
must install the system package first:

```bash
brew install zbar            # macOS
# sudo apt install libzbar0  # Debian / Ubuntu
```

Then create a virtual environment and install the Python dependencies:

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install cryptography qrcode pillow pyzbar numpy opencv-python
```

Versions this was built and verified against:

| Package | Version |
|---------|---------|
| cryptography | 49.0.0 |
| qrcode | 8.2 |
| pillow | 12.3.0 |
| pyzbar | 0.1.9 |
| numpy | 2.5.1 |
| opencv-python | 5.0.0.93 |

Confirm the install with a single quick gate (should print `17/17 passed`):

```bash
python test_roundtrip.py
```

The full test/gate list and what "green" means is in
[Testing and verification](#testing-and-verification).

## Running the demo

The live decoder needs a real camera, so run it by hand:

1. **Generate a page for a known secret.** This also creates `key.bin` if it
   doesn't exist; the same key is reused for decoding.

   ```bash
   python -c "from compose_page import compose_page; compose_page('meet at dawn - 0400 pier 7', 'demo_page.png'); print('wrote demo_page.png')"
   ```

2. **Show `demo_page.png` large.** Open it and make it fill the screen. **A phone
   screen works** - no print needed. Bigger, sharper QR modules decode far more
   reliably than a small thumbnail.

3. **Run the live decoder** (uses the same `key.bin`):

   ```bash
   python live_decode.py
   ```

   Flags:

   | Flag | Effect |
   |------|--------|
   | `--selfcheck` | Headless: compose a known secret, render it to a frame, decode it, and assert the round-trip - no camera. |
   | `--camera-index N` | Force a specific webcam (it probes 0, 1, 2 by default). |
   | `--decode-every N` | Run decode on every Nth captured frame; every frame is still displayed (default 2). |
   | `--decode-max-dim PX` | Cap the long edge (px) fed to zbar; `0` = full resolution (default 900). |
   | `--key-path PATH` | Use a different `key.bin`. |

4. **Point the webcam at the page and get close - about 10–20 cm.** The single
   biggest factor in the live test was **distance**: the QR has to fill the frame
   with big, in-focus modules. A live window shows the feed with a "Scanning…"
   status and a small live **fps / decode-ms** readout. Hold steady and let the
   camera focus; the loop tries frame after frame, so one clean frame wins.

5. **Confirm the recovery.** On the first good frame the window **freezes** with a
   big green **DECODED OK** banner and the recovered plaintext, and the full
   plaintext prints to the terminal - it must read `meet at dawn - 0400 pier 7`,
   the same secret from step 1. Press **Esc** to quit (releases the camera).

**What the live test actually showed (real webcam, verified - not simulation):**

- **Distance is everything - get to ~10–20 cm.** Held close so the QR filled the
  frame, decoding was quick and steady; too far and it never locks on.
- **A phone screen decoded fine** - a physical print is not needed for the demo.
- **Short and ~1 KB secrets both decoded at that distance**, about the same in
  practice: once you are close enough for the modules to be sharp, the five-QR
  page recovers about as readily as a one-QR page. (This is *better* than the
  camera-free gate predicts - see [Testing and
  verification](#testing-and-verification) - because at close range the page
  occupies far more pixels than the gate's whole-page downscale simulates.)
- If a frame's QRs reassemble but the key/authentication is wrong, the window
  shows **INVALID - auth failed** and keeps scanning - a bad decode is never shown
  as if it were real.
- If it *consistently* fails, the cause is almost certainly **optical**, not the
  crypto/QR format (proven by the gates): move closer, kill glare, let it focus,
  and flatten/square the page to the lens.

### Live-loop responsiveness

Two responsiveness fixes were added in the final round; the other two intended
fixes were already present (zbar is restricted to **QR symbology only** in
`compose_page`, and the loop already **freezes and stops scanning** on the first
success). The two changes: **display is decoupled from decode** (every raw frame
shown, decode only every 2nd frame), and **decode runs on a grayscale copy
downscaled to a 900 px long edge**. Downscaling applies to the live path only -
the gates and tests still decode at full resolution, so nothing proven changes.

Before → after (medians over a camera-free benchmark on the same decode path;
real-camera per-frame decode was the 13–70 ms range noted above):

| Case (720p frame) | Before: full-res, every frame | After: ≤900 px, every 2nd frame |
|---|---|---|
| Per-frame decode, page in view | ~18–22 ms | **~14–16 ms** (~1.5× faster) |
| Per-frame decode, hunting (no page) | ~41 ms | **~27 ms** |
| Preview fps | gated by decode (~24 fps worst case, ~14 fps on the 70 ms real frames) | **tracks camera native (~30 fps), smooth** - decode is off the display path and paid on half the frames |

## Testing and verification

Run these from the repo root with the venv active. "Green" is called out for each.

| Command | Green means | Notes |
|---------|-------------|-------|
| `python test_roundtrip.py` | `GATE PASSED`, **17/17** | Prompt 1: clean digital QR round-trips. |
| `python test_page.py` | all **5** `PASS` lines | Prompt 2: page compose → save → reopen → decode; includes wrong-key. |
| `python gate_page.py` | `GATE PASSED`; short & ~256 B at **100%** | Prompt 2 gate: simulated-photo degradation (see reliability note below). |
| `python test_live.py` | all **4** `PASS` lines | Prompt 3/4: decode-from-frame plumbing, incl. the downscaled live path. |
| `python live_decode.py --selfcheck` | `decode_frame round-trip: PASS` | End-to-end decode with no camera. |

**Reliability, stated honestly.** The gate composes a page, saves it, reopens it,
then stacks all four photograph degradations (downscale, JPEG, ±5° rotation,
brightness gradient) over ~20 seeded variants per payload size:

- **Small payloads (short, ~256 B) decode at ~100%** through the combined
  simulation - this is the gated bar, and it must hold.
- **~1 KB is *not* a gated size, and it shows a range run-to-run: ~85–95%.** This
  is expected, not a regression. AES-GCM uses a fresh random nonce on every
  encrypt, so each run produces different ciphertext and therefore different QR
  bit patterns; a five-QR page sits right at zbar's decode margin under stacked
  degradation, so the combined pass count jitters between roughly 85% and 95%
  across runs while the QR format itself is unchanged. (The change to the live
  loop cannot affect this number at all: `gate_page.py` imports only from
  `compose_page`, never `live_decode`, and decodes at full resolution.)
- In the **live** test at ~10–20 cm, the ~1 KB page decoded reliably regardless,
  because up close the page fills the frame at high resolution - far more pixels
  than the gate's whole-page downscale models. The simulation is deliberately
  harsher than close-range capture.

The takeaway: quote the ~1 KB gate result as a **range (~85–95%)**, not a single
best run, and treat payload size as a margin question - it matters most when the
page is small in frame, so get close, and keep secrets modest if you cannot.

## Known limitations and threat model

Kitsune provides **privacy through an unreadable artifact**, at MVP scope - it is
**not** an "unbreakable" system and does not claim to be. What it actually
delivers: a person who glances at, photographs, or even cleanly scans the page
recovers only AES-256-GCM ciphertext - uniformly random bytes that reveal nothing
about the plaintext without the key, and cannot be silently altered without
failing the authentication tag. That is a real, useful privacy property against a
casual onlooker or shoulder-surfer. Its limits, stated plainly:

- **Local key only.** The AES-256 key is a single `key.bin` on one machine
  (gitignored, never committed). Anyone with that file - or the machine - can
  decrypt; key security *is* file security here.
- **No key distribution or exchange.** Encode and decode must share the same
  `key.bin`. There is no protocol to get a key to a second party, so the demo is
  effectively **single-user / single-machine**.
- **Not evaluated against a determined adversary.** No side-channel, traffic, or
  metadata analysis; the *presence* of a secret is obvious (it is visibly a page
  of QR codes). The claim is confidentiality of *contents* against a casual
  viewer - not steganography, and not resistance to a motivated attacker who
  already has the file.
- **System `zbar` is required.** `pip install pyzbar` does not bundle the native
  library; without `brew install zbar` / `apt install libzbar0`, importing the
  decode path fails with an `ImportError`/`OSError`.
- **Large-payload headroom is distance-sensitive.** Up close (~10–20 cm) a ~1 KB
  five-QR page decodes reliably, but its margin degrades with distance and under
  stacked degradation (see the gate range above). Keep secrets modest, or get
  closer.
- **Out of scope by design:** AR glasses, headsets, live tracking, multi-user
  auth, and key distribution.

## Future work

Deliberately deferred, not failures - the natural next steps beyond the MVP:

- **Decoy cover-text layer (the scoped-out "Fork A").** An earlier plan hid the
  page's purpose behind innocuous-looking cover text alongside the QR channel.
  It was cut to keep the MVP focused on the core "unreadable artifact" property;
  the security here comes from the ciphertext being unreadable, not from hiding
  that a secret exists.
- **Key distribution.** A real way to get a key to a second party (exchange,
  wrapping, per-recipient keys) - the single biggest gap between this demo and
  anything multi-user.
- **AR / live overlay.** The original ambition: recover and render the plaintext
  in an AR/heads-up view aligned to the page, rather than in a desktop window.
