using System.IO;
using UnityEngine;
public class SaveService
{
    string GetPath(int slot)//Esta función crea la ruta del archivo de guardado.
        //recibe int slot que es el número de slot.
    {
        return Application.persistentDataPath + $"/sudoku_save_{slot}.json";
        //Luego devuelve una ruta como: Application.persistentDataPath + "/sudoku_save_0.json"
        //Application.persistentDataPath es una carpeta especial de Unity para guardar datos persistentes.
        //En Windows puede ser algo parecido a: C:/Users/usuario/AppData/LocalLow/Empresa/Juego
    }
    public void Save(SudokuSaveData data, int slot)//Esta función guarda una partida.
        //recibe SudokuSaveData data que son Los datos que se quieren guardar.
        //Ahí debería venir información como: tablero , notas , tiempo , errores , dificultad , pistas restantes
        //tambien recibe un int slot que es el slot donde guardar
    {
        string json = JsonUtility.ToJson(data, true);//Convierte data a texto JSON. JSON es un formato de texto para guardar datos
        //El true significa que el JSON se guarda bonito/formateado, con saltos de línea e indentación.
        //Ejemplo conceptual:
        //{
        // "remainingHints": 3,
        // "elapsedSeconds": 120
        //}
        var path = GetPath(slot);//Obtiene la ruta del archivo según el slot.
        //ejemplo : sudoku_save_0.json
        File.WriteAllText(path, json);//Escribe el JSON en el archivo. Si el archivo no existe, lo crea. Si ya existe, lo reemplaza.
        //en simple guarda la partida en disco
    }
    public bool Load(int slot, out SudokuSaveData data)//Esta función carga una partida.
        //recibe int slot que es El slot que quiere cargar. y lo devuelve como bool osea true si pudo cargar y false si no habia archivo
        //tambien devuelve por out SudokuSaveData data que son los datos cargados
    {
        string path = GetPath(slot);//Obtiene la ruta del archivo.
        if (!File.Exists(path))//Pregunta si el archivo no existe.
            //! significa “no”. Entonces esta condición significa: Si NO existe el archivo...
        {
            //Si no existe, no hay partida guardada.
            data = null;
            return false;
        }
        //Si el archivo sí existe:
        string json = File.ReadAllText(path);//Lee todo el contenido del archivo como texto.
        data = JsonUtility.FromJson<SudokuSaveData>(json);//Convierte el texto JSON de vuelta a un objeto SudokuSaveData.
        //O sea: archivo JSON -> objeto C#
        return true;//Devuelve que la carga fue exitosa.
    }
    public void Delete(int slot)//Esta función borra una partida guardada.
        ////recibe int slot que es El slot que quiere borrar
    {
        string path = GetPath(slot);//Primero obtiene la ruta
        if (File.Exists(path))//Luego revisa si existe
            File.Delete(path);//Si existe, lo borra
    }
    public bool Exists(int slot)//Esta función revisa si hay partida guardada en un slot.
        //recibe un int slot que es el slot que se quiere verificar si existe archivo
    {
        return File.Exists(GetPath(slot));//si existe arcivo entonces devuelve true si no existe devuelve false
    }
}