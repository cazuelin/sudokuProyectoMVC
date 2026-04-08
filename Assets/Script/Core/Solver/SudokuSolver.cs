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

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board[row, col] == 0)
                {
                    for (int num = 1; num <= 9; num++)
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

                    return false;
                }
            }
        }

        return true;
    }
    public int CountSolutions(int[,] board)
    {
        bitMask.Init(board);

        int count = 0;
        SolveCount(board, ref count);
        return count;
    }
    bool SolveCount(int[,] board, ref int count)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board[row, col] == 0)
                {
                    for (int num = 1; num <= 9; num++)
                    {
                        if (bitMask.CanPlace(row, col, num))
                        {
                            board[row, col] = num;
                            bitMask.Place(row, col, num);

                            SolveCount(board, ref count);

                            board[row, col] = 0;
                            bitMask.Remove(row, col, num);

                            if (count > 1) return true;
                        }
                    }
                    return false;
                }
            }
        }

        count++;
        return count > 1;
    }
}