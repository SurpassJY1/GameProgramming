# Game Concept and Design

## Game Idea

`Dungeon Key Run` is a Unity 2D top-down dungeon shooter. The player explores
dungeon floors, defeats enemies, collects the gold key, reaches the blue exit
door, and pushes deeper into the run to improve the saved local best score.

## Intended Player Experience

The game is designed to feel tense but readable. The player should quickly
understand the current objective, use movement and shooting to control danger,
watch the HUD for key and floor state, and feel rewarded by upgrades between
floors.

## Core Mechanic

The core loop is:

1. Explore the current floor.
2. Defeat enemies for XP and score.
3. Collect the gold key.
4. Reach the blue exit door.
5. Choose a passive floor-clear upgrade.
6. Continue to deeper floors and boss encounters.

Every third floor is a boss floor. On those floors, the exit stays sealed until
the boss is defeated.

## Scope

The project is scoped as a polished dungeon run vertical slice:

- One main playable Unity scene.
- Top-down movement, mouse aiming, and shooting.
- Enemy encounters, boss floors, lives, scoring, XP, and level-up choices.
- Key pickup and exit-door progression.
- Passive upgrade choices after floor clears.
- Start, pause, game-over, restart, and leaderboard UI.
- Runtime fallback sprites/sounds plus documented third-party assets.

Features such as multiplayer, network play, inventory systems, cutscenes, and a
large handcrafted campaign are out of scope.

## Tools and Resources

- Engine: Unity `2022.3.62f1`
- Language: C#
- Input: keyboard movement, mouse aiming, and mouse shooting
- Assets: project-generated sprites, runtime fallback art/audio, Kenney CC0
  assets, and one CC BY 4.0 music track
- Version control: Git and GitHub

## Legal, Ethical, Social, Accessibility, and Security Issues

- Legal: third-party resources are limited to documented free assets. Kenney
  image/audio packs are CC0, and the Kevin MacLeod music track is credited under
  CC BY 4.0 in the README and CREDITS file.
- Ethical/social: the game uses stylized fantasy combat and non-graphic failure
  feedback.
- Accessibility: controls are stated in the README, key state and objectives are
  shown through text, and the HUD does not rely only on colour.
- Security: the project does not collect personal data, use networking, or load
  remote files.

## Success Criteria

The submission is successful if a first-time player can open the documented
scene, start a run, understand the objective, move and shoot, collect the key,
clear normal and boss-floor objectives, choose upgrades, and see clear run-end
feedback without needing extra explanation.
