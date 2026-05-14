using UnityEngine;
using UnityEngine.UI;
public class SudokuLivesUI : MonoBehaviour
{
    [SerializeField] Image[] hearts;
    [SerializeField] Sprite fullHeart;
    [SerializeField] Sprite emptyHeart;
    public void UpdateLives(int mistakes)
    {
        int maxLives = 3;
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < (maxLives - mistakes) ? fullHeart : emptyHeart;
            hearts[i].enabled = true; // Asegurar que estén visibles
        }
    }
}