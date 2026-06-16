# Star Blaster — Credits & Citations

## Engine
- Unity 6 (default Built-in Render Pipeline, 2D mode).

## Code
- All gameplay scripts written for this assignment; no third-party C# code,
  asset-store packages, or copied tutorials.

## Art
- **All sprites generated procedurally in `Assets/Scripts/Art2D.cs`**:
  - `SolidCircle` — anti-aliased disc (player bullet, stars, particles).
  - `Triangle` — player ship.
  - `Diamond` — enemies.
  - `Square` — power-up icons.
- No external images, sprite sheets, or texture files used.

## Audio
- **All sounds synthesized at runtime in `Assets/Scripts/Art2D.cs`**:
  - `Tone(freq, dur)` — sine wave with exponential decay (shoot, click).
  - `Noise(dur)` — white noise with exponential decay (hits, enemy death).
  - `Chime(base, dur)` — three-overtone bell (power-up pickup).
- No external audio clips, music files, or sound libraries used.

## Fonts
- Unity's built-in `LegacyRuntime.ttf` (loaded via `Resources.GetBuiltinResource`).
  Comes bundled with the Unity editor; no external font files.

## Documentation referenced (per the assignment's "search official docs")
- Unity Manual — Sprite Renderer, Rigidbody2D, Trigger Colliders 2D.
- Unity Manual — Canvas, RectTransform, EventSystem, StandaloneInputModule.
- Unity Scripting API — `AudioClip.Create`, `Texture2D.SetPixel`,
  `PlayerPrefs`, `Time.timeScale`, `Object.FindObjectsByType`.

## Notes
- No borrowed art, audio, fonts, tutorials, or starter assets that require
  external citation. Everything in this project folder is original work for
  this assignment.
