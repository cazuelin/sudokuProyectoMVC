public class SudokuBitMaskSolver
{
    int[] rows = new int[9];
    int[] cols = new int[9];
    int[] boxes = new int[9];

    public void Init(int[,] board)
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                int num = board[r, c];
                if (num != 0)
                    Place(r, c, num);
            }
    }

    public bool CanPlace(int row, int col, int num)
    {
        int mask = 1 << num;
        int box = (row / 3) * 3 + col / 3;

        return (rows[row] & mask) == 0 &&
               (cols[col] & mask) == 0 &&
               (boxes[box] & mask) == 0;
    }

    public void Place(int row, int col, int num)
    {
        int mask = 1 << num;
        int box = (row / 3) * 3 + col / 3;

        rows[row] |= mask;
        cols[col] |= mask;
        boxes[box] |= mask;
    }

    public void Remove(int row, int col, int num)
    {
        int mask = ~(1 << num);
        int box = (row / 3) * 3 + col / 3;

        rows[row] &= mask;
        cols[col] &= mask;
        boxes[box] &= mask;
    }
}
