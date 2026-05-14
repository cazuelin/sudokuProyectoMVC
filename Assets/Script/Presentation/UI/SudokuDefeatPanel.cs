using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuDefeatPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] Button restartButton;
    [SerializeField] Button newGameButton;
    [SerializeField] SudokuGameFlowController gameFlowController;

    void OnEnable()
    {
        if (titleText != null)
            titleText.text = "GAME OVER";

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => OnRestartClicked());
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(() => OnNewGameClicked());
        }
    }

    void OnRestartClicked()
    {
        if (gameFlowController != null)
            gameFlowController.RestartLevel();
    }

    void OnNewGameClicked()
    {
        if (gameFlowController != null)
            gameFlowController.OpenNewGamePanel();
    }
}
