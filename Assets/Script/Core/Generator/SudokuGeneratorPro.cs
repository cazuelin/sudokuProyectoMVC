using System.Collections.Generic;
using UnityEngine;
public class SudokuGeneratorPro
{
    SudokuGenerator baseGenerator = new SudokuGenerator();
    SudokuSolver solver = new SudokuSolver();
    SudokuDifficultyEvaluatorPro evaluator = new SudokuDifficultyEvaluatorPro();

    public SudokuBoardData Generate(SudokuGameManager.Difficulty targetDifficulty)
    {
        var profile = GetProfile(targetDifficulty);
        int targetRemoved = Random.Range(profile.minRemoved, profile.maxRemoved + 1);
        float startTime = Time.realtimeSinceStartup;
        SudokuDebugMode.Log(
            $"Generate start diff={targetDifficulty} removeRange={profile.minRemoved}-{profile.maxRemoved} targetRemoved={targetRemoved}"
        );

        int[,] solution = baseGenerator.GenerateSolution();
        int[,] puzzle = BuildUniquePuzzle(solution, targetRemoved);

        // Diagnóstico: no bloquea generación, solo informa dificultad real alcanzada.
        var ctx = new SudokuContext
        {
            board = (int[,])puzzle.Clone(),
            notesMask = new int[81]
        };
        var humanSolver = new SudokuHumanSolver(targetDifficulty);
        bool solved = humanSolver.Solve(ctx);
        var realDiff = evaluator.Evaluate(humanSolver.techniquesUsed);
        SudokuDebugMode.Log(
            $"Generate done target={targetDifficulty} real={realDiff} solvedByHuman={solved} removed={CountRemoved(puzzle)} elapsed={Time.realtimeSinceStartup - startTime:F2}s"
        );

        return baseGenerator.ConvertToBoardData(solution, puzzle);
    }

    int[,] BuildUniquePuzzle(int[,] solution, int targetRemoved)
    {
        int[,] puzzle = (int[,])solution.Clone();
        List<int> order = BuildShuffledPairSeeds();
        int removed = 0;

        for (int i = 0; i < order.Count && removed < targetRemoved; i++)
        {
            int index = order[i];
            int r = index / 9;
            int c = index % 9;
            int r2 = 8 - r;
            int c2 = 8 - c;

            if (puzzle[r, c] == 0)
                continue;

            int oldA = puzzle[r, c];
            int oldB = puzzle[r2, c2];

            puzzle[r, c] = 0;
            int removedNow = 1;
            if (r != r2 || c != c2)
            {
                if (puzzle[r2, c2] == 0)
                {
                    puzzle[r, c] = oldA;
                    continue;
                }
                puzzle[r2, c2] = 0;
                removedNow = 2;
            }

            // Validación cara, pero con una sola pasada real de generación.
            if (solver.CountSolutions((int[,])puzzle.Clone()) == 1)
            {
                removed += removedNow;
            }
            else
            {
                puzzle[r, c] = oldA;
                if (r != r2 || c != c2)
                    puzzle[r2, c2] = oldB;
            }
        }

        SudokuDebugMode.Log($"Unique removal finished removed={removed} target={targetRemoved}");
        return puzzle;
    }

    List<int> BuildShuffledPairSeeds()
    {
        // Solo se evalúa cada par simétrico una vez (incluye centro).
        List<int> indices = new List<int>(41);
        for (int i = 0; i < 81; i++)
        {
            int mirror = 80 - i;
            if (i <= mirror)
                indices.Add(i);
        }

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        return indices;
    }

    int CountRemoved(int[,] puzzle)
    {
        int removed = 0;
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (puzzle[r, c] == 0)
                    removed++;
        return removed;
    }

    (int minRemoved, int maxRemoved) GetProfile(SudokuGameManager.Difficulty difficulty)
    {
        return difficulty switch
        {
            SudokuGameManager.Difficulty.Easy => (30, 36),
            SudokuGameManager.Difficulty.Medium => (37, 44),
            SudokuGameManager.Difficulty.Hard => (45, 52),
            SudokuGameManager.Difficulty.Expert => (53, 58),
            SudokuGameManager.Difficulty.Extreme => (59, 64),
            _ => (37, 44)
        };
    }
}