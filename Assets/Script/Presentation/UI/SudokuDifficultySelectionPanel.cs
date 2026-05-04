using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuDifficultySelectionPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] Transform buttonsContainer;
    [SerializeField] Button difficultyButtonPrefab;
    [SerializeField] SudokuGameFlowController gameFlowController;

    void OnEnable()
    {
        if (titleText != null)
            titleText.text = "Selecciona Dificultad";

        BuildDifficultyButtons();
    }

    void BuildDifficultyButtons()
    {
        if (buttonsContainer == null || difficultyButtonPrefab == null)
            return;

        // Limpiar botones anteriores
        foreach (Transform child in buttonsContainer)
            Destroy(child.gameObject);

        string[] difficultyNames = { "Fácil", "Medio", "Difícil", "Experto", "Extremo" };

        for (int i = 0; i < difficultyNames.Length; i++)
        {
            var btn = Instantiate(difficultyButtonPrefab, buttonsContainer);
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = difficultyNames[i];

            int difficulty = i;
            btn.onClick.AddListener(() => OnDifficultySelected(difficulty));
        }
    }

    void OnDifficultySelected(int difficultyIndex)
    {
        if (gameFlowController != null)
            gameFlowController.StartNewGameWithDifficulty(difficultyIndex);
    }
}
