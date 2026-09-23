# Build and deploy

## Prerequisites

- **Unity 6 LTS — 6000.6.2f1** (installed via Unity Hub).
- **Android Build Support** with **SDK & NDK Tools** (NDK r27c) and **OpenJDK 17** modules.
  Unity's bundled copies are used.
- Packages (resolved from `Packages/manifest.json`):
  `com.unity.xr.management`, `com.unity.xr.openxr`, `com.unity.xr.meta-openxr`,
  `com.unity.inputsystem`, `com.meta.xr.sdk.core` (via `com.meta.xr.mrutilitykit` 205.0.0),
  `com.gamelovers.mcp-unity`.
- **ZXing.Net** `netstandard2.0` build placed at `Assets/Plugins/ZXing/zxing.dll`.
- **TextMeshPro Essentials** imported (Assets/TextMesh Pro) — required for text to render.

The `Meta/` folder in the repository root is **not committed** (see `DECISIONS.md` D18); it
holds the Meta Quest Developer Hub and Meta XR Simulator installers for local use.

## First-time Unity setup

1. Open the project with Unity 6000.6.2f1. Unity resolves packages and imports TextMesh Pro.
2. If any dialog blocks startup, dismiss it ("Android SDK not Found" appears when the Android
   SDK path is unset — install the Android modules or set the path).
3. Build the scene if needed: menu **Schlusselbild / Build Main Scene** (creates
   `Assets/Schlusselbild/Scenes/Main.unity` and adds it at build index 0).

## Run in the Editor

1. Open `Assets/Schlusselbild/Scenes/Main.unity`.
2. Press **Play**. `UiBootstrap` builds the UI; the Home screen appears.
3. The passthrough **Scan** screen reports that the camera is unavailable in the Editor
   (expected — the Passthrough Camera API is device-only).

### Meta XR Simulator (controllers without a headset)

1. Install `Meta/meta_xr_simulator_v205.0.msi` (needs admin elevation).
2. In Unity: **Meta → Meta XR Simulator → Activate**.
3. Press **Play**; simulated controllers drive the UI through `OVRInputModule`.

## Build the Android APK

Headless (Editor must be closed):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.2f1\Editor\Unity.exe" `
  -batchmode -quit -projectPath "D:\Schl-sselbild" `
  -executeMethod DevTools.BuildAndroid `
  -logFile "D:\Schl-sselbild\Logs\android_build.log"
```

`DevTools.BuildAndroid` configures everything deterministically:

- `com.schlusselbild.quest`, version `0.5.0` (code 5), min API 32
- **IL2CPP**, **ARM64**, **Linear** colour space
- Android SDK/NDK/JDK from the editor's `PlaybackEngines/AndroidPlayer`, set via
  `UnityEditor.Android.AndroidExternalToolsSettings`
- Output: `Builds/Schlusselbild.apk`

The Meta SDK logs a harmless post-build `NullReferenceException` from its telemetry
(`OVRGradleGeneration.SendXRPluginSettingsTelemetry`); the build still reports
`Build Successful`.

## Deploy to the Quest 3

```powershell
adb devices
adb install -r "D:\Schl-sselbild\Builds\Schlusselbild.apk"

# Provide the same key the PC used, or type it in-headset with "Enter key".
adb push D:\AR-glyph-encryption\key.bin `
  /sdcard/Android/data/com.schlusselbild.quest/files/key.bin
```

## Troubleshooting

| Symptom | Cause / fix |
|---------|-------------|
| `JDK not found` during build | OpenJDK module missing; install `android-open-jdk-17.0.18+8`. |
| Meta SDK fails to compile (`AndroidExternalToolsSettings`) | Android Build Support not installed. |
| `Package [com.meta.xr.sdk.all] cannot be found` | That package id does not exist; MRUK pulls `com.meta.xr.sdk.core`. |
| Push rejected: file exceeds 100 MB | A large installer was staged; ensure `/Meta/` and `*.msi` are gitignored. |
| New scripts not compiling in the open Editor | Trigger an AssetDatabase refresh (e.g. create any asset) — a bare recompile does not import brand-new files. |
| Editor threw `PlatformNotSupportedException` | Do not use `System.Security.Cryptography.AesGcm`; use `ManagedGcm`. |
