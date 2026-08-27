# Arquitectura

El proyecto sigue un patrón **MVC** (Modelo-Vista-Controlador) organizado en 4 capas dentro de `Assets/Script/`.

## Diagrama de capas

```
┌─────────────────────────────────────────────────────────────┐
│  Presentation (Vista)                                        │
│  - SudokuBoardView, SudokuCell, NumberPanel, paneles de UI  │
└──────────────────────────────┬──────────────────────────────┘
                               │ Render / eventos de UI
┌──────────────────────────────▼──────────────────────────────┐
│  Game / Controller + State + System (Controlador)           │
│  - SudokuGameManager, SudokuGameFlowController              │
│  - SudokuBoardController, SudokuInputController             │
│  - SudokuMistakeSystem, SudokuHintSystem, SudokuHighlight   │
└──────────────────────────────┬──────────────────────────────┘
                               │ usa
┌──────────────────────────────▼──────────────────────────────┐
│  Core (Modelo) — lógica pura, sin dependencia de Unity UI   │
│  - Board: SudokuRules, SudokuBoardData, SudokuBitMask       │
│  - Generator: SudokuGenerator                               │
│  - Solver: SudokuSolver, SudokuHumanSolver, técnicas        │
└──────────────────────────────┬──────────────────────────────┘
                               │ persiste
┌──────────────────────────────▼──────────────────────────────┐
│  Infrastructure                                             │
│  - Save: SaveService, SudokuSaveManager, SudokuAutoSave     │
│  - Time: SudokuTimer                                        │
└─────────────────────────────────────────────────────────────┘
```

## Regla de dependencias

- **Presentation** → solo conoce **Game** (eventos e interfaces públicas).
- **Game** → usa **Core** e **Infrastructure**.
- **Core** → no referencia nada de las demás capas (es la más segura).
- **Infrastructure** → toca archivos/tiempo, usada por Game.

## Flujo de una partida nueva

```
MainMenu.unity
  SudokuMenuSlotsController (crea slots, muestra guardados)
      └─ HandleDifficultySelected
          └─ SudokuSessionController.StartNewGame(slot, difficulty, variant)
              └─ sessionContext (ScriptableObject): guarda slot/dificultad/variante
                  └─ LoadSceneAsync("Game")

Game.unity
  SudokuGameManager.Start()
      ├─ SudokuRules.SetVariant(variant)      → dimensiona el tablero
      ├─ boardView.CreateBoard()              → instancia las celdas visuales
      ├─ highlightSystem.Init(cells, controller)
      └─ flowController.Initialize()
          ├─ ¿LoadFromSave? → saveManager.LoadGame(slot) + LoadBoardToView()
          └─ si no → GenerateGame() → generator.Generate(difficulty)
                                        → boardController.SetBoardData(data)
                                        → boardView.UpdateBoard(...)
```

## Flujo de una jugada del jugador

```
SudokuCell.OnClick()
  └─ SudokuInputController.SelectCell(cell)      → highlightSystem.SelectCell(cell)
NumberPanel.OnNumberPressed(number)
  └─ SudokuInputController.SetNumber(number)
      ├─ valida celda fija / estado Playing
      ├─ si es incorrecto → boardView.SetCellError(...) + mistakeSystem.RegisterMistake()
      └─ si es correcto → boardController.ApplyMove(move)
          ├─ bitMask.Place / RemoveNotesFromPeers
          ├─ undoStack.Add(move)
          └─ OnBoardChanged → boardView.UpdateBoard(...) + flowController.CheckWin()
```

## Comunicación por eventos

El desacoplamiento entre scripts se hace con eventos C# (`event Action<...>`):

| Evento | Emisor | Receptor |
|---|---|---|
| `OnBoardChanged` | `SudokuBoardController` | Vista, flow, input (UI de números) |
| `OnMistakeChanged` / `OnGameOver` | `SudokuMistakeSystem` | `SudokuGameFlowController` (UI vidas / derrota) |
| `OnHintsChanged` | `SudokuInputController` | `SudokuGameFlowController` (UI pistas) |
| `OnNumberPressed` (estático) | `NumberPanel` | `SudokuInputController` |
| `OnCellClicked` (estático) | `SudokuCell` | `SudokuInputController` |
| `OnDifficultySelected` | `SudokuDifficultySelectionPanel` | Flow (en juego) y MenuSlots (en menú) |
| `OnVariantSelected` | `SudokuVariantSelectionPanel` | `SudokuMenuSlotsController` |

**Regla importante**: todo script que se suscribe en `Start`/`OnEnable` debe desuscribirse en `OnDestroy`/`OnDisable` para evitar fugas y llamadas a objetos destruidos.

## Estado del juego

Definido en `SudokuGameState` y reflejado en `SessionContext.GameState`:

```
Generating → Playing ⇄ Paused
                │
            (3 errores)     (tablero completo)
                ▼                 ▼
             Defeat             Victory
```

- `SessionContext` (ScriptableObject) es la **única memoria compartida entre escenas**: slot seleccionado, dificultad, variante, `LoadFromSave` y estado.
- `SudokuGameManager` tiene además su propio `gameState` local; idealmente el estado debería vivir solo en `SessionContext` (ver "Mejoras pendientes").

