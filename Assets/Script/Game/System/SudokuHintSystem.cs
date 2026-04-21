using System.Collections.Generic;
using UnityEngine;
public class SudokuHintSystem : MonoBehaviour
{
    SudokuTechniques solver;
    [SerializeField] SudokuBoardController boardController;
    public void Init(SudokuGameManager.Difficulty difficulty)
    {
        solver = new SudokuTechniques(difficulty);
    }
    public bool TryGetHint(out SudokuHint hint)
    {
        var ctx = new SudokuContext
        {
            board = boardController.GetBoardMatrix(),
            notesMask = boardController.boardData.notesMask
        };

        return solver.GetHint(ctx, out hint);
    }
}