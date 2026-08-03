using UnityEngine;
using UnityEngine.UI;
public class SudokuLivesUI : MonoBehaviour
{
    [SerializeField] Image[] hearts;//Este arreglo guarda las imágenes de corazones.
    //Por ejemplo, si tienes 3 vidas:
    //hearts[0]
    //hearts[1]
    //hearts[2]
    //Cada elemento es una imagen UI.
    //Se asignan desde el Inspector.
    [SerializeField] Sprite fullHeart;//Sprite del corazón lleno.
    //Es la imagen que se muestra cuando esa vida todavía está disponible.
    [SerializeField] Sprite emptyHeart;//Sprite del corazón vacío.
    //Es la imagen que se muestra cuando esa vida ya se perdió por un error.
    public void UpdateLives(int mistakes)//Esta función actualiza los corazones según los errores.
        //recibe int mistakes : La cantidad de errores cometidos.
        //ejemplo:
        //mistakes = 0  entonces significa ningún error.
        //mistakes = 2  significa dos errores.
    {
        int maxLives = 3;//Define el máximo de vidas. Aquí está fijo en 3.
        //Eso significa: 3 errores máximos / 3 corazones
        for (int i = 0; i < hearts.Length; i++)//Recorre todos los corazones del arreglo.
        {
            hearts[i].sprite = i < (maxLives - mistakes) ? fullHeart : emptyHeart;
            //Esta línea decide si cada corazón se muestra lleno o vacío.
            //Primero calcula: maxLives - mistakes : Eso representa cuántas vidas quedan.
            //ejemplo: 
            //maxLives = 3 : que es la cantidad de vidas maximas
            //mistakes = 1 : el error cometido
            //vidas restantes = 2 : la cantidad de vidas restantes despues del mistake
            //Entonces la condición: i < (maxLives - mistakes) decide si ese corazón debe estar lleno.
            
            //Ejemplo con mistakes = 1: maxLives - mistakes = 2
            //Para cada corazón:
            //i = 0 -> 0 < 2 true  -> fullHeart
            //i = 1 -> 1 < 2 true  -> fullHeart
            //i = 2 -> 2 < 2 false -> emptyHeart
            //resultado lleno - lleno - vacío

            hearts[i].enabled = true;//Asegura que la imagen del corazón esté visible.
            //Aunque el corazón esté vacío, sigue activo para que se vea el sprite vacío.
        }
    }
}