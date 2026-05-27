# Test Log

## Manual Regression Test Plan

| Area | Test | Expected Result | Status |
| --- | --- | --- | --- |
| Menu | Start from main menu | Game begins and HUD appears | Pass in code review |
| Instructions | Open instructions and return | Controls and goal are shown, back button returns | Pass in code review |
| Movement | Move with WASD/arrows | Player moves in four directions | Pass in code review |
| Collision | Walk into walls | Player is blocked by dungeon walls | Pass in code review |
| Key | Touch gold key | Key disappears and HUD says collected | Pass in code review |
| Locked exit | Touch exit before key | Objective says key is needed | Pass in code review |
| Win | Touch exit after key | Victory page appears and time is shown | Pass in code review |
| Enemy | Touch guard | Player loses one life and receives feedback | Pass in code review |
| Game over | Lose all lives | Fail page appears | Pass in code review |
| Pause | Press ESC during play | Pause menu appears, resume returns to game | Pass in code review |
| Restart | Restart after pause/win/fail | State resets for a new run | Pass in code review |

## Testing Notes

Manual tests should be repeated inside the Unity Editor before final submission. The current implementation was checked structurally through script review and linter diagnostics; final playtesting in Unity should confirm object placement, movement feel, and guard difficulty.

## Changes Made From Testing and Review

- The key state is shown with text, not only colour, to improve readability.
- The exit gives objective feedback if the player reaches it before collecting the key.
- Guards use patrol plus short-range chase behaviour to create challenge without making the level too complex.
- The final scope was kept to one polished level to match the vertical slice requirement.

## Known Issues and Limitations

- The Unity scene file name remains `StarBlaster.unity` from earlier classroom work, although the built game content is `Dungeon Key Run`.
- There are no automated Unity tests.
- The level is intentionally small and contains only one key and one exit.
