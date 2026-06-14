# Final Report

## Overview

`Dungeon Key Run` is a Unity 2D top-down dungeon shooter. The player explores
dungeon floors, defeats enemies for XP, collects the gold key, reaches the blue
exit door, and tries to improve the saved local best score. Every third floor is
a boss encounter, and each completed floor gives the player a passive upgrade
choice before the next floor begins.

The final project focuses on a complete playable loop rather than a large
unfinished world. It includes movement, shooting, enemies, boss floors, key and
exit progression, upgrade choices, HUD feedback, pause/game-over flows, local
records, and documented asset credits.

## Design Choices

The key-and-exit structure gives each floor a clear objective. Before the key is
collected, the exit is locked; after the key is collected, the player's goal is
to survive long enough to reach the exit. This creates a simple state change that
is easy to show through the HUD and objective text.

Combat gives the player an active way to manage pressure. Defeating enemies
grants XP, and leveling up opens weapon upgrade choices, so the player has a
reason to engage with enemies instead of only avoiding them.

Boss floors are used as pacing breaks. A regular floor is about finding the key
and exit, while every third floor changes the objective: defeat the boss before
the exit opens. This gives the run a stronger rhythm without requiring many
hand-built levels.

Passive upgrades after floor clears add progression between rooms. They make
later floors feel different while keeping the control scheme simple.

## Technical Decisions

The main scene is `Project2DImproved 2/Assets/Scenes/StarBlaster.unity`.
Although the scene name comes from an earlier classroom project, the runtime
content is now `Dungeon Key Run`.

The game is assembled mostly at runtime by `GameBootstrap.cs` and coordinated by
`GameManager.cs`. This keeps the Unity scene lightweight and makes the core game
flow easier to inspect from code.

The main gameplay scripts are:

- `Player.cs`: movement, health, collisions, pickups, exits, and damage.
- `PlayerCombat.cs`: mouse aiming, shooting, weapon stats, and weapon upgrades.
- `Enemy.cs`: normal enemy and boss behaviour.
- `GameManager.cs`: run state, floors, score, XP, upgrades, records, and flow.
- `HUD.cs`: objective text and in-game readouts.
- `Menus.cs`: start, pause, game-over, restart, and leaderboard UI.
- `Art2D.cs`: asset loading plus procedural fallback sprites and sounds.

Assets are loaded from `Assets/StreamingAssets/`. Third-party resources are kept
under `Assets/StreamingAssets/thirdparty/` with local license or attribution
files. Project-generated sprites are kept under `Assets/StreamingAssets/generated/`.
Detailed credits live in `Project2DImproved 2/CREDITS.md`.

## Development Process

The project was developed in staged milestones:

- Direction and baseline: confirm `Dungeon Key Run` as the final game direction,
  restore Unity project hygiene, and make the repository tell one clear story.
- Core playable loop: implement movement, combat, enemies, key pickup, exit
  rules, and basic win/fail flow.
- Challenge and progression: add floor advancement, boss floors, XP, weapon
  upgrades, passive upgrades, scoring, and local record tracking.
- UI and feedback: add menus, HUD objective text, pause flow, game-over flow,
  leaderboard display, audio cues, camera feedback, and clearer instructions.
- Documentation and assessment evidence: update README, credits, testing notes,
  and final report so the project can be assessed from the repository.

## Testing and Improvement

Testing focused on the full player journey: opening the scene, starting a run,
moving, aiming, shooting, collecting the key, using the exit, surviving enemy
damage, clearing boss floors, selecting upgrades, pausing, restarting, ending a
run, and checking saved local records. The manual regression checklist is in
`docs/TestLog.md`.

Feedback improvements included:

- Objective text for the current floor state.
- HUD fields for floor, lives, key state, defeated enemies, score, level, XP,
  timer, and objective.
- Separate UI flows for start, pause, game over, restart, and leaderboard.
- License and attribution notes for external image and audio resources.
- README restructuring so run instructions and implemented features appear
  before the detailed asset list.

## Presentation Outline

The demo should show:

1. The README quick start and main scene path.
2. The start menu and controls.
3. Movement, mouse aiming, shooting, enemy damage, and HUD feedback.
4. The key-and-exit rule on a normal floor.
5. XP gain and weapon upgrade selection.
6. Passive upgrade selection after clearing a floor.
7. A boss floor where the exit is sealed until the boss is defeated.
8. Game-over or restart flow plus local best score and Top 5 leaderboard.

Important code to mention during presentation:

- `GameBootstrap.cs` for runtime setup.
- `GameManager.cs` for state, floors, scoring, upgrades, and records.
- `Player.cs` and `PlayerCombat.cs` for player control and shooting.
- `Enemy.cs` for enemies and bosses.
- `HUD.cs`, `Menus.cs`, and upgrade UI scripts for player-facing feedback.

## Problems and Limitations

The scene file name still uses the earlier classroom name `StarBlaster.unity`.
This is documented in the README and quick start so the correct scene is easy to
open.

The game uses a compact runtime-generated structure rather than a large authored
campaign. This keeps the project stable and explainable, but it means there are
not many handcrafted levels.

There are no automated Unity tests. Testing is manual because most
assessment-relevant behaviours are interactive player flows.

## Reflection

The project developed from a simple key-and-exit idea into a fuller dungeon run
with combat, upgrades, bosses, score tracking, and documented assets. The most
important improvement was keeping the player goal clear even as more systems
were added. Each floor still has a readable objective, and the HUD explains what
the player should do next.

If developed further, the next improvements would be more enemy patterns, more
distinct floor layouts, optional accessibility settings, richer boss mechanics,
and automated play-mode tests for core state transitions.
