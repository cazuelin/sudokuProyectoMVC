using UnityEngine;
using UnityEngine.SceneManagement;
using BayatGames.SaveGameFree;

public class MainMenuManager : MonoBehaviour
{
    public GameObject continueButton;
    public GameObject newGameButton;

    void Start()
    {
        bool hasSave = SaveGame.Exists("boardData");

        continueButton.SetActive(hasSave);
        newGameButton.SetActive(hasSave);
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void NewGame(int difficulty)
    {
        SaveGame.DeleteAll();
        SaveGame.Save("selectedDifficulty", difficulty);

        SceneManager.LoadScene("Game");
    }
}