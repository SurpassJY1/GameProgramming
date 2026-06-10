# Final Report

## Overview

`Dungeon Key Run` is a compact Unity 2D dungeon escape vertical slice. The player collects a gold key, avoids guards, and reaches the exit door. The design focuses on one readable objective and a small number of complete systems rather than many unfinished features.

## Design Choices

The game uses a single-key escape structure because it gives the player a clear goal and creates a simple before/after state: before the key, the exit is locked; after the key, the route back to the exit becomes the challenge.

The enemies are guards rather than monsters or combat targets. This supports the route-planning design because the player succeeds by timing movement and avoiding risk, not by fighting.

The level is small on purpose. A vertical slice should feel complete for its size, so the project prioritises working controls, readable feedback, and a complete win/fail loop.

## Technical Decisions

The scene is assembled at runtime by `GameBootstrap.cs`. This keeps the project lightweight and avoids relying on external prefabs or art assets. It also makes the game easier to inspect because the main objects are created in one place.

The game uses component-based collision checks. The player detects `KeyPickup`, `ExitDoor`, and `Enemy` components directly, which avoids custom tag setup and makes the scripts clearer.

The player uses circle casts against a wall layer for movement blocking. This gives simple top-down collision without adding complex physics movement.

The guard AI combines patrol and chase behaviour. Guards move between two patrol points, then chase the player if the player is close and visible. This is enough to create pressure while staying realistic for the project scope.

## Authorship and AI Assistance

The final game direction, Unity integration, gameplay decisions, manual testing, and submitted build are student-completed work. AI assistance was used for brainstorming, issue drafting, code review suggestions, comment wording, and documentation refinement. AI-assisted material was reviewed and adapted before inclusion, and no third-party gameplay starter kit or copied tutorial project is included.

## Testing and Improvement

Testing focused on the main player flow: menu, movement, wall collision, key pickup, locked exit, unlocked exit, enemy damage, fail state, pause, restart, and victory. The tests are recorded in `TestLog.md`.

Feedback improvements included:

- Text objective updates when the exit is locked or unlocked.
- HUD text for key state so the player does not rely only on colour.
- Separate victory and fail pages with elapsed time.
- Clear instructions in both the game and README.

## Problems and Limitations

The main limitation is that the game is a single-level vertical slice. It does not include multiple levels, save data, combat, or advanced enemy pathfinding. This was an intentional scope decision to keep the final result stable and explainable.

The scene file name still uses an earlier classroom name, `StarBlaster.unity`, but the runtime content and documentation are now focused on `Dungeon Key Run`.

There are no automated tests. Testing is manual because the project is small and most assessment-relevant behaviours are player interaction flows.

## Reflection

The project developed from a simple key-and-exit idea into a more complete vertical slice by adding guard behaviour, lives, UI, feedback, and documentation. The most important improvement was keeping the scope small enough to finish properly. A larger game idea would have reduced polish and made testing harder.

If developed further, the next improvements would be additional levels, better guard pathfinding, optional accessibility settings, and more varied feedback sounds.
