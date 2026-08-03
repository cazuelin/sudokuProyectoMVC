using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SudokuSessionController : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Game";//Guarda el nombre de la escena del juego. por defecto Game . Se usa cuando quieres entrar al Sudoku
    [SerializeField] string menuSceneName = "MainMenu";//Guarda el nombre de la escena del menú. por defecto MainMenu . Se usa cuando quieres volver al menú
    [SerializeField] SudokuSaveManager saveManager;//Referencia al sistema de guardado. Sirve para guardar la partida actual
    [SerializeField] SessionContext sessionContext;//Referencia al contexto de sesión.
    public void StartNewGame(int slot, SudokuGameManager.Difficulty difficulty)//Esta función empieza una partida nueva.
        //recibe int slot , El slot donde se va a guardar la partida.
        //recibe SudokuGameManager.Difficulty difficulty , La dificultad elegida
    {
        if (sessionContext == null)//Si no existe sessionContext, no puede guardar los datos de la sesión, así que se detiene.
            return;
        //si existe sessionContext entonces guarda : 
        sessionContext.SelectedSlot = slot;//Guarda el slot elegido.
        sessionContext.LoadFromSave = false;//esto es clave significa No voy a cargar una partida guardada. Voy a generar una nueva.
        sessionContext.SelectedDifficulty = difficulty;//Guarda la dificultad seleccionada.
        StartCoroutine(LoadGameSceneWithOverlay(gameSceneName));//Carga la escena del juego usando una corrutina.
    }
    public void ContinueGame(int slot)//Esta función continúa una partida guardada.
        //recibe int slot , El slot donde se va a guardar la partida.
    {
        if (sessionContext == null)//Si no existe sessionContext, no puede guardar los datos de la sesión, así que se detiene.
            return;
        //si existe sessionContext entonces guarda :
        sessionContext.SelectedSlot = slot;//El slot desde donde se cargará la partida.
        sessionContext.LoadFromSave = true;//esto indica Cuando entres a la escena Game, carga datos guardados en vez de generar un Sudoku nuevo.
        StartCoroutine(LoadGameSceneWithOverlay(gameSceneName));//Finalmente carga la escena
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
    IEnumerator LoadGameSceneWithOverlay(string sceneName)//Esta función carga una escena de forma asíncrona.
        //recibe string sceneName que es El nombre de la escena que se quiere cargar.
    {
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);//Empieza a cargar la escena en segundo plano.
        asyncLoad.allowSceneActivation = true;//Permite que Unity active la escena cuando termine de cargar.
        while (!asyncLoad.isDone)//Mientras la carga no termine, espera un frame.
            yield return null;
        //yield return null significa: Espera al siguiente frame y vuelve a revisar.
    }
    public void SaveAndGoToMenu()//Esta función guarda la partida y vuelve al menú.
    {
        SaveCurrentSlot();//Guarda la partida actual.
        SceneManager.LoadScene(menuSceneName);//Carga la escena del menú.
    }
}
