using System.Collections.Generic;
using UnityEngine;
public class SudokuHumanSolver
{
    SudokuSolverEngine engine;
    SudokuGameManager.Difficulty targetDifficulty;
    public List<string> techniquesUsed = new();
    public SudokuHumanSolver(SudokuGameManager.Difficulty diff)
    {
        targetDifficulty = diff;
        engine = new SudokuSolverEngine(diff);
        SudokuDebugMode.Log(
            $"Solver init diff={diff} techniques=[{string.Join(", ", engine.GetTechniqueNames())}]"
        );
    }
    public bool Solve(SudokuContext ctx)
    {
        techniquesUsed.Clear();
        InitializeCandidates(ctx);
        int safety = 0;
        while (engine.Step(ctx, out var hint))
        {
            techniquesUsed.Add(hint.technique);
            if (SudokuDebugMode.Enabled && SudokuDebugMode.LogEachTechniqueStep)
                SudokuDebugMode.Log($"Step {safety + 1}: {hint.technique} actions={hint.actions.Count}");

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
            RebuildCandidates(ctx);
            safety++;
            if (safety > 500)
            {
                SudokuDebugMode.Warn($"Safety break reached for diff={targetDifficulty} at step={safety}");
                break;
            }
        }
        bool solved = IsSolved(ctx.board);
        if (SudokuDebugMode.Enabled)
            SudokuDebugMode.Log(
                $"Solve done diff={targetDifficulty} solved={solved} steps={safety} used={techniquesUsed.Count}"
            );
        return solved;
    }
    void InitializeCandidates(SudokuContext ctx)
    {
        for (int i = 0; i < 81; i++)
        {
            if (ctx.board[i / 9, i % 9] != 0)
            {
                ctx.notesMask[i] = 0;
                continue;
            }

            int mask = 0;
            for (int n = 1; n <= 9; n++)
            {
                if (ctx.CanPlace(i / 9, i % 9, n))
                    mask |= 1 << (n - 1);
            }
            ctx.notesMask[i] = mask;
        }
    }
    void RebuildCandidates(SudokuContext ctx)
    {
        InitializeCandidates(ctx);
    }
    bool IsSolved(int[,] board)
    {
        for (int i = 0; i < 81; i++)
            if (board[i / 9, i % 9] == 0)
                return false;
        return true;
    }
}