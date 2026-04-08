using System.Collections.Generic;

[System.Serializable]
public class SudokuSaveData
{
    public SudokuBoardData board;

    public float time;
    public int difficulty;
    public int mistakes;

    public List<SudokuMove> undoStack;
    public List<SudokuMove> redoStack;

    public int[] previewValues;
}