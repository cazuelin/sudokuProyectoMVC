using System.Collections.Generic;
public class SudokuHint
{
    public string technique;//Guarda el nombre de la técnica que generó la pista.
    public List<int> highlightCells = new();//Lista de celdas que deben resaltarse como parte principal de la pista.
    //Cada int es un índice de celda del 0 al 80.
    //Ejemplo: highlightCells = [23] entonces significa Resalta la celda 23.
    public List<int> affectedCells = new();//Lista de celdas afectadas por la pista.
    //Normalmente se usa cuando una técnica elimina candidatos/notas.
    public int candidateMask;//Guarda el candidato involucrado usando bits.
    //Por ejemplo, si la pista trata sobre el número 5, el mask sería: 1 << (5 - 1) en bits seria 000010000
    public List<SudokuAction> actions = new();//Lista de acciones que deben aplicarse al tablero.
}