# Asset Credits

## Engine

- Unity `2022.3.62f1`
- Built-in Render Pipeline, 2D project setup

## Code

All gameplay scripts were written for this module project.

Key scripts:

- `GameBootstrap.cs`: builds the runtime scene, dungeon, UI, player, key, exit, and guards
- `GameManager.cs`: stores game state and transitions
- `Player.cs`: movement, wall blocking, pickups, exit use, and damage
- `Enemy.cs`: guard patrol and chase behaviour
- `HUD.cs` and `Menus.cs`: player-facing interface

## Art

All sprites are generated procedurally in `Art2D.cs`:

- `SolidCircle`: player and simple circular shapes
- `Diamond`: key and guards
- `Square`: walls, floor, and exit door

No external images, sprite sheets, or texture files are used.

## Audio

All sound effects are generated at runtime in `Art2D.cs`:

- `Tone`: button click style sounds
- `Noise`: hit feedback
- `Chime`: key and win feedback

No external audio files, music, or sound libraries are used.

## Fonts

The UI uses Unity's built-in `LegacyRuntime.ttf`, loaded through `Resources.GetBuiltinResource`.

## External Sources

No third-party game assets, tutorials, starter kits, or copied gameplay code are included.
