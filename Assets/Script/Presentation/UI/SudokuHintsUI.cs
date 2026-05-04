using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuHintsUI : MonoBehaviour
{
    [SerializeField] TMP_Text hintText;
    [SerializeField] Button hintButton;

    public void UpdateHints(int remainingHints)
    {
        if (hintText != null)
            hintText.text = $"Pistas restantes: {remainingHints}";

        if (hintButton != null)
            hintButton.interactable = remainingHints > 0;
    }
}
