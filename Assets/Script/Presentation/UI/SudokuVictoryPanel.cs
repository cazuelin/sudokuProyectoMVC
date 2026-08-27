using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuVictoryPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] Button returnMenuButton;
    [SerializeField] SudokuSessionController sessionController;

    void OnEnable()
    {
        if (sessionController == null)
            sessionController = FindFirstObjectByType<SudokuSessionController>();

        if (titleText != null)
            titleText.text = "VICTORIA";

        if (returnMenuButton != null)
        {
            returnMenuButton.onClick.RemoveAllListeners();
            returnMenuButton.onClick.AddListener(ReturnToMenu);
        }
    }

    void ReturnToMenu()
    {
        if (sessionController != null)
            sessionController.SaveAndGoToMenu();
    }
}
