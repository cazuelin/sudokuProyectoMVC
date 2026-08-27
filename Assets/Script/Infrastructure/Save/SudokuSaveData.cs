using System.Collections.Generic;
[System.Serializable]
public class SudokuSaveData//es, en esencia, el “paquete” que junta todo el estado necesario para reconstruir una partida de Sudoku guardada.
//No tiene lógica propia: solo guarda datos para luego convertirlos a JSON y recuperarlos después.
{
    public SudokuBoardData board;//Guarda el estado actual completo del tablero.
    //Según SudokuBoardData, aquí entran: values , solution , fixedCells , notesMask , time , difficulty. Es la parte principal de la partida.
    public SudokuBoardData initialBoard;//Guarda el tablero inicial, es decir, cómo estaba el Sudoku cuando empezó la partida.
    //Sirve para restaurar las pistas originales y distinguir qué celdas eran fijas desde el inicio.
    public float time;//Guarda el tiempo transcurrido de la partida. Normalmente se usa para reanudar el cronómetro exactamente donde quedó.
    public int difficulty;//Guarda la dificultad actual.
    //En SudokuSaveManager, se guarda como int usando: difficulty = (int)sessionContext.SelectedDifficulty
    //Eso significa que el enum se convierte a número para poder serializarlo fácil.
    public List<SudokuMove> undoStack;//Guarda el historial de movimientos para soportar Undo.
    //Cada SudokuMove contiene: indice de celda , valor anterior , valor nuevo , notas anteriores , notas nuevas
    public int undoBarrierIndex;//Guarda desde qué punto el undo puede retroceder después de una pista.
    public int mistakes;//Guarda cuántos errores lleva el jugador. Se usa para restaurar el estado del sistema de errores al cargar.
    public int[] previewValues;//Guarda una copia de los valores del tablero en formato de arreglo.
    //En SaveGame, se llena así: previewValues = (int[])board.boardData.values.Clone()
    //O sea, se hace una copia independiente del arreglo actual. 
    public int remainingHints;//Guarda cuántas pistas le quedan al jugador.
    //Permite que al volver a cargar la partida no se pierdan las pistas disponibles.
}
