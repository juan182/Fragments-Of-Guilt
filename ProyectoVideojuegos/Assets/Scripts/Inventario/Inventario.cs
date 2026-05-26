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
    private const int MaxCapacidadInventario = 9;
    public event Action OnInventarioChanged;

    [SerializeField] private List<SlotInventario> listaItemsInventario = new List<SlotInventario>();
    // CORREGIDO: Ahora la Hotbar también almacena Slots para recordar la cantidad del stack (ej. 20 pociones)
    [SerializeField] private SlotInventario[] hotbarSlots = new SlotInventario[4];

    public void OnBeforeSerialize()
    {
        while (listaItemsInventario.Count < MaxCapacidadInventario) listaItemsInventario.Add(null);
        while (listaItemsInventario.Count > MaxCapacidadInventario) listaItemsInventario.RemoveAt(listaItemsInventario.Count - 1);
    }

    public void OnAfterDeserialize()
    {
        if (listaItemsInventario == null) listaItemsInventario = new List<SlotInventario>();
        while (listaItemsInventario.Count < MaxCapacidadInventario) listaItemsInventario.Add(null);

        if (hotbarSlots == null || hotbarSlots.Length != 4) hotbarSlots = new SlotInventario[4];
    }

    public Inventario()
    {
        listaItemsInventario = new List<SlotInventario>(MaxCapacidadInventario);
        for (int i = 0; i < MaxCapacidadInventario; i++) listaItemsInventario.Add(null);
        hotbarSlots = new SlotInventario[4];
    }

    public IReadOnlyList<SlotInventario> ListaItemsIn_ReadOnly { get { return listaItemsInventario.AsReadOnly(); } }
    public SlotInventario[] HotbarSlots => hotbarSlots;

    public bool AgregarItem(ItemContainer itemContainerArrives)
    {
        if (itemContainerArrives == null || itemContainerArrives.ItemDATA == null) return false;

        if (listaItemsInventario == null)
        {
            listaItemsInventario = new List<SlotInventario>(MaxCapacidadInventario);
            for (int i = 0; i < MaxCapacidadInventario; i++) listaItemsInventario.Add(null);
        }

        foreach (var slot in listaItemsInventario)
        {
            if (slot != null && slot.itemData != null && slot.itemData.TipoItemString.Equals(itemContainerArrives.ItemDATA.TipoItemString))
            {
                int espacioDisponible = slot.itemData.CantidadMaxima - slot.cantidad;
                if (espacioDisponible > 0)
                {
                    int cantidadATransferir = Mathf.Min(espacioDisponible, itemContainerArrives.Cantidad);
                    slot.cantidad += cantidadATransferir;
                    itemContainerArrives.Cantidad -= cantidadATransferir;

                    if (itemContainerArrives.Cantidad <= 0)
                    {
                        UnityEngine.Object.Destroy(itemContainerArrives.gameObject);
                        OnInventarioChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        for (int i = 0; i < listaItemsInventario.Count; i++)
        {
            if (listaItemsInventario[i] == null || listaItemsInventario[i].itemData == null)
            {
                listaItemsInventario[i] = new SlotInventario { itemData = itemContainerArrives.ItemDATA, cantidad = itemContainerArrives.Cantidad };
                UnityEngine.Object.Destroy(itemContainerArrives.gameObject);
                OnInventarioChanged?.Invoke();
                return true;
            }
        }

        OnInventarioChanged?.Invoke();
        return false;
    }

    public void IntercambiarPosiciones(int indexA, int indexB)
    {
        if (indexA >= 0 && indexA < listaItemsInventario.Count && indexB >= 0 && indexB < listaItemsInventario.Count)
        {
            SlotInventario temp = listaItemsInventario[indexA];
            listaItemsInventario[indexA] = listaItemsInventario[indexB];
            listaItemsInventario[indexB] = temp;
            OnInventarioChanged?.Invoke();
        }
    }

    public bool RemoverItem(int index)
    {
        if (index >= 0 && index < listaItemsInventario.Count)
        {
            listaItemsInventario[index] = null;
            OnInventarioChanged?.Invoke();
            return true;
        }
        return false;
    }

    // CORREGIDO: Transfiere el Slot completo de la mochila (con toda su cantidad) a la Hotbar
    public void AsignarAHotbar(int hotbarIndex, Item item, int mochilaIndexOrigen = -1)
    {
        if (hotbarIndex >= 0 && hotbarIndex < hotbarSlots.Length)
        {
            if (mochilaIndexOrigen >= 0 && mochilaIndexOrigen < listaItemsInventario.Count && listaItemsInventario[mochilaIndexOrigen] != null)
            {
                // Mover el contenedor completo de la mochila (mantiene el total recolectado, ej: 20)
                hotbarSlots[hotbarIndex] = listaItemsInventario[mochilaIndexOrigen];
                listaItemsInventario[mochilaIndexOrigen] = null;
            }
            else
            {
                // Asignación de respaldo por si no viene directamente de un arrastre físico
                hotbarSlots[hotbarIndex] = new SlotInventario { itemData = item, cantidad = 1 };
            }

            OnInventarioChanged?.Invoke();
        }
    }

    // CORREGIDO: Consume una unidad del stack y limpia el acceso rápido solo al llegar a 0
    public void UsarItemHotbar(int hotbarIndex, PlayerController jugador)
    {
        if (hotbarIndex < 0 || hotbarIndex >= hotbarSlots.Length || hotbarSlots[hotbarIndex] == null || hotbarSlots[hotbarIndex].itemData == null) return;

        SlotInventario slotHotbar = hotbarSlots[hotbarIndex];
        Item itemEnSlot = slotHotbar.itemData;

        if (itemEnSlot.Tipo == Item.TipoItem.PocionRoja && jugador.VidaJugador < 100)
        {
            jugador.VidaJugador += 25;
            slotHotbar.cantidad--;
        }
        else if (itemEnSlot.Tipo == Item.TipoItem.PocionAzul && jugador.Stamina < 100)
        {
            jugador.Stamina += 25;
            slotHotbar.cantidad--;
        }
        else return;

        // Si se acabaron todas las pociones del paquete, vaciamos el slot de la hotbar
        if (slotHotbar.cantidad <= 0)
        {
            hotbarSlots[hotbarIndex] = null;
        }

        OnInventarioChanged?.Invoke();
    }

    public int ObtenerCantidadTotalDeItem(Item item)
    {
        if (item == null) return 0;
        int total = 0;
        foreach (var slot in listaItemsInventario)
        {
            if (slot != null && slot.itemData == item) total += slot.cantidad;
        }
        return total;
    }

    public int ObtenerCantidadTotalItems()
    {
        int total = 0;
        foreach (var slot in listaItemsInventario) if (slot != null) total += slot.cantidad;
        return total;
    }

    public int ObtenerSlotsVacios()
    {
        int vacios = 0;
        foreach (var slot in listaItemsInventario) if (slot == null || slot.itemData == null) vacios++;
        return vacios;
    }
}