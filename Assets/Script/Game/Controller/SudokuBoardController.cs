using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
public class SudokuBoardController : MonoBehaviour
{
    public SudokuBoardData boardData { get; private set; }//guarda el tablero actual
    [Header("Undo")]
    [SerializeField] int maxUndoSteps = 20;//Cantidad máxima de movimientos que se guardan para undo.
    SudokuBitMask bitMask = new SudokuBitMask();//Usa SudokuBitMask para validar números rápidamente.
    readonly List<SudokuMove> undoStack = new List<SudokuMove>();//Historial de movimientos del jugador.
    int undoBarrierIndex;//Marca hasta dónde no se puede deshacer después de una pista.
    public List<SudokuMove> GetUndoStack() => new List<SudokuMove>(undoStack);//Devuelve una copia del historial de undo. Se usa para guardar la partida.
    public int GetUndoBarrierIndex() => undoBarrierIndex;
    public void SetUndoBarrierIndex(int barrierIndex)
    {
        undoBarrierIndex = Mathf.Clamp(barrierIndex, 0, undoStack.Count);
    }
    int SIZE => SudokuRules.Size;
    public void SetUndoRedo(List<SudokuMove> undo)//Carga el historial de undo desde una partida guardada.
    {
        undoStack.Clear();//limpia la variable undostack
        if (undo != null)//verifica si la variable undo viene con datos guardados
            undoStack.AddRange(undo);//copia todos los movimientos guardados en el json a undostack
        TrimUndoStack();
    }
    void EnsureBoardDataArrays()
    {
        if (boardData == null)
            return;

        int size = SudokuRules.CellCount;
        if (boardData.values == null || boardData.values.Length != size)
            boardData.values = new int[size];
        if (boardData.solution == null || boardData.solution.Length != size)
            boardData.solution = new int[size];
        if (boardData.fixedCells == null || boardData.fixedCells.Length != size)
            boardData.fixedCells = new bool[size];
        if (boardData.hintCells == null || boardData.hintCells.Length != size)
            boardData.hintCells = new bool[size];
        if (boardData.notesMask == null || boardData.notesMask.Length != size)
            boardData.notesMask = new int[size];
    }
    SudokuBoardData initialData;//Guarda el tablero inicial para poder reiniciar.
    public event System.Action OnBoardChanged;//Evento que avisa cuando el tablero cambia. Otros scripts lo escuchan para actualizar UI, guardar, revisar victoria, etc.
    void PlaceNumber(int index, int number, bool notifyBoardChanged = true)
    {
        if (boardData.fixedCells[index])//verifica si el numero que se quiere remplazar no tiene la variable de fixedcell
            return;//si el numero es fixedcell entonces retorna y no hace nada
        //ejemplo
        //si el row es = 2 y la column es = 5
        //entonces el index se calcula = row * 9 + column que seria = 2 * 9 + 5 = 23 que seria el index
        int r = SudokuRules.GetRow(index);
        //entonces r = index /SIZE seria 2 que seria la row 2
        int c = SudokuRules.GetCol(index);
        //entonces c = index % 9 seria 23 / 9 = 2 ,y de 18 a 23 sobran 5 entonces la column seria 5
        int old = boardData.values[index];//Guarda el valor anterior de la celda.
        if (old != 0)//Si la celda tenía un número anterior
            bitMask.Remove(r, c, old); //lo quita del bitMask. ejemplo si antes la celda tenia 4 y vas a poner 7 primero se remueve el 4 
        boardData.values[index] = number; //Ahora cambia el valor de la celda. si la funcion es bordaData.values[index] = number; entonces quedaria values[23] = 7
        if (number != 0)//Solo hace lo siguiente si el número nuevo no es cero.
        {
            bitMask.Place(r, c, number);//Registra el nuevo número en el bitMask.
            RemoveNotesFromPeers(r, c, number);//Borra ese número de las notas de las celdas relacionadas.
            boardData.notesMask[index] = 0;//Limpia las notas de la celda donde colocaste el número.
        }
        if (notifyBoardChanged)
            NotifyBoardChanged();//Avisa al resto del juego:
    }
    bool CanPlaceNumber(int r, int c, int number)
    {
        int index = r * SIZE + c;//Convierte fila y columna a índice.
        //ejemplo r = 2; c = 5; SIZE = 9; index = 2 * 9 + 5 = 23
        int current = boardData.values[index];//Obtiene el valor actual de esa celda. ejemplo = values[23] = 7
        if (current != 0)//Si la celda ya tiene un número
            bitMask.Remove(r, c, current);//lo quita temporalmente del bitMask
        bool canPlace = bitMask.CanPlace(r, c, number);//Pregunta al bitMask si el número está libre en: fila,columna,caja, Si no aparece en ninguna, devuelve true.
        if (current != 0)//Si la celda ya tiene un número
            bitMask.Place(r, c, current);//Si quitó un número temporalmente, lo vuelve a poner. Esto deja el bitMask como estaba antes de la consulta.
        return canPlace;//Devuelve el resultado.
    }
    public bool IsCorrect(int r, int c, int value)
    {
        return boardData.solution[r * SIZE + c] == value;//verifica si el numero es correcto segun la solucion
    }
    public bool CheckWin()//Esta función revisa si el jugador ganó.
    {
        //MODO SIN VIDAS: se gana al COMPLETAR el tablero (todas las celdas llenas), aunque haya
        //números equivocados el juego NO avisa errores: solo pide completar la cuadrícula.
        if (PlayerPrefs.GetInt("Sudoku_LivesEnabled", 1) == 0)
        {
            for (int i = 0; i < SudokuRules.CellCount; i++)
                if (boardData.values[i] == 0)
                    return false;//Aún quedan celdas vacías: no se ha completado.
            return true;//Tablero completo: se gana (aunque algunos números estén mal).
        }
        for (int i = 0; i < SudokuRules.CellCount; i++)//Recorre las celdas del Sudoku.
            if (boardData.values[i] == 0 || boardData.values[i] != boardData.solution[i])
                //boardData.values[i] == 0 Pregunta si la celda está vacía. En tu juego, 0 significa vacío.
                //boardData.values[i] != boardData.solution[i] = Pregunta si el valor actual es diferente de la solución.
                return false;//si alguna celda está vacía o alguna celda no coincide con la solución,todavía no ganó
        return true;//Si termina de revisar todas las celdas y ninguna falló: Entonces el tablero está completo y correcto.
    }
    public SudokuBoardData GetBoardData()
    {
        return boardData;//Devuelve el tablero actual.Sirve para que otros scripts puedan pedir los datos del tablero.
    }
    public void SetBoardData(SudokuBoardData data)//Esta función carga un tablero nuevo en el controller.
    {
        if (data == null)//Si no llega ningún tablero
            return;//entonces sale de la función.
        boardData = data.Clone();//Copia el tablero recibido.Esto es importante porque evita modificar accidentalmente el objeto original.
        EnsureBoardDataArrays();
        undoBarrierIndex = 0;
        InitBitMask();//Reconstruye el bitMask.
        //Porque cuando cargas un tablero, el bitMask necesita saber qué números ya existen en filas, columnas y cajas.
    }

