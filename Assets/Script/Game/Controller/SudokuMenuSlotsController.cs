using System.Collections.Generic;
using UnityEngine;

public class SudokuMenuSlotsController : MonoBehaviour//Este script controla los slots del menú principal.
//Su trabajo es crear visualmente los slots, revisar si tienen partida guardada, y conectar los botones de crear, continuar o borrar.
{
    static readonly SudokuRules.SudokuVariant[] SlotVariants =
    {
        SudokuRules.SudokuVariant.Variant2x3,
        SudokuRules.SudokuVariant.Standard3x3,
        SudokuRules.SudokuVariant.Variant3x4,
        SudokuRules.SudokuVariant.Standard4x4
    };

    static readonly string[] SlotTitles =
    {
        "Sudoku 2x3",
        "Sudoku 3x3",
        "Sudoku 3x4",
        "Sudoku 4x4"
    };

    [SerializeField] SudokuSaveManager saveManager;//Referencia al sistema de guardado.
    //Sirve para revisar, cargar datos o borrar slots.
    //saveManager le dice al menú si un slot está vacío o tiene partida guardada.
    [SerializeField] SudokuSessionController sessionController;//Referencia al controlador de sesión.
    //Sirve para pasar del menú a la escena del juego.
    //sessionController prepara los datos de sesión y carga la escena Game.
    [SerializeField] SudokuSaveSlotItem prefab;//Es el prefab visual de un slot.
    //prefab es el molde visual de cada slot del menú.
    [SerializeField] Transform container;//Es el contenedor donde se van a crear los slots.
    //container es el lugar donde aparecen los slots en pantalla.
    [SerializeField] SudokuDifficultySelectionPanel difficultySelectionPanel;
    //Referencia al panel de selección de dificultad.
    [SerializeField] GameObject difficultyPanelPrefab;
    //PREFAB COMPARTIDO del panel de dificultad (el mismo que usan las escenas de juego).
    //Si la escena del menú no tiene el panel (o lo eliminaste al convertirlo en prefab),
    //se instancia este prefab: editar el prefab actualiza el menú y el juego a la vez.
    readonly List<SudokuSaveSlotItem> slotViews = new List<SudokuSaveSlotItem>();//Esta lista guarda los slots visuales que se crean.
    void Start()//Start es una función de Unity. Se ejecuta automáticamente cuando el GameObject entra en escena.
    {
        if (difficultySelectionPanel == null)//pregunta ¿No tengo asignado el panel de dificultad?
        {
            difficultySelectionPanel = FindInactiveComponent<SudokuDifficultySelectionPanel>();
            //Si no está asignado desde el Inspector, intenta buscarlo automáticamente:
            //IMPORTANTE: el panel del menú está INACTIVO en la escena, por eso se busca con
            //FindInactiveComponent (FindFirstObjectByType no encuentra objetos inactivos).
        }
        if (difficultySelectionPanel == null && difficultyPanelPrefab != null)
        {
            //La escena no tiene el panel (se convirtió en prefab): se instancia el prefab compartido
            //como hijo del Canvas del menú, para que sea el MISMO que aparece en las escenas de juego.
            var canvas = FindFirstObjectByType<Canvas>();
            var go = Instantiate(difficultyPanelPrefab, canvas != null ? canvas.transform : null);
            go.SetActive(false);
            difficultySelectionPanel = go.GetComponent<SudokuDifficultySelectionPanel>();
        }
        if (difficultySelectionPanel != null)//Si encontró el panel o si existe el difficultySelectionPanel
        {            
            difficultySelectionPanel.OnDifficultySelected += HandleDifficultySelected;//se suscribe al evento
        //OnDifficultySelected significa : Cuando el jugador elija una dificultad, llama HandleDifficultySelected.
        }
        if (prefab == null || container == null)
        //primera validacion prefab == null : pregunta ¿si no tengo asignado algun prefab?
        //segunda validacion container == null : pregunta ¿si no tengo asignado algun container?
        //Si falta alguno, no puede construir el menú.
        {
            return;//y se detiene
        }
        //Si todo está bien y pasa todas las validaciones pasa aca
        BuildView();//Primero crea los slots visuale
        RefreshAll();//Luego actualiza cada slot según tenga o no partida guardada.
    }

    void OnDestroy()//OnDestroy se ejecuta cuando el GameObject se destruye o cuando se cambia de escena.
        //Su trabajo es desuscribirse de eventos.
    {
        if (difficultySelectionPanel != null)////Si encontró el panel o si existe el difficultySelectionPanel
            difficultySelectionPanel.OnDifficultySelected -= HandleDifficultySelected;//se desuscribe
        foreach (var slot in slotViews)//Luego recorre todos los slots creados
            //slotViews contiene los SudokuSaveSlotItem que se crearon en BuildView.
        {
            if (slot == null)//Si algún slot ya fue destruido o es null.
                continue;//lo salta

            //Después se desuscribe de los eventos de cada slot:
            slot.OnContinueRequested -= HandleContinueRequested;
            slot.OnDeleteRequested -= HandleDeleteRequested;
            slot.OnCreateRequested -= HandleCreateRequested;
            //Esto evita que un slot destruido siga intentando llamar métodos de este controlador.
        }
    }
    void BuildView()//Esta función construye visualmente los slots del menú.
    {
        foreach (Transform child in container)//reccore todos los hijos del contenedor
            Destroy(child.gameObject);//si hay alguno los borra todos los hijos actuales
        //Esto limpia cualquier slot viejo que ya estuviera dentro del contenedor.
        //Por ejemplo, si había slots creados antes, los elimina para reconstruirlos.

        slotViews.Clear();//Limpia la lista interna de slots.
        //Esto es importante porque si destruiste los objetos visuales, también debes limpiar sus referencias guardadas.
        for (int i = 0; i < SlotVariants.Length; i++)//Crea exactamente un slot por cada variante disponible.
        {
            var slot = Instantiate(prefab, container);//Crea una copia del prefab del slot dentro del contenedor.
            slot.Init(i, SlotTitles[i]);//Inicializa el slot con su índice y su nombre visible.

            //Luego conecta eventos del slot
            slot.OnContinueRequested += HandleContinueRequested;
            slot.OnDeleteRequested += HandleDeleteRequested;
            slot.OnCreateRequested += HandleCreateRequested;
            //esto significa Si este slot pide continuar, borrar o crear, llama al método correspondiente del controlador.

            slotViews.Add(slot);//Guarda el slot creado en la lista.
            //Así después RefreshAll puede recorrerlos y actualizarlos.
        }
    }
    void RefreshAll()//Esta función actualiza todos los slots.
    {
        for (int i = 0; i < slotViews.Count; i++)//Recorre la lista de slots creados.
            RefreshSlot(i);//Actualiza ese slot individual.
    }
    void RefreshSlot(int slotIndex)//Esta función actualiza visualmente un slot específico.
        //recibe int slotIndex : El índice del slot que se quiere actualizar.
    {
        var view = slotViews[slotIndex];//Obtiene el objeto visual del slot.
        //Ese objeto es un SudokuSaveSlotItem.
        var data = saveManager.GetSlotData(slotIndex);//Intenta cargar los datos guardados de ese slot.
        //Si el slot tiene partida guardada,
        //data tendrá información como : difficulty , time , board , mistakes , remainingHints
        //Si no tiene guardado, data será null.
        if (data == null)//Si no hay datos
        {
            view.RenderEmpty();//muestra el slot como vacío.
            return;//Luego hace return para terminar.
        }
        view.RenderSaved((SudokuGameManager.Difficulty)data.difficulty, data.time);//Muestra el slot como guardado.
    }
    void HandleContinueRequested(int slotIndex)//Esta función se llama cuando el jugador presiona Continuar en un slot.
    {
        if (!saveManager.HasSlot(slotIndex))//pregunta ¿No existe guardado en este slot?
        {
            RefreshSlot(slotIndex);//Actualiza visualmente el slot, probablemente dejándolo como vacío.
            return;//Y termina
        }
        if (slotIndex < 0 || slotIndex >= SlotVariants.Length)
            return;
        sessionController.ContinueGame(slotIndex, SlotVariants[slotIndex]);
    }

    void HandleDeleteRequested(int slotIndex)//Esta función se llama cuando el jugador presiona Borrar en un slot.
    {
        saveManager.DeleteSlot(slotIndex);//Borra el archivo guardado de ese slot.
        RefreshSlot(slotIndex);//Actualiza visualmente el slot.
    }
    void HandleCreateRequested(int slotIndex)//Esta función se llama cuando el jugador presiona Crear en un slot vacío.
    {
        if (slotIndex < 0 || slotIndex >= SlotVariants.Length)
            return;

        if (difficultySelectionPanel != null)//Abre el panel de dificultad directamente.
        {
            difficultySelectionPanel.Open(slotIndex);
        }
    }
    void HandleDifficultySelected(int slotIndex, SudokuGameManager.Difficulty difficulty)
    {
        if (slotIndex < 0 || slotIndex >= SlotVariants.Length)
            return;

        sessionController.StartNewGame(slotIndex, difficulty, SlotVariants[slotIndex]);
    }
    T FindInactiveComponent<T>() where T : Component
    {
        //FindFirstObjectByType no encuentra objetos inactivos, por eso los paneles del menú
        //(que están inactivos en la escena) se buscan con Resources.FindObjectsOfTypeAll.
        //Filtra por scene.IsValid() para descartar assets de prefabs cargados en memoria.
        foreach (var candidate in Resources.FindObjectsOfTypeAll<T>())
        {
            var go = candidate.gameObject;
            if (go.scene.IsValid() && (go.hideFlags & HideFlags.HideInHierarchy) == 0)
                return candidate;
        }
        return null;
    }
}
