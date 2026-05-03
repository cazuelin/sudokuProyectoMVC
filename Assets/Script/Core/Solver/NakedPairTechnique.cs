using System.Collections.Generic;
public class NakedPairTechnique : ISudokuTechnique
{
    public string Name => "Naked Pair";
    public bool TryApply(SudokuContext ctx, out SudokuHint hint)
    {
        for (int row = 0; row < 9; row++)
        {
            if (TryInUnit(ctx, GetRowUnit(row), out hint))
                return true;
        }

        for (int col = 0; col < 9; col++)
        {
            if (TryInUnit(ctx, GetColUnit(col), out hint))
                return true;
        }

        for (int box = 0; box < 9; box++)
        {
            if (TryInUnit(ctx, GetBoxUnit(box), out hint))
                return true;
        }

        hint = null;
        return false;
    }

    bool TryInUnit(SudokuContext ctx, int[] unit, out SudokuHint hint)
    {
        var notesMask = ctx.notesMask;
        Dictionary<int, List<int>> pairs = new();

        for (int i = 0; i < 9; i++)
        {
            int index = unit[i];
            int r = index / 9;
            int c = index % 9;

            if (ctx.board[r, c] != 0)
                continue;

            int mask = notesMask[index];
            if (CountBits(mask) != 2)
                continue;

            if (!pairs.ContainsKey(mask))
                pairs[mask] = new List<int>();

            pairs[mask].Add(index);
        }

        foreach (var kv in pairs)
        {
            if (kv.Value.Count != 2)
                continue;

            int pairMask = kv.Key;
            int a = kv.Value[0];
            int b = kv.Value[1];

            for (int i = 0; i < 9; i++)
            {
                int index = unit[i];
                if (index == a || index == b)
                    continue;

                int r = index / 9;
                int c = index % 9;
                if (ctx.board[r, c] != 0)
                    continue;

                if ((notesMask[index] & pairMask) == 0)
                    continue;

                hint = new SudokuHint
                {
                    technique = Name,
                    candidateMask = pairMask
                };
                hint.highlightCells.Add(a);
                hint.highlightCells.Add(b);
                hint.affectedCells.Add(index);
                hint.actions.Add(new SudokuAction
                {
                    type = SudokuActionType.RemoveNotes,
                    index = index,
                    mask = pairMask,
                    technique = Name
                });
                return true;
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

    int[] GetRowUnit(int row)
    {
        int[] unit = new int[9];
        for (int c = 0; c < 9; c++)
            unit[c] = row * 9 + c;
        return unit;
    }

    int[] GetColUnit(int col)
    {
        int[] unit = new int[9];
        for (int r = 0; r < 9; r++)
            unit[r] = r * 9 + col;
        return unit;
    }

    int[] GetBoxUnit(int box)
    {
        int[] unit = new int[9];
        int startRow = (box / 3) * 3;
        int startCol = (box % 3) * 3;
        int k = 0;
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                unit[k++] = (startRow + r) * 9 + (startCol + c);
        return unit;
    }
}