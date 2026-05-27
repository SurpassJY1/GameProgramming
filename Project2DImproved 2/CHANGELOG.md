# Dungeon Key Run - Change Log

## Final Project Direction

The final game direction is `Dungeon Key Run`, a Unity 2D top-down dungeon escape vertical slice. Earlier classroom shooter work is not part of the final assessment submission.

## Current Vertical Slice

### Core Goal

The player explores a compact dungeon, collects the gold key, avoids guards, and reaches the blue exit door.

### Gameplay Systems

- Top-down movement using `WASD` or arrow keys.
- Wall blocking using 2D circle casts against dungeon walls.
- Gold key pickup that unlocks the exit.
- Exit door that gives feedback if the key has not been collected.
- Victory state after reaching the exit with the key.
- Lives and game-over state when guards catch the player.
- Pause, restart, and return-to-menu flow.

### Enemy Behaviour

- Guards patrol between two points.
- Guards chase the player when close and visible.
- Wall raycasts stop guards from chasing through walls.

### UI and Feedback

- Main menu, instructions, credits, pause, victory, and fail pages.
- HUD displays lives, key state, elapsed time, and objective text.
- Key state is shown with text as well as colour.
- Runtime-generated sounds provide button, key, hit, and win feedback.

### Technical Notes

- `GameBootstrap.cs` builds the runtime level, objects, and UI.
- Sprites and audio are generated procedurally in `Art2D.cs`.
- The active Unity scene is included in Build Settings.
- `.gitignore` excludes Unity cache and local editor files.

## Testing Evidence

Testing is recorded in `../../docs/TestLog.md`. The main regression path covers menu flow, movement, collision, key pickup, locked exit, unlocked exit, guard damage, failure, pause, restart, and victory.

## Known Limitations

- One level only, because this is a vertical slice.
- Manual testing only.
- Simple guard AI rather than full pathfinding.
- The scene filename remains `StarBlaster.unity` from earlier classroom work, but the runtime content is `Dungeon Key Run`.
