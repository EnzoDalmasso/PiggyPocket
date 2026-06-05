# PiggyPocket

PiggyPocket is a 2D mini-platformer made in Unity. The goal is to traverse a short level, collect coins, break barrels, avoid enemies and traps, and reach the final goal.

The project is designed as a portfolio prototype, focusing on simple gameplay, clean architecture, and support for WebGL and Android.

## Gameplay

- Lateral movement, jumping, and coyote time.
- Melee attack with a two-hit combo.
- Health, damage, knockback, hit, injured, poison, and death systems.
- Reusable base enemy, currently applied to a caterpillar.
- Contact damage, stomping on enemies, and visual feedback when taking hits.
- Bronze and gold coins with different values.
- Breakable barrels with drops: health, coins, a bomb, or poison.
- Spike traps paintable with Tile Palette.
- HUD with health, coins, pause, settings, controls help, victory, and game over screens.
- Controls for keyboard, mouse, and mobile touch.
- Music, sound effects, and VFX when collecting coins.

## Architecture

The project separates responsibilities into small, reusable scripts:

- `Player`: input, movement, attack, animations, health, state, and wallet.
- `Enemies`: base enemy, patrol movement, contact damage, stomping, and animations.
- `Collectibles`: coins and health.
- `Breakables`: breakable objects and drop system.
- `Hazards`: traps and area-of-effect (AoE).
- `UI`: HUD, pause, settings, victory, defeat, main menu, and mobile controls.
- `Audio`: centralized manager for music and SFX.
- `Level`: final goal, camera, and parallax.

## Controls

### PC / Web

- `A / D` or Arrow Keys: Move.
- `Space`: Jump.
- `J` or Left Click: Attack.
- `Esc` or `P`: Pause.

### Mobile

- Touch buttons for left, right, jump, attack, and pause.

## Scenes

The main scenes included in the Build Settings are:

- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/LevelComplete.unity`

## Requirements

- Unity `6000.3.5f2` or compatible.
- Recommended modules:
  - Android Build Support for mobile builds.
  - WebGL Build Support for web builds.

## How to Run

1. Clone the repository.
2. Open the project via Unity Hub.
3. Open `Assets/Scenes/MainMenu.unity`.
4. Press Play.

## Build

### Android

1. Go to `File > Build Profiles`.
2. Select `Android`.
3. Click `Switch Platform`.
4. Generate the APK with `Build` or deploy directly with `Build And Run`.

Current configuration:

- Product Name: `PiggyPocket`
- Company Name: `EnzoDev`
- Package Name: `com.enzodev.piggypocket`
- Version: `1.0.0`

### WebGL

1. Go to `File > Build Profiles`.
2. Select `WebGL`.
3. Click `Switch Platform`.
4. Generate the build.

## Project Status

Playable prototype of the first level. Open for further polishing:

- Final visual design for menus.
- More levels.
- More enemies.
- Better difficulty balancing.
- WebGL / Android deployment.

## Credits

Assets used:

- Player, enemies, barrels, health, coins, vegetation, and tileset:  
  https://crusenho.itch.io/beriesadventureseaside

- UI:  
  https://cga-creative-game-assets.itch.io/gold-2d-mobile-ui-for-casual-game

- Mobile controls:  
  https://opengameart.org/content/mobile-controls

Music and sound effects added for the prototype.

## Author

Developed by Enzo as a Unity portfolio project.
