using UnityEngine;
using UnityEngine.UIElements;

public class MenuScript : MonoBehaviour
{
    [Header("Sistema de Audio Local (SFX)")]
    public AudioSource reproductor;
    public AudioClip[] sonidos;

    private UIDocument menuDocument;
    private VisualElement root;
    private VisualElement menuOpciones;

    private Button btnPlay;
    private Button btnOptions;
    private Button btnVolver;
    private Button btnCerrarTop;

    // ==========================================
    // COMPONENTE NUEVO: EL SLIDER DE AUDIO
    // ==========================================
    private Slider sliderMusica;

    private void Awake()
    {
        menuDocument = GetComponent<UIDocument>();
        root = menuDocument.rootVisualElement;

        if (reproductor == null) reproductor = gameObject.AddComponent<AudioSource>();

        AsignarReferencias();
        ConfigurarCallbacks();

        if (menuOpciones != null)
        {
            menuOpciones.style.display = DisplayStyle.None;
            menuOpciones.AddToClassList("oculto");
        }
    }

    private void AsignarReferencias()
    {
        btnPlay = root.Q<Button>("Play");
        btnOptions = root.Q<Button>("Options");
        btnVolver = root.Q<Button>("BtnVolver");
        btnCerrarTop = root.Q<Button>("BtnCerrarTop");
        menuOpciones = root.Q<VisualElement>("MenuOpciones");

        // ------Uso Aqui---- Buscamos el Slider por su nombre exacto del UXML
        sliderMusica = root.Q<Slider>("SliderMusica");
    }

    private void ConfigurarCallbacks()
    {
        if (btnPlay != null) btnPlay.RegisterCallback<ClickEvent>(OnJugarPressed);
        if (btnOptions != null) btnOptions.RegisterCallback<ClickEvent>(OnOpcionesOpenPressed);
        if (btnVolver != null) btnVolver.RegisterCallback<ClickEvent>(OnOpcionesClosePressed);
        if (btnCerrarTop != null) btnCerrarTop.RegisterCallback<ClickEvent>(OnOpcionesClosePressed);

        // ------Uso Aqui---- ESCUCHA DE CAMBIO DEL SLIDER
        if (sliderMusica != null)
        {
            sliderMusica.RegisterCallback<ChangeEvent<float>>(OnVolumenMusicaChanged);

            // Inicializamos el volumen de la música con el valor que tenga el slider por defecto en el UXML (Ej: 67 / 100 = 0.67)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CambiarVolumenMusica(sliderMusica.value / 100f);
            }
        }

        Button[] todosLosBotones = { btnPlay, btnOptions, btnVolver, btnCerrarTop };
        foreach (Button btn in todosLosBotones)
        {
            if (btn != null) btn.RegisterCallback<MouseEnterEvent>(OnBottonHoverIn);
        }
    }

    private void ReproducirSFXLocal(int indice)
    {
        if (reproductor != null && sonidos != null && indice >= 0 && indice < sonidos.Length)
        {
            if (sonidos[indice] != null) reproductor.PlayOneShot(sonidos[indice]);
        }
    }

    // ==========================================
    // MANEJADORES DE CALLBACKS (EVENTOS)
    // ==========================================

    // ------Uso Aqui---- Se ejecuta en tiempo real cada vez que el usuario mueve el slider
    private void OnVolumenMusicaChanged(ChangeEvent<float> evt)
    {
        // evt.newValue nos da el número entre 0 y 100 actual.
        float volumenConvertido = evt.newValue / 100f; // Lo pasamos a escala de 0.0 a 1.0

        if (GameManager.Instance != null)
        {
            // Le enviamos el nuevo valor directamente al AudioSource persistente del GameManager
            GameManager.Instance.CambiarVolumenMusica(volumenConvertido);
        }
    }

    private void OnBottonHoverIn(MouseEnterEvent evt)
    {
        ReproducirSFXLocal(1);
    }

    private void OnJugarPressed(ClickEvent evt)
    {
        ReproducirSFXLocal(0);
        if (GameManager.Instance != null && GameManager.Instance.sceneManager != null)
        {
            GameManager.Instance.sceneManager.CargarNivelesDeJuego("FASE1");
        }
    }

    private void OnOpcionesOpenPressed(ClickEvent evt)
    {
        ReproducirSFXLocal(0);
        if (menuOpciones != null)
        {
            menuOpciones.style.display = DisplayStyle.Flex;
            menuOpciones.schedule.Execute(() => {
                menuOpciones.RemoveFromClassList("oculto");
            }).StartingIn(10);
        }
    }

    private void OnOpcionesClosePressed(ClickEvent evt)
    {
        ReproducirSFXLocal(0);
        if (menuOpciones != null)
        {
            menuOpciones.AddToClassList("oculto");
            menuOpciones.schedule.Execute(() => {
                if (menuOpciones.ClassListContains("oculto"))
                {
                    menuOpciones.style.display = DisplayStyle.None;
                }
            }).StartingIn(300);
        }
    }
}