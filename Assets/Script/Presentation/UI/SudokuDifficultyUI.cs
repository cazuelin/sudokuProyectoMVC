using TMPro;
using UnityEngine;
public class SudokuDifficultyUI : MonoBehaviour
{
    [SerializeField] TMP_Text difficultyText;
    public void SetDifficulty(SudokuGameManager.Difficulty diff)
    {
        difficultyText.text = diff.ToString();
    }
}