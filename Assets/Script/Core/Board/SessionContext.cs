using UnityEngine;

[CreateAssetMenu(fileName = "SessionContext", menuName = "Sudoku/Session Context")]//permite crear un assets desde unity
public class SessionContext : ScriptableObject//es un asset de tipo compartido para pasar informacion entre scenas
{
    [SerializeField] int selectedSlot = -1;//slot seleccionado -1 significa ninguno ,0 significa slot 1,1 significa slot 2 y 3 significa slot 2
    [SerializeField] bool loadFromSave;//indica si la scene Game debe cargar una partida guardada
    [SerializeField] SudokuGameManager.Difficulty selectedDifficulty = SudokuGameManager.Difficulty.Medium;//dificultad seleccionada
    [SerializeField] SudokuGameState gameState = SudokuGameState.Generating;//estado actual del juego

    public int SelectedSlot//permite leer y cambiar el slot desde otros scripts
    {
        get => selectedSlot;
        set => selectedSlot = value;
    }

    public bool LoadFromSave//Permite saber si se debe cargar o generar una partida.
    {
        get => loadFromSave;
        set => loadFromSave = value;
    }

    public SudokuGameManager.Difficulty SelectedDifficulty//Permite guardar la dificultad elegida.
    {
        get => selectedDifficulty;
        set => selectedDifficulty = value;
    }

    public SudokuGameState GameState//Permite leer/cambiar el estado del juego.
    {
        get => gameState;
        set => gameState = value;
    }
}
