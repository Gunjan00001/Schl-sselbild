# UI design

The interface is a single world-space Canvas (`UiCanvas`, 1240 x 940 canvas px at 1000 px/m)
placed in front of the viewer at a comfortable distance. It uses the **Soft Glass** direction:
a navy gradient backdrop with a violet glow, layered surfaces with a top sheen, big rounded
corners, deep soft shadows, and one violet accent colour.

## Design tokens (`UiTheme`)

Palette (default / colorblind-safe where different):

| Token | Default | Use |
|-------|---------|-----|
| `Background` | `#141824` | Canvas backdrop (violet glow overlay on top) |
| `Panel` | `#1C2230` | Screen card surface |
| `PanelRaised` | `#262D3D` | Secondary buttons, controls |
| `Border` | `#363F52` | 2 px card/button border |
| `Text` | `#F2F4FA` | Primary text |
| `Accent` | `#7A65FF` | Primary actions, focus, rays, glow |
| `Success` | `#3DD68C` (cblind `#58A6FF`) | OK status |
| `Warning` | `#F5B54A` (cblind `#C9D2DE`) | Scanning status |
| `Error` | `#F26D6D` (cblind `#FF9A3D`) | Invalid status |

Geometry and type:

| Token | Value |
|-------|-------|
| `CardRadius` | 56 px |
| `ButtonRadius` | 40 px |
| `BorderThickness` | 2 px |
| `PixelsPerMeter` | 1000 |
| Title / Body / Button / Caption | 64 / 40 / 40 / 32 px (x S 0.85, M 1.0, L 1.25) |

Spacing tokens (`UiFactory`): `Pad` 44, `Gap` 26, `ButtonHeight` 96.

## Components (`UiFactory`, `UiSprites`)

- **Rounded sprites are generated at runtime** (`UiSprites`) as white 9-slice textures with
  anti-aliased corners; colour is applied by tinting. `RoundedStroke` produces a ring-only
  sprite used for the 2 px border. `VerticalGradient` is a white top-down sheen used for the
  glass highlight (texture alpha 1.0 at the top, 0.55 at the bottom). This means no art
  assets are required.
- **Panel** = optional soft drop shadow + border ring + filled surface + low-alpha sheen.
- **Button** = rounded surface, subtle border (non-primary), colour-tint states
  (normal/highlight/pressed/disabled). Primary buttons use the accent fill.
- **Slider** = rounded track with an accent fill and a round handle.
- **Toggle** = rounded box with an accent check.
- **ScrollView** = rounded panel with a slim rounded scroll indicator on the right.

## Screens

| Screen | Contents |
|--------|----------|
| **Home** | Title + accent underline, subtitle, `Create page`, `Scan page`, key status + `Enter key`, `Settings`, `Help`, version. |
| **Create** | Secret field, `Preview` area, on-screen keypad, `Encrypt` / `Export` / `Self-test` / `Back`, status line. |
| **Scan** | Status chip, live preview area, `Back`. |
| **Decoded** | Status chip, recovered plaintext (scrollable), `Back`, `Clear`. |
| **Settings** | Text size S/M/L, UI distance slider, seated mode, snap turn, colorblind-safe, `Back`. |

Modals (Help, Enter key) appear on a dimmed layer above the current screen.

## Transitions

Each screen fades in over ~160 ms from alpha 0 with a very slight scale-up (0.985 -> 1),
using unscaled time so comfort settings never freeze it. Screens are hidden by deactivating
their root.

## Accessibility

- **Text scale S/M/L** is applied app-wide through `UiTheme` and rebuilds the UI live.
- **Status is never colour alone:** every state is `icon + word + colour`
  (`•` OK, `×` INVALID, `…` SCANNING) via `UiTheme.ResolveStatus`.
- **Colourblind-safe palette** swaps success/warning/error to blue/grey/orange.
- **Comfort:** seated vs standing tracking origin (no forced locomotion), adjustable UI
  distance (0.6–1.6 m), snap-turn setting surfaced.
- **Large targets:** buttons are 96 px (about 9.6 cm) tall with generous spacing; the keypad
  keys are large and evenly spaced.
- **Controllers and hands:** input goes through `OVRInputModule`, so both are supported; the
  active hand's ray is highlighted.
