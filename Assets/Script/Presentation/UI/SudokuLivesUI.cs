using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SudokuLivesUI : MonoBehaviour
{
    [SerializeField] Image[] hearts;
    [SerializeField] Sprite fullHeart;
    [SerializeField] Sprite emptyHeart;
    [SerializeField] SessionContext sessionContext;

    public void Setup(Image[] hearts, Sprite fullHeart, Sprite emptyHeart)
    //Reasigna las referencias de la UI. Lo usa SudokuGameUI al construir la interfaz nueva.
    {
        this.hearts = hearts;
        this.fullHeart = fullHeart;
        this.emptyHeart = emptyHeart;
    }

    public void UpdateLives(int mistakes)
    {
        //Resolución perezosa: si los corazones viven en un prefab instanciado en runtime
        //(DatosNivel), se buscan solos en la escena por nombre (Live*, heart, corazon).
        if (hearts == null || hearts.Length == 0)
            EnsureHearts();
        if (hearts == null || hearts.Length == 0)
            return;

        //MODO SIN VIDAS: se desactiva la interfaz de corazones (no se muestra el HUD de vidas).
        if (PlayerPrefs.GetInt("Sudoku_LivesEnabled", 1) == 0)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] != null)
                    hearts[i].enabled = false;
            }
            return;
        }
        //Si las vidas están activadas, se asegura de que los corazones vuelvan a verse.
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].enabled = true;
        }

        if (sessionContext == null)
        {
            var contexts = Resources.FindObjectsOfTypeAll<SessionContext>();
            if (contexts != null && contexts.Length > 0)
                sessionContext = contexts[0];
        }

        int maxLives = sessionContext != null ? sessionContext.MaxMistakes : hearts.Length;
        maxLives = Mathf.Clamp(maxLives, 1, hearts.Length);
        int remainingLives = Mathf.Clamp(maxLives - mistakes, 0, maxLives);

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
                continue;

            bool filled = i < remainingLives;
            if (emptyHeart != null)//Si hay sprite de corazón vacío, se usa.
            {
                hearts[i].sprite = filled ? fullHeart : emptyHeart;
                hearts[i].color = Color.white;
            }
            else//Si no hay sprite vacío, se atenúa el corazón lleno.
            {
                hearts[i].sprite = fullHeart;
                hearts[i].color = filled ? Color.white : new Color(1f, 1f, 1f, 0.25f);
            }
            hearts[i].enabled = true;
        }
    }
    void EnsureHearts()//Busca las imágenes de vidas en los prefabs instanciados (por nombre).
    {
        var found = new List<Image>();
        var all = Resources.FindObjectsOfTypeAll<Image>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null || !all[i].gameObject.scene.IsValid())
                continue;//Descarta assets de prefabs (solo instancias de escena).
            string n = all[i].gameObject.name.ToLowerInvariant();
            if (n.StartsWith("live") || n.Contains("heart") || n.Contains("corazon"))
                found.Add(all[i]);
        }
        if (found.Count == 0)
            return;
        hearts = found.ToArray();
        if (fullHeart == null && hearts.Length > 0)
            fullHeart = hearts[0].sprite;//Usa el sprite del propio prefab si no se asignó otro.
    }
}