## Responsabilidades clave

### SudokuGameManager
Coordinador de la escena de juego: configura variante, crea el tablero, inicia el flujo y conecta `OnBoardChanged` con la vista. Expone Pause/Resume/SetGameState.

### SudokuGameFlowController
Orquesta la partida: `Initialize()` decide cargar o generar; maneja victoria/derrota con corrutinas, reinicio de nivel, panel de nueva partida y limpieza de UI.

### SudokuBoardController
El "modelo lógico" del tablero: `boardData`, bitMask de validación, undo, notas, bloqueo de números completados, `CheckWin()`.

### SudokuInputController
Traduce la entrada del jugador (clics en celdas y números) en movimientos válidos, controla el modo notas, las pistas y desactiva botones de números completados.

### SudokuGenerator
Genera puzzles con solución única y dificultad objetivo (ver [SISTEMAS.md](SISTEMAS.md)).

## UI centralizada (SudokuGameUI)

`SudokuGameUI` se coloca UNA vez en el Canvas de cada escena de Sudoku (`[DefaultExecutionOrder(-100)]`,
se ejecuta antes que los demás scripts). En su `Awake`:

1. Guarda fuente y sprites de la UI anterior (para conservar el estilo).
2. Destruye la UI vieja de la escena (botones, textos, paneles duplicados).
3. Construye toda la interfaz por código: barra superior (dificultad + timer + pistas), vidas,
   botones de acción (Notas, Undo, Borrar, AutoNotas, Pista, Pausa), panel de números y paneles
   (pausa, victoria, derrota, dificultad).
4. Reasigna las referencias de los scripts existentes (`Setup`, `SetTimerText`,
   `ConfigureEndGamePanels`, etc.) y conecta los botones a los controladores.

**Ventajas**
- Editar estilo o añadir botones se hace en UN script, no en 4 escenas.
- `NumberPanel` genera los botones de número a partir de UN prefab (`numberButtonPrefab`) o de uno
  por defecto, según el variante actual (6, 9, 12 o 16).
- Paneles de victoria/derrota/dificultad aceptan prefabs opcionales (`victoryPanelPrefab`,
  `defeatPanelPrefab`, `difficultyPanelPrefab`): si se asignan, se instancian; si no, se construyen
  por código. El menú de pausa también (`pausePanelPrefab`), conectando sus botones por etiqueta
  (Reanudar, Reiniciar, Nueva partida, Menú).
- Todos los paneles de prefab se ajustan a **pantalla completa** al instanciarse y al abrirse
  (`SetFullScreenRect` / normalización en `Open`/`OpenPause`), para que se posicionen encima aunque
  el prefab quedara a un lado o sobresaliendo del mapa.
- Barra superior (`topBarPrefab`, antes `DatosNivel`) y fila de ajustes (`actionRowPrefab`, antes
  `PanelAjustes`) también aceptan prefabs: se instancian y se conectan automáticamente buscando
  sus textos por nombre (`Difficulty*`, `Timer*`, `Hint*/Pistas`) y sus botones por etiqueta o
  nombre (`Notas`, `Undo`, `Borrar`, `Auto*`, `Pista/Hint`, `Pausa/Pause`).
- Al entrar a la escena de juego se **descargan las demás escenas abiertas** (por si el editor dejó
  el menú cargado junto al juego), evitando que se vea la imagen del menú de fondo.

### Resolución robusta de referencias (SudokuSceneRef)

Al convertir la UI en prefabs, las referencias serializadas de managers/controllers/systems pueden
quedar apuntando a un **asset de prefab** (no es null, pero activarlo o suscribirse a sus eventos no
funciona) o a **objetos destruidos** (la UI vieja). `SudokuSceneRef` (Core) resuelve esto:

- `Resolve<T>(current)` — valida que la referencia sea una instancia viva de la escena; si no, la
  busca (activa y luego inactiva) para "acoplarse" a la instancia real creada por `SudokuGameUI`.
- `ResolvePanel(current, type)` — lo mismo para paneles (GameObject).
- `ResolveSession(current)` — busca el `SessionContext` si falta.
- `IsSceneInstance(obj)` — ¿es una instancia de escena (no un asset de prefab ni objeto destruido)?

Se aplica en: `SudokuGameManager`, `SudokuGameFlowController`, `SudokuInputController`,
`SudokuSaveManager`, `SudokuPauseUI`, `SudokuHintSystem` y `SudokuAutoSave`. Así los managers se
acoplan automáticamente a las instancias de los prefabs y siguen funcionando al cambiar la UI.

## Mejoras pendientes de arquitectura

1. `Difficulty` vive anidado en `SudokuGameManager` (capa Game) pero Core lo usa → conviene moverlo a Core (p. ej. `SudokuDifficulty`).
2. `SudokuRules` es estado estático mutable; cambiar variante a mitad de juego rompería datos inconsistentes. Aceptable, pero documentado.
3. `SudokuGameManager.gameState` duplica a `SessionContext.GameState` (dos fuentes de verdad).
4. `EnsureBoardCreated()` accede a `cells[0,0]` sin validar que `cells` no sea `null`.
5. `SaveService` no maneja excepciones de IO (try/catch).
