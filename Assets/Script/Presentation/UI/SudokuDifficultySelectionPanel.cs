using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuDifficultySelectionPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;//Texto del título del panel.
    //Se usa para mostrar: Selecciona dificultad
    [SerializeField] Button easyButton;//boton del panel que representa la dificultad Easy
    [SerializeField] Button mediumButton;//boton del panel que representa la dificultad Medium
    [SerializeField] Button hardButton;//boton del panel que representa la dificultad Hard
    [SerializeField] Button expertButton;//boton del panel que representa la dificultad Expert
    [SerializeField] Button extremeButton;//boton del panel que representa la dificultad Easy
    [SerializeField] Button cancelButton;//sirve para cerrar el panel sin elegir dificultad.
    int currentSlot = -1;//Guarda el slot donde se va a crear la partida.
    //ejemplo : currentSlot = 0  significa que la partida se creará en el slot 0.
    //el valor - 1 significa No hay slot seleccionado.
    public event Action<int, SudokuGameManager.Difficulty> OnDifficultySelected;
    //Este evento avisa a otro script cuando el jugador elige dificultad.
    //Envía dos datos : int : El slot seleccionado. 
    //SudokuGameManager.Difficulty : La dificultad elegida.
    //ejemplo : OnDifficultySelected?.Invoke(0, SudokuGameManager.Difficulty.Hard);
    //significa Crear partida en slot 0 con dificultad Hard.
    void OnEnable()//OnEnable se ejecuta cada vez que el panel se activa.
    {
        if (titleText != null)//Si el texto existe
            titleText.text = "Selecciona dificultad";//pone el título del panel.
        SetupButtons();//Configura los botones de dificultad.
        if (cancelButton != null)//Si el boton existe
        {
            cancelButton.onClick.RemoveAllListeners();//Limpia eventos anteriores.
            cancelButton.onClick.AddListener(() => Close(true));//Agrega la acción de cerrar el panel al presionar cancelar.
        }
    }
    public void Setup(TextMeshProUGUI title, Button[] difficultyButtons, Button cancel)
    //Reasigna las referencias del panel construido por código. Lo usa SudokuGameUI.
    //difficultyButtons debe traer: [Easy, Medium, Hard, Expert, Extreme].
    {
        titleText = title;
        if (difficultyButtons != null && difficultyButtons.Length >= 5)
        {
            easyButton = difficultyButtons[0];
            mediumButton = difficultyButtons[1];
            hardButton = difficultyButtons[2];
            expertButton = difficultyButtons[3];
            extremeButton = difficultyButtons[4];
        }
        cancelButton = cancel;
    }
    public void Open(int slot)//Esta función abre el panel.
        //recibe int slot : El slot donde se quiere crear la partida.
    {
        currentSlot = slot;//Guarda ese slot.
        //Asegura que el panel cubra toda la pantalla (se posiciona encima) aunque el prefab
        //esté posicionado a un lado o sobresaliendo del mapa.
        var rt = transform as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }
        gameObject.SetActive(true);//Activa el panel en pantalla.
        //Cuando se activa, Unity llama automáticamente a OnEnable.
    }
    public void Close(bool fromCancel = true)//Esta función cierra el panel.
        //Recibe un parámetro opcional: bool fromCancel = true
        //Eso significa que si llamas: Close(); por defecto es como: Close(true);
    {
        if (!fromCancel)//Si fromCancel es false, no cierra.
            return;
        //Esto está hecho para evitar cerrar el panel en ciertos casos,
        //por ejemplo cuando se selecciona dificultad y se quiere evitar mostrar la escena anterior durante la transición.

        currentSlot = -1;//Limpia el slot actual.
        gameObject.SetActive(false);//Oculta el panel.
    }
    void SetupButtons()//Esta función conecta cada botón con su dificultad.
    {
        if (easyButton != null)//revisa que el botón exista.
        {
            easyButton.onClick.RemoveAllListeners();//limpia listeners anteriores
            easyButton.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Easy));
            //Después agrega una acción : SelectDifficulty(SudokuGameManager.Difficulty.Easy));
            //Eso significa: Cuando presiones Easy, llama SelectDifficulty(Easy).
        }
        if (mediumButton != null)//revisa que el botón exista.
        {
            mediumButton.onClick.RemoveAllListeners();//limpia listeners anteriores
            mediumButton.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Medium));
            //Después agrega una acción : SelectDifficulty(SudokuGameManager.Difficulty.Medium));
            //Eso significa: Cuando presiones Medium, llama SelectDifficulty(Medium).
        }
        if (hardButton != null)//revisa que el botón exista.
        {
            hardButton.onClick.RemoveAllListeners();//limpia listeners anteriores
            hardButton.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Hard));
            //Después agrega una acción : SelectDifficulty(SudokuGameManager.Difficulty.Hard));
            //Eso significa: Cuando presiones Hard, llama SelectDifficulty(Hard).
        }
        if (expertButton != null)//revisa que el botón exista.
        {
            expertButton.onClick.RemoveAllListeners();//limpia listeners anteriores
            expertButton.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Expert));
            //Después agrega una acción : SelectDifficulty(SudokuGameManager.Difficulty.Expert));
            //Eso significa: Cuando presiones Expert, llama SelectDifficulty(Expert).
        }
        if (extremeButton != null)//revisa que el botón exista.
        {
            extremeButton.onClick.RemoveAllListeners();//limpia listeners anteriores
            extremeButton.onClick.AddListener(() => SelectDifficulty(SudokuGameManager.Difficulty.Extreme));
            //Después agrega una acción : SelectDifficulty(SudokuGameManager.Difficulty.Extreme));
            //Eso significa: Cuando presiones Extreme, llama SelectDifficulty(Extreme).
        }
    }
    void SelectDifficulty(SudokuGameManager.Difficulty difficulty)//Esta función se llama cuando el jugador presiona una dificultad.
        //recibe SudokuGameManager.Difficulty difficulty : La dificultad seleccionada.
    {
        if (currentSlot < 0)//Si no hay slot válido, no hace nada.
            return;

        OnDifficultySelected?.Invoke(currentSlot, difficulty);//Dispara el evento.
        //Envía: currentSlot y difficulty
        //Ejemplo: slot 1, dificultad Expert
        Close(true);//Cierra el panel al elegir dificultad, aunque la escena tarde en cambiar.
    }
}
