# Sprint Plan

## Sprint 0: Project Direction and Baseline

Goal: make the repository tell one clear project story.

Completed:

- Confirmed `Dungeon Key Run` as the final game direction.
- Replaced the classroom shooter runtime direction with the dungeon key objective.
- Restored Unity `.gitignore`.
- Added the active scene to Unity Build Settings.

Review:

- The project now has one final gameplay direction.
- Earlier classroom shooter scripts are no longer part of the main player experience.

## Sprint 1: Core Playable Loop

Goal: make the smallest complete version of the game.

Tasks:

- Add top-down player movement.
- Add wall blocking and a compact dungeon layout.
- Add key pickup.
- Add locked exit rule.
- Add victory state.

Acceptance:

- The player can move from start, collect the key, touch the exit, and win.

## Sprint 2: Challenge and Game Feel

Goal: make the vertical slice require timing and awareness.

Tasks:

- Add guard patrol behaviour.
- Add chase behaviour when the player gets close.
- Add player lives and fail state.
- Place guards so they affect route planning.

Acceptance:

- The player can lose by being caught.
- The player can avoid guards through timing and movement.

## Sprint 3: UI and Feedback

Goal: make the game understandable without extra explanation.

Tasks:

- Add main menu, instructions, credits, pause, win, and fail pages.
- Add HUD for lives, key state, time, and objective.
- Add key, hit, and win sound feedback.
- Add readable colours and text labels for important state.

Acceptance:

- A new player can understand the controls and goal from the game itself.

## Sprint 4: Testing, Report, and Presentation

Goal: prepare final assessment evidence.

Tasks:

- Complete manual regression testing.
- Record known limitations.
- Write final report.
- Prepare a short presentation plan.

Acceptance:

- GitHub, documentation, and game content together show design, implementation, testing, and reflection.
