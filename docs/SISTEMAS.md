# Sistemas del juego

## 1. Variantes de tablero (`SudokuRules`)

`SudokuRules` es una clase estática que define el tamaño del tablero actual. Toda la lógica del juego (índices, cajas, máximos) lee de aquí, por eso los cambios de variante funcionan en todos los sistemas automáticamente.

| Variante | Size | BoxRows × BoxCols | Valores |
|---|---|---|---|
| `Variant2x3` | 6 | 2 × 3 | 1-6 |
| `Standard3x3` | 9 | 3 × 3 | 1-9 |
| `Variant3x4` | 12 | 3 × 4 | 1-12 (9=A, 10=B...) |
| `Standard4x4` | 16 | 4 × 4 | 1-16 (10=A, 11=B...) |

- `ValueToLabel()` / `LabelToValue()` convierten entre el número interno y el texto mostrado (letras para valores > 9).
- `SetVariant(variant)` debe llamarse al inicio de cada escena de juego (lo hace `SudokuGameManager.Start`).

## 2. Generación de puzzles (`SudokuGenerator`)

Flujo de `Generate(difficulty)`:

1. **Elegir perfil de borrado**: `GetProfile(difficulty)` define cuántas celdas quitar según dificultad (rango en función de `CellCount/2`).
2. **Generar solución** (`GenerateSolution`): patrón base + barajado de números, filas dentro de su bloque y columnas dentro de su bloque (transformaciones que preservan validez).
3. **Construir puzzle único** (`BuildUniquePuzzle`): borra pares de celdas simétricas; tras cada borrado valida con `solver.CountSolutions() == 1`. Si pierde unicidad, revierte el borrado.
4. **Evaluar dificultad real** (`TryEvaluateDifficulty`): intenta resolver con el solver humano y guarda qué técnicas usó; `SudokuDifficultyEvaluatorPro` traduce eso a dificultad.
5. **Seleccionar el mejor intento**: hasta `MaxGenerationAttempts()` intentos (10 para tableros pequeños, 6 para 4x4). Devuelve el puzzle con dificultad más cercana a la pedida.

## 3. Solver humano y técnicas (`Core/Solver`)

`SudokuHumanSolver` resuelve con técnicas humanas (no fuerza bruta) y registra las usadas en `techniquesUsed`.

| Técnica | Archivo | Dificultad asociada |
|---|---|---|
| Naked Single | `NakedSingleTechnique.cs` | Easy |
| Hidden Single | `HiddenSingleTechnique.cs` | Easy/Medium |
| Naked Pair | `NakedPairTechnique.cs` | Hard |
| Pointing Pair | `PointingPairTechnique.cs` | Expert |
| X-Wing | `XWingTechnique.cs` | Extreme |

- `SudokuTechniqueFactory` instancia las técnicas permitidas según dificultad objetivo.
- `SudokuSolver` es el solver de fuerza bruta con conteo de soluciones (usado por el generador).
- `SudokuSolverUtils` contiene helpers comunes (candidatos, unidades, etc.).

## 4. Pistas (`SudokuHintSystem` + `SudokuInputController`)

- `maxHints` por partida (3 por defecto), restaurado al cargar guardado (`remainingHints`).
- `TryGetHint(out hint)` elige una celda vacía aleatoria y crea una acción `Place` con el valor de la solución.
- Al usar pista: se descuenta, se resalta la celda (`highlightSystem.ShowHint`) y se aplican las acciones (`boardController.ApplyActions`).
- **Limitación actual**: las pistas no usan el solver humano; sería mejor que mostraran la técnica real (Naked Single, X-Wing...) y las celdas implicadas.

## 5. Errores y derrota (`SudokuMistakeSystem`)

- `maxMistakes = 3`. Cada error (`RegisterMistake`) dispara `OnMistakeChanged`.
- Al llegar al máximo dispara `OnGameOver` → `SudokuGameFlowController.HandleDefeat()`.
- La UI de vidas (`SudokuLivesUI`) se actualiza con el evento.
- El número equivocado se muestra en rojo (visual) pero **no se guarda** en el tablero real.

## 6. Notas y undo (`SudokuBoardController`)

- **Notas**: máscara de bits por celda (`notesMask`). `ToggleNote` activa/desactiva; `AutoFillNotes` rellena candidatos válidos; al colocar un número se limpian las notas de la celda y de sus peers.
- **Undo**: lista `undoStack` (máx. 20). `SudokuMove` guarda `index`, `oldValue/newValue`, `oldNotes/newNotes`. Al deshacer se restaura todo y se actualiza el bitMask.
- El undo stack se guarda y restaura con la partida.

## 7. Bloqueo de números completados

Cuando los 9 (o N según variante) de un número están correctos, `LockCompletedNumber`:
- convierte esas celdas en fijas,
- limpia sus notas,
- limpia el undo stack (no se puede deshacer más allá de ese momento),
- `SudokuInputController` desactiva el botón del número en `NumberPanel`.

## 8. Guardado (`Infrastructure/Save`)

- **Formato**: JSON (`JsonUtility`) en `Application.persistentDataPath/sudoku_save_<slot>.json`, 3 slots.
- `SudokuSaveData` guarda: tablero actual, tablero inicial, tiempo, dificultad, undo stack, errores, vista previa de valores y pistas restantes.
- `SudokuSaveManager` orquesta; `SaveService` hace IO a disco.
- **Autoguardado**: `SudokuAutoSave` guarda periódicamente la partida en curso.
- Al **ganar** se borra el slot (`DeleteSlot`); al salir al menú se guarda (`SaveAndGoToMenu`).

## 9. Temporizador (`SudokuTimer`)

- Acumula `Time.deltaTime`; actualiza el texto solo cuando cambia el segundo (optimización).
- API: `StartTimer`, `StopTimer`, `ResetTime`, `GetTime`, `SetTime`.
- Se pausa con el juego, se restaura al cargar partida.

## 10. Resaltado (`SudokuHighlightSystem`)

Al seleccionar una celda:
1. Limpia resaltados anteriores.
2. Pinta fila + columna + caja con `highlightColor`.
3. Pinta números iguales con `sameNumberColor`.
4. Marca conflictos (mismo número repetido en fila/columna/caja) con `conflictColor`.
5. Pinta la celda seleccionada con `selectedColor`.

También gestiona el color del botón de notas y la visualización de pistas (amarillo = celda clave, rojo = celdas afectadas).

## 11. Flujo de escenas y sesión

- `SessionContext` (ScriptableObject) es la memoria entre escenas.
- `SudokuSessionController`: `StartNewGame` (marca `LoadFromSave=false`), `ContinueGame` (`LoadFromSave=true`), `SaveCurrentSlot`, `SaveAndGoToMenu`.
- `SudokuMenuSlotsController`: construye los slots, muestra vista previa (dificultad • tiempo) y conecta crear/continuar/borrar.

## 12. Estados finales

- **Victoria**: `HandleVictory` detiene el timer, borra el slot y tras `victoryDelay` muestra el panel; luego carga `MainMenu`.
- **Derrota**: `HandleDefeat` muestra panel y acciones (Reintentar / Nueva partida / Menú).
- `RestartLevel` reinicia al estado inicial guardado (`boardController.ResetBoard()`).
