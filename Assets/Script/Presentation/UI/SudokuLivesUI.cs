using UnityEngine;
using UnityEngine.UI;

public class SudokuLivesUI : MonoBehaviour
{
    [SerializeField] Image[] hearts;

    public void UpdateLives(int mistakes)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i >= mistakes;
        }
    }
}