using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuHintsUI : MonoBehaviour
{
    [SerializeField] TMP_Text hintText;//Referencia al texto donde se muestra la cantidad de pistas.
    //Ejemplo visual:Pistas: 3
    //Se asigna desde el Inspector de Unity.
    [SerializeField] Button hintButton;//Referencia al botón que el jugador presiona para usar una pista.
    //Se usa para activar o desactivar el botón según queden pistas.
    public void UpdateHints(int remainingHints)//Esta es la única función del script.
        //Esta función es llamada cuando cambia la cantidad de pistas, por ejemplo desde SudokuGameFlowController:
        //hintsUI?.UpdateHints(inputController.RemainingHints);
        //o o cuando SudokuInputController dispara: OnHintsChanged
        //recibe int remainingHints : que son La cantidad de pistas disponibles.
    {
        if (hintText != null)//Revisa que el texto esté asignado.
            hintText.text = $"Pistas: {remainingHints}";//si existe Actualiza el texto.

        if (hintButton != null)//Revisa que el botón esté asignado.
            hintButton.interactable = remainingHints > 0;//si existe Esto decide si el botón se puede presionar.
        //si remainingHints > 0 es verdadero, el botón queda activo.
        //Ejemplo 1:
        //remainingHints = 1
        //entonces : hintButton.interactable = true;
        //El botón queda activado.
        //ejemplo 2: 
        //remainingHints = 0
        //entonces : hintButton.interactable = false;
        //El botón queda desactivado.
    }
}
