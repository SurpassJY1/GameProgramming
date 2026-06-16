# Dungeon Key Run - Credits and Citations

## Engine

- Unity `2022.3.62f1`
- Built-in Render Pipeline, 2D mode

## Code

All gameplay code was written for this module project. No third-party C# gameplay code, starter kits, or copied tutorial projects are included.

Student-completed work:

- Final game concept, Unity project integration, gameplay direction, testing, and presentation decisions.
- Core ownership of the submitted build, including reviewing and accepting all code and documentation changes.

AI-assisted work:

- Brainstorming and planning support.
- Drafting and refining code comments, documentation wording, GitHub issue text, and presentation notes.
- Review-style suggestions for readability, feature explanation, and academic transparency.

AI assistance was used as a tool, not as an external asset pack or copied third-party project. The student remains responsible for the final implementation and submission.

## Art

Primary gameplay visuals use project-generated original sprites loaded from
`Assets/StreamingAssets/generated/`, with documented third-party CC0 assets kept
in `Assets/StreamingAssets/thirdparty/` where they are used or available as
fallback/reference resources.

Several high-readability gameplay sprites were created specifically for this
module after testing showed some third-party tiles were not visually clear
enough in game:

- License/status: original project work; no external image license required
- Local files used:
  - `Assets/StreamingAssets/generated/clear-dungeon/player.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/floor_tile.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/wall_tile.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/key.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/exit_door.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/bosses/slime_king.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/bosses/frost_queen.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/bosses/shade_overlord.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/bosses/crystal_titan.png`
- Purpose:
  - `wall_tile.png` is the normal wall sprite.
  - `floor_tile.png` is the normal floor/background tile.
  - `key.png` is the normal collectible key sprite.
  - `exit_door.png` is the normal exit door sprite.
  - `player.png` is the normal player sprite.
  - `bosses/*.png` are the normal boss and later elite enemy sprites.

Kenney Tiny Dungeon:

- Author: Kenney
- Source: `https://kenney.nl/assets/tiny-dungeon`
- License: Creative Commons Zero (CC0 1.0)
- Local license copy: `Assets/StreamingAssets/thirdparty/kenney-tiny-dungeon/License.txt`
- Local files used:
  - `Assets/StreamingAssets/thirdparty/kenney-tiny-dungeon/tiles/*.png`
  - `Assets/StreamingAssets/thirdparty/kenney-tiny-dungeon/Tilemap/tilemap.png`

Kenney UI Pack:

- Author: Kenney
- Source: `https://kenney.nl/assets/ui-pack`
- License: Creative Commons Zero (CC0 1.0)
- Local license copy: `Assets/StreamingAssets/thirdparty/kenney-ui-pack/License.txt`
- Local files used:
  - `Assets/StreamingAssets/thirdparty/kenney-ui-pack/png/Blue/Default/button_rectangle_depth_flat.png`
  - `Assets/StreamingAssets/thirdparty/kenney-ui-pack/png/Blue/Default/button_square_depth_flat.png`
  - `Assets/StreamingAssets/thirdparty/kenney-ui-pack/png/Extra/Default/input_rectangle.png`
  - `Assets/StreamingAssets/thirdparty/kenney-ui-pack/png/Extra/Default/divider.png`
  - `Assets/StreamingAssets/thirdparty/kenney-ui-pack/png/Extra/Default/icon_play_light.png`

Procedural visuals remain in `Assets/Scripts/Art2D.cs` only as fallback/generated effects:

- `SolidCircle` for particle/fallback shapes
- `SoftRectangle`, `Diamond`, `Square`, `Key`, `ExitGate`, and enemy icons when an external file is missing
- Weapon/passive upgrade icons and small transient VFX

The runtime enemy roster also uses generated/project visuals for normal enemy
roles and boss/elite variants, including Slime Scout, Tiny Bat, Shield Guard,
Spark Spitter, Bomb Sprite, Frost Wisp, Dash Imp, Healer Fairy, Summoner Shade,
Crystal Brute, Slime King, Frost Queen, Shade Overlord, and Crystal Titan.

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

Primary sound effects use third-party CC0 assets loaded from `Assets/StreamingAssets/thirdparty/`.

Kenney Interface Sounds:

- Author: Kenney
- Source: `https://kenney.nl/assets/interface-sounds`
- License: Creative Commons Zero (CC0 1.0)
- Local license copy: `Assets/StreamingAssets/thirdparty/kenney-audio/interface/License.txt`
- Local files used:
  - `Assets/StreamingAssets/thirdparty/kenney-audio/interface/Audio/click_002.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/interface/Audio/confirmation_001.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/interface/Audio/select_003.ogg`

Kenney RPG Audio:

- Author: Kenney
- Source: `https://kenney.nl/assets/rpg-audio`
- License: Creative Commons Zero (CC0 1.0)
- Local license copy: `Assets/StreamingAssets/thirdparty/kenney-audio/rpg/License.txt`
- Local files used:
  - `Assets/StreamingAssets/thirdparty/kenney-audio/rpg/Audio/handleCoins.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/rpg/Audio/knifeSlice.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/rpg/Audio/doorOpen_1.ogg`

Kenney Impact Sounds:

- Author: Kenney
- Source: `https://kenney.nl/assets/impact-sounds`
- License: Creative Commons Zero (CC0 1.0)
- Local license copy: `Assets/StreamingAssets/thirdparty/kenney-audio/impact/License.txt`
- Local files used:
  - `Assets/StreamingAssets/thirdparty/kenney-audio/impact/Audio/impactPunch_heavy_000.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/impact/Audio/impactMetal_light_000.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/impact/Audio/impactGeneric_light_000.ogg`

Background music:

- Track: `"8bit Dungeon Level"`
- Author: Kevin MacLeod (`incompetech.com`)
- Source: `https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1200066`
- License: Creative Commons Attribution 4.0
- Required attribution: `"8bit Dungeon Level" Kevin MacLeod (incompetech.com), Licensed under Creative Commons: By Attribution 4.0 License, http://creativecommons.org/licenses/by/4.0/`
- Local file: `Assets/StreamingAssets/thirdparty/incompetech/8bit-dungeon-level.mp3`
- Local attribution copy: `Assets/StreamingAssets/thirdparty/incompetech/ATTRIBUTION.txt`

Synthesized sounds remain in `Assets/Scripts/Art2D.cs` only as fallback if external audio files are missing:

- `Tone` for menu button feedback
- `Noise` for damage feedback
- `Chime` for key pickup and victory feedback

## Fonts

The UI uses Unity's built-in `LegacyRuntime.ttf`, loaded through `Resources.GetBuiltinResource`.

## Unity Packages

Unity `2022.3.62f1` and packages in `Packages/manifest.json` are used under Unity's package and editor terms.

## Documentation Referenced

- Unity Manual: Sprite Renderer, Rigidbody2D, Trigger Colliders 2D
- Unity Manual: Canvas, RectTransform, EventSystem, StandaloneInputModule
- Unity Scripting API: `AudioClip.Create`, `Texture2D.SetPixel`, `Time.timeScale`, `Physics2D.CircleCast`

## Notes

Third-party assets are limited to clearly documented free sprites and audio with source and license notes in this file and a short summary in `README.md`.
Project-generated sprites are listed separately above so they are not mistaken for external asset-pack work.
