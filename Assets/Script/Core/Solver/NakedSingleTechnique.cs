using System.Collections.Generic;

public class NakedSingleTechnique : ISudokuTechnique
{
    public string Name => "Naked Single";
    public int DifficultyScore => 1;

    public bool TryApply(SudokuContext ctx, out SudokuHint hint)
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (ctx.board[r, c] != 0) continue;

                int index = r * 9 + c;
                int mask = ctx.notesMask[index];

                if (CountBits(mask) == 1)
                {
                    int value = GetSingle(mask);

                    hint = new SudokuHint
                    {
                        technique = Name,
                        candidateMask = mask
                    };

                    hint.highlightCells.Add(index);

                    hint.actions.Add(new SudokuAction
                    {
                        type = SudokuActionType.Place,
                        index = index,
                        value = value,
                        technique = Name
                    });

                    return true;
                }
            }
        }

        hint = null;
        return false;
    }

    int CountBits(int m)
    {
        int count = 0;
        while (m != 0) { m &= (m - 1); count++; }
        return count;
    }

    int GetSingle(int mask)
    {
        for (int i = 0; i < 9; i++)
            if ((mask & (1 << i)) != 0)
                return i + 1;

        return -1;
    }
}