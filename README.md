# Dungeon Key Run

`Dungeon Key Run` is a small Unity 2D top-down adventure vertical slice. The player explores a compact dungeon, collects a gold key, avoids patrol guards, and reaches the exit door to win.

The project is scoped for a game programming module assessment: one playable level, clear rules, stable controls, visible feedback, documented testing, and GitHub history that shows development progress.

## How to Run

1. Open Unity Hub.
2. Add the project folder: `Project2DImproved 2`.
3. Open the scene: `Assets/Scenes/StarBlaster.unity`.
4. Press Play.

The scene uses `GameBootstrap.cs` to build the full level at runtime and load selected sprite assets from `Assets/StreamingAssets`.

## Controls

- Move: `WASD` or arrow keys
- Pause: `ESC`
- Goal: collect the gold key, then touch the blue exit door

## Main Features

- 2D top-down player movement with wall collision
- Compact dungeon layout with walls, key, exit, and guards
- 10 cute pixel enemy types that unlock progressively from floor 1 through floor 10+
- Lives, key state, timer, objective text, pause menu, victory, and game over
- Runtime-loaded cute pixel sprites for player/enemy/floor/wall/items/UI plus documented CC0 fallback sprites
- Runtime-generated sound effects
- Documentation for concept, sprint planning, testing, report, presentation, and credits

## Authorship and AI Assistance

Student-completed work:

- Final game concept, Unity project integration, gameplay direction, testing, and presentation decisions.
- Review and ownership of the submitted build, including code, assets, documentation, and credits.

AI-assisted work:

- Brainstorming improvements, drafting GitHub issue text, reviewing code readability, and refining documentation.
- Drafting concise comments and academic transparency wording, which were reviewed before inclusion.

AI assistance was used as a development aid. It did not replace student review, testing, final design decisions, or responsibility for the submission.

## Project Structure

```text
GameProgramming/
  README.md
  docs/
    GameConcept.md
    SprintPlan.md
    TestLog.md
    AssetCredits.md
    FinalReport.md
    PresentationPlan.md
  Project2DImproved 2/
    Assets/
    Packages/
    ProjectSettings/
```

## Credits and Licensing

- **Project code and gameplay logic**: original work for this project.
- **Unity engine and packages**: Unity `2022.3.62f1` and packages in `Project2DImproved 2/Packages/manifest.json`; used under Unity's package and editor terms.
- **Fonts**: Unity built-in `LegacyRuntime.ttf`, loaded through `Resources.GetBuiltinResource`; no external font files are included.
- **Sound effects**: generated procedurally in `Project2DImproved 2/Assets/Scripts/Art2D.cs`; no external audio files are included.
- **Project-generated cute pixel visual assets**:
  - The current primary visual set is original project-generated pixel art created for this project.
  - License/status: original project work for this module; no external image license is required.
  - Local files:
    - `Project2DImproved 2/Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/floor_tile.png`
    - `Project2DImproved 2/Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/wall_tile.png`
    - `Project2DImproved 2/Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/player.png`
    - `Project2DImproved 2/Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/enemy.png`
    - `Project2DImproved 2/Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/key.png`
    - `Project2DImproved 2/Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/exit_gate.png`
    - `Project2DImproved 2/Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/projectile.png`
    - `Project2DImproved 2/Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/ui_panel.png`
    - `Project2DImproved 2/Assets/StreamingAssets/generated/pixel-cute-dungeon/selected/ui_button.png`
  - Additional fallback sprites, lighting glows, shadows, upgrade icons, and the 10 procedural enemy type sprites are generated in `Project2DImproved 2/Assets/Scripts/Art2D.cs` and `Project2DImproved 2/Assets/Scripts/GameBootstrap.cs`.
- **Third-party fallback sprite assets (Kenney Topdown Shooter)**:
  - Source: [Kenney - Topdown Shooter (OpenGameArt mirror)](https://opengameart.org/content/topdown-shooter)
  - License: **Creative Commons Zero (CC0 1.0)**.
  - Local license copy: `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/License.txt`
  - Tile texture provenance (academic integrity):
    - In-use wall texture: `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/wall_tile.png`
    - In-use floor texture: `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/floor_tile.png`
    - Both are retained under `Assets/StreamingAssets/thirdparty/topdown-shooter/` from the credited Kenney pack.
  - Local files:
    - `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/player.png`
    - `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/enemy.png`
    - `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/floor.png`
    - `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/floor_tile.png`
    - `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/wall.png`
    - `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/wall_tile.png`
    - `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/wall_tile_contrast.png`
  - Attribution note: the source indicates credit is optional, but this README includes explicit credit for academic transparency and to avoid misconduct concerns.

All local runtime assets are documented above with source and license/status. No paid or restricted-license assets are included. A network attempt was made to reach the planned itch.io resource, but the environment could not connect, so no Ninja Adventure or Adventure UI files are included or claimed as used.

## Known Limitations

- The final submission is one vertical slice level, not a full multi-level game.
- The scene filename is still `StarBlaster.unity` from earlier classroom work, but the runtime game content is `Dungeon Key Run`.
- Testing is recorded manually in `docs/TestLog.md`.
