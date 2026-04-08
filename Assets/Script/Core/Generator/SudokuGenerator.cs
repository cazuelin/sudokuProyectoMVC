using UnityEngine;

public class SudokuGenerator
{
    const int SIZE = 9;

    public SudokuBoardData GenerateBoard(int removeAmount)
    {
        var solution = GenerateSolution();
        var puzzle = CreatePuzzle(solution, removeAmount);

        var data = new SudokuBoardData();

        for (int r = 0; r < SIZE; r++)
            for (int c = 0; c < SIZE; c++)
            {
                int i = r * SIZE + c;
                int val = puzzle[r, c];

                data.values[i] = val;
                data.solution[i] = solution[r, c];
                data.fixedCells[i] = val != 0;
            }

        return data;
    }

    public int[,] GenerateSolution()
    {
        int[,] grid = new int[SIZE, SIZE];

        for (int r = 0; r < SIZE; r++)
            for (int c = 0; c < SIZE; c++)
                grid[r, c] = (r * 3 + r / 3 + c) % SIZE + 1;

        ShuffleNumbers(grid);
        ShuffleRows(grid);
        ShuffleColumns(grid);

        return grid;
    }

    public int[,] CreatePuzzle(int[,] solution, int removeAmount)
    {
        int[,] puzzle = (int[,])solution.Clone();

        int removed = 0;
        int attempts = 0;
        int maxAttempts = removeAmount * 3;

        while (removed < removeAmount && attempts < maxAttempts)
        {
            int r = Random.Range(0, 9);
            int c = Random.Range(0, 9);

            if (puzzle[r, c] == 0)
            {
                attempts++;
                continue;
            }

            int r2 = 8 - r;
            int c2 = 8 - c;

            if (puzzle[r2, c2] == 0)
            {
                attempts++;
                continue;
            }

            puzzle[r, c] = 0;
            puzzle[r2, c2] = 0;

            removed += (r == r2 && c == c2) ? 1 : 2;

            attempts++;
        }

        return puzzle;
    }

    void ShuffleNumbers(int[,] grid)
    {
        int[] map = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        for (int i = 0; i < SIZE; i++)
        {
            int rand = Random.Range(i, SIZE);
            (map[i], map[rand]) = (map[rand], map[i]); // swap moderno
        }

        for (int r = 0; r < SIZE; r++)
            for (int c = 0; c < SIZE; c++)
                grid[r, c] = map[grid[r, c] - 1];
    }

    void ShuffleRows(int[,] grid)
    {
        for (int block = 0; block < 3; block++)
            SwapRows(grid,
                block * 3 + Random.Range(0, 3),
                block * 3 + Random.Range(0, 3));
    }

    void SwapRows(int[,] g, int r1, int r2)
    {
        for (int c = 0; c < SIZE; c++)
            (g[r1, c], g[r2, c]) = (g[r2, c], g[r1, c]);
    }

    void ShuffleColumns(int[,] grid)
    {
        for (int block = 0; block < 3; block++)
            SwapColumns(grid,
                block * 3 + Random.Range(0, 3),
                block * 3 + Random.Range(0, 3));
    }

    void SwapColumns(int[,] g, int c1, int c2)
    {
        for (int r = 0; r < SIZE; r++)
            (g[r, c1], g[r, c2]) = (g[r, c2], g[r, c1]);
    }
    public SudokuBoardData ConvertToBoardData(int[,] solution, int[,] puzzle)
    {
        SudokuBoardData data = new SudokuBoardData();

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                int index = r * 9 + c;

                data.values[index] = puzzle[r, c];
                data.solution[index] = solution[r, c];
                data.fixedCells[index] = puzzle[r, c] != 0;
            }
        }

        return data;
    }
}