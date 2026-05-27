# Dungeon Key Run

`Dungeon Key Run` is a small Unity 2D top-down adventure vertical slice. The player explores a compact dungeon, collects a gold key, avoids patrol guards, and reaches the exit door to win.

The project is scoped for a game programming module assessment: one playable level, clear rules, stable controls, visible feedback, documented testing, and GitHub history that shows development progress.

## How to Run

1. Open Unity Hub.
2. Add the project folder: `Project2DImproved 2`.
3. Open the scene: `Assets/Scenes/StarBlaster.unity`.
4. Press Play.

The scene uses `GameBootstrap.cs` to build the full level at runtime, so no external art or audio assets are required.

## Controls

- Move: `WASD` or arrow keys
- Pause: `ESC`
- Goal: collect the gold key, then touch the blue exit door

## Main Features

- 2D top-down player movement with wall collision
- Compact dungeon layout with walls, key, exit, and guards
- Patrol and chase-style enemy behaviour
- Lives, key state, timer, objective text, pause menu, victory, and game over
- Procedural sprites and runtime-generated sound effects
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

## Credits

All code, sprites, and sound effects are original for this project. Sprites and audio are generated procedurally in `Project2DImproved 2/Assets/Scripts/Art2D.cs`. No third-party art, music, fonts, or Asset Store packages are used.

## Known Limitations

- The final submission is one vertical slice level, not a full multi-level game.
- The scene filename is still `StarBlaster.unity` from earlier classroom work, but the runtime game content is `Dungeon Key Run`.
- Testing is recorded manually in `docs/TestLog.md`.
