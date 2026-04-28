using System.Collections.Generic;
public class SudokuHumanSolver
{
    SudokuSolverEngine engine;
    public List<string> techniquesUsed = new();
    public SudokuHumanSolver(SudokuGameManager.Difficulty diff)
    {
        engine = new SudokuSolverEngine(diff);
    }
    public bool Solve(SudokuContext ctx)
    {
        techniquesUsed.Clear();
        int safety = 0;
        while (engine.Step(ctx, out var hint))
        {
            techniquesUsed.Add(hint.technique);
            foreach (var action in hint.actions)
            {
                if (action.type == SudokuActionType.Place)
                {
                    int r = action.index / 9;
                    int c = action.index % 9;
                    ctx.board[r, c] = action.value;
                }
                else if (action.type == SudokuActionType.RemoveNotes)
                {
                    ctx.notesMask[action.index] &= ~action.mask;
                }
            }
            safety++;
            if (safety > 500) break;
        }
        return IsSolved(ctx.board);
    }
    bool IsSolved(int[,] board)
    {
        for (int i = 0; i < 81; i++)
            if (board[i / 9, i % 9] == 0)
                return false;
        return true;
    }
}