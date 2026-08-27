# Sudoku MVC

Juego de Sudoku completo hecho en **Unity 6** (6000.3.6f1) con arquitectura **MVC** (Modelo-Vista-Controlador).

## Características

- **4 variantes de tablero**: 2x3, 3x3 (clásico), 3x4 y 4x4 (con letras para valores > 9).
- **5 dificultades**: Easy, Medium, Hard, Expert y Extreme.
- **Generador de puzzles** con solución única (usa un solver de conteo de soluciones).
- **Evaluador de dificultad real**: analiza qué técnicas humanas se usan para resolver el puzzle.
- **Solver humano** con técnicas clásicas:
  - Naked Single
  - Hidden Single
  - Naked Pair
  - Pointing Pair
  - X-Wing
- **Pistas** (`SudokuHintSystem`): el jugador tiene un número limitado de pistas.
- **Vidas/errores**: 3 errores permitidos antes de la derrota.
- **Notas/candidatos** (modo notas).
- **Undo** (hasta 20 movimientos).
- **Bloqueo de números completados**: al completar los 9 de un número, sus celdas se bloquean.
- **3 slots de guardado** en JSON (`Application.persistentDataPath`).
- **Autoguardado** y continuar partida.
- **Temporizador** de partida.
- **Resaltado inteligente**: fila/columna/caja, números iguales y conflictos.
- **Soporte de variantes dinámicas** vía `SudokuRules` (tamaño de tablero configurable).

## Requisitos

- Unity **6000.3.6f1** o superior (proyecto URP 2D).
- No requiere paquetes adicionales fuera de los incluidos en `Packages/manifest.json`.

## Cómo abrir el proyecto

1. Abre Unity Hub → **Add project from disk** → selecciona la carpeta del proyecto.
2. Abre la escena `Assets/Scenes/MainMenu.unity`.
3. Pulsa **Play**.

## Escenas

| Escena | Descripción |
|---|---|
| `Assets/Scenes/MainMenu.unity` | Menú principal con los 3 slots (crear, continuar, borrar). |
| `Assets/Scenes/Sudoku3x3.unity` | Escena de juego principal. Se adapta a cualquier variante (2x3, 3x3, 3x4, 4x4) según la sesión. |
| `Assets/Scenes/Sudoku2x3.unity` | Juego en tablero 2x3 (escena legacy). |
| `Assets/Scenes/Sudoku3x4.unity` | Juego en tablero 3x4 (escena legacy). |
| `Assets/Scenes/Sudoku4x4.unity` | Juego en tablero 4x4 (escena legacy). |

> Las 4 escenas de juego llevan el componente `SudokuGameUI` en su Canvas: construye toda la UI
> (timer, vidas, botones, panel de números, paneles) en runtime desde un único lugar, sin tocar escenas.

## Controles

| Acción | Descripción |
|---|---|
| Clic en una celda | Selecciona la celda (resalta fila/columna/caja e iguales). |
| Botones del panel numérico | Coloca un número en la celda seleccionada. |
| Botón Notas | Activa/desactiva el modo notas. |
| Borrar (0) | Vacía la celda seleccionada. |
| Botón Undo | Deshace el último movimiento. |
| Botón Pista | Coloca automáticamente un número correcto (consume una pista). |

## Estructura del proyecto

```
Assets/
└── Script/
    ├── Core/              # Lógica pura, sin dependencias de Unity UI
    │   ├── Board/         # Reglas, datos del tablero, notas, contextos
    │   ├── Generator/     # Generación de puzzles con solución única
    │   └── Solver/        # Solver humano + técnicas + evaluador de dificultad
    ├── Game/
    │   ├── Controller/    # Controladores: flujo, tablero, input, sesión, slots
    │   ├── State/         # Estado del juego y GameManager
    │   └── System/        # Sistemas: errores, pistas, resaltado
    ├── Infrastructure/
    │   ├── Save/          # Guardado/carga en JSON
    │   └── Time/          # Temporizador
    └── Presentation/
        ├── Board/         # Vista del tablero y celdas
        └── UI/            # Paneles e interfaces (victoria, derrota, menú...)
```

## Documentación

- [Arquitectura](docs/ARQUITECTURA.md) — capas, flujo de datos y responsabilidades.
- [Sistemas del juego](docs/SISTEMAS.md) — generación, dificultad, pistas, guardado, etc.

## Notas

- El proyecto está comentado en español línea por línea con fines didácticos.
- Los guardados se almacenan en `Application.persistentDataPath` con formato `sudoku_save_<slot>.json`.
