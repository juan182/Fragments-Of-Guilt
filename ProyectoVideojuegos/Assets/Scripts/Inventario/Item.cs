using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;


[System.Serializable]
// Nombre que se le asigna cuando se crea es "NewItem" y Ubicacion : "Items/Item"
[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item")] //<- Esto crea la propiedad para poder crearlos desde creador de assets

public sealed class Item : ScriptableObject
{
    //Los tipos de Item que podemos elegir
    public enum TipoItem {Oro, PocionVerde, PocionRoja,Almas,FragmentoEspecial}

    [SerializeField] private TipoItem tipo; //Con esto en el inspector podremos ver una lista plegable de los enum que hay
                                            

    [HideInInspector][SerializeField] private string nombre; //Con esto le estamos asignando un nombre a el Item
    [SerializeField] private GameObject prefab; //Prefab del objeto aqui
    [SerializeField] public Sprite sprite;  //Esto es con el asignar una imagen y que el UI_INVENTARIO LA TOME
    

    // Diccionario de límites, cada Enum de tipo solo puede almacenar cierta cantidad
    private static readonly Dictionary<TipoItem, int> MaxCantidadPorTipo = new()
    {
        { TipoItem.Oro,          30 },
        { TipoItem.FragmentoEspecial,       3 },
        { TipoItem.PocionRoja,   15 },
        { TipoItem.PocionVerde,   3 },
    };

    
    //Metodo Get de tipo.
    public TipoItem Tipo => tipo; //Por ahora nadie lo usa y ni me acuerdo porque lo puse :V


    //Metodo Get y Sett del atributo nombre
    /// Nombre que se muestra en UI.  
    /// Se actualiza automáticamente cuando cambias el enum 
    public string Nombre
    {
        get => nombre;
        set => nombre = value ?? $"Unnamed_{tipo}";
    }

    // Este metodo obtiene el valor maximo definido para cada objeto del inventario.
    // recibe la clave de el mismo.
    public int CantidadMaxima => MaxCantidadPorTipo[tipo]; //Este metodo Get es usado en ItemContainer.

    // Metodo Get de tipo, pero no como enum si no como string.
    public string TipoItemString => tipo.ToString();

    // Se ejecuta no mas se haga una instancia de esta clase.
    private void OnValidate()
    {
        // Nombre tendra la referencia de un tipo como nombre
        // Si el tipo cambia el nombre igual
        // La logica de este metodo luego debe ser cambiada att:MIguelito
        nombre = TipoItemString;
    }
}
    