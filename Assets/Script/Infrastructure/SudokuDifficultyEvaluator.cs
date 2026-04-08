public class SudokuDifficultyEvaluator
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public Difficulty Evaluate(int[,] puzzle, int steps)
    {
        int empty = CountEmpty(puzzle);

        if (empty < 35 && steps < 40)
            return Difficulty.Easy;

        if (empty < 50 && steps < 60)
            return Difficulty.Medium;

        return Difficulty.Hard;
    }

    int CountEmpty(int[,] board)
    {
        int count = 0;

        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (board[r, c] == 0)
                    count++;

        return count;
    }
}