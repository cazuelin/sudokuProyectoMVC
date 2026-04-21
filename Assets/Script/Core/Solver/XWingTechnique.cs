using System.Collections.Generic;

public class XWingTechnique : ISudokuTechnique
{
    public string Name => "X-Wing";

    public bool TryApply(SudokuContext ctx, out SudokuHint hint)
    {
        var notes = ctx.notesMask;

        for (int num = 1; num <= 9; num++)
        {
            int mask = 1 << (num - 1);

            var rowCols = new List<(int row, int c1, int c2)>();

            for (int row = 0; row < 9; row++)
            {
                int c1 = -1, c2 = -1, count = 0;

                for (int col = 0; col < 9; col++)
                {
                    int index = row * 9 + col;

                    if ((notes[index] & mask) != 0)
                    {
                        if (count == 0) c1 = col;
                        else if (count == 1) c2 = col;

                        count++;
                        if (count > 2) break;
                    }
                }

                if (count == 2)
                    rowCols.Add((row, c1, c2));
            }

            for (int i = 0; i < rowCols.Count; i++)
            {
                for (int j = i + 1; j < rowCols.Count; j++)
                {
                    var a = rowCols[i];
                    var b = rowCols[j];

                    if (a.c1 == b.c1 && a.c2 == b.c2)
                    {
                        for (int r = 0; r < 9; r++)
                        {
                            if (r == a.row || r == b.row) continue;

                            int idx = r * 9 + a.c1;

                            if ((notes[idx] & mask) != 0)
                            {
                                hint = new SudokuHint
                                {
                                    technique = Name,
                                    candidateMask = mask
                                };

                                hint.highlightCells.Add(a.row * 9 + a.c1);
                                hint.highlightCells.Add(a.row * 9 + a.c2);
                                hint.highlightCells.Add(b.row * 9 + b.c1);
                                hint.highlightCells.Add(b.row * 9 + b.c2);

                                hint.affectedCells.Add(idx);

                                hint.actions.Add(new SudokuAction
                                {
                                    type = SudokuActionType.RemoveNotes,
                                    index = idx,
                                    mask = mask,
                                    technique = Name
                                });

                                return true;
                            }
                        }
                    }
                }
            }
        }

        hint = null;
        return false;
    }
}