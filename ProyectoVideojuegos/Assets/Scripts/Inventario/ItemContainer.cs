using System;
using UnityEngine;

public class ItemContainer : MonoBehaviour
{
    [Header("Asigne aqui el scriptableObject")]
    [SerializeField] private Item itemData;


    [Header("Asigne aqui la cantidad del item")]
    [SerializeField] private int cantidad;

    // Metodos Getter y Setter
    public Item ItemDATA => itemData;  // Metodo Get para obtener informacion del Item en cuestion

    public int Cantidad
    {
        get => cantidad;
        set => cantidad = Mathf.Clamp(value, 0, itemData.CantidadMaxima);
    }

}
