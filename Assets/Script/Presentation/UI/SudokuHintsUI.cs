using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuHintsUI : MonoBehaviour
{
    [SerializeField] TMP_Text hintText;//Referencia al texto donde se muestra la cantidad de pistas.
    //Ejemplo visual:Pistas: 3
    //Se asigna desde el Inspector de Unity o se busca solo en los prefabs instanciados.
    [SerializeField] Button hintButton;//Referencia al botón que el jugador presiona para usar una pista.
    //Se usa para activar o desactivar el botón según queden pistas.
    public void Setup(TMP_Text text, Button button)//Reasigna las referencias de la UI. Lo usa SudokuGameUI.
    {
        hintText = text;
        hintButton = button;
    }
    public void UpdateHints(int remainingHints)//Esta función se llama cuando cambia la cantidad de pistas.
        //recibe int remainingHints : que son La cantidad de pistas disponibles.
    {
        //Resolución perezosa: si el texto o el botón aún no están asignados (porque viven en un
        //prefab instanciado en runtime), se buscan solos en la escena.
        if (hintText == null)
        {
            hintText = SudokuSceneRef.FindInScene<TMP_Text>(t =>
            {
                string n = t.gameObject.name.ToLowerInvariant();
                return n.Contains("hint") || n.Contains("pistas");
            });
        }
        if (hintButton == null)
        {
            hintButton = SudokuSceneRef.FindInScene<Button>(b =>
            {
                var label = b.GetComponentInChildren<TMP_Text>(true);
                string n = (label != null && !string.IsNullOrEmpty(label.text)) ? label.text : b.name;
                n = n.Trim().ToLowerInvariant();
                return n.Contains("pista") || n.Contains("hint");
            });
        }
        if (hintText != null)//Revisa que el texto esté asignado.
            hintText.text = $"Pistas: {remainingHints}";//si existe Actualiza el texto.

        if (hintButton != null)//Revisa que el botón esté asignado.
            hintButton.interactable = remainingHints > 0;//si existe Esto decide si el botón se puede presionar.
        //si remainingHints > 0 es verdadero, el botón queda activo.
    }
}
