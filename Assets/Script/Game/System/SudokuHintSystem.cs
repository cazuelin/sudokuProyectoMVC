using UnityEngine;

public class SudokuHintSystem : MonoBehaviour
{
    SudokuTechniques solver;

    [SerializeField] SudokuBoardController boardController;

    public void Init(SudokuGameManager.Difficulty difficulty)
    {
        solver = new SudokuTechniques(difficulty);
    }
    public bool TryGetHint(out int row, out int col, out int value, out string technique)
    {
        int[,] board = boardController.GetBoardMatrix();

        return solver.GetHint(board, out row, out col, out value, out technique);
    }
}