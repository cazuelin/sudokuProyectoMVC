using UnityEngine;
public class SudokuInputController : MonoBehaviour
{
    [SerializeField] SudokuHintSystem hintSystem;//Guarda una referencia al sistema de pistas.
    //hintSystem calcula o entrega pistas para el jugador.
    [SerializeField] SudokuBoardView boardView;//Referencia a la vista visual del tablero.
    //boardView se encarga de mostrar visualmente errores o limpiar errores en las celdas.
    [SerializeField] SudokuHighlightSystem highlightSystem;//Referencia al sistema de resaltado.
    //highlightSystem pinta o resalta celdas según selección, modo notas o hints.
    [SerializeField] SudokuBoardController boardController;//boardController maneja la lógica real del tablero.
    //boardController es quien modifica y valida el Sudoku real.
    [SerializeField] SudokuMistakeSystem mistakeSystem;//Referencia al sistema de errores.
    //mistakeSystem cuenta errores y puede provocar derrota.
    [SerializeField] SessionContext sessionContext;//Referencia al contexto de sesión.
    //sessionContext le dice al input si la partida está activa o pausada/terminada.
    SudokuCell selectedCell;//Guarda la celda actualmente seleccionada. selectedCell es la casilla donde se colocará el número o la nota.
    bool noteMode;//Guarda si el jugador está en modo notas. noteMode decide si el botón numérico coloca un número grande o una nota pequeña.
    int remainingHints;//Guarda cuántas pistas le quedan al jugador. remainingHints es el contador interno de pistas disponibles.
    bool hintsInitialized;//Sirve para saber si las pistas ya fueron inicializadas.
    //hintsInitialized evita que una partida cargada pierda su cantidad de pistas guardadas.
    public int RemainingHints => remainingHints;//Esta es una propiedad de solo lectura. Permite que otros scripts lean las pistas restantes
    public event System.Action<int> OnHintsChanged;//Este evento avisa cuando cambia la cantidad de pistas. Envía un int, que es la nueva cantidad de hints.
    //OnHintsChanged avisa a la interfaz cuando cambia el contador de pistas.

    void Start()//Start es una función de Unity.
        //Se ejecuta automáticamente cuando empieza la escena y el GameObject está activo.
    {
        //Resuelve las referencias de forma robusta (se "acoplan" a las instancias reales aunque
        //se hayan convertido en prefabs). Si apuntan a un asset o a un objeto destruido, las busca.
        hintSystem = SudokuSceneRef.Resolve(hintSystem);
        boardView = SudokuSceneRef.Resolve(boardView);
        highlightSystem = SudokuSceneRef.Resolve(highlightSystem);
        boardController = SudokuSceneRef.Resolve(boardController);
        mistakeSystem = SudokuSceneRef.Resolve(mistakeSystem);
        sessionContext = SudokuSceneRef.ResolveSession(sessionContext);
        hintSystem?.Configure(sessionContext);//Vincula el contexto compartido para que las pistas usen el mismo máximo en todas las escenas.
        mistakeSystem?.Configure(sessionContext);//Vincula el contexto compartido para que los errores usen el mismo máximo en todas las escenas.
        if (!hintsInitialized)//pregunta ¿Las pistas todavía no fueron inicializadas?
            //!hintsInitialized significa que hintsInitialized es false.
            //Esto existe porque hay dos formas de iniciar las pistas:}
            //1 Partida nueva: se deben reiniciar al máximo.
            //2 Partida cargada: se deben restaurar desde el guardado.
        {
            //Si todavía no estaban inicializadas:
            ResetHints();//Llama a ResetHints, que pone las pistas al máximo.
            //Pero si antes se llamó: SetRemainingHints(saved.remainingHints);
            //entonces hintsInitialized ya está en true, y Start no las sobrescribe.
            //Esto evita este problema: Cargas una partida con 1 pista restante, pero Start la vuelve a poner en 3.
        }
        UpdateNumberPanelButtons();//Actualiza los botones del panel numérico.
        //Esto revisa si algún número ya está completo en el tablero y desactiva su botón.
        //ejemplo Si ya están puestos todos los 5 correctamente, desactiva el botón 5.
    }
    public void SetRemainingHints(int hints)//Esta función asigna manualmente cuántas pistas quedan.
        //Se usa sobre todo al cargar una partida guardada.
        //recibe int hints que son La cantidad de pistas que quieres establecer.
    {
        remainingHints = Mathf.Max(0, hints);//Guarda las pistas, pero nunca permite que bajen de 0.
        //Mathf.Max(0, hints) devuelve el número más grande entre 0 y hints.
        hintsInitialized = true;//Marca que las pistas ya fueron inicializadas.
        //Esto evita que Start llame después a ResetHints y sobrescriba el valor.
        OnHintsChanged?.Invoke(remainingHints);//Dispara el evento de cambio de pistas.
        //El ?. significa: Si hay alguien escuchando este evento, avísale.
    }
    public void ResetHints()//Esta función reinicia las pistas al máximo.
        //Se usa cuando empieza una partida nueva o cuando reinicias nivel.
    {
        if (hintSystem != null)//Si existe hintSystem
            remainingHints = hintSystem.MaxHints;//toma el máximo de pistas desde ahí
        else
            remainingHints = 3;//Si hintSystem no está asignado, usa 3 como valor de emergencia.
        //fallback significa:valor de respaldo. Así el juego sigue funcionando aunque falte la referencia.
        hintsInitialized = true;//Marca que ya se inicializaron las pistas.
        OnHintsChanged?.Invoke(remainingHints);//Avisa a la UI que cambió la cantidad de pistas. Así el texto o icono de hints se actualiza.
    }
    public void SelectCell(SudokuCell cell)//Esta función se llama cuando el jugador hace clic en una celda.
        //Se conecta mediante el evento estático de SudokuCell
    {
        if (IsPaused()) return;//Primero revisa si el juego no está en estado Playing.
        //Si está pausado, en derrota, victoria o generando, no deja seleccionar. return detiene la funcion

        //si no esta pausado pasa aqui
        selectedCell = cell;//Guarda la celda seleccionada.
        //Luego, cuando el jugador presione un número, SetNumber usará esta celda.
        highlightSystem.SelectCell(cell);//Le avisa al sistema de resaltado que esta celda fue seleccionada.
        //Ese sistema puede pintar : la celda seleccionada , la fila , la columna , la caja , numeros iguales
        //en simple Selecciona la celda y actualiza los colores del tablero.
    }
    public void ToggleNotesMode()//Esta función activa o desactiva el modo notas.
    {
        noteMode = !noteMode;//Cambia el valor al contrario.
        //si estaba false pasa a true y si estaba true pasa a false
        highlightSystem.SetNotesMode(noteMode);//Le avisa al sistema de resaltado que el modo notas cambió.
        //Esto puede servir para cambiar visualmente el botón de notas o el estilo del tablero.
    }
    public void SetNumber(int number)//Esta es una de las funciones más importantes del input.
        //Se llama cuando el jugador presiona un número del panel.
        //recibe un int number : que puede ser del 1 al 9 que es colocar un numero o un 0 que es borrar un numero
    {
        if (IsPaused()) return;//Primero revisa si el juego está pausado o no está en estado Playing.
        //Si no se puede jugar, detiene la función.
        //Esto evita colocar números cuando estás en victoria, derrota, pausa o generación.
        if (selectedCell == null) return;//Si no hay una celda seleccionada, no puede colocar nada.
        //ejemplo : El jugador presionó el 5, pero no había elegido una casilla. Entonces sale.
        int index = SudokuRules.GetCellIndex(selectedCell.row, selectedCell.column);//Convierte fila y columna a índice lineal.
        //Ejemplo:
        //row = 2
        //column = 5
        //index = 2 * 9 + 5 = 23
        var data = boardController.boardData;//Obtiene los datos actuales del tablero.
        //ahi estan : values , fixedcells , notemask , solution
        if (data.fixedCells[index] || (data.hintCells != null && data.hintCells[index])) return;//Si la celda es fija o fue colocada por pista, no deja modificarla.
        //Una celda fija puede ser: 
        //1 una pista inicial
        //2 un número bloqueado porque ya se completaron todos los de ese valor
        if (noteMode)//Si el modo notas está activado, no coloca número grande.
        {
            boardController.ToggleNote(index, number);//Eso activa o desactiva la nota de ese número en la celda.
            return;//luego hace return para no seguir con la lógica de colocar número.
        }
        if (number != 0 && !boardController.IsCorrect(selectedCell.row, selectedCell.column, number))
        //Si no estás en modo notas, revisa si el número es correcto.
        //primero number != 0   Esto evita validar cuando el jugador quiere borrar. Si number es 0, significa borrar, no colocar.
        //luego !boardController.IsCorrect(...)    Pregunta si el número no coincide con la solución. Si el número es incorrecto, entra.
        {
            //MODO SIN VIDAS: se coloca el número igual (sin marcar en rojo ni contar errores).
            //Se permite escribir lo que sea; solo se gana al COMPLETAR el tablero (no avisa errores).
            if (PlayerPrefs.GetInt("Sudoku_LivesEnabled", 1) == 0)
            {
                boardView.SetCellError(selectedCell.row, selectedCell.column, false);
                var freeMove = new SudokuMove
                {
                    index = index,
                    oldValue = data.values[index],
                    newValue = number,
                    oldNotes = data.notesMask[index],
                    newNotes = 0
                };
                boardController.ApplyMove(freeMove);
                return;
            }
            boardView.SetCellError(selectedCell.row, selectedCell.column, true, number);//Muestra el número incorrecto en rojo en la celda.
            mistakeSystem?.RegisterMistake();//Registra un error.
            //El ?. significa: Si mistakeSystem existe, llama RegisterMistake.
            //Esto puede aumentar contador de errores y eventualmente causar derrota.
            //Como el número era incorrecto, no lo guarda en el tablero real.
            //Solo lo muestra como error visual y registra el fallo.
            return;//y regresa
        }
        //Si llega aquí, significa que el número es válido o es borrar.
        boardView.SetCellError(selectedCell.row, selectedCell.column, false);//Limpia cualquier error visual de esa celda.
        var move = new SudokuMove//Luego crea un movimiento:
        //SudokuMove guarda lo necesario para aplicar el cambio y poder hacer undo.
        {
            index = index,//La celda modificada.
            oldValue = data.values[index],//El valor anterior de la celda.
            newValue = number,//El nuevo valor que se quiere colocar.
            oldNotes = data.notesMask[index],//Las notas que tenía antes.
            newNotes = 0//Después de colocar un número, las notas quedan en cero.
        };
        boardController.ApplyMove(move);//finalmente Le pasa el movimiento al boardController.
        //El boardController es el que realmente modifica el tablero, limpia notas relacionadas, guarda undo y notifica cambios.
    }
    void OnEnable()//OnEnable es una función de Unity. Se ejecuta cuando el GameObject se activa. Aquí el script se suscribe a eventos.
    {
        NumberPanel.OnNumberPressed += SetNumber;//Escucha cuando se presiona un número.
        //Entonces cuando NumberPanel hace: OnNumberPressed?.Invoke(5); se llama a SetNumber(5);
        SudokuCell.OnCellClicked += SelectCell;//Escucha cuando se presiona una celda.
        //Entonces cuando una celda hace: OnCellClicked?.Invoke(this); se llama a SelectCell(cell);
        if (boardController != null)
            boardController.OnBoardChanged += OnBoardChanged;//Escucha cuando cambia el tablero.
        //cuando eso paso llama a OnBoardChanged(); Esto sirve para actualizar los botones del panel numérico.
    }
    void OnDisable()//OnDisable se ejecuta cuando el GameObject se desactiva. Aquí el script se desuscribe a eventos.
    {
        NumberPanel.OnNumberPressed -= SetNumber;//Deja de escuchar botones numéricos.
        SudokuCell.OnCellClicked -= SelectCell;//Deja de escuchar clics en celdas.
        if (boardController != null)
            boardController.OnBoardChanged -= OnBoardChanged;//Deja de escuchar cambios del tablero.
        //Esto evita eventos duplicados o llamadas a objetos desactivados.
    }
    void OnBoardChanged()//Esta función se llama cuando el tablero cambia.
    {
        UpdateNumberPanelButtons();//Eso revisa si algún número ya fue completado en todo el Sudoku.
        //Si un número está completo, desactiva su botón en NumberPanel.
        //ejemplo : Si ya están todos los 8 colocados correctamente, desactiva el botón 8 para que no se pueda usar más.
    }
    public void UseHint()//Esta función se ejecuta cuando el jugador usa una pista.
    {
        if (IsPaused()) return;//Si el juego no está en estado Playing, no permite usar pistas.
        //Por ejemplo, si estás en pausa, derrota o victoria, la función se detiene.
        if (remainingHints <= 0) return;//Revisa si todavía quedan pistas.
        //Si remainingHints es 0 o menor, no hace nada. Esto evita que el jugador use pistas infinitas.
        if (hintSystem.TryGetHint(out var hint))//Le pide una pista al hintSystem. TryGetHint intenta encontrar una pista lógica.
            //Si encuentra una, devuelve true y llena la variable: hint
            //Ese hint contiene información como: tecnica usada , celda a resaltar , acciones a aplicar
        {
            remainingHints--;//Resta una pista disponible.
            OnHintsChanged?.Invoke(remainingHints);//Avisa a otros scripts que cambió la cantidad de pistas.
            //Por ejemplo, SudokuGameFlowController escucha este evento y actualiza la UI: hintsUI?.UpdateHints(remainingHints);
            highlightSystem.ShowHint(hint);//Le manda el hint al sistema de resaltado.
            //Esto sirve para marcar visualmente las celdas importantes de la pista.
            if (hint.actions.Count > 0)//Si la pista trae acciones, las aplica directamente al tablero.
                //ejemplo de accion
                //place : coloca un número.
                //RemoveNotes : elimina notas/candidatos.
                // en simple Si la pista sabe qué hacer, el juego aplica esa ayuda automáticamente.
            {
                boardController.ApplyActions(hint.actions, true);//ejecuta la accion correspondiente y marca la jugada como pista.
            }
        }
    }
    bool IsPaused()//Esta función revisa si el input debe estar bloqueado.
                   //Devuelve true cuando no se debería permitir jugar.
    {
        return sessionContext == null || sessionContext.GameState != SudokuGameState.Playing;
        //sessionContext == null : Si no hay sessionContext, el script no sabe el estado del juego. 
        //Por seguridad, considera que está pausado/bloqueado.
        //sessionContext.GameState != SudokuGameState.Playing : Si el estado del juego no es Playing, también bloquea.
    }
    void UpdateNumberPanelButtons()//Esta función actualiza los botones del panel numérico.
        //su objetivo es Si un número ya fue completado correctamente en todo el Sudoku, bloquear ese número y desactivar su botón.
    {
        if (NumberPanel.Instance == null || boardController == null)
            //NumberPanel.Instance == null : primera validacion Si no existe el panel numérico
            //boardController == null : segunda valicion si no existe el controlador del tablero
            return;//si no existe uno o el otro entonces no hace nada y regresa

        //en caso de que si posea los elementos entra aqui
        var data = boardController.boardData;//Obtiene los datos actuales del tablero.
        if (data == null || data.solution == null)
            //data == null : primera validacion si no hay datos  
            //data.solution == null : segunda validacion si no hay solucion. Necesita solution para saber cuántos números correctos faltan.
            return;//si no existe una o el otro entonces se detiene y retorna

        //en caso de que los elementos de datos esten pasa aca
        for (int number = 1; number <= SudokuRules.MaxValue; number++)//Revisa cada número del Sudoku
        {
            bool allCorrect = true;//Asume que ese número está completo. Luego buscará si encuentra una celda donde falta.
            for (int i = 0; i < SudokuRules.CellCount; i++)//Recorre las celdas del tablero.
            {
                if (data.solution[i] == number && data.values[i] != number)
                //Esta condición significa Si en la solución esta celda debería tener este número, pero actualmente no lo tiene...
                //ejemplo
                //number = 5
                //data.solution[i] == 5
                //data.values[i] != 5
                //Significa: Esta celda debería ser 5, pero aún no está puesta como 5.
                {
                    allCorrect = false;//Marca que el número no está completo.
                    break;//break corta el ciclo porque ya no necesita seguir revisando.
                }
            }
            //en caso de que al revisar la celda
            if (allCorrect)//Si todas las apariciones de ese número están correctas
                boardController.LockCompletedNumber(number);//entonces activa el metodo
            //Esto marca esas celdas como fijas y limpia el undo hasta ese punto.
            //en simple Cuando completas todos los 5, esos 5 quedan bloqueados.

            NumberPanel.Instance.SetNumberButtonInteractable(number, !allCorrect);//Activa o desactiva el botón del número.
            //Si allCorrect es true, entonces: !allCorrect = false  El botón queda desactivado.
            //Si allCorrect es false, entonces: !allCorrect = true  El botón queda activo.
        }
    }
    public void ClearSelection()//Esta función limpia la celda seleccionada.
    {
        selectedCell = null;//Ya no hay ninguna celda seleccionada.
        //Sirve cuando quieres evitar que el jugador siga colocando números en una celda vieja,
        //por ejemplo al cambiar de estado, cerrar paneles o reiniciar algo.
    }
}
