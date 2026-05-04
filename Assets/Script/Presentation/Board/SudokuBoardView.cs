using UnityEngine;
public class SudokuBoardView : MonoBehaviour
{
    [SerializeField] GameObject cellPrefab;
    [SerializeField] Transform[] boxes;
    SudokuCell[,] cells = new SudokuCell[9, 9];
    const int SIZE = 9;
    public void CreateBoard()
    {
        for (int r = 0; r < SIZE; r++)
            for (int c = 0; c < SIZE; c++)
            {
                int boxIndex = (r / 3) * 3 + (c / 3);
                var obj = Instantiate(cellPrefab, boxes[boxIndex]);
                var cell = obj.GetComponent<SudokuCell>();
                cell.row = r;
                cell.column = c;
                cells[r, c] = cell;
            }
    }
    public void UpdateBoard(int[] values, bool[] fixedCells, int[] notesMask)
    {
        for (int i = 0; i < 81; i++)
        {
            int r = i / SIZE;
            int c = i % SIZE;
            cells[r, c].Render(
                values[i],
                fixedCells[i],
                notesMask[i]
            );
        }
    }
    public SudokuCell[,] GetCells() => cells;

    public void SetCellError(int row, int col, bool active)
    {
        if (cells[row, col] != null)
            cells[row, col].SetError(active);
    }

    public void ClearAllErrors()
    {
        for (int r = 0; r < SIZE; r++)
            for (int c = 0; c < SIZE; c++)
                cells[r, c]?.SetError(false);
    }
}