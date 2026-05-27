# Dungeon Key Run - Credits and Citations

## Engine

- Unity `2022.3.62f1`
- Built-in Render Pipeline, 2D mode

## Code

All gameplay code was written for this module project. No third-party C# gameplay code, starter kits, or copied tutorial projects are included.

## Art

All sprites are generated procedurally in `Assets/Scripts/Art2D.cs`:

- `SolidCircle` for the player and simple round shapes
- `Diamond` for the key and guard shapes
- `Square` for walls, floor, and exit door

No external images, sprite sheets, textures, or Asset Store art packages are used.

## Audio

All sounds are synthesized at runtime in `Assets/Scripts/Art2D.cs`:

- `Tone` for menu button feedback
- `Noise` for damage feedback
- `Chime` for key pickup and victory feedback

No external music, sound libraries, or audio clips are used.

## Fonts

The UI uses Unity's built-in `LegacyRuntime.ttf`, loaded through `Resources.GetBuiltinResource`.

## Documentation Referenced

- Unity Manual: Sprite Renderer, Rigidbody2D, Trigger Colliders 2D
- Unity Manual: Canvas, RectTransform, EventSystem, StandaloneInputModule
- Unity Scripting API: `AudioClip.Create`, `Texture2D.SetPixel`, `Time.timeScale`, `Physics2D.CircleCast`

## Notes

The project avoids third-party assets to keep licensing simple and to demonstrate original programming and design work.
