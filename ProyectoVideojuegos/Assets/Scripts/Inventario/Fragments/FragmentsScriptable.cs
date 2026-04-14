using System;
using UnityEngine;

[System.Serializable]
// Nombre que se le asigna cuando se crea es "NewItem" y Ubicacion : "Items/Item"
[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Fragmentos")] //<- Esto crea la propiedad para poder crearlos desde creador de assets
public class FragmentsScriptable : ScriptableObject
{
    public enum TipoFragmento{ Lanza, FragmentoMagia, FragmentoParry_AtaqueFuerte, libroEGVA}
    public TipoFragmento tipoFragmento;
    [SerializeField] private string nombre = "";
    [SerializeField] public Sprite sprite;


    public string Nombre
    {
        get {  return nombre; }
    }

    
    private void AsignarNombre()
    {
        switch (tipoFragmento)
        {
            case TipoFragmento.Lanza:
                nombre = "Lanza Ancestral";
                break;
            case TipoFragmento.FragmentoMagia:
                nombre = "Fragmento de Magia";
                break;
            case TipoFragmento.FragmentoParry_AtaqueFuerte:
                nombre = "Fragmetos de Ataques";
                break;
            case TipoFragmento.libroEGVA:
                nombre = "Libro perdido de EGVA";
                break;
        }
    }

    private void OnValidate()
    {
        AsignarNombre();
    }
}
