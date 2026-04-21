using System.Collections.Generic;
[System.Serializable]
public class SudokuSaveData
{
    public SudokuBoardData board;
    public SudokuBoardData initialBoard;
    public float time;
    public int difficulty;
    public List<SudokuMove> undoStack;
    public List<SudokuMove> redoStack;
    public int mistakes;
    public int[] previewValues;
}