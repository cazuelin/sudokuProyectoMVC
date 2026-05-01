using System.Collections.Generic;
public class PointingPairTechnique : ISudokuTechnique
{
    public string Name => "Pointing Pair";
    public bool TryApply(SudokuContext ctx, out SudokuHint hint)
    {
        var notesMask = ctx.notesMask;

        for (int box = 0; box < 9; box++)
        {
            int startRow = (box / 3) * 3;
            int startCol = (box % 3) * 3;
            for (int num = 1; num <= 9; num++)
            {
                int mask = 1 << (num - 1);
                List<int> positions = new();
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        int rr = startRow + r;
                        int cc = startCol + c;
                        int index = rr * 9 + cc;
                        if (ctx.board[rr, cc] == 0 && (notesMask[index] & mask) != 0)
                            positions.Add(index);
                    }
                }
                if (positions.Count < 2) continue;
                int baseRow = positions[0] / 9;
                bool sameRow = true;
                foreach (var idx in positions)
                    if (idx / 9 != baseRow)
                        sameRow = false;
                if (sameRow)
                {
                    List<int> affected = new();
                    for (int c = 0; c < 9; c++)
                    {
                        int index = baseRow * 9 + c;
                        if (IsInsideBox(index, box)) continue;
                        if (ctx.board[baseRow, c] == 0 && (notesMask[index] & mask) != 0)
                            affected.Add(index);
                    }

                    if (affected.Count > 0)
                    {
                        hint = BuildHint(Name, mask, positions, affected);
                        return true;
                    }
                }
                int baseCol = positions[0] % 9;
                bool sameCol = true;
                foreach (var idx in positions)
                    if (idx % 9 != baseCol)
                        sameCol = false;
                if (sameCol)
                {
                    List<int> affected = new();
                    for (int r = 0; r < 9; r++)
                    {
                        int index = r * 9 + baseCol;
                        if (IsInsideBox(index, box)) continue;
                        if (ctx.board[r, baseCol] == 0 && (notesMask[index] & mask) != 0)
                            affected.Add(index);
                    }

                    if (affected.Count > 0)
                    {
                        hint = BuildHint(Name, mask, positions, affected);
                        return true;
                    }
                }
            }
        }
        hint = null;
        return false;
    }
    SudokuHint BuildHint(string tech, int mask, List<int> highlights, List<int> affected)
    {
        var hint = new SudokuHint
        {
            technique = tech,
            candidateMask = mask
        };
        hint.highlightCells.AddRange(highlights);
        foreach (int idx in affected)
        {
            hint.affectedCells.Add(idx);
            hint.actions.Add(new SudokuAction
            {
                type = SudokuActionType.RemoveNotes,
                index = idx,
                mask = mask,
                technique = tech
            });
        }
        return hint;
    }
    bool IsInsideBox(int index, int box)
    {
        int r = index / 9;
        int c = index % 9;
        int br = (box / 3) * 3;
        int bc = (box % 3) * 3;
        return r >= br && r < br + 3 && c >= bc && c < bc + 3;
    }
}