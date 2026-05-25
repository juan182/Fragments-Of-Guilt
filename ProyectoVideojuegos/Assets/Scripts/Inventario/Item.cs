using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item")]
public sealed class Item : ScriptableObject
{
    // ==========================================
    // ENUMS Y CONFIGURACIÓN ESTÁTICA
    // ==========================================
    public enum TipoItem { Oro, PocionVerde, PocionRoja, Almas, FragmentoEspecial }

    // Diccionario de límites de apilamiento máximos indexados por el tipo de ítem
    private static readonly Dictionary<TipoItem, int> MaxCantidadPorTipo = new()
    {
        { TipoItem.Oro,               30 },
        { TipoItem.FragmentoEspecial,  3 },
        { TipoItem.PocionRoja,        15 },
        { TipoItem.PocionVerde,        3 },
    };

    // ==========================================
    // VARIABLES Y CAMPOS SERIALIZADOS (CAMPOS)
    // ==========================================
    [SerializeField] private TipoItem tipo;              // Desplegable en el inspector para definir la identidad del ítem.
    [SerializeField] private GameObject prefab;          // Referencia al objeto físico que se instancia en el mundo 2D.
    [SerializeField] public Sprite sprite;               // Icono representativo texturizado que lee el UI_Inventario.

    [HideInInspector]
    [SerializeField] private string nombre;              // Identificador de cadena de texto interno expuesto por propiedad.

    // ==========================================
    // PROPIEDADES DE ACCESO (GETTERS Y SETTERS)
    // ==========================================
    public TipoItem Tipo => tipo;                        // Retorna el identificador del enumerador puro.

    public string Nombre
    {
        get => nombre;
        set => nombre = value ?? $"Unnamed_{tipo}";     // Asigna un nombre por defecto estructurado si el valor es nulo.
    }

    public int CantidadMaxima => MaxCantidadPorTipo[tipo]; // Devuelve el límite de stack consultando el diccionario estático.

    public string TipoItemString => tipo.ToString();     // Conversión explícita del enumerador actual a cadena de texto.

    // ==========================================
    // MÉTODOS DE VALIDACIÓN DEL EDITOR
    // ==========================================
    private void OnValidate()
    {
        nombre = TipoItemString;                         // Sincroniza automáticamente el nombre con el enum al cambiarlo en el Inspector.
    }
}