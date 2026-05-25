using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Inventario : MonoBehaviour
{
    [Header("Referencias del Jugador")]
    public PlayerController playerController;            // Controlador lógico del personaje.
    public GameSessionSO sesion;                         // Contenedor de persistencia de datos del jugador.

    [Header("Nombres de Equipamiento (Editables)")]
    [SerializeField] private string textoLanza = "LANZA CELESTIAL";
    [SerializeField] private string textoMagia = "FRAGMENTO MAGIA";
    [SerializeField] private string textoParry = "CONTRAATAQUE FUERTE";
    [SerializeField] private string textoLibro = "LIBRO DE EGVA";
    [SerializeField] private string textoVacio = "VACIO";

    // Elementos del Toolkit UI (Estructura Base)
    private UIDocument uiDocument;                      // Componente puente de la interfaz.
    private VisualElement root;                         // Nodo contenedor raíz del UXML.
    private bool inventarioActivo = false;              // Control de estado de la ventana.

    // Elementos del Contenedor HUD e Inventario
    private VisualElement hudInGame;                    // Panel principal en estado de exploración.
    private VisualElement menuInventario;               // Panel principal en estado de pausa/mochila.
    private ProgressBar barraVidaHud;                   // Medidor de salud en la interfaz de acción.
    private ProgressBar barraStaminaHUD;                // Medidor de energía en la interfaz de acción.
    private ProgressBar barraVidaMenu;                  // Medidor de salud dentro del menú de estado.
    private ProgressBar barraStaminaMenu;               // Medidor de energía dentro del menú de estado.

    // Slots de Equipamiento / Habilidades
    private VisualElement slotEquip1;                   // Espacio para la Lanza.
    private VisualElement slotEquip2;                   // Espacio para el Fragmento de Magia.
    private VisualElement slotEquip3;                   // Espacio para el Fragmento de Parry.
    private VisualElement slotEquip4;                   // Espacio para el Libro EGVA.

    // Colecciones y Gestión de Arrastre (Drag and Drop)
    private List<VisualElement> slotsHotbar = new List<VisualElement>();    // Lista de accesos rápidos del HUD.
    private List<VisualElement> slotsMochila = new List<VisualElement>();   // Lista de cuadrículas de la mochila.
    private VisualElement slotSeleccionado;             // Elemento de interfaz que retiene el puntero.
    private VisualElement iconoFantasma;                // Sprite flotante durante el arrastre.
    private int indexOrigen = -1;                       // Posición inicial del ítem arrastrado.

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        if (playerController == null) Debug.LogError("PlayerController sin asignar");

        // Enlace de paneles contenedores principales
        hudInGame = root.Q<VisualElement>("HudInGame");
        menuInventario = root.Q<VisualElement>("MenuInventario");

        // Enlace de ranuras fijas de equipamiento
        slotEquip1 = root.Q<VisualElement>("EquipSlot1");
        slotEquip2 = root.Q<VisualElement>("EquipSlot2");
        slotEquip3 = root.Q<VisualElement>("EquipSlot3");
        slotEquip4 = root.Q<VisualElement>("EquipSlot4");

        // Enlace de medidores visuales
        barraVidaHud = root.Q<ProgressBar>("BarraVidaHUD");
        barraStaminaHUD = root.Q<ProgressBar>("BarraStaminaHUD");
        barraVidaMenu = root.Q<ProgressBar>("BarraVidaMenu");
        barraStaminaMenu = root.Q<ProgressBar>("BarraStaminaMenu");

        // Inicialización de componentes internos
        ConfigurarHotbar();
        CrearIconoFantasma();
        ConfigurarMochila();
        RefrescarVisibilidadPantallas(); // Configura visibilidad inicial (HUD activo por defecto).
    }

    private void OnEnable()
    {
        // Registro a eventos del inventario y del personaje
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
        // Remoción de suscripciones para evitar fugas en memoria
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
        if (Input.GetKeyDown(KeyCode.I)) ActivarUI();   // Alterna la visibilidad del menú.
    }

    // CONFIGURACIÓN E INICIALIZACIÓN DE ELEMENTOS
    private void ConfigurarHotbar()
    {
        slotsHotbar.Clear();
        string[] posiciones = { "Superior", "Izquierdo", "Derecho", "Inferior" };

        foreach (string pos in posiciones)
        {
            VisualElement slot = root.Q<VisualElement>($"HotbarSlot{pos}");
            if (slot != null) slotsHotbar.Add(slot);
        }
    }

    private void ConfigurarMochila()
    {
        slotsMochila.Clear();
        for (int i = 1; i <= 9; i++) // Recorre las 9 ranuras del inventario físico.
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
        iconoFantasma.style.width = 20;
        iconoFantasma.style.height = 20;
        iconoFantasma.style.position = Position.Absolute;
        iconoFantasma.style.visibility = Visibility.Hidden;
        iconoFantasma.pickingMode = PickingMode.Ignore; // Evita que interfiera con la detección de slots debajo.
        iconoFantasma.style.opacity = 1f;
        root.Add(iconoFantasma);
    }

    // GESTIÓN DEL SISTEMA DRAG AND DROP (PUNTERO)
    private void OnPointerDownCustom(PointerDownEvent evt)
    {
        if (evt.button != 0) return;// Restringe la acción solo al clic izquierdo.

        VisualElement target = evt.currentTarget as VisualElement;
        int index = slotsMochila.IndexOf(target);
        var inv = sesion.playerDATOS.Inventario.ListaItemsIn_ReadOnly;

        if (index >= 0 && index < inv.Count && inv[index]?.itemData != null)
        {
            slotSeleccionado = target;
            indexOrigen = index;
            slotSeleccionado.CapturePointer(evt.pointerId); // Bloquea la interacción en este elemento.

            iconoFantasma.style.backgroundImage = new StyleBackground(inv[index].itemData.sprite);
            iconoFantasma.style.visibility = Visibility.Visible;
        }
    }

    private void OnPointerMoveCustom(PointerMoveEvent evt)
    {
        if (slotSeleccionado != null) ActualizarPosicionFantasma(evt.position);
    }

    private void OnPointerUpCustom(PointerUpEvent evt)
    {
        if (slotSeleccionado == null || evt.button != 0) return;

        slotSeleccionado.ReleasePointer(evt.pointerId); // Libera el bloqueo de puntero.
        iconoFantasma.style.visibility = Visibility.Hidden;

        VisualElement debajo = root.panel.Pick(evt.position);
        VisualElement slotDestino = BuscarSlotEnPadres(debajo);

        if (slotDestino != null)
        {
            int indexDestino = slotsMochila.IndexOf(slotDestino);
            if (indexDestino != -1 && indexDestino != indexOrigen)
            {
                IntercambiarEnDatos(indexOrigen, indexDestino);
            }
        }
        slotSeleccionado = null;
        indexOrigen = -1;
    }

    private void ActualizarPosicionFantasma(Vector2 mousePos)
    {
        iconoFantasma.style.left = mousePos.x - (iconoFantasma.layout.width / 2);
        iconoFantasma.style.top = mousePos.y - (iconoFantasma.layout.height / 2);
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

    private void IntercambiarEnDatos(int a, int b)
    {
        sesion.playerDATOS.Inventario.IntercambiarPosiciones(a, b);
        ActualizarVisualizacion();
    }

    // ACTUALIZACIÓN DE INTERFAZ Y ESTADÍSTICAS
    public void ActivarUI()
    {
        inventarioActivo = !inventarioActivo;
        RefrescarVisibilidadPantallas();

        if (inventarioActivo) ActualizarVisualizacion();
    }

    private void RefrescarVisibilidadPantallas()
    {
        if (inventarioActivo)
        {
            hudInGame?.AddToClassList("oculto");
            if (hudInGame != null) hudInGame.pickingMode = PickingMode.Ignore;

            menuInventario?.RemoveFromClassList("oculto");
            if (menuInventario != null) menuInventario.pickingMode = PickingMode.Position;
        }
        else
        {
            hudInGame?.RemoveFromClassList("oculto");
            if (hudInGame != null) hudInGame.pickingMode = PickingMode.Position;

            menuInventario?.AddToClassList("oculto");
            if (menuInventario != null) menuInventario.pickingMode = PickingMode.Ignore;
        }
    }

    public void ActualizarVisualizacion()
    {
        if (sesion?.playerDATOS?.Inventario == null) return;
        var listaItems = sesion.playerDATOS.Inventario.ListaItemsIn_ReadOnly;

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

    // SISTEMA DE EQUIPAMIENTO / HABILIDADES
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
                imagenIcono.RemoveFromClassList("imagen-fragmento-oculto"); // Muestra la imagen.
                tituloLabel.text = textoHabilidad;
            }
            else
            {
                imagenIcono.AddToClassList("imagen-fragmento-oculto");    // Oculta la imagen vía USS.
                tituloLabel.text = textoVacio;
            }
        }
    }
}