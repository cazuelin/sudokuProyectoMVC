using System.Collections.Generic;
using UnityEngine;

public class SudokuMenuSlotsController : MonoBehaviour//Este script controla los slots del menú principal.
//Su trabajo es crear visualmente los slots, revisar si tienen partida guardada, y conectar los botones de crear, continuar o borrar.
{
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
    [SerializeField] int slotCount = 3;//Cantidad de slots que se van a crear.
    //ejemplo : Si cambias slotCount a 5, debería crear 5 slots.
    [SerializeField] SudokuDifficultySelectionPanel difficultySelectionPanel;
    //Referencia al panel de selección de dificultad.
    //Se usa cuando el jugador presiona “crear” en un slot vacío.
    //difficultySelectionPanel permite elegir la dificultad antes de crear una partida nueva.
    readonly List<SudokuSaveSlotItem> slotViews = new List<SudokuSaveSlotItem>();//Esta lista guarda los slots visuales que se crean.
    //ejemplo:
    //slotViews[0] -> Slot 1
    //slotViews[1] -> Slot 2
    //slotViews[2] -> Slot 3
    //slotViews guarda las referencias a todos los slots creados para poder actualizarlos después.
    void Start()//Start es una función de Unity. Se ejecuta automáticamente cuando el GameObject entra en escena.
    {
        // Si no hay panel asignado, busca en la escena
        if (difficultySelectionPanel == null)//pregunta ¿No tengo asignado el panel de dificultad?
        {
            difficultySelectionPanel = FindFirstObjectByType<SudokuDifficultySelectionPanel>();
            //Si no está asignado desde el Inspector, intenta buscarlo automáticamente:
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
        //Deja de escuchar el evento de selección de dificultad.
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
        for (int i = 0; i < slotCount; i++)//recorre la cantidad maxima de slot para poder instanciarlo y posterior crearlos
        {
            var slot = Instantiate(prefab, container);//Crea una copia del prefab del slot dentro del contenedor.
            slot.Init(i);//Inicializa el slot con su índice.
            //Ejemplo:
            //i = 0 -> Slot 1
            //i = 1 -> Slot 2
            //i = 2 -> Slot 3

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
            //Si tienes 3 slots:
            //i = 0
            //i = 1
            //i = 2
            RefreshSlot(i);//Actualiza ese slot individual.
        //RefreshSlot revisa si hay guardado en ese índice.
        //Si no hay, muestra: Nuevo juego
        //si hay, muestra: Dificultad • Tiempo
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
            //RenderEmpty cambia la UI a algo como: 
            //Nuevo juego
            //[Crear]
            return;//Luego hace return para terminar.
        }
        view.RenderSaved((SudokuGameManager.Difficulty)data.difficulty, data.time);//Muestra el slot como guardado.
        //Convierte la dificultad guardada desde int a enum: (SudokuGameManager.Difficulty)data.difficulty
        //Y le pasa también el tiempo: data.time
        //Eso termina mostrando algo como:
        //Hard • 05:32
        //[Continuar] [Borrar]
    }
    void HandleContinueRequested(int slotIndex)//Esta función se llama cuando el jugador presiona Continuar en un slot.
        //recibe slotIndex : El slot que quiere continuar.
    {
        if (!saveManager.HasSlot(slotIndex))//pregunta ¿No existe guardado en este slot?
            //El ! significa “no”.
        {
            //Si no existe:
            RefreshSlot(slotIndex);//Actualiza visualmente el slot, probablemente dejándolo como vacío.
            return;//Y termina
        }
        //sí existe guardado
        sessionController.ContinueGame(slotIndex);
        //Le dice al SudokuSessionController: Prepara la sesión para cargar este slot y entra a la escena del juego.
        //Dentro de ContinueGame, se marca: LoadFromSave = true
    }

    void HandleDeleteRequested(int slotIndex)//Esta función se llama cuando el jugador presiona Borrar en un slot.
    {
        saveManager.DeleteSlot(slotIndex);//Borra el archivo guardado de ese slot.
        RefreshSlot(slotIndex);//Actualiza visualmente el slot.
        //Después de borrar, debería mostrarse como:
        //Nuevo juego
        //[Crear]
    }
    void HandleCreateRequested(int slotIndex)//Esta función se llama cuando el jugador presiona Crear en un slot vacío.
        //Le pasa el int slotIndex : Así el panel sabe en qué slot se creará la partida.
    {
        if (difficultySelectionPanel != null)//Si existe el panel de dificultad
        {
            difficultySelectionPanel.Open(slotIndex);//abre el panel de dificultad
        }            
    }
    void HandleDifficultySelected(int slotIndex, SudokuGameManager.Difficulty difficulty)
    //Esta función se llama cuando el jugador ya eligió una dificultad en el panel.
    //recibe int slotIndex : El slot donde se creará la partida.
    //recibe SudokuGameManager.Difficulty difficulty : La dificultad elegida.
    {
        sessionController.StartNewGame(slotIndex, difficulty);//Esto prepara la sesión
        //SelectedSlot = slotIndex
        //SelectedDifficulty = difficulty
        //LoadFromSave = false
        //y carga la escena del juego.
    }
}
