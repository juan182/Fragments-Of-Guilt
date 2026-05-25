using UnityEngine;
using UnityEngine.UIElements;

public class MenuScript : MonoBehaviour
{
    // ==========================================
    // REFERENCIAS DE UI ELEMENTS (ELEMENTOS Y CONTENEDORES)
    // ==========================================
    private UIDocument menuDocument;
    private VisualElement root;
    private VisualElement menuOpciones;

    // ==========================================
    // COMPONENTES DE INTERACCIÓN (BOTONES)
    // ==========================================
    private Button btnPlay;
    private Button btnOptions;
    private Button btnVolver;
    private Button btnCerrarTop;

    // ==========================================
    // CICLO DE VIDA DE UNITY (LIFECYCLE)
    // ==========================================
    private void Awake()
    {
        menuDocument = GetComponent<UIDocument>();
        root = menuDocument.rootVisualElement;

        AsignarReferencias();
        ConfigurarCallbacks();

        // SOLUCIÓN RADICAL: Forzar el apagado directo en el sistema de layout de Unity
        if (menuOpciones != null)
        {
            menuOpciones.style.display = DisplayStyle.None;
            menuOpciones.AddToClassList("oculto");
        }
    }

    // ==========================================
    // CONFIGURACIÓN E INICIALIZACIÓN
    // ==========================================
    private void AsignarReferencias()
    {
        btnPlay = root.Q<Button>("Play");
        btnOptions = root.Q<Button>("Options");
        btnVolver = root.Q<Button>("BtnVolver");
        btnCerrarTop = root.Q<Button>("BtnCerrarTop");
        menuOpciones = root.Q<VisualElement>("MenuOpciones");
    }

    private void ConfigurarCallbacks()
    {
        btnPlay.RegisterCallback<PointerDownEvent>(OnJugarPressed, TrickleDown.TrickleDown);
        btnOptions.RegisterCallback<PointerDownEvent>(OnOpcionesOpenPressed, TrickleDown.TrickleDown);
        btnVolver.RegisterCallback<PointerDownEvent>(OnOpcionesClosePressed, TrickleDown.TrickleDown);
        btnCerrarTop.RegisterCallback<PointerDownEvent>(OnOpcionesClosePressed, TrickleDown.TrickleDown);
    }

    // ==========================================
    // CONTROLADORES DE EVENTOS (MANEJADORES DE CALLBACKS)
    // ==========================================
    private void OnJugarPressed(PointerDownEvent evt)
    {
        GameManager.Instance.sceneManager.CargarNivelesDeJuego("Nivel1");
    }

    private void OnOpcionesOpenPressed(PointerDownEvent evt)
    {
        if (menuOpciones != null)
        {
            menuOpciones.style.display = DisplayStyle.Flex; // Primero lo habilitamos en el Layout
            // Usamos un pequeño truco de temporizador para que Unity procese el cambio de display antes de la opacidad
            menuOpciones.schedule.Execute(() => {
                menuOpciones.RemoveFromClassList("oculto");  // Desvanece hacia arriba (Alpha 1)
            }).StartingIn(10);
        }
    }

    private void OnOpcionesClosePressed(PointerDownEvent evt)
    {
        if (menuOpciones != null)
        {
            menuOpciones.AddToClassList("oculto");       // Inicia desvanecimiento hacia abajo (Alpha 0)

            // Esperamos los 300ms (0.3s) que dura tu animación antes de cortar el display por completo
            menuOpciones.schedule.Execute(() => {
                if (menuOpciones.ClassListContains("oculto"))
                {
                    menuOpciones.style.display = DisplayStyle.None;
                }
            }).StartingIn(300);
        }
    }
}