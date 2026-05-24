using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Inventario : MonoBehaviour
{
    public PlayerController playerController;
    // Acceso a los datos del jugador
    public GameSessionSO sesion;

    [Header("Estado de la ui")]
    private bool inventarioActivo = false;
    private UIDocument uiDocument;
    private VisualElement root;

    [Header("Contenedores Principales")]
    private VisualElement hudInGame;
    private VisualElement menuInventario;

    [Header("Elementos del HUD exploracion")]
    private ProgressBar barraVidaHud;
    private ProgressBar barraStaminaHUD;
    private List<VisualElement> slotsHotbar = new List<VisualElement>();


    [Header("Elementos del Menu Inventario")]
    private ProgressBar barraVidaMenu;
    private ProgressBar barraStaminaMenu;


    private VisualElement slotSeleccionado;
    private int indexOrigen = -1;
    private VisualElement iconoFantasma;
    private List<VisualElement> slotsMochila = new List<VisualElement>();

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        if (playerController == null) Debug.LogError("PlayerController sin asignar");

        // 1. Vincular los contenedores principales según tu UXML
        hudInGame = root.Q<VisualElement>("HudInGame");
        menuInventario = root.Q<VisualElement>("MenuInventario");

        // 2. Vincular componentes del HUD In-Game
        barraVidaHud = root.Q<ProgressBar>("BarraVidaHUD");
        barraStaminaHUD = root.Q<ProgressBar>("BarraStaminaHUD");
        ConfigurarHotbar();

        // 3. Vincular componentes del Menú de Estado
        barraVidaMenu = root.Q<ProgressBar>("BarraVidaMenu");
        barraStaminaMenu = root.Q<ProgressBar>("BarraStaminaMenu");

        CrearIconoFantasma();
        ConfigurarMochila();

        // Inicializar el estado de las pantallas (Comienzas explorando con el HUD activo)
        RefrescarVisibilidadPantallas();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ActivarUI();
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
        }
    }

    private void CrearIconoFantasma()
    {
        iconoFantasma = new VisualElement();
        iconoFantasma.style.width = 20;  // Ajusta al tamaño de tus slots
        iconoFantasma.style.height = 20;
        iconoFantasma.style.position = Position.Absolute; // Para moverlo libremente
        iconoFantasma.style.visibility = Visibility.Hidden;
        iconoFantasma.pickingMode = PickingMode.Ignore; // IMPORTANTE: Que no tape los clics
        iconoFantasma.style.opacity = 1f;
        root.Add(iconoFantasma);
    }

    private void ConfigurarMochila()
    {
        //Limpiamos la lista donde estan los botones
        slotsMochila.Clear();

        //En la configuracion del archivo uXML tenemos 6 VisualElement con nombre [sl1,sls...sl6] 
        // Necesitamos meter la referencia a esos botones/ mas que nada necesitamos que se le queden registrados eventos a cada uno
        for (int i = 1; i <= 9; i++)//Es por eso que hacemos la siguiente iteracion.
        {
            //Los botones son visualElement : Asi que debemos buscarlos desde root y hacer una Query en la Hierarchy del archivo uXML
            VisualElement slot = root.Q<VisualElement>($"sl{i}");
            //Si el slot no esta vacio. O mejor dicho es encontrado
            if (slot != null)
            {
                // Lo agregamos a nuestra lista
                slotsMochila.Add(slot);
                //asignamos PickingMode que permite la "seleccion"
                slot.pickingMode = PickingMode.Position;
                // Registramos los diferentes eventos.
                slot.RegisterCallback<PointerDownEvent>(OnPointerDownCustom, TrickleDown.TrickleDown);
                slot.RegisterCallback<PointerUpEvent>(OnPointerUpCustom, TrickleDown.TrickleDown);

                // Este es un evento que lo podemos denominar como : Sensor de movimiento :v porque se ejecutara cada vez que pasemos el cursor por  el elemento registrado.
                slot.RegisterCallback<PointerMoveEvent>(OnPointerMoveCustom, TrickleDown.TrickleDown);
            }
        }
    }

    private void ConfigurarHotbar()
    {
        slotsHotbar.Clear();
        string[] posiciones = { "Superior", "Izquierdo", "Derecho", "Inferior" };

        foreach (string pos in posiciones)
        {
            VisualElement slot = root.Q<VisualElement>($"HotbarSlot{pos}");
            if (slot != null)
            {
                slotsHotbar.Add(slot);
            }
        }
    }

    //Punto de partida de la interaccion.
    private void OnPointerDownCustom(PointerDownEvent evt)
    {
        if (evt.button != 0) return; //Esto es para solo usar el click izquierdo y asi no tomar el registro del click derecho

        VisualElement target = evt.currentTarget as VisualElement;

        int index = slotsMochila.IndexOf(target);

        var inv = sesion.playerDATOS.Inventario.ListaItemsIn_ReadOnly;

        if (index >= 0 && index < inv.Count && inv[index]?.itemData != null)
        {
            //Guardamos momentaneamente el boton se picamos.
            slotSeleccionado = target;
            //Guardamos el indice, esto con el proposito de saber en que indice de la mochila de "botones" esta
            indexOrigen = index;

            slotSeleccionado.CapturePointer(evt.pointerId);

            // ACTIVAR ICONO FANTASMA
            iconoFantasma.style.backgroundImage = new StyleBackground(inv[index].itemData.sprite);
            iconoFantasma.style.visibility = Visibility.Visible;
        }
    }

    private void OnPointerMoveCustom(PointerMoveEvent evt)
    {
        if (slotSeleccionado != null)
        {
            ActualizarPosicionFantasma(evt.position);
        }
    }

    // Punto de finalizacion de la interaccion.
    private void OnPointerUpCustom(PointerUpEvent evt)
    {
        if (slotSeleccionado == null || evt.button != 0) return;

        slotSeleccionado.ReleasePointer(evt.pointerId);

        // OCULTAR ICONO FANTASMA
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

    public void ActivarUI()
    {
        inventarioActivo = !inventarioActivo;
        RefrescarVisibilidadPantallas();

        if (inventarioActivo)
        {
            ActualizarVisualizacion();
        }
    }

    // Reemplaza por completo el método RefrescarVisibilidadPantallas de tu script con este:
    private void RefrescarVisibilidadPantallas()
    {
        if (inventarioActivo)
        {
            // 1. Desvanecer HUD In-game (Barras de exploración)
            hudInGame?.AddToClassList("oculto");
            if (hudInGame != null) hudInGame.pickingMode = PickingMode.Ignore;

            // 2. Aparecer Menú de Inventario gradualmente
            menuInventario?.RemoveFromClassList("oculto");
            if (menuInventario != null) menuInventario.pickingMode = PickingMode.Position;
        }
        else
        {
            // 1. Aparecer HUD In-game gradualmente
            hudInGame?.RemoveFromClassList("oculto");
            if (hudInGame != null) hudInGame.pickingMode = PickingMode.Position;

            // 2. Desvanecer Menú de Inventario
            menuInventario?.AddToClassList("oculto");
            if (menuInventario != null) menuInventario.pickingMode = PickingMode.Ignore;
        }
    }

    private void CambiarVisualizacionVida(int vidaActual, int vidaMaxima)
    {
        // Se ejecuta al instante cada vez que el player corre TakeDamage()
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
        // Se ejecutará cuando implementemos el gasto de stamina en el movimiento/ataque
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
}