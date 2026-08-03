using System.Collections.Generic;
using UnityEngine;
public class SudokuSaveManager : MonoBehaviour//es el script que coordina el guardado y la carga de partidas.
//No guarda archivos directamente: arma/recupera los datos y delega el trabajo real a SaveService
{
    SaveService saveService = new SaveService();//Crea una instancia de SaveService.
    //SaveService es el que realmente guarda en archivo:
    //saveService.Save(...) , saveService.Load(...) , saveService.Delete(...) , saveService.Exists(...)
    //En palabras simples: SudokuSaveManager prepara los datos, y SaveService los escribe en disco.
    [SerializeField] SudokuBoardController board;//Referencia al controlador del tablero.
    [SerializeField] SudokuTimer timer;//Referencia al timer. Se usa para guardar y cargar el tiempo
    [SerializeField] SudokuMistakeSystem mistakeSystem;//Referencia al sistema de errores. Se usa para guardar y cargar errores
    [SerializeField] SudokuInputController inputController;//Referencia al controlador de input.
    //Aquí se usa principalmente para guardar y restaurar las pistas restantes
    [SerializeField] SessionContext sessionContext;//Referencia al contexto de sesión. Se usa para guardar y restaurar la dificultad:
    public void SaveGame(int slot)//Esta función guarda la partida actual en un slot.
    {
        if (board == null || timer == null || mistakeSystem == null)//Primero revisa si existen referencias básicas del juego.
            //Si falta board, timer o mistakeSystem, no puede guardar una partida.
        {
            return;
        }
        if (inputController == null)//Si no tiene referencia al inputController.
        {
            inputController = FindFirstObjectByType<SudokuInputController>();//intenta buscarlo automáticamente en la escena
            //Esto se hace porque necesita leer: inputController.RemainingHints  para guardar las pistas restantes.
        }
        //si estan todas las referencias entonces Aquí crea un objeto SudokuSaveData.
        //Este objeto es el paquete completo que se guardará en JSON.
        var data = new SudokuSaveData
        {
            board = board.boardData,//Guarda el estado actual del tablero. 
            //Incluye números, celdas fijas, notas, solución, etc., según tu SudokuBoardData.
            initialBoard = board.GetInitialData(),//Guarda el estado inicial del tablero. 
            //Esto sirve para poder reiniciar la partida con RestartLevel.
            time = timer.GetTime(),//Guarda el tiempo actual de la partida.
            difficulty = (int)sessionContext.SelectedDifficulty,//Guarda la dificultad como número entero.
            //el enum seria Easy , medium , hard , expert y extreme  internamente se puede convertir a int.
            undoStack = board.GetUndoStack() ?? new List<SudokuMove>(),//Guarda la pila/lista de movimientos para poder restaurar el undo.
            //El operador ?? significa:  Si board.GetUndoStack() devuelve null, usa una lista vacía.
            mistakes = mistakeSystem.GetMistakes(),//Guarda la cantidad actual de errores.
            previewValues = (int[])board.boardData.values.Clone(),//Guarda una copia de los valores actuales del tablero.
            //Clone() crea una copia del arreglo. Esto sirve como vista previa del slot, por ejemplo para mostrar un resumen del guardado en el menú.
            remainingHints = inputController != null ? inputController.RemainingHints : 0 //Guarda las pistas restantes.
            //Esto usa operador ternario: condición ? valorSiTrue : valorSiFalse
            //significa : Si inputController existe, guarda sus hints restantes. Si no existe, guarda 0.
        };
        saveService.Save(data, slot);//Finalmente manda los datos a SaveService. Ahí se convierten a JSON y se escriben en disco.
    }
    public bool LoadGame(int slot)//Esta función carga una partida guardada. devuelve true si logra cargar y false si no pudo
    {
        if (!saveService.Load(slot, out var data))//Intenta cargar el archivo del slot.
            return false;//Si SaveService no encuentra archivo o falla, devuelve false.
        if (data == null || data.board == null)//Verifica que los datos cargados sean válidos.
        {
            return false;//Si data es null o no tiene tablero, considera que el guardado está corrupto y devuelve false.
        }
        //si pasa las validaciones entonces puede comenzar a cargar los datos del juego
        board.SetBoardData(data.board);//Restaura el tablero actual.
        board.SetUndoRedo(data.undoStack);//Restaura la lista de undo/redo. Así el jugador puede seguir usando undo después de cargar.
        if (data.initialBoard != null)//Si existe estado inicial guardado, lo restaura.
        {
            board.SetInitialState(data.initialBoard);//restaura el estado inicial guardado
            //Esto permite que RestartLevel vuelva al comienzo correcto de esa partida.
        }
        timer.SetTime(data.time);//Restaura el tiempo.
        sessionContext.SelectedDifficulty =
            (SudokuGameManager.Difficulty)data.difficulty;//Restaura la dificultad. Como se guardó como int, aquí se convierte de vuelta al enum
        mistakeSystem.Init(data.mistakes);//Restaura la cantidad de errores.
        if (inputController == null)//Si no tiene referencia al input, intenta encontrarlo.
        {
            inputController = FindFirstObjectByType<SudokuInputController>();//busca alguna referencia al inputController en la partida
        }
        if (inputController != null)//si la referencia la inputController no es nula
            inputController.SetRemainingHints(data.remainingHints);//Restaura las pistas restantes.
        return true;//Devuelve que la carga fue exitosa.
    }
    public void DeleteSlot(int slot)//Borra el archivo del slot.
    {
        saveService.Delete(slot);//Este método delega directamente en SaveService.
        //Se usa, por ejemplo, cuando el jugador gana: saveManager?.DeleteSlot(sessionContext.SelectedSlot);
    }
    public bool HasSlot(int slot)//Revisa si existe una partida guardada en ese slot.
    {
        return saveService.Exists(slot);//devuelve true si existe archivo guardado y false si no encuentra nada
        //Esto sirve para el menú: Si hay guardado, mostrar botón Continuar. Si no hay guardado, mostrar slot vacío.
    }
    public SudokuSaveData GetSlotData(int slot)//Carga los datos de un slot y los devuelve.
    {
        if (!saveService.Load(slot, out var data))//pregunta si existe datos para cargar
            return null;//si no existe datos devuelve null
        return data;//si existe datos para cargar los devuelve
        //Esto sirve cuando necesitas leer información sin cargar toda la partida en el tablero.
        //Por ejemplo: mostrar vista previa ,  leer remainingHints , mostrar dificultad , mostrar tiempo
    }
}
