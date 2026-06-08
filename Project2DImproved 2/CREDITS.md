# Dungeon Key Run - Credits and Citations

## Engine

- Unity `2022.3.62f1`
- Built-in Render Pipeline, 2D mode

## Code

All gameplay code was written for this module project. No third-party C# gameplay code, starter kits, or copied tutorial projects are included.

## Art

Most visuals are generated procedurally in `Assets/Scripts/Art2D.cs`:

- `SolidCircle` for particle/fallback shapes
- `Diamond` for key and fallback guard shape
- `Square` for walls, floor, and exit door

Project-generated selected sprites are original project work for this module and do not require an external image license:

- `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/player.png`
- `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/enemy.png`
- `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/floor_tile.png`
- `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/wall_tile.png`
- `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/key.png`
- `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/exit_gate.png`
- `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/projectile.png`
- `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/ui_panel.png`
- `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/ui_button.png`

Third-party fallback sprites are from Kenney Topdown Shooter:

- Source: `Topdown Shooter` by Kenney on OpenGameArt
- URL: `https://opengameart.org/content/topdown-shooter`
- License: Creative Commons Zero (CC0 1.0)
- Local license copy: `Assets/StreamingAssets/thirdparty/topdown-shooter/License.txt`
- Files included in this project:
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/player.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/enemy.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/floor.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/floor_tile.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/wall.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/wall_tile.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/wall_tile_contrast.png`
  - Wall texture source trace: `.tmp_assets/topdown-shooter/PNG/Tiles/tile_74.png` (same image content)

## Audio

All sounds are synthesized at runtime in `Assets/Scripts/Art2D.cs`:

- `Tone` for menu button feedback
- `Noise` for damage feedback
- `Chime` for key pickup and victory feedback

No external music, sound libraries, or audio clips are used.

## Fonts

The UI uses Unity's built-in `LegacyRuntime.ttf`, loaded through `Resources.GetBuiltinResource`.

## Unity Packages

Unity `2022.3.62f1` and packages in `Packages/manifest.json` are used under Unity's package and editor terms.

## Documentation Referenced

- Unity Manual: Sprite Renderer, Rigidbody2D, Trigger Colliders 2D
- Unity Manual: Canvas, RectTransform, EventSystem, StandaloneInputModule
- Unity Scripting API: `AudioClip.Create`, `Texture2D.SetPixel`, `Time.timeScale`, `Physics2D.CircleCast`

## Notes

Third-party assets are limited to clearly documented free sprites with source and license notes in both this file and `README.md`.
