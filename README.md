# PiggyPocket

PiggyPocket es un mini platformer 2D hecho en Unity. El objetivo es recorrer un nivel corto, juntar monedas, romper barriles, evitar enemigos y trampas, y llegar a la meta final.

El proyecto esta pensado como prototipo de portfolio, con foco en gameplay simple, arquitectura ordenada y soporte para WebGL y Android.

## Gameplay

- Movimiento lateral, salto y coyote time.
- Ataque cuerpo a cuerpo con combo de dos golpes.
- Sistema de vida, danio, knockback, hit, injured, poison y death.
- Enemigo base reutilizable, actualmente aplicado a una oruga.
- Danio por contacto, pisoton sobre enemigos y feedback visual al recibir golpes.
- Monedas bronze y gold con valores distintos.
- Barriles rompibles con drops: vida, monedas, bomba o veneno.
- Trampas de pinchos pintables con Tile Palette.
- HUD con vida, monedas, pausa, ajustes, ayuda de controles, victoria y game over.
- Controles para teclado, mouse y mobile touch.
- Musica, efectos de sonido y VFX al recolectar monedas.

## Arquitectura

El proyecto separa responsabilidades en scripts chicos y reutilizables:

- `Player`: input, movimiento, ataque, animaciones, vida, estado y wallet.
- `Enemigos`: enemigo base, movimiento por patrulla, danio por contacto, pisoton y animaciones.
- `Collectibles`: monedas y vida.
- `Breakables`: objetos rompibles y sistema de drops.
- `Hazards`: trampas y efectos de area.
- `UI`: HUD, pausa, ajustes, victoria, derrota, menu principal y controles mobile.
- `Audio`: manager centralizado para musica y SFX.
- `Level`: meta final, camara y parallax.

## Controles

### PC / Web

- `A / D` o flechas: mover.
- `Space`: saltar.
- `J` o click: atacar.
- `Esc` o `P`: pausa.

### Mobile

- Botones tactiles para izquierda, derecha, salto, ataque y pausa.

## Escenas

Las escenas principales incluidas en Build Settings son:

- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/LevelComplete.unity`

## Requisitos

- Unity `6000.3.5f2` o compatible.
- Modulos recomendados:
  - Android Build Support para build mobile.
  - WebGL Build Support para build web.

## Como Ejecutarlo

1. Clonar el repositorio.
2. Abrir el proyecto desde Unity Hub.
3. Abrir `Assets/Scenes/MainMenu.unity`.
4. Presionar Play.

## Build

### Android

1. Ir a `File > Build Profiles`.
2. Seleccionar `Android`.
3. Usar `Switch Platform`.
4. Generar APK con `Build` o instalar directo con `Build And Run`.

Configuracion actual:

- Product Name: `PiggyPocket`
- Company Name: `EnzoDev`
- Package Name: `com.enzodev.piggypocket`
- Version: `1.0.0`

### WebGL

1. Ir a `File > Build Profiles`.
2. Seleccionar `WebGL`.
3. Usar `Switch Platform`.
4. Generar la build.

## Estado Del Proyecto

Prototipo jugable del primer nivel. Queda abierto para seguir puliendo:

- Disenio visual final de menus.
- Mas niveles.
- Mas enemigos.
- Mejor balance de dificultad.
- Publicacion WebGL / Android.

## Creditos

Assets utilizados:

- Player, enemigos, barriles, vida, monedas, vegetacion y tileset:  
  https://crusenho.itch.io/beriesadventureseaside

- UI:  
  https://cga-creative-game-assets.itch.io/gold-2d-mobile-ui-for-casual-game

- Controles mobile:  
  https://opengameart.org/content/mobile-controls

Musica y efectos de sonido agregados para el prototipo.

## Autor

Desarrollado por Enzo como proyecto de portfolio en Unity.
