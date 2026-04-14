using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Inventario : MonoBehaviour
{
    // Acceso a los datos del jugador
    public GameSessionSO sesion;

    // Elementos globales del Inventario.
    private bool inventarioActivo = false;
    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement contenedorGloblal;

    //Elementos propios del intercambio de Slots.
    private VisualElement slotSeleccionado;
    private int indexOrigen = -1;
    private VisualElement iconoFantasma;


    //Lista donde estan almacenados los componentes visuales del Inventario de la UI.
    private List<VisualElement> slotsMochila = new List<VisualElement>();

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        contenedorGloblal = root.Q<VisualElement>("contenedor");

        CrearIconoFantasma();
        ConfigurarMochila();
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

    private void OnEnable()
    {
        if (sesion?.playerDATOS?.Inventario != null)
            sesion.playerDATOS.Inventario.OnInventarioChanged += ActualizarVisualizacion;
        ActualizarVisualizacion();
    }

    private void OnDisable()
    {
        if (sesion?.playerDATOS?.Inventario != null)
            sesion.playerDATOS.Inventario.OnInventarioChanged -= ActualizarVisualizacion;
    }

    private void ConfigurarMochila()
    {
        //Limpiamos la lista donde estan los botones
        slotsMochila.Clear();

        //En la configuracion del archivo uXML tenemos 6 VisualElement con nombre [sl1,sls...sl6] 
        // Necesitamos meter la referencia a esos botones/ mas que nada necesitamos que se le queden registrados eventos a cada uno
        for (int i = 1; i <= 6; i++)//Es por eso que hacemos la siguiente iteracion.
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


    //Punto de partida de la interaccion.
    private void OnPointerDownCustom(PointerDownEvent evt) //Siempre que un metodo se envie a un metodo de tipo RegisterCallBack es necesario el tipo de evento declarado
    {// el parametro evt tiene mucha informacion como por ejemplo : En que posicion esta el mouse, que boton presiono.
        if (evt.button != 0) return; //Esto es para solo usar el click izquierdo y asi no tomar el registro del click derecho


        // evt.currentTarget is like: este es lo que se presiono.
        // as VisualElement es un casteo. Como yo se que eso que esta ahi es de cierto tipo, a eso debo hacer la conversion.
        // currentTarget es un objeto generico contiene mucha informacion / para que me entienda quien lea esto es como : currentTarget es alguien que no quiere definir su sexo, pero tu a fuerzas le dices: Sos hombre cabron dejese de mamadas mi compa, que lo necesito como hombre
        VisualElement target = evt.currentTarget as VisualElement; 

        // Sacamos el indice del que se haya seleccionado.
        // Simplemente vamos a la mochile y le decimos. Ey tienes a alguien con este nombre? (Aunque creo que es : Ey tienes la referencia de este men en tu lista?
        int index = slotsMochila.IndexOf(target);

        // Tomamos los items que tiene nuestro player en su inventario.
        var inv = sesion.playerDATOS.Inventario.ListaItemsIn_ReadOnly;

        // Si el indice esta en el rango de 0 a 6  y la posicion en la lista es diferente de vacio y .itemData es diferente de vacio.
        // Puedo seguir.
        if (index >= 0 && index < inv.Count && inv[index]?.itemData != null)
        {
            //Guardamos momentaneamente el boton se picamos.
            slotSeleccionado = target;
            //Guardamos el indice, esto con el proposito de saber en que indice de la mochila de "botones" esta
            indexOrigen = index;


            //Este punto es critico en toda la interaccion.
            //Lo que hace realmente es "que se quede bloqueado el click", es como si definieramos algo asi.
            //Mientras yo tenga el click presionado vas a estar registrando a el boton.
            // ¿Porque es fundamental?
            //Porque sin el simplemente no funcionaria nada
            //El evento PointerDownEvent solo registra cuando uno hace click sobre algun elemento. no captura constancia de tener presionado.
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

        // Aqui ya podemos quitar el evento del metodo que arranca todo.
        // Dejamos de registrar slotSeleccionado.CapturePointer(evt.pointerId);
        slotSeleccionado.ReleasePointer(evt.pointerId);

        // OCULTAR ICONO FANTASMA
        iconoFantasma.style.visibility = Visibility.Hidden;

        //evt.position retorna las coordenadas exactas donde se solto el evento (Recordemos que PointerUpEvent ejecute un evento cuando se suelta el click)
        VisualElement debajo = root.panel.Pick(evt.position); //con Pick lo que hacemos es buscar quien esta debajo del click

        //Hacemos uso del metodo BuscarSlotEnPadres para buscar el elemento
        VisualElement slotDestino = BuscarSlotEnPadres(debajo);

        //si el slotDestino no es null.
        if (slotDestino != null)
        {
            // tomamos el indice destino.
            int indexDestino = slotsMochila.IndexOf(slotDestino);

            //Si el indice destino es diferente de -1 y indicedestino es diferente del indice de origen
            if (indexDestino != -1 && indexDestino != indexOrigen)
            {
                //Ejecutamos IntercambiarEnDatos
                IntercambiarEnDatos(indexOrigen, indexDestino);
            }
        }
        //slotSeleecionado lo volvemos null.
        slotSeleccionado = null;
        //Volvemos indice de origen a -1.
        indexOrigen = -1;
    }

    private void ActualizarPosicionFantasma(Vector2 mousePos)
    {
        // Centramos el icono en el cursor (restando la mitad de su tamaño)
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
        //Accedemos a el inventario del player y enviamos las posiciones a cambiar.
        sesion.playerDATOS.Inventario.IntercambiarPosiciones(a, b);
        //Actualizamos UI.
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

    // ... (Tu código de ActivarUI se mantiene igual)
    public void ActivarUI()
    {
        Debug.Log("EjecutandoAnimacion");
        inventarioActivo = !inventarioActivo;
        if (inventarioActivo)
        {
            contenedorGloblal.RemoveFromClassList("contenedor");
            contenedorGloblal.AddToClassList("contenedor-activo");
            //contenedorGloblal.pickingMode = PickingMode.Position;
        }
        else
        {
            contenedorGloblal.RemoveFromClassList("contenedor-activo");
            contenedorGloblal.AddToClassList("contenedor");
            //contenedorGloblal.pickingMode = PickingMode.Ignore;
        }
    }
}