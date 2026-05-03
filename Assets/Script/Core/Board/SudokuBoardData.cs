[System.Serializable]
public class SudokuBoardData
{
    public int[] values = new int[81];
    public int[] solution = new int[81];
    public bool[] fixedCells = new bool[81];
    public int[] notesMask = new int[81];
    public float time;
    public int difficulty;

    public SudokuBoardData Clone()
    {
        return new SudokuBoardData
        {
            values = (int[])values.Clone(),
            solution = (int[])solution.Clone(),
            fixedCells = (bool[])fixedCells.Clone(),
            notesMask = (int[])notesMask.Clone(),
            time = time,
            difficulty = difficulty
        };
    }
}
