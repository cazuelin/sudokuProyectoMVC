using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SudokuSessionController : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Sudoku3x3";//Escena base del Sudoku 3x3.
    [SerializeField] string sudoku2x3SceneName = "Sudoku2x3";//Escena del Sudoku 2x3.
    [SerializeField] string sudoku3x4SceneName = "Sudoku3x4";//Escena del Sudoku 3x4.
    [SerializeField] string sudoku4x4SceneName = "Sudoku4x4";//Escena del Sudoku 4x4.
    [SerializeField] string menuSceneName = "MainMenu";//Guarda el nombre de la escena del menú. por defecto MainMenu . Se usa cuando quieres volver al menú
    [SerializeField] SudokuSaveManager saveManager;//Referencia al sistema de guardado. Sirve para guardar la partida actual
    [SerializeField] SessionContext sessionContext;//Referencia al contexto de sesión.
    void Awake()//Resuelve las referencias de forma robusta (se "acoplan" a las instancias reales).
    {
        saveManager = SudokuSceneRef.Resolve(saveManager);
        sessionContext = SudokuSceneRef.ResolveSession(sessionContext);
    }
    public void StartNewGame(int slot, SudokuGameManager.Difficulty difficulty, SudokuRules.SudokuVariant variant = SudokuRules.SudokuVariant.Standard3x3)//Esta función empieza una partida nueva.
        //recibe int slot , El slot donde se va a guardar la partida.
        //recibe SudokuGameManager.Difficulty difficulty , La dificultad elegida.
        //recibe SudokuRules.SudokuVariant variant , La variante de sudoku elegida (por defecto 3x3).
    {
        if (sessionContext == null)//Si no existe sessionContext, no puede guardar los datos de la sesión, así que se detiene.
            return;
        //si existe sessionContext entonces guarda : 
        sessionContext.SelectedSlot = slot;//Guarda el slot elegido.
        sessionContext.LoadFromSave = false;//esto es clave significa No voy a cargar una partida guardada. Voy a generar una nueva.
        sessionContext.SelectedDifficulty = difficulty;//Guarda la dificultad seleccionada.
        sessionContext.SelectedVariant = variant;//Guarda la variante seleccionada.
        StartCoroutine(LoadGameSceneWithOverlay(GetSceneNameForVariant(variant)));//Carga la escena correcta según la variante.
    }
    public void ContinueGame(int slot, SudokuRules.SudokuVariant variant)//Esta función continúa una partida guardada.
        //recibe int slot , El slot donde se va a guardar la partida.
    {
        if (sessionContext == null)//Si no existe sessionContext, no puede guardar los datos de la sesión, así que se detiene.
            return;
        //si existe sessionContext entonces guarda :
        sessionContext.SelectedSlot = slot;//El slot desde donde se cargará la partida.
        sessionContext.SelectedVariant = variant;//Guarda la variante asociada a ese slot.
        sessionContext.LoadFromSave = true;//esto indica Cuando entres a la escena Game, carga datos guardados en vez de generar un Sudoku nuevo.
        StartCoroutine(LoadGameSceneWithOverlay(GetSceneNameForVariant(variant)));//Finalmente carga la escena correcta
    }
    public void ContinueGame(int slot)//Esta función continúa una partida guardada.
        //recibe int slot , El slot donde se va a guardar la partida.
    {
        ContinueGame(slot, sessionContext != null ? sessionContext.SelectedVariant : SudokuRules.SudokuVariant.Standard3x3);
    }
    public void SaveCurrentSlot()//Esta función guarda la partida actual en el slot seleccionado.
    {
        if (saveManager == null)//Primero valida: Si no hay sistema de guardado, no puede guardar.
            return;
        if (sessionContext == null)//luego valida : Si no hay contexto, no sabe qué slot está seleccionado.
            return;
        int slot = sessionContext.SelectedSlot;//Obtiene el slot actual.
        if (slot >= 0)//si el slot es valido
            saveManager.SaveGame(slot);//entonces guarda la partida
        //slot >= 0 evita intentar guardar en un slot inválido como -1.
    }
    IEnumerator LoadGameSceneWithOverlay(string sceneName)//Esta función carga la escena del juego.
        //recibe string sceneName que es El nombre de la escena que se quiere cargar.
    {
        //Espera un frame para que la UI cierre los paneles abiertos antes del cambio.
        yield return null;
        //Carga la escena en modo Single: la escena anterior (menú) se descarga por completo,
        //sin dejar rastros visibles detrás de la escena nueva.
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    public void SaveAndGoToMenu()//Esta función guarda la partida y vuelve al menú.
    {
        SaveCurrentSlot();//Guarda la partida actual.
        SceneManager.LoadScene(menuSceneName);//Carga la escena del menú.
    }

    string GetSceneNameForVariant(SudokuRules.SudokuVariant variant)
    {
        return variant switch
        {
            SudokuRules.SudokuVariant.Variant2x3 => sudoku2x3SceneName,
            SudokuRules.SudokuVariant.Standard3x3 => gameSceneName,
            SudokuRules.SudokuVariant.Variant3x4 => sudoku3x4SceneName,
            SudokuRules.SudokuVariant.Standard4x4 => sudoku4x4SceneName,
            _ => gameSceneName
        };
    }
}
