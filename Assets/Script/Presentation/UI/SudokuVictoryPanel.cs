using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuVictoryPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] Button returnMenuButton;

    void OnEnable()
    {
        if (titleText != null)
            titleText.text = "¡VICTORIA!";

        if (returnMenuButton != null)
        {
            returnMenuButton.onClick.RemoveAllListeners();
            returnMenuButton.onClick.AddListener(() => ReturnToMenu());
        }
    }

    void ReturnToMenu()
    {
        Debug.Log("Volviendo al menú...");
        // El controlador ya lo hace automáticamente
    }
}
