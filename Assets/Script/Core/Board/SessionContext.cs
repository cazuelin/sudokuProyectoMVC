using UnityEngine;

[CreateAssetMenu(fileName = "SessionContext", menuName = "Sudoku/Session Context")]//permite crear un assets desde unity
public class SessionContext : ScriptableObject//es un asset de tipo compartido para pasar informacion entre scenas
{
    [SerializeField] int selectedSlot = -1;//slot seleccionado -1 significa ninguno ,0 significa slot 1,1 significa slot 2 y 3 significa slot 2
    [SerializeField] bool loadFromSave;//indica si la scene Game debe cargar una partida guardada
    [SerializeField] SudokuGameManager.Difficulty selectedDifficulty = SudokuGameManager.Difficulty.Medium;//dificultad seleccionada
    [SerializeField] SudokuRules.SudokuVariant selectedVariant = SudokuRules.SudokuVariant.Standard3x3;//variante seleccionada del sudoku
    [SerializeField] SudokuGameState gameState = SudokuGameState.Generating;//estado actual del juego
    [SerializeField] int maxHints = 3;//cantidad máxima de pistas compartida entre escenas
    [SerializeField] int maxMistakes = 3;//cantidad máxima de errores compartida entre escenas

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

    public SudokuRules.SudokuVariant SelectedVariant//Permite guardar la variante del sudoku elegida.
    {
        get => selectedVariant;
        set => selectedVariant = value;
    }

    public SudokuGameState GameState//Permite leer/cambiar el estado del juego.
    {
        get => gameState;
        set => gameState = value;
    }

    public int MaxHints//Permite compartir el máximo de pistas entre escenas.
    {
        get => maxHints;
        set => maxHints = Mathf.Max(0, value);
    }

    public int MaxMistakes//Permite compartir el máximo de errores entre escenas.
    {
        get => maxMistakes;
        set => maxMistakes = Mathf.Max(1, value);
    }
}
