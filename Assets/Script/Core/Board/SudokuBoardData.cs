
using System.Collections.Generic;

[System.Serializable]
public class SudokuBoardData
{
    public int[] values = new int[81];
    public int[] solution = new int[81];
    public bool[] fixedCells = new bool[81];
    public int[] notesMask = new int[81];
    public float time;
    public int difficulty;
}
