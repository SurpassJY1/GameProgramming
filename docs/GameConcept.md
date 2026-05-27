# Game Concept and Design

## Game Idea

`Dungeon Key Run` is a short 2D top-down dungeon escape game. The player starts in a locked dungeon, must collect a gold key, avoid patrolling guards, and reach the exit door.

## Intended Player Experience

The game is designed to feel tense but readable. The player should understand the goal quickly, plan a route through the dungeon, watch guard movement, and feel rewarded when the exit unlocks after collecting the key.

## Core Mechanic

The main mechanic is route planning under pressure. The key and exit are separated, and guards patrol between important routes. The player does not fight; success comes from movement, timing, and awareness.

## Scope

The project is intentionally scoped as one polished vertical slice:

- One complete dungeon level
- One player character
- One key and one exit
- Two guards with patrol and chase behaviour
- Menus, HUD, win state, fail state, and pause state

Features such as multiplayer, inventory systems, combat, procedural campaign generation, and multiple levels are out of scope.

## Tools and Resources

- Engine: Unity `2022.3.62f1`
- Language: C#
- Input: Unity legacy input axes for simple keyboard controls
- Assets: procedural sprites and audio generated in code
- Version control: Git and GitHub

## Legal, Ethical, Social, Accessibility, and Security Issues

- Legal: no third-party art, audio, fonts, or Asset Store assets are used, so licensing risk is low.
- Ethical/social: the game uses abstract guards and non-graphic failure feedback.
- Accessibility: controls are stated in the menu and README, key state is shown through text as well as colour, and the goal is repeated through objective text.
- Security: the project does not collect personal data, use networking, or load external files at runtime.

## Success Criteria

The submission is successful if a first-time player can start the game, understand the goal, collect the key, avoid guards, reach the exit, and see a clear win or fail result without needing extra explanation.
