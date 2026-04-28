using UnityEngine;
public class SudokuGeneratorPro
{
    SudokuGenerator baseGenerator = new SudokuGenerator();
    SudokuSolver solver = new SudokuSolver();
    public SudokuBoardData Generate(SudokuGameManager.Difficulty targetDifficulty)
    {
        while (true)
        {
            int[,] solution = baseGenerator.GenerateSolution();
            int[,] puzzle = (int[,])solution.Clone();
            RemoveNumbersSmart(puzzle);
            if (solver.CountSolutions((int[,])puzzle.Clone()) != 1)
                continue;
            var ctx = new SudokuContext
            {
                board = (int[,])puzzle.Clone(),
                notesMask = new int[81]
            };
            var humanSolver = new SudokuHumanSolver(targetDifficulty);
            bool solved = humanSolver.Solve(ctx);
            if (!solved)
                continue;
            var evaluator = new SudokuDifficultyEvaluatorPro();
            var realDiff = evaluator.Evaluate(humanSolver.techniquesUsed);
            if (realDiff == targetDifficulty)
            {
                Debug.Log($"Puzzle válido: {realDiff}");
                return baseGenerator.ConvertToBoardData(solution, puzzle);
            }
        }
    }
    void RemoveNumbersSmart(int[,] puzzle)
    {
        int attempts = 0;
        while (attempts < 100)
        {
            int r = Random.Range(0, 9);
            int c = Random.Range(0, 9);
            if (puzzle[r, c] == 0)
            {
                attempts++;
                continue;
            }
            int backup = puzzle[r, c];
            puzzle[r, c] = 0;
            var solver = new SudokuSolver();
            if (solver.CountSolutions((int[,])puzzle.Clone()) != 1)
            {
                puzzle[r, c] = backup;
            }
            attempts++;
        }
    }
}