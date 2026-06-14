# Test Log

## Manual Regression Test Plan

| Area | Test | Expected Result | Status |
| --- | --- | --- | --- |
| Project setup | Open `Project2DImproved 2/Assets/Scenes/StarBlaster.unity` | Main playable scene opens in Unity | Pass in code review |
| Menu | Start from main menu | Run begins and HUD appears | Pass in code review |
| Movement | Move with WASD/arrows | Player moves in four directions | Pass in code review |
| Aiming | Move mouse while playing | Player aim direction follows the mouse | Pass in code review |
| Shooting | Hold left mouse button | Player fires projectiles | Pass in code review |
| Enemy combat | Hit enemies with projectiles | Enemies take damage and can be defeated | Pass in code review |
| Player damage | Touch or get hit by an enemy | Player loses lives and receives feedback | Pass in code review |
| Key | Touch gold key | Key state updates and objective changes | Pass in code review |
| Locked exit | Touch exit before key | Objective indicates the key is needed | Pass in code review |
| Normal floor clear | Touch exit after key | Floor-clear flow starts | Pass in code review |
| Passive upgrade | Clear a floor | Passive upgrade choice appears before the next floor | Pass in code review |
| XP and level up | Defeat enough enemies | Weapon upgrade choice appears | Pass in code review |
| Boss floor | Reach every third floor | Boss objective appears and exit stays sealed | Pass in code review |
| Boss defeated | Defeat boss | Exit unlocks for that floor | Pass in code review |
| HUD | Play a run | Floor, lives, key, score, level, XP, time, and objective are visible | Pass in code review |
| Pause | Press ESC during play | Pause menu appears, resume returns to game | Pass in code review |
| Game over | Lose all lives | Game-over flow appears | Pass in code review |
| Restart | Restart after pause/game over | State resets for a new run | Pass in code review |
| Records | End a run | Best score/floor and Top 5 leaderboard are saved locally | Pass in code review |

## Testing Notes

Manual tests should be repeated inside the Unity Editor before final submission.
The current implementation was checked structurally through script review and
documentation review; final playtesting in Unity should confirm movement feel,
enemy difficulty, boss pacing, audio balance, and readability of upgrade choices.

## Changes Made From Testing and Review

- The README now puts quick start, demo flow, implemented features, and project
  layout before the detailed asset list.
- The key state and current objective are shown as text in the HUD.
- Boss floors use a sealed-exit rule so the player has a clear combat objective.
- Upgrade choices were separated into weapon level-up choices and passive
  floor-clear choices.
- Asset credits were moved to the project README and CREDITS file so the source
  and license trail is easier to find.

## Known Issues and Limitations

- The Unity scene file name remains `StarBlaster.unity` from earlier classroom
  work, although the built game content is `Dungeon Key Run`.
- There are no automated Unity tests.
- Floor layouts and enemy behaviours are intentionally compact for a vertical
  slice.
