# Star Blaster — Change Log

## What the game was before
Starting point was an empty Unity 2D project with only a default `SampleScene`.

## What was added / changed in this update

### Player goal (one sentence)
> Survive as long as you can while shooting falling enemies and grabbing
> power-ups; chase the high score.

### Menu system
- **Main menu** with four buttons: Start, Instructions, Credits, Quit.
- **Instructions page** explaining controls, power-ups, and the goal.
- **Credits page** crediting code/art/audio.
- **Pause menu** (ESC during play): Resume, Restart, Main Menu.
- **Game Over screen**: shows final score, flags new high score, with
  Play Again and Main Menu buttons.
- All sub-pages have a clear `< Back` button so navigation is reversible.

### HUD (`HUD.cs`)
- Top-left: live **Score**, **High Score**, **Lives**.
- Top-right: **Level**, **Time**, active **Power-up timers** (Rapid xs / Shield xs).
- Bottom-center: **Objective text** ("Survive! Shoot the falling enemies. Grab
  power-ups.").
- Bottom-right hint: "ESC = pause".
- Re-paints from `GameManager.OnStateChanged` events so it stays in sync
  even on restart.

### Gameplay improvement (the main rubric item)
- **Three enemy types** with distinct behaviors:
  - *Straight* — descends vertically.
  - *Zigzag* — sine-wave horizontal sway as it falls.
  - *Chaser* — biases toward the player.
- **Difficulty scaling**:
  - Spawn interval shrinks ~12% per level (floor 0.35s).
  - Enemy speed grows ~7% per level.
  - Type weighting shifts toward harder enemies as level rises.
  - Level auto-advances every 25 seconds (`GameManager.secondsPerLevel`).
- **Three power-ups**:
  - *Rapid Fire* (yellow) — triple-shot for 6s.
  - *Shield* (blue) — absorbs the next contact hit.
  - *Extra Life* (pink) — +1 life.
- Player has a brief invulnerability window after taking a hit so multiple
  enemies can't drain lives in one frame.

### Feedback (audio + visual)
- **Procedural sounds** generated in `Art2D.cs` — shoot blip, enemy noise burst,
  player hit noise, power-up chime, button click. No audio files shipped.
- **Camera shake** on player hits and enemy kills (`CameraShake.Pulse`).
- **Death particles** — small color-matched dots that fly out and fade.
- **Player tint** — blue tint while shielded, red flicker while invulnerable.
- **High-score flag** "*** NEW! ***" on the Game Over screen when beaten.
- High score persists between runs via `PlayerPrefs`.

### Polish
- **Procedural sprites** generated in `Art2D.cs` (anti-aliased disc, triangle,
  diamond, square) — zero binary assets to manage.
- **Single-scene self-assembly**: `GameBootstrap.cs` builds the entire scene at
  Awake, so the scene file stays trivially small and merge-friendly.
- **Component-based collision** (no custom Tags required) — works in any default
  Unity project without touching `TagManager.asset`.
- **Restart hygiene**: switching from Game Over → Play Again destroys all live
  enemies/bullets/power-ups so the next run starts clean.

## What was tested
- Complete player flow: Main → Start → Play → Pause → Resume → Play → Die →
  Game Over → Play Again → Die → Main Menu → Instructions → Back → Credits →
  Back → Quit.
- Score / high score persistence across restarts.
- Power-up timers count down on HUD; Rapid Fire produces three bullets;
  Shield absorbs one hit and clears.
- Difficulty visibly increases — by Level 3 spawn rate and chaser frequency
  are noticeably higher.
- ESC during play opens pause; ESC ignored on menu/game-over.

## Known limitations
- No background music (kept silent on purpose for class demo).
- No level-select (single endless mode).
- No accessibility options (color-blind palette etc).