    public bool LockCompletedNumber(int number)//este metodo bloquea todos los numeros relaciones en caso de que se completen los 9 numeros iguales en el tablero
    {
        if (boardData == null || number < 1 || number > SudokuRules.MaxValue)//pregunta primero si existe tablero y luego si el número está entre 1 y el máximo del tablero
            return false;//si no es ninguna devuelve false
        for (int i = 0; i < SudokuRules.CellCount; i++)//recorre las celdas del sudoku
        {
            if (boardData.solution[i] == number && boardData.values[i] != number)
                //boardData.solution[i] == number : primero pregunta todas las posiciones donde la solución sea igual al number que se recibe
                //ejemplo busca todas las posiciones donde la solucion sea el numero 5 en el tablero
                //boardData.values[i] != number : luego si alguna de esa posiciones aun no tiene el number recibido
                //ejemplo si alguna de las posiciones aun no tiene el numero 5 en values
                return false;//si falta alguna de las 2 condiciones retorna false
        }
        bool changed = false;//Guarda si realmente se cambió alguna celda.
        for (int i = 0; i < SudokuRules.CellCount; i++)//recorre las celdas del sudoku
        {
            if (boardData.values[i] == number && !boardData.fixedCells[i])
            //boardData.values[i] == number : busca celdas que tengan ese numero
            //!boardData.fixedCells[i] : y que todavia no sea fijas
            {
                boardData.fixedCells[i] = true;//Convierte esa celda en fija. Desde ahora no se puede modificar.
                boardData.notesMask[i] = 0;//Limpia las notas de esa celda.
                changed = true;//Marca que sí hubo un cambio.
            }
        }
        if (!changed)//Si no encontró nada que bloquear
            return false;//devuelve false.
        undoStack.Clear();//Limpia el historial de undo.Esto hace que el jugador no pueda volver atrás antes del momento en que se completó ese número.
        undoBarrierIndex = 0;
        NotifyBoardChanged();//Avisa que el tablero cambió
        return true;//evuelve true.
    }
    public void ToggleNote(int index, int number)//Activa o desactiva una nota en una celda.
    {
        if (boardData.fixedCells[index]) return;//Si la celda es fija, no permite editar notas.
        NotesUtil.Toggle(ref boardData.notesMask[index], number);//Activa o desactiva la nota. ref significa que modifica directamente boardData.notesMask[index].
        NotifyBoardChanged();//Avisa que el tablero cambió para que la vista se actualice.
    }
    public void AutoFillNotes()//Esta función llena automáticamente las notas posibles de todas las celdas vacías.
    {
        for (int r = 0; r < SIZE; r++)//Recorre filas.
        {
            for (int c = 0; c < SIZE; c++)//Recorre columnas.
            {
                int index = SudokuRules.GetCellIndex(r, c);//Convierte fila y columna a índice.
                //ejemplo r = 2; c = 5; index = 2 * 9 + 5 = 23
                if (boardData.values[index] != 0)//Si la celda ya tiene un número, la salta.
                    continue;//continue significa no sigas con esta celda, pasa a la siguiente
                boardData.notesMask[index] = 0;//Limpia las notas antiguas de esa celda.
                for (int n = 1; n <= SudokuRules.MaxValue; n++)//Prueba cada número del 1 al tamaño del tablero.
                {
                    if (CanPlaceNumber(r, c, n))//Pregunta si el número n puede ir en esa celda según reglas del Sudoku.
                    {
                        boardData.notesMask[index] |= (1 << (n - 1));//Si puede ir, agrega ese número como nota.
                        //  1 << (n - 1) crea el bit del número.
                        //  |= enciende ese bit.
                    }
                }
            }
        }
        NotifyBoardChanged();//Al final avisa que las notas cambiaron.
    }
    void RemoveNotesFromPeers(int row, int col, int number)//Esta función elimina una nota de todas las celdas relacionadas con una celda.
        //ejemplo si coloco un 5 entonces ese 5 ya no deberia aparecer como nota en la misma fila en la misma columna y la misma caja
    {
        int mask = ~(1 << (number - 1));//Esta línea crea una máscara para apagar la nota del número.
        //ejemplo si number es 5 entonces se veria asi : 1 << (5 - 1) y en bits asi : 000010000 y el caracter ~ invierte los bits osea lo pasa a 0 y lo desactiva
        for (int i = 0; i < SIZE; i++)//Recorre todas las celdas de una unidad.
        //Ese mismo i sirve para recorrer: todas las columnas de una fila y todas las filas de una columna
        {
            boardData.notesMask[SudokuRules.GetCellIndex(row, i)] &= mask;//Borra esa nota de toda la fila.
            //ejemplo si row es 2 entonces i empieza a recorrer las columnas seria row 2,columna 0,row 2,columna 1,row 2,columna 3......y asi hasta recorrer todas
            boardData.notesMask[SudokuRules.GetCellIndex(i, col)] &= mask;//Borra esa nota de toda la columna.
            //ejemplo si col es 5 entonces i empieza a recorrer las las filas seria fila 0,columna 5,fila 1,columna 5,fila 2,columna 5....... y asi hasta recorrer todas 
        }
        int startRow = (row / SudokuRules.BoxRows) * SudokuRules.BoxRows;//Calcula dónde empieza la caja en la fila.
        int startCol = (col / SudokuRules.BoxCols) * SudokuRules.BoxCols;//Calcula dónde empieza la caja en la columna.
        for (int r = 0; r < SudokuRules.BoxRows; r++)//Recorre las celdas de esa caja en la fila.
            for (int c = 0; c < SudokuRules.BoxCols; c++)//Recorre las celdas de esa caja en la columna.
            {
                int index = SudokuRules.GetCellIndex(startRow + r, startCol + c);//Convierte cada celda de esa caja a índice.
                //revisamos la fila 3+0 y revisamos sus 3 columnas
                //ejemplo index = (3 + 0) * 9 + (6 + 0); index = 3 * 9 + 6 y luego 9 * 3 = 27 + 6 = 33
                //ejemplo index = (3 + 0) * 9 + (6 + 1); index = 3 * 9 + 7 y luego 9 * 3 = 27 + 7 = 34
                //ejemplo index = (3 + 0) * 9 + (6 + 2); index = 3 * 9 + 8 y luego 9 * 3 = 27 + 8 = 35

                //revisamos la fila 3+1 y revisamos sus 3 columnas
                //ejemplo index = (3 + 1) * 9 + (6 + 0); index = 4 * 9 + 6 y luego 9 * 4 = 36 + 6 = 42
                //ejemplo index = (3 + 1) * 9 + (6 + 1); index = 4 * 9 + 7 y luego 9 * 4 = 36 + 7 = 43
                //ejemplo index = (3 + 1) * 9 + (6 + 2); index = 4 * 9 + 8 y luego 9 * 5 = 36 + 8 = 44

                //revisamos la fila 3+2 y revisamos sus 3 columnas
                //ejemplo index = (3 + 2) * 9 + (6 + 0); index = 5 * 9 + 6 y luego 9 * 5 = 45 + 6 = 51
                //ejemplo index = (3 + 2) * 9 + (6 + 1); index = 5 * 9 + 7 y luego 9 * 5 = 45 + 7 = 52
                //ejemplo index = (3 + 2) * 9 + (6 + 2); index = 5 * 9 + 8 y luego 9 * 5 = 45 + 8 = 53
                boardData.notesMask[index] &= mask;//Borra esa nota segun su indice de la celda.
            }
    }
    public void ApplyMove(SudokuMove move)//Esta función aplica una jugada del jugador.A diferencia de PlaceNumber, esta sí guarda el movimiento para Undo.
    {
        if (boardData.fixedCells[move.index])//Si la celda es fija, no permite modificarla.
            return;//regresa sin hacer nada si la celda es fija
        int r = SudokuRules.GetRow(move.index);//Convierte el índice de la celda a fila.
        //ejemplo si el index es 23 entonces r = 23 / 9 = 2
        int c = SudokuRules.GetCol(move.index);//Convierte el índice de la celda a columna.
        //ejemplo si el index es 23 entonces r = 23 / 9 = 2 y el resto es si 9 * 2 = 18 entonces de 23 - 18 = 5
        int old = boardData.values[move.index];//Obtiene el valor anterior de la celda.
        if (old != 0)//Si había un valor anterior, lo quita del bitMask.Esto mantiene actualizada la memoria rápida de filas, columnas y cajas.
            bitMask.Remove(r, c, old);//remueve el valor del bitmask si habia un valor antes
        boardData.values[move.index] = move.newValue;//Coloca el nuevo valor.
        boardData.notesMask[move.index] = move.newNotes;//Cambia las notas de la celda al nuevo estado.porque una celda con número no necesita notas.
        if (move.newValue != 0)//Solo si el nuevo valor no es vacío.
        {
            bitMask.Place(r, c, move.newValue);//Agrega el nuevo número al bitMask.
            RemoveNotesFromPeers(r, c, move.newValue);//Borra ese número como nota de las celdas relacionadas.
        }
        AddUndoMove(move);//Guarda este movimiento para poder deshacerlo.
        NotifyBoardChanged();//Avisa al resto del juego que el tablero cambió.
    }
    void AddUndoMove(SudokuMove move)//Esta función guarda un movimiento en la lista de undo.
    {
        undoStack.Add(move);//Agrega el movimiento al final de la lista.
        TrimUndoStack();//Después de agregar, revisa si la lista está muy larga.
    }
    void TrimUndoStack()
    {
        while (undoStack.Count > maxUndoSteps)//Mientras haya más movimientos que el máximo permitido, sigue borrando.
            undoStack.RemoveAt(0);//Borra el primer movimiento de la lista.
        //ejemplo El índice 0 es el movimiento más antiguo. [move1, move2, move3, ..., move21] Después de borrar índice 0: [move2, move3, ..., move21]
    }
    public void Undo()//Esta función deshace el último movimiento posible.
    {
        while (undoStack.Count > undoBarrierIndex)//Mientras existan movimientos por encima de la barrera de pista.
        {
            var move = undoStack[undoStack.Count - 1];//Toma el último movimiento
            //ejemplo undoStack = [move1, move2, move3] entonces toma el move3 ya que es el ultimo
            undoStack.RemoveAt(undoStack.Count - 1);//Quita ese movimiento de la lista.

            if (boardData.fixedCells[move.index])//Si la celda ahora es fija, no la deshace.
                continue;//cuando completas todos los números de un tipo, esas celdas quedan fijas y el undo no debe volver atrás

            int r = SudokuRules.GetRow(move.index);//Convierte índice a fila.
            //ejemplo si el index es 23 entonces r = 23 / 9 = 2
            int c = SudokuRules.GetCol(move.index);//Convierte índice a columna.
            //ejemplo si el index es 23 entonces r = 23 / 9 = 2 y el resto es si 9 * 2 = 18 entonces de 23 - 18 = 5
            int current = boardData.values[move.index];//Obtiene el valor actual de la celda.
            if (current != 0)//verifica si hay un numero en esa celda
                bitMask.Remove(r, c, current);//si hay un numero lo Quita del bitMask el valor actual.
            boardData.values[move.index] = move.oldValue;//Restaura el valor anterior.
            boardData.notesMask[move.index] = move.oldNotes;//Restaura las notas anteriores.
            if (move.oldValue != 0)//Si el valor anterior no era vacío, 
                bitMask.Place(r, c, move.oldValue);//lo vuelve a poner en el bitMask.
            NotifyBoardChanged();//Avisa que el tablero cambió.
            return;//Termina la función después de deshacer un movimiento.
        }
    }
    public void NotifyBoardChanged()//Esta función avisa que el tablero cambió.
    {
        OnBoardChanged?.Invoke();//Un evento es como una alarma. Otros scripts pueden “escuchar” esa alarma.
        //El ?. evita error si nadie está escuchando.
    }
    public void ResetBoard()//Esta función reinicia el tablero al estado inicial de la partida.Se usa cuando presionas reiniciar partida.
    {
        if (initialData == null) return;//Primero revisa si existe un tablero inicial guardado.initialData se guarda antes con: SetInitialState(data);
        //Si initialData es null, significa:no tengo un tablero inicial al que volver.Entonces hace return, o sea, sale de la función.
        boardData = initialData.Clone();//Restaura el tablero actual usando una copia del tablero inicial.
        //¿Por qué usa Clone()? .Porque si hiciera:boardData = initialData;ambos apuntarían al mismo objeto. Entonces, si luego modificas boardData, también modificarías initialData.
        //Con Clone():boardData recibe una copia independiente.initialData queda intacto.Esto permite reiniciar varias veces sin destruir el estado original.
        EnsureBoardDataArrays();
        InitBitMask();//Reconstruye el bitMask.Cuando reemplazas boardData, el bitMask viejo ya no sirve, porque tenía registrados los números del tablero anterior.
        //Entonces InitBitMask() recorre el tablero restaurado y vuelve a registrar todos los números existentes en filas, columnas y cajas.
        undoStack.Clear();//Limpia el historial de undo.Tiene sentido porque reiniciar partida debería borrar los movimientos anteriores.
        undoBarrierIndex = 0;
        NotifyBoardChanged();//Avisa a los demás scripts que el tablero cambió.
    }
    public void SetInitialState(SudokuBoardData data)//Esta función guarda el estado inicial del tablero.El tablero inicial es el puzzle apenas empezó la partida:
    {
        if (data == null)//Si no llegó ningún dato, sale.
            return;

        initialData = data.Clone();//Guarda una copia.
    }
    public SudokuBoardData GetInitialData()//Esta función devuelve el tablero inicial guardado.No modifica nada. Solo entrega el valor de initialData.
    {
        return initialData;
    }
    void InitBitMask()//Esta función reconstruye el SudokuBitMask usando el tablero actual.se usa cuando cargas o reinicias un tablero:
    {
        bitMask.Clear();//Limpia todo lo que sabía antes.
        for (int i = 0; i < SudokuRules.CellCount; i++)//Recorre todas las celdas.
        {
            int val = boardData.values[i];//Obtiene el valor de esa celda.
            if (val != 0)//Si tiene número:
            {
                int r = SudokuRules.GetRow(i);//Convierte índice a fila y columna.
                //ejemplo si el index es 23 entonces r = 23 / 9 = 2
                int c = SudokuRules.GetCol(i);//Convierte índice a fila y columna.
                //ejemplo si el index es 23 entonces r = 23 / 9 = 2 y el resto es si 9 * 2 = 18 entonces de 23 - 18 = 5
                bitMask.Place(r, c, val);//Registra ese número en:su fila en su columna y en su caja 3x3
            }
        }
    }
    public void ApplyActions(List<SudokuAction> actions, bool fromHint = false)//Esta función aplica acciones generadas por pistas o técnicas del solver.
    {
        foreach (var action in actions)//Primero recorre todas las actions de la lista que pueden ser Place o RemoveNotes para Luego pregunta el tipo.
        {
            if (action.type == SudokuActionType.Place)//si es de tipo place
            {
                PlaceNumber(action.index, action.value, false);//Coloca el número sin guardar undo. Las pistas no deben entrar al historial.
                if (fromHint && boardData != null)
                {
                    boardData.fixedCells[action.index] = true;
                    boardData.hintCells[action.index] = true;
                    boardData.notesMask[action.index] = 0;
                }
            }
            else if (action.type == SudokuActionType.RemoveNotes)//si es de tipo RemoveNotes
            {
                boardData.notesMask[action.index] &= ~action.mask;//Esto apaga los bits indicados por action.mask.
            }
        }
        if (fromHint)
            undoBarrierIndex = undoStack.Count;
        NotifyBoardChanged();//Avisa que el tablero cambió.
    }
}
