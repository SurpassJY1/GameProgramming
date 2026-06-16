# Dungeon Key Run - Change Log

## Final Project Direction

The final game direction is `Dungeon Key Run`, a Unity 2D top-down dungeon
action vertical slice. Earlier classroom shooter work has been reframed into a
key-and-exit dungeon run with shooting, XP, upgrades, enemy variety, boss
encounters, and local run records.

## Current Vertical Slice

### Core Goal

The player explores rebuilt dungeon floors, defeats enemies for XP, collects
the gold key, reaches the blue exit door, chooses upgrades, and continues until
all lives are lost.

Every third floor is a boss floor. On boss floors, the exit stays sealed until
the boss is defeated as well as the key being collected.

### Player Systems

- Top-down movement using `WASD` or arrow keys.
- Mouse aiming and left-click shooting.
- Wall blocking using 2D circle casts against dungeon walls.
- Damage, lives, invulnerability frames, hit feedback, and game-over state.
- Weapon upgrades for extra projectiles, rapid fire, damage, piercing, burn,
  slow, and explosive shots.

### Floor and Progression Systems

- Gold key pickup and locked exit feedback.
- Floor rebuild/reset flow after each passive upgrade choice.
- XP, score, level-up weapon choices, and floor-clear passive choices.
- Local best score, best floor, and Top 5 leaderboard stored with PlayerPrefs.
- Tutorial prompts for early successful runs.

### Enemy and Boss Behaviour

- Enemy spawning and enemy defeat tracking.
- Patrol/chase movement and contact damage.
- Progressive enemy unlocks across floors.
- Ranged attacks, slowing projectiles, proximity explosions, dash attacks,
  ally healing, summoning, and elite pressure.
- Boss floors every third floor with boss health bar and sealed-exit rule.
- Boss kinds can reappear later as elite enemies without sealing the exit.

### UI and Feedback

- Main menu, instructions, credits, pause, upgrade, passive upgrade, game-over,
  restart, and leaderboard flows.
- HUD displays floor, lives, key state, defeated enemies, score, level, XP,
  elapsed time, objective text, upgrade summaries, and boss health.
- Objective text explains missing key, boss seal, key collected, and boss
  defeated states.
- Camera follow, camera shake, sound effects, background music, and runtime
  fallback audio/visual effects.

### Assets and Documentation

- Project-generated sprites for clear dungeon gameplay, selected pixel-cute
  assets, boss sprites, fallback/effect visuals, and upgrade icons.
- Kenney CC0 image/audio assets included with local license files.
- Kevin MacLeod `8bit Dungeon Level` music included under CC BY 4.0 with
  attribution.
- Documentation updated for concept/design, sprint plan, test log, asset
  credits, final report, presentation plan, README, credits, and this change log.

## Testing Evidence

Testing is recorded in `../../docs/TestLog.md`. The main regression path covers
project open, menu flow, movement, wall collision, aiming, shooting, enemy
defeat, XP, upgrades, key pickup, locked exit, normal floor clear, boss-floor
sealing, player damage, game over, restart, leaderboard, and asset fallback
behavior.

## Known Limitations

- This is a vertical slice rather than a full campaign.
- Manual testing only; no automated Unity test suite is included.
- PlayerPrefs records are local only.
- The scene filename remains `DungeonKeyRun.unity` from earlier classroom work,
  but the runtime content is `Dungeon Key Run`.
