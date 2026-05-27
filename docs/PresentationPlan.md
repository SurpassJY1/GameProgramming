# Presentation Plan

## Opening

Introduce `Dungeon Key Run` as a small 2D dungeon escape vertical slice made in Unity. The goal is to collect the gold key, avoid guards, and reach the blue exit door.

## Demo Order

1. Show the main menu and instructions.
2. Start the game and explain the HUD: lives, key state, timer, and objective.
3. Move through the dungeon and show wall collision.
4. Approach the exit before collecting the key to show the locked-door rule.
5. Collect the key and point out the HUD/objective change.
6. Show guard patrol/chase behaviour.
7. Reach the exit and show the victory screen.
8. Briefly show restart or pause if time allows.

## Design Explanation

The game is intentionally small so the final result can be stable and complete. The main design idea is route planning under pressure: the player must decide when to move past guards and when to head for the exit.

## Programming Explanation

Important systems to mention:

- `GameBootstrap.cs` builds the dungeon scene and UI at runtime.
- `GameManager.cs` controls menu, play, pause, win, and game over states.
- `Player.cs` handles movement, wall blocking, pickups, exit checks, and damage.
- `Enemy.cs` handles patrol and chase behaviour.
- `HUD.cs` and `Menus.cs` provide player feedback and flow control.

## Testing and Reflection

Explain that testing focused on the full player journey rather than isolated features. Mention that feedback from review led to clearer objective text, key state shown in text, and a tightly scoped one-level design.

## Strengths and Limitations

Strengths:

- Clear goal and complete win/fail loop
- Stable small-scope vertical slice
- Original procedural assets
- Good process evidence through GitHub and documentation

Limitations:

- One level only
- Manual testing only
- Simple guard AI rather than full pathfinding

## Possible Questions

Why one level?

Because the assessment rewards a polished vertical slice more than a large unfinished game.

Why no external assets?

Using procedural art and audio avoids licensing risk and keeps the project original.

What would be improved next?

More levels, better guard pathfinding, difficulty options, and additional accessibility settings.
