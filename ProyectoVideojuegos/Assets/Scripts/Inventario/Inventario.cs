using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SlotInventario
{
    public Item itemData;
    public int cantidad;
}

[System.Serializable]
public class Inventario : ISerializationCallbackReceiver
{
    private const int MaxCapacidadInventario = 6;
    public event Action OnInventarioChanged;

   
    [SerializeField] private List<SlotInventario> listaItemsInventario = new List<SlotInventario>();


    // Metodos de serializacion de la informacion ! Aun no comprendo muy bien su comportamiento.
    public void OnBeforeSerialize()
    {
        while (listaItemsInventario.Count < MaxCapacidadInventario)
        {
            listaItemsInventario.Add(null);
        }
        while (listaItemsInventario.Count > MaxCapacidadInventario)
        {
            listaItemsInventario.RemoveAt(listaItemsInventario.Count - 1);
        }
    }

    public void OnAfterDeserialize()
    {
        if (listaItemsInventario == null)
        {
            listaItemsInventario = new List<SlotInventario>();
        }


        while (listaItemsInventario.Count < MaxCapacidadInventario)
        {
            listaItemsInventario.Add(null);
        }
    }

    // Constructor para cuando alguien inicie la clase
    public Inventario()
    {
        listaItemsInventario = new List<SlotInventario>(MaxCapacidadInventario);
        for (int i = 0; i < MaxCapacidadInventario; i++)
        {
            listaItemsInventario.Add(null);
        }
    }

    //Esto es un metodo Get de solo lectura, para que las demas clases puedan leer que es lo que hay en el inventario.
    public IReadOnlyList<SlotInventario> ListaItemsIn_ReadOnly { get { return listaItemsInventario.AsReadOnly(); } }




    //Metodo para agregar un item
    public bool AgregarItem(ItemContainer itemContainerArrives)
    {
        
        if (itemContainerArrives == null)
        {
            Debug.LogWarning("Error en el item : null");
            return false;
        }

        if (itemContainerArrives.ItemDATA == null)
        {
            Debug.LogWarning("itemContainerArrives.ItemDATA es null");
            return false;
        }

        if (listaItemsInventario == null)
        {
            listaItemsInventario = new List<SlotInventario>(MaxCapacidadInventario);
            for (int i = 0; i < MaxCapacidadInventario; i++)
            {
                listaItemsInventario.Add(null);
            }
        }

        Debug.Log($"Intentando agregar: {itemContainerArrives.ItemDATA.Nombre} x{itemContainerArrives.Cantidad}");
        Debug.Log($"Slots totales: {listaItemsInventario.Count}, Slots vacíos: {ObtenerSlotsVacios()}");

        // Si el item que nos llega lo tenemos en nuestro inventario, simplemente lo acumulamos.
        foreach (var slot in listaItemsInventario)
        {
            if (slot != null && slot.itemData != null && slot.itemData.TipoItemString.Equals(itemContainerArrives.ItemDATA.TipoItemString))
            {
                Debug.Log($"Se encontro un slot:  {slot.itemData.Nombre} (cantidad: {slot.cantidad}/{slot.itemData.CantidadMaxima})");

                // Calculamos el espacio disponible
                int espacioDisponible = slot.itemData.CantidadMaxima - slot.cantidad;

                if (espacioDisponible > 0)
                {
                    // Calculamos cuánto transferir
                    int cantidadATransferir = Mathf.Min(espacioDisponible, itemContainerArrives.Cantidad);

                    // Transferimos
                    slot.cantidad += cantidadATransferir;
                    itemContainerArrives.Cantidad -= cantidadATransferir;

                    Debug.Log($"Transferidos {cantidadATransferir} items. Slot ahora tiene {slot.cantidad}/{slot.itemData.CantidadMaxima}");

                    // Si la cantidad que le queda a el item que acabamos de recoger es 0 lo destruimos.
                    if (itemContainerArrives.Cantidad <= 0)
                    {
                        Debug.Log("La cantidad del item fue acumalada completamente");
                        UnityEngine.Object.Destroy(itemContainerArrives.gameObject);
                        OnInventarioChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        // Si del Item que recoletamos aun hay cantidad y tenemos espacio disponible pues lo ponemos en otro slot.
        for (int i = 0; i < listaItemsInventario.Count; i++)
        {
            if (listaItemsInventario[i] == null || listaItemsInventario[i].itemData == null)
            {
                listaItemsInventario[i] = new SlotInventario
                {
                    itemData = itemContainerArrives.ItemDATA,
                    cantidad = itemContainerArrives.Cantidad
                };

                Debug.Log($"Se asignoa cantidad restante : {itemContainerArrives.Cantidad}");
                UnityEngine.Object.Destroy(itemContainerArrives.gameObject);
                OnInventarioChanged?.Invoke();
                return true;
            }
        }

        // Si no se cumplieron ninguna de las anteriores condiciones, chulo, no hay espacio
        Debug.LogWarning($"Inventario LLENO mi fafa No hay espacio para {itemContainerArrives.Cantidad} {itemContainerArrives.ItemDATA.Nombre}");
        Debug.LogWarning($"Slots en inventario: {listaItemsInventario.Count}");
        Debug.LogWarning($"Slots vacíos: {ObtenerSlotsVacios()}");

        OnInventarioChanged?.Invoke(); // Notificar incluso del fallo
        return false;
    }

    //Metodo para intercambiar los indices de la lista
    public void IntercambiarPosiciones(int indexA, int indexB)
    {
        //Verificamos que los índices estén en rango
        if (indexA >= 0 && indexA < listaItemsInventario.Count && indexB >= 0 && indexB < listaItemsInventario.Count)
        {
            //Guardamos temporalmente la información de A
            SlotInventario temp = listaItemsInventario[indexA];
            //Asignamos B a A
            listaItemsInventario[indexA] = listaItemsInventario[indexB];
            //Asignamos el temporal (A) a B
            listaItemsInventario[indexB] = temp;
            OnInventarioChanged?.Invoke();
        }
    }

    // ✅ CORREGIDO: En lugar de RemoveAt (que encoge la lista), asignamos null
    public bool RemoverItem(int index)
    {
        if (index >= 0 && index < listaItemsInventario.Count)
        {
            listaItemsInventario[index] = null; // Reemplazar por null, no eliminar de la lista
            OnInventarioChanged?.Invoke();
            return true;
        }
        return false;
    }

    // Metodo para resumen del inventario
    public int ObtenerCantidadTotalItems()
    {
        int total = 0;
        foreach (var slot in listaItemsInventario)
        {
            if (slot != null)
            {
                total += slot.cantidad;
            }
        }
        return total;
    }

    // Metodo para slots vacios
    public int ObtenerSlotsVacios()
    {
        int vacios = 0;
        foreach (var slot in listaItemsInventario)
        {
            if (slot == null || slot.itemData == null)
            {
                vacios++;
            }
        }
        return vacios;
    }
}