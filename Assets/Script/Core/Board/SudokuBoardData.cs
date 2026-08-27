[System.Serializable]
public class SudokuBoardData
{
    public SudokuBoardData()
    {
        int size = SudokuRules.CellCount;
        values = new int[size];
        solution = new int[size];
        fixedCells = new bool[size];
        hintCells = new bool[size];
        notesMask = new int[size];
    }

    public int[] values;
    public int[] solution;
    public bool[] fixedCells;
    public bool[] hintCells;
    public int[] notesMask;
    public float time;
    public int difficulty;

    public SudokuBoardData Clone()
    {
        return new SudokuBoardData
        {
            values = (int[])values.Clone(),
            solution = (int[])solution.Clone(),
            fixedCells = (bool[])fixedCells.Clone(),
            hintCells = hintCells != null ? (bool[])hintCells.Clone() : null,
            notesMask = (int[])notesMask.Clone(),
            time = time,
            difficulty = difficulty
        };
    }
}
