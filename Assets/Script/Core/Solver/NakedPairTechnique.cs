using System.Collections.Generic;
public class NakedPairTechnique : ISudokuTechnique
{
    public string Name => "Naked Pair";
    public bool TryApply(SudokuContext ctx, out SudokuHint hint)
    {
        var notesMask = ctx.notesMask;
        for (int row = 0; row < 9; row++)
        {
            Dictionary<int, List<int>> pairs = new();
            for (int col = 0; col < 9; col++)
            {
                int index = row * 9 + col;
                int mask = notesMask[index];
                if (CountBits(mask) == 2)
                {
                    if (!pairs.ContainsKey(mask))
                        pairs[mask] = new List<int>();

                    pairs[mask].Add(col);
                }
            }
            foreach (var kv in pairs)
            {
                if (kv.Value.Count == 2)
                {
                    int pairMask = kv.Key;
                    for (int col = 0; col < 9; col++)
                    {
                        if (kv.Value.Contains(col)) continue;
                        int index = row * 9 + col;
                        if ((notesMask[index] & pairMask) != 0)
                        {
                            hint = new SudokuHint
                            {
                                technique = Name,
                                candidateMask = pairMask
                            };
                            hint.highlightCells.Add(row * 9 + kv.Value[0]);
                            hint.highlightCells.Add(row * 9 + kv.Value[1]);
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
}