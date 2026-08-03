using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SudokuSaveSlotItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI title;//Texto del título del slot.
    [SerializeField] TextMeshProUGUI info;//Texto de información del slot.
    //Si está vacío, muestra: Nuevo juego
    //Si tiene guardado, muestra algo como: Hard • 05:32
    [SerializeField] Button createButton;//Botón para crear una partida nueva en ese slot.
    [SerializeField] Button continueButton;//Botón para continuar una partida guardada.
    [SerializeField] Button deleteButton;//Botón para borrar el guardado de ese slot.
    int slotIndex;//Guarda el índice interno del slot.
    //Ejemplo: slotIndex = 0  representa visualmente: Slot 1
    //Porque en código se empieza desde 0, pero al jugador se le muestra desde 1.

    //Estos eventos avisan a otro script que el jugador pidió algo en este slot.
    //Cada evento manda un int, que es el slotIndex.
    public event Action<int> OnContinueRequested;//El jugador quiere continuar este slot.
    public event Action<int> OnDeleteRequested;//El jugador quiere borrar este slot.
    public event Action<int> OnCreateRequested;//El jugador quiere crear una partida en este slot.
    public void Init(int index)//Esta función inicializa el slot.
        //recibe int index : El índice del slot.
    {
        slotIndex = index;//Guarda el índice recibido.
        createButton.onClick.RemoveAllListeners();//Limpia eventos anteriores de los botones.
        continueButton.onClick.RemoveAllListeners();//Limpia eventos anteriores de los botones.
        deleteButton.onClick.RemoveAllListeners();//Limpia eventos anteriores de los botones.
        //Esto evita que un botón llame varias veces la misma función si Init se ejecuta más de una vez.

        //Conecta cada botón con su función interna.
        createButton.onClick.AddListener(OnCreate);//Cuando presionas createButton, llama: OnCreate()
        continueButton.onClick.AddListener(OnContinue);//Cuando presionas continueButton, llama: OnContinue()
        deleteButton.onClick.AddListener(OnDelete);//Cuando presionas deleteButton, llama: OnDelete()
        title.text = $"Slot {slotIndex + 1}";//Actualiza el texto del título.
        //Si: slotIndex = 0  entonces muestra : Slot 1
        //Si: slotIndex = 2  entonces muestra : Slot 3
    }
    public void RenderEmpty()//Esta función dibuja el slot cuando está vacío.
    {
        info.text = "Nuevo juego";//Muestra que no hay partida guardada.
        createButton.gameObject.SetActive(true);//Muestra el botón de crear partida.
        continueButton.gameObject.SetActive(false);//Oculta el boton de continuar.
        deleteButton.gameObject.SetActive(false);//Oculta el boton de borrar.
        //Porque si no hay guardado, no puedes continuar ni borrar.
    }
    public void RenderSaved(SudokuGameManager.Difficulty difficulty, float time)
    //Esta función dibuja el slot cuando sí tiene una partida guardada.
    //recibe SudokuGameManager.Difficulty difficulty : La dificultad guardada.
    //recibe float time : El tiempo de esa partida.
    {
        info.text = $"{difficulty} • {FormatTime(time)}";//Muestra dificultad y tiempo.
        //Ejemplo: Hard • 04:25    FormatTime(time) convierte segundos a formato mm:ss.
        createButton.gameObject.SetActive(false);//Oculta el botón de crear. Porque el slot ya tiene una partida.
        continueButton.gameObject.SetActive(true);//Muestra el botón de continuar.
        deleteButton.gameObject.SetActive(true);//Muestra el botón de borrar.
    }
    string FormatTime(float t)//Esta función convierte segundos a formato de tiempo.
        //Recibe float t : Tiempo en segundos.
        //Ejemplo: t = 125
    {
        int min = Mathf.FloorToInt(t / 60);//Calcula minutos.
        //ejemplo : 125 / 60 = 2
        int sec = Mathf.FloorToInt(t % 60);//Calcula segundos restantes.
        //ejemplo : 125 % 60 = 5
        return $"{min:00}:{sec:00}";//Devuelve texto con dos dígitos.
        //ejemplo : 02:05
    }
    void OnContinue()//Se llama cuando presionas el botón continuar.
    {
        OnContinueRequested?.Invoke(slotIndex);//Dispara el evento OnContinueRequested y manda el índice del slot.
        //El ?. significa : Si hay alguien escuchando, avísale.
    }
    void OnDelete()//Se llama cuando presionas borrar.
    {
        OnDeleteRequested?.Invoke(slotIndex);//Dispara el evento OnDeleteRequested con el slot actual.
        //El ?. significa : Si hay alguien escuchando, avísale.
    }
    void OnCreate()//Se llama cuando presionas crear.
    {
        OnCreateRequested?.Invoke(slotIndex);//Dispara el evento OnCreateRequested con el slot actual.
        //El ?. significa : Si hay alguien escuchando, avísale.
    }
}