using UnityEngine;
public class SudokuAutoSave : MonoBehaviour//Este script se encarga de hacer guardado automático de la partida.
    //la idea es Cuando el tablero cambia, espera un poco y guarda la partida en el slot actual.
{
    [SerializeField] SudokuSaveManager saveManager;//referencia al SudokuSaveManager.
    //Este es el que realmente sabe guardar la partida : saveManager.SaveGame(slot);
    [SerializeField] SudokuBoardController board;//Referencia al controlador del tablero.
    //Se usa para escuchar este evento: board.OnBoardChanged  
    //O sea : Cuando el tablero cambie, avísame.
    [SerializeField] SessionContext sessionContext;//Referencia al contexto de sesión.
    //Se usa para saber en qué slot se debe guardar: sessionContext.SelectedSlot
    float saveDelay = 1.0f;//Tiempo de espera antes de guardar automáticamente. 
    //1.0f significa: 1 segundo. La idea es no guardar inmediatamente en cada cambio, sino esperar un poco.
    float timer;//Contador interno. Sirve para medir cuánto tiempo ha pasado desde el último cambio del tablero.
    bool dirty;//dirty significa algo como: Hay cambios pendientes por guardar.
    //Si dirty es true, el script sabe que debe guardar pronto. 
    //Si dirty es false, no hay nada pendiente.
    void Start()//Start se ejecuta cuando inicia la escena.
    {
        //Resuelve las referencias de forma robusta (se "acoplan" a las instancias reales).
        board = SudokuSceneRef.Resolve(board);
        sessionContext = SudokuSceneRef.ResolveSession(sessionContext);
        board.OnBoardChanged += MarkDirty;//Aquí el script se suscribe al evento del tablero: board.OnBoardChanged += MarkDirty;
        //eso significa Cuando el tablero cambie, llama a MarkDirty.
        //Ejemplo: el jugador coloca un número, borra una nota, usa undo, etc.
        //El board dispara OnBoardChanged, y SudokuAutoSave queda marcado para guardar.
    }
    void OnDestroy()//OnDestroy se ejecuta cuando el objeto se destruye o la escena cambia.
    {
        board.OnBoardChanged -= MarkDirty;//Aquí se desuscribe del evento: board.OnBoardChanged -= MarkDirty;
        //Esto es importante para evitar que el evento intente llamar a un objeto que ya no existe.
    }
    void MarkDirty()//Esta función se llama cuando cambia el tablero.
    {
        dirty = true;//Marca que hay cambios pendientes por guardar.
        timer = 0f;//Reinicia el contador.
        //Esto significa: Espera 1 segundo desde este último cambio antes de guardar.
        //Si el jugador hace varios cambios rápidos, el timer se reinicia cada vez. Así no guarda 10 veces seguidas.
    }
    void Update()//Update se ejecuta cada frame.
    {
        if (!dirty) return;//Si no hay cambios pendientes, se sale.
        //!dirty significa: ¿si dirty es false?.
        
        //si hay cambios entonces pasa aca
        timer += Time.deltaTime;//sí hay cambios pendientes, aumenta el timer.
        //Time.deltaTime es el tiempo que pasó desde el último frame.
        if (timer >= saveDelay)//Pregunta: ¿Ya pasó el tiempo de espera para guardar?
            //Si saveDelay es 1.0f, espera hasta que pase 1 segundo.
        {
            Save();//Cuando ya pasó el tiempo: guarda
            dirty = false;//Marca que ya no hay cambios pendientes.
        }
    }
    void Save()//Esta función hace el guardado real.
    {
        //Resolución perezosa: si el Start no alcanzó a resolver, se resuelve aquí para no fallar.
        saveManager = SudokuSceneRef.Resolve(saveManager);
        sessionContext = SudokuSceneRef.ResolveSession(sessionContext);
        if (saveManager == null || sessionContext == null)
            return;//Sin sistema de guardado o sin sesión no se puede guardar.
        int slot = sessionContext.SelectedSlot;//Obtiene el slot actual.
        if (slot < 0) return;//Si el slot es inválido, se detiene.
        //Un slot negativo, como -1, significa: No hay slot seleccionado.
        saveManager.SaveGame(slot);//Guarda la partida en ese slot.
        //Esto llama al SudokuSaveManager, que prepara los datos y usa SaveService.
    }
    void OnApplicationPause(bool pause)//Unity llama esta función cuando la aplicación se pausa.
    {
        if (pause)//Si pause es true, significa: La aplicación acaba de pausarse.
            //Por ejemplo, en móvil, cuando el usuario minimiza la app.
            Save();//entonces Guarda inmediatamente.
    }
    void OnApplicationQuit()//Unity llama esta función cuando la aplicación se está cerrando.
    {
        Save();//Aquí guarda inmediatamente antes de salir.
    }
}