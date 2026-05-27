using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Inventario : MonoBehaviour
{
    [Header("Referencias del Jugador")]
    public PlayerController playerController;
    public GameSessionSO sesion;

    [Header("Nombres de Equipamiento (Editables)")]
    [SerializeField] private string textoLanza = "LANZA CELESTIAL";
    [SerializeField] private string textoMagia = "FRAGMENTO MAGIA";
    [SerializeField] private string textoParry = "CONTRAATAQUE FUERTE";
    [SerializeField] private string textoLibro = "LIBRO DE EGVA";
    [SerializeField] private string textoVacio = "VACIO";

    private UIDocument uiDocument;
    private VisualElement root;
    private bool inventarioActivo = false;

    private VisualElement hudInGame;
    private VisualElement menuInventario;
    private ProgressBar barraVidaHud;
    private ProgressBar barraStaminaHUD;
    private ProgressBar barraVidaMenu;
    private ProgressBar barraStaminaMenu;

    private VisualElement slotEquip1;
    private VisualElement slotEquip2;
    private VisualElement slotEquip3;
    private VisualElement slotEquip4;

    private List<VisualElement> slotsHotbar = new List<VisualElement>();
    private List<VisualElement> slotsMochila = new List<VisualElement>();
    private VisualElement slotSeleccionado;
    private VisualElement iconoFantasma;
    private int indexOrigen = -1;

    private VisualElement cartelInteraccion;
    private Label textoAccion;

    private Item itemArrastrandose;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        if (playerController == null) Debug.LogError("PlayerController sin asignar");

        hudInGame = root.Q<VisualElement>("HudInGame");
        menuInventario = root.Q<VisualElement>("MenuInventario");

        cartelInteraccion = root.Q<VisualElement>("CartelInteraccion");
        textoAccion = root.Q<Label>("TextoAccion");

        slotEquip1 = root.Q<VisualElement>("EquipSlot1");
        slotEquip2 = root.Q<VisualElement>("EquipSlot2");
        slotEquip3 = root.Q<VisualElement>("EquipSlot3");
        slotEquip4 = root.Q<VisualElement>("EquipSlot4");

        barraVidaHud = root.Q<ProgressBar>("BarraVidaHUD");
        barraStaminaHUD = root.Q<ProgressBar>("BarraStaminaHUD");
        barraVidaMenu = root.Q<ProgressBar>("BarraVidaMenu");
        barraStaminaMenu = root.Q<ProgressBar>("BarraStaminaMenu");

        ConfigurarHotbar();
        CrearIconoFantasma();
        ConfigurarMochila();
        RefrescarVisibilidadPantallas();
    }

    public void AlternarCartelInteraccion(bool mostrar, string mensaje = "")
    {
        if (cartelInteraccion == null || textoAccion == null) return;
        if (inventarioActivo)
        {
            cartelInteraccion.AddToClassList("cartel-oculto");
            return;
        }

        if (mostrar)
        {
            textoAccion.text = mensaje;
            cartelInteraccion.RemoveFromClassList("cartel-oculto");
        }
        else
        {
            cartelInteraccion.AddToClassList("cartel-oculto");
        }
    }

    private void RefrescarVisibilidadPantallas()
    {
        if (inventarioActivo)
        {
            hudInGame?.AddToClassList("oculto");
            if (hudInGame != null) hudInGame.pickingMode = PickingMode.Ignore;
            menuInventario?.RemoveFromClassList("oculto");
            if (menuInventario != null) menuInventario.pickingMode = PickingMode.Position;
            cartelInteraccion?.AddToClassList("cartel-oculto");
        }
        else
        {
            hudInGame?.RemoveFromClassList("oculto");
            if (hudInGame != null) hudInGame.pickingMode = PickingMode.Position;
            menuInventario?.AddToClassList("oculto");
            if (menuInventario != null) menuInventario.pickingMode = PickingMode.Ignore;
        }
    }

    private void OnEnable()
    {
        if (sesion?.playerDATOS?.Inventario != null)
            sesion.playerDATOS.Inventario.OnInventarioChanged += ActualizarVisualizacion;

        if (playerController != null)
        {
            playerController.OnVidaChanged += CambiarVisualizacionVida;
            playerController.OnStaminaChanged += CambiarVisualizacionStamina;
            playerController.OnHabilidadesChanged += ActualizarSlotsEquipamiento;
        }
        ActualizarVisualizacion();
    }

    private void OnDisable()
    {
        if (sesion?.playerDATOS?.Inventario != null)
            sesion.playerDATOS.Inventario.OnInventarioChanged -= ActualizarVisualizacion;

        if (playerController != null)
        {
            playerController.OnVidaChanged -= CambiarVisualizacionVida;
            playerController.OnStaminaChanged -= CambiarVisualizacionStamina;
            playerController.OnHabilidadesChanged -= ActualizarSlotsEquipamiento;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) ActivarUI();

        if (Input.GetKeyDown(KeyCode.Alpha1)) UsarItemHotbarPorTeclado(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UsarItemHotbarPorTeclado(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UsarItemHotbarPorTeclado(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UsarItemHotbarPorTeclado(3);
    }

    private void ConfigurarHotbar()
    {
        slotsHotbar.Clear();
        string[] posiciones = { "Superior", "Izquierdo", "Derecho", "Inferior" };
        for (int i = 0; i < posiciones.Length; i++)
        {
            VisualElement slot = root.Q<VisualElement>($"HotbarSlot{posiciones[i]}");
            if (slot != null)
            {
                slot.userData = i;
                slotsHotbar.Add(slot);
            }
        }
    }

    private void ConfigurarMochila()
    {
        slotsMochila.Clear();
        for (int i = 1; i <= 9; i++)
        {
            VisualElement slot = root.Q<VisualElement>($"sl{i}");
            if (slot != null)
            {
                slotsMochila.Add(slot);
                slot.pickingMode = PickingMode.Position;
                slot.RegisterCallback<PointerDownEvent>(OnPointerDownCustom, TrickleDown.TrickleDown);
                slot.RegisterCallback<PointerUpEvent>(OnPointerUpCustom, TrickleDown.TrickleDown);
                slot.RegisterCallback<PointerMoveEvent>(OnPointerMoveCustom, TrickleDown.TrickleDown);
            }
        }
    }

    private void CrearIconoFantasma()
    {
        iconoFantasma = new VisualElement();
        iconoFantasma.style.width = 45;
        iconoFantasma.style.height = 45;
        iconoFantasma.style.position = Position.Absolute;
        iconoFantasma.style.visibility = Visibility.Hidden;
        iconoFantasma.pickingMode = PickingMode.Ignore;
        iconoFantasma.style.opacity = 0.8f;
        root.Add(iconoFantasma);
    }

    private void OnPointerDownCustom(PointerDownEvent evt)
    {
        if (evt.button != 0) return;
        VisualElement target = evt.currentTarget as VisualElement;
        int index = slotsMochila.IndexOf(target);
        var inv = sesion.playerDATOS.Inventario.ListaItemsIn_ReadOnly;

        if (index >= 0 && index < inv.Count && inv[index]?.itemData != null)
        {
            slotSeleccionado = target;
            indexOrigen = index;
            itemArrastrandose = inv[index].itemData;

            slotSeleccionado.CapturePointer(evt.pointerId);
            iconoFantasma.style.backgroundImage = new StyleBackground(itemArrastrandose.sprite);
            iconoFantasma.style.visibility = Visibility.Visible;
            ActualizarPosicionFantasma(evt.position);
        }
    }

    private void OnPointerMoveCustom(PointerMoveEvent evt)
    {
        if (slotSeleccionado != null) ActualizarPosicionFantasma(evt.position);
    }

    private void OnPointerUpCustom(PointerUpEvent evt)
    {
        if (slotSeleccionado == null || evt.button != 0) return;
        slotSeleccionado.ReleasePointer(evt.pointerId);
        iconoFantasma.style.visibility = Visibility.Hidden;

        if (menuInventario != null) menuInventario.pickingMode = PickingMode.Ignore;
        VisualElement debajo = root.panel.Pick(evt.position);
        if (menuInventario != null) menuInventario.pickingMode = PickingMode.Position;

        VisualElement slotHotbarDestino = BuscarSlotHotbarEnPadres(debajo);
        if (slotHotbarDestino != null && itemArrastrandose != null)
        {
            if (itemArrastrandose.Tipo == Item.TipoItem.PocionRoja || itemArrastrandose.Tipo == Item.TipoItem.PocionAzul)
            {
                int hotbarIndex = (int)slotHotbarDestino.userData;
                sesion.playerDATOS.Inventario.AsignarAHotbar(hotbarIndex, itemArrastrandose, indexOrigen);
            }
        }
        else
        {
            VisualElement slotMochilaDestino = BuscarSlotEnPadres(debajo);
            if (slotMochilaDestino != null)
            {
                int indexDestino = slotsMochila.IndexOf(slotMochilaDestino);
                if (indexDestino != -1 && indexDestino != indexOrigen) IntercambiarEnDatos(indexOrigen, indexDestino);
            }
        }

        slotSeleccionado = null;
        indexOrigen = -1;
        itemArrastrandose = null;
    }

    private void ActualizarPosicionFantasma(Vector2 mousePos)
    {
        iconoFantasma.style.left = mousePos.x - 22;
        iconoFantasma.style.top = mousePos.y - 22;
    }

    private VisualElement BuscarSlotEnPadres(VisualElement elemento)
    {
        while (elemento != null)
        {
            if (slotsMochila.Contains(elemento)) return elemento;
            elemento = elemento.parent;
        }
        return null;
    }

    private VisualElement BuscarSlotHotbarEnPadres(VisualElement elemento)
    {
        while (elemento != null)
        {
            if (slotsHotbar.Contains(elemento)) return elemento;
            elemento = elemento.parent;
        }
        return null;
    }

    private void IntercambiarEnDatos(int a, int b)
    {
        sesion.playerDATOS.Inventario.IntercambiarPosiciones(a, b);
        ActualizarVisualizacion();
    }

    public void ActivarUI()
    {
        inventarioActivo = !inventarioActivo;
        RefrescarVisibilidadPantallas();
        if (inventarioActivo) ActualizarVisualizacion();
    }

    private void UsarItemHotbarPorTeclado(int index)
    {
        sesion.playerDATOS.Inventario.UsarItemHotbar(index, playerController);
    }

    public void ActualizarVisualizacion()
    {
        if (sesion?.playerDATOS?.Inventario == null) return;
        var inventarioComponente = sesion.playerDATOS.Inventario;
        var listaItems = inventarioComponente.ListaItemsIn_ReadOnly;

        // UI de la Mochila
        for (int i = 0; i < slotsMochila.Count; i++)
        {
            VisualElement iconoUI = slotsMochila[i].Q<VisualElement>("img");
            Label cantidadUI = slotsMochila[i].Q<Label>("numeroContador");
            if (i < listaItems.Count && listaItems[i]?.itemData != null)
            {
                iconoUI.style.backgroundImage = new StyleBackground(listaItems[i].itemData.sprite);
                iconoUI.style.display = DisplayStyle.Flex;
                cantidadUI.text = listaItems[i].cantidad.ToString();
                cantidadUI.style.display = DisplayStyle.Flex;
            }
            else
            {
                iconoUI.style.display = DisplayStyle.None;
                cantidadUI.style.display = DisplayStyle.None;
            }
        }

        // UI de la Hotbar (CORREGIDO)
        for (int i = 0; i < slotsHotbar.Count; i++)
        {
            VisualElement iconoHotbar = slotsHotbar[i].Q<VisualElement>("img");
            Label cantidadHotbar = slotsHotbar[i].Q<Label>("numeroContador");

            // Accedemos al Slot completo para extraer el item y la cantidad real asignada
            SlotInventario slotRapido = inventarioComponente.HotbarSlots[i];

            if (slotRapido != null && slotRapido.itemData != null)
            {
                iconoHotbar.style.backgroundImage = new StyleBackground(slotRapido.itemData.sprite);
                iconoHotbar.style.display = DisplayStyle.Flex;

                // Pintamos de forma dinámica el número real (ej: si eran 20, dirá 20)
                cantidadHotbar.text = slotRapido.cantidad.ToString();
                cantidadHotbar.style.display = DisplayStyle.Flex;
            }
            else
            {
                iconoHotbar.style.display = DisplayStyle.None;
                cantidadHotbar.style.display = DisplayStyle.None;
            }
        }
    }

    private void CambiarVisualizacionVida(int vidaActual, int vidaMaxima)
    {
        if (barraVidaHud != null)
        {
            barraVidaHud.highValue = vidaMaxima;
            barraVidaHud.value = vidaActual;
            barraVidaHud.title = $"{vidaActual} / {vidaMaxima}";
        }
        if (barraVidaMenu != null)
        {
            barraVidaMenu.highValue = vidaMaxima;
            barraVidaMenu.value = vidaActual;
            barraVidaMenu.title = $"{vidaActual} / {vidaMaxima}";
        }
    }

    private void CambiarVisualizacionStamina(int staminaActual, int staminaMaxima)
    {
        if (barraStaminaHUD != null)
        {
            barraStaminaHUD.highValue = staminaMaxima;
            barraStaminaHUD.value = staminaActual;
            barraStaminaHUD.title = $"{staminaActual} / {staminaMaxima}";
        }
        if (barraStaminaMenu != null)
        {
            barraStaminaMenu.highValue = staminaMaxima;
            barraStaminaMenu.value = staminaActual;
            barraStaminaMenu.title = $"{staminaActual} / {staminaMaxima}";
        }
    }

    private void ActualizarSlotsEquipamiento(bool tieneLanza, bool tieneMagia, bool tieneParry, bool tieneLibro)
    {
        AjustarEstadoSlot(slotEquip1, tieneLanza, textoLanza);
        AjustarEstadoSlot(slotEquip2, tieneMagia, textoMagia);
        AjustarEstadoSlot(slotEquip3, tieneParry, textoParry);
        AjustarEstadoSlot(slotEquip4, tieneLibro, textoLibro);
    }

    private void AjustarEstadoSlot(VisualElement slotElement, bool estaDesbloqueado, string textoHabilidad)
    {
        if (slotElement == null) return;
        VisualElement imagenIcono = slotElement.Q<VisualElement>("img");
        Label tituloLabel = slotElement.Q<Label>("TituloSlot");
        if (imagenIcono != null && tituloLabel != null)
        {
            if (estaDesbloqueado)
            {
                imagenIcono.RemoveFromClassList("imagen-fragmento-oculto");
                tituloLabel.text = textoHabilidad;
            }
            else
            {
                imagenIcono.AddToClassList("imagen-fragmento-oculto");
                tituloLabel.text = textoVacio;
            }
        }
    }
}