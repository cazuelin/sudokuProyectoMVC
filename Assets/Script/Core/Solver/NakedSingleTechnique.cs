using System.Collections.Generic;

public class NakedSingleTechnique : ISudokuTechnique
{
    public bool TryApply(int[,] board, out int row, out int col, out int value)
    {
        for (row = 0; row < 9; row++)
        {
            for (col = 0; col < 9; col++)
            {
                if (board[row, col] != 0) continue;

                var candidates = GetCandidates(board, row, col);

                if (candidates.Count == 1)
                {
                    value = candidates[0];
                    return true;
                }
            }
        }

        row = col = value = -1;
        return false;
    }

    List<int> GetCandidates(int[,] board, int row, int col)
    {
        List<int> list = new List<int>();

        for (int num = 1; num <= 9; num++)
        {
            if (IsValid(board, row, col, num))
                list.Add(num);
        }

        return list;
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
        return "Único posible";
    }
}