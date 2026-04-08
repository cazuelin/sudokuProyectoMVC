public class HiddenSingleTechnique : ISudokuTechnique
{
    public bool TryApply(int[,] board, out int row, out int col, out int value)
    {
        // Revisar filas
        for (row = 0; row < 9; row++)
        {
            for (int num = 1; num <= 9; num++)
            {
                int count = 0;
                int lastCol = -1;

                for (int c = 0; c < 9; c++)
                {
                    if (board[row, c] == 0 && IsValid(board, row, c, num))
                    {
                        count++;
                        lastCol = c;
                    }
                }

                if (count == 1)
                {
                    col = lastCol;
                    value = num;
                    return true;
                }
            }
        }

        // Revisar columnas
        for (col = 0; col < 9; col++)
        {
            for (int num = 1; num <= 9; num++)
            {
                int count = 0;
                int lastRow = -1;

                for (int r = 0; r < 9; r++)
                {
                    if (board[r, col] == 0 && IsValid(board, r, col, num))
                    {
                        count++;
                        lastRow = r;
                    }
                }

                if (count == 1)
                {
                    row = lastRow;
                    value = num;
                    return true;
                }
            }
        }

        row = col = value = -1;
        return false;
    }

    bool IsValid(int[,] board, int row, int col, int num)
    {
        for (int i = 0; i < 9; i++)
        {
            if (board[row, i] == num) return false;
            if (board[i, col] == num) return false;
        }

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;

        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                if (board[startRow + r, startCol + c] == num)
                    return false;

        return true;
    }
    public string GetName()
    {
        return "Solo obvio";
    }
}