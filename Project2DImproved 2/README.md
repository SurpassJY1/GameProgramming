# Dungeon Key Run

Unity 2D dungeon game project.

## Assets and Licenses

This README lists every image and audio resource group included in the project, with source,
license/status, and local paths. `CREDITS.md` contains the same credit trail with additional context.

### Project-Generated Image Assets

These images were created for this module project and are treated as original project work. No
external image license is required.

Clear dungeon gameplay sprites:

- Source/status: original project-generated art for this module
- License/status: original project work; no external license required
- Local files:
  - `Assets/StreamingAssets/generated/clear-dungeon/player.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/floor_tile.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/wall_tile.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/key.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/exit_door.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/bosses/slime_king.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/bosses/frost_queen.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/bosses/shade_overlord.png`
  - `Assets/StreamingAssets/generated/clear-dungeon/bosses/crystal_titan.png`

Selected pixel-cute fallback/UI sprites:

- Source/status: original project-generated art for this module
- License/status: original project work; no external license required
- Local files:
  - `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/player.png`
  - `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/enemy.png`
  - `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/floor_tile.png`
  - `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/wall_tile.png`
  - `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/key.png`
  - `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/exit_gate.png`
  - `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/projectile.png`
  - `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/ui_panel.png`
  - `Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/ui_button.png`

Procedural fallback/effect sprites:

- Source/status: generated at runtime by `Assets/Scripts/Art2D.cs`
- License/status: original project code output; no external image license required
- Used for fallback shapes, upgrade icons, shadows, glows, bullet impacts, pulses, and simple VFX.

### Third-Party Image Assets

Kenney Tiny Dungeon:

- Author/source: Kenney, `https://kenney.nl/assets/tiny-dungeon`
- License: Creative Commons Zero, CC0 1.0
- License URL: `http://creativecommons.org/publicdomain/zero/1.0/`
- Local license copy: `Assets/StreamingAssets/thirdparty/kenney-tiny-dungeon/License.txt`
- Local image resources:
  - All PNG files in `Assets/StreamingAssets/thirdparty/kenney-tiny-dungeon/tiles/`
  - `Assets/StreamingAssets/thirdparty/kenney-tiny-dungeon/Tilemap/tilemap.png`
  - `Assets/StreamingAssets/thirdparty/kenney-tiny-dungeon/Tilemap/tilemap_packed.png`

Kenney UI Pack:

- Author/source: Kenney, `https://kenney.nl/assets/ui-pack`
- License: Creative Commons Zero, CC0 1.0
- License URL: `http://creativecommons.org/publicdomain/zero/1.0/`
- Local license copy: `Assets/StreamingAssets/thirdparty/kenney-ui-pack/License.txt`
- Local image resources:
  - `Assets/StreamingAssets/thirdparty/kenney-ui-pack/png/Blue/Default/button_rectangle_depth_flat.png`
  - `Assets/StreamingAssets/thirdparty/kenney-ui-pack/png/Blue/Default/button_square_depth_flat.png`
  - `Assets/StreamingAssets/thirdparty/kenney-ui-pack/png/Extra/Default/input_rectangle.png`
  - `Assets/StreamingAssets/thirdparty/kenney-ui-pack/png/Extra/Default/divider.png`
  - `Assets/StreamingAssets/thirdparty/kenney-ui-pack/png/Extra/Default/icon_play_light.png`

Kenney Topdown Shooter:

- Author/source: Kenney Vleugels / Kenney, OpenGameArt mirror
- Source URL: `https://opengameart.org/content/topdown-shooter`
- License: Creative Commons Zero, CC0 1.0
- License URL: `http://creativecommons.org/publicdomain/zero/1.0/`
- Local license copy: `Assets/StreamingAssets/thirdparty/topdown-shooter/License.txt`
- Local image resources:
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/player.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/enemy.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/floor.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/floor_tile.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/wall.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/wall_tile.png`
  - `Assets/StreamingAssets/thirdparty/topdown-shooter/wall_tile_contrast.png`

### Third-Party Audio Assets

Kenney Interface Sounds:

- Author/source: Kenney, `https://kenney.nl/assets/interface-sounds`
- License: Creative Commons Zero, CC0 1.0
- License URL: `http://creativecommons.org/publicdomain/zero/1.0/`
- Local license copy: `Assets/StreamingAssets/thirdparty/kenney-audio/interface/License.txt`
- Local audio resources:
  - `Assets/StreamingAssets/thirdparty/kenney-audio/interface/Audio/click_002.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/interface/Audio/confirmation_001.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/interface/Audio/select_003.ogg`

Kenney RPG Audio:

- Author/source: Kenney Vleugels / Kenney, `https://kenney.nl/assets/rpg-audio`
- License: Creative Commons Zero, CC0 1.0
- License URL: `http://creativecommons.org/publicdomain/zero/1.0/`
- Local license copy: `Assets/StreamingAssets/thirdparty/kenney-audio/rpg/License.txt`
- Local audio resources:
  - `Assets/StreamingAssets/thirdparty/kenney-audio/rpg/Audio/doorOpen_1.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/rpg/Audio/handleCoins.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/rpg/Audio/knifeSlice.ogg`

Kenney Impact Sounds:

- Author/source: Kenney, `https://kenney.nl/assets/impact-sounds`
- License: Creative Commons Zero, CC0 1.0
- License URL: `http://creativecommons.org/publicdomain/zero/1.0/`
- Local license copy: `Assets/StreamingAssets/thirdparty/kenney-audio/impact/License.txt`
- Local audio resources:
  - `Assets/StreamingAssets/thirdparty/kenney-audio/impact/Audio/impactGeneric_light_000.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/impact/Audio/impactMetal_light_000.ogg`
  - `Assets/StreamingAssets/thirdparty/kenney-audio/impact/Audio/impactPunch_heavy_000.ogg`

Background music:

- Track: `"8bit Dungeon Level"`
- Author/source: Kevin MacLeod, `incompetech.com`
- Source URL: `https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1200066`
- License: Creative Commons Attribution 4.0
- License URL: `http://creativecommons.org/licenses/by/4.0/`
- Required attribution: `"8bit Dungeon Level" Kevin MacLeod (incompetech.com), Licensed under Creative Commons: By Attribution 4.0 License, http://creativecommons.org/licenses/by/4.0/`
- Local audio file: `Assets/StreamingAssets/thirdparty/incompetech/8bit-dungeon-level.mp3`
- Local attribution copy: `Assets/StreamingAssets/thirdparty/incompetech/ATTRIBUTION.txt`

### Project-Generated Audio Fallbacks

These audio clips are synthesized at runtime by `Assets/Scripts/Art2D.cs` only when external audio
files are missing.

- Source/status: original project code output
- License/status: original project work; no external audio license required
- Fallback generators:
  - `Tone`
  - `Noise`
  - `Chime`

### Fonts

- UI font: Unity built-in `LegacyRuntime.ttf`
- Source: Unity built-in runtime resource via `Resources.GetBuiltinResource`
- License/status: covered by Unity editor/runtime terms

### Notes

- Kenney CC0 assets do not require attribution, but credit is included for academic transparency.
- No paid or restricted-license image or audio assets are included.
- All local third-party license/attribution text files are kept next to their corresponding assets.
