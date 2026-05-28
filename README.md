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
- Patrol and chase-style enemy behaviour
- Lives, key state, timer, objective text, pause menu, victory, and game over
- Runtime-loaded CC0 sprites for player/enemy/wall plus procedural fallback sprites
- Runtime-generated sound effects
- Documentation for concept, sprint planning, testing, report, presentation, and credits

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
- **Sound effects**: generated procedurally in `Project2DImproved 2/Assets/Scripts/Art2D.cs`.
- **Third-party sprite assets (player/enemy/wall)**:
  - Source: [Kenney - Topdown Shooter (OpenGameArt mirror)](https://opengameart.org/content/topdown-shooter)
  - Wall texture provenance (academic integrity):
    - In-use wall texture: `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/wall_tile.png`
    - Original pack file: `.tmp_assets/topdown-shooter/PNG/Tiles/tile_74.png` (same image content)
  - Local files used:
    - `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/player.png`
    - `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/enemy.png`
    - `Project2DImproved 2/Assets/StreamingAssets/thirdparty/topdown-shooter/wall_tile.png`
  - License: **CC0 / public domain style reuse** as listed on the source page.
  - Attribution note: the source indicates credit is optional, but this README includes explicit credit for academic transparency and to avoid misconduct concerns.

All reused assets are documented above with source and license. No paid or restricted-license assets are included.

## Known Limitations

- The final submission is one vertical slice level, not a full multi-level game.
- The scene filename is still `StarBlaster.unity` from earlier classroom work, but the runtime game content is `Dungeon Key Run`.
- Testing is recorded manually in `docs/TestLog.md`.
