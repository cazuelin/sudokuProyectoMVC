public class SudokuSolver
{
    SudokuBitMaskSolver bitMask = new SudokuBitMaskSolver();
    public int CountSteps(int[,] board, int maxSteps = 2000)
    {
        bitMask.Init(board);
        int steps = 0;
        Solve(board, ref steps, maxSteps);
        return steps == 0 ? maxSteps : steps;
    }
    bool Solve(int[,] board, ref int steps, int maxSteps)
    {
        if (steps >= maxSteps)
            return true;

        if (!FindBestEmptyCell(board, out int row, out int col, out int candidateMask))
            return row == -1;

        int num = 1;
        int mask = candidateMask;
        while (mask != 0)
        {
            if ((mask & 1) != 0)
            {
                if (bitMask.CanPlace(row, col, num))
                {
                    board[row, col] = num;
                    bitMask.Place(row, col, num);
                    steps++;
                    if (Solve(board, ref steps, maxSteps))
                        return true;
                    board[row, col] = 0;
                    bitMask.Remove(row, col, num);
                }
            }
            mask >>= 1;
            num++;
        }
        return false;
    }
    public int CountSolutions(int[,] board)
    {
        bitMask.Init(board);
        int count = 0;
        SolveCount(board, ref count, 2);
        return count;
    }
    bool SolveCount(int[,] board, ref int count, int maxSolutions)
    {
        if (!FindBestEmptyCell(board, out int row, out int col, out int candidateMask))
        {
            if (row != -1)
                return false;
            count++;
            return count >= maxSolutions;
        }

        int num = 1;
        int mask = candidateMask;
        while (mask != 0)
        {
            if ((mask & 1) != 0)
            {
                if (bitMask.CanPlace(row, col, num))
                {
                    board[row, col] = num;
                    bitMask.Place(row, col, num);
                    if (SolveCount(board, ref count, maxSolutions))
                    {
                        board[row, col] = 0;
                        bitMask.Remove(row, col, num);
                        return true;
                    }
                    board[row, col] = 0;
                    bitMask.Remove(row, col, num);
                }
            }
            mask >>= 1;
            num++;
        }
        return false;
    }

    bool FindBestEmptyCell(int[,] board, out int bestRow, out int bestCol, out int candidateMask)
    {
        bestRow = -1;
        bestCol = -1;
        candidateMask = 0;
        int bestCount = 10;

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board[row, col] != 0)
                    continue;

                int mask = BuildCandidateMask(row, col);
                int count = CountBits(mask);

                if (count == 0)
                {
                    bestRow = -2; // dead-end
                    return false;
                }

                if (count < bestCount)
                {
                    bestCount = count;
                    bestRow = row;
                    bestCol = col;
                    candidateMask = mask;

                    if (bestCount == 1)
                        return true;
                }
            }
        }

        // No hay celdas vacías: tablero resuelto.
        return bestRow != -1;
    }

    int BuildCandidateMask(int row, int col)
    {
        int mask = 0;
        for (int num = 1; num <= 9; num++)
        {
            if (bitMask.CanPlace(row, col, num))
                mask |= 1 << (num - 1);
        }
        return mask;
    }

    int CountBits(int mask)
    {
        int count = 0;
        while (mask != 0)
        {
            mask &= mask - 1;
            count++;
        }
        return count;
    }

}