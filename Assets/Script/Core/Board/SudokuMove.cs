public struct SudokuMove
{
    public int index;//celda que se modifica ya que el tablero guarda 81 array
    public int oldValue;//valor que tenia la celda antes del movimiento
    public int newValue;//valor nuevo que puso el jugador
    public int oldNotes;//notas que tenia la celda antes del movimiento
    public int newNotes;//notas que quedan despues del movimiento
}