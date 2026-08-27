using UnityEngine;

public static class SudokuSceneRef
//Utilidad para resolver referencias de escena de forma robusta con prefabs.
//Problema que resuelve: al convertir la UI en prefabs, las referencias serializadas de los
//managers/controllers/systems pueden quedar apuntando a:
//  1. Objetos destruidos en runtime (la UI vieja)  -> Unity los devuelve como null.
//  2. Assets de prefab (si arrastras el prefab al Inspector) -> NO son null, pero tampoco son
//     instancias de la escena: activarlos o suscribirse a sus eventos NO funciona.
//Con estas funciones, cada script revalida su referencia y, si no es una instancia viva de la
//escena, la busca automáticamente (activa o inactiva) para "acoplarse" a la instancia real.
{
    //¿Es una instancia viva de la escena (no un asset de prefab ni un objeto destruido)?
    public static bool IsSceneInstance(Object obj)
    {
        if (obj == null)
            return false;
        if (obj is Component comp)
            return comp.gameObject.scene.IsValid();
        if (obj is GameObject go)
            return go.scene.IsValid();
        return true;//ScriptableObjects y demás assets no se descartan.
    }

    //Devuelve current si es una instancia válida de la escena; si no, la busca:
    //primero entre activos (FindFirstObjectByType) y luego incluyendo inactivos.
    public static T Resolve<T>(T current) where T : Component
    {
        if (current != null && IsSceneInstance(current))
            return current;
        T found = FindFirstObjectByType<T>();
        if (found != null)
            return found;
        foreach (var candidate in Resources.FindObjectsOfTypeAll<T>())
        {
            var go = candidate.gameObject;
            if (go.scene.IsValid() && (go.hideFlags & HideFlags.HideInHierarchy) == 0)
                return candidate;
        }
        return null;
    }

    //Igual que Resolve pero para paneles (GameObject): busca el primer objeto de la escena
    //que tenga el componente indicado (por ejemplo SudokuVictoryPanel), aunque esté inactivo.
    public static GameObject ResolvePanel(GameObject current, System.Type componentType)
    {
        if (current != null && IsSceneInstance(current))
            return current;
        var all = Resources.FindObjectsOfTypeAll(componentType);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] is Component comp && comp.gameObject.scene.IsValid() &&
                (comp.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0)
                return comp.gameObject;
        }
        return null;
    }

    //Devuelve current si no es null; si no, busca el ScriptableObject de sesión (SessionContext).
    public static T ResolveSession<T>(T current) where T : ScriptableObject
    {
        if (current != null)
            return current;
        var all = Resources.FindObjectsOfTypeAll<T>();
        return all != null && all.Length > 0 ? all[0] : null;
    }

    public static T FindFirstObjectByType<T>() where T : Component
    {
        return Object.FindFirstObjectByType<T>();
    }

    //Busca un objeto en las instancias de la escena (incluidos los prefabs instanciados en runtime,
    //como DatosNivel o PanelAjustes) que cumpla una condición. Sirve para que cada script de UI
    //pueda encontrar SOLO su elemento (por nombre, etiqueta...) sin depender de asignaciones.
    public static T FindInScene<T>(System.Func<T, bool> predicate) where T : Object
    {
        var all = Resources.FindObjectsOfTypeAll<T>();
        for (int i = 0; i < all.Length; i++)
        {
            T candidate = all[i];
            if (candidate == null)
                continue;
            if (candidate is Component comp && !comp.gameObject.scene.IsValid())
                continue;//Descarta assets de prefabs (no instancias).
            if (predicate(candidate))
                return candidate;
        }
        return null;
    }
}
