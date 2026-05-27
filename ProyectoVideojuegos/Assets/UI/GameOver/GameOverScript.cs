using UnityEngine;
using UnityEngine.UIElements;

public class GameOverScript : MonoBehaviour
{
    [Header("Sistema de Audio Local (SFX)")]
    public AudioSource reproductorSFX;

    // Arreglo indexado: [0] = Impacto/Sonido de Muerte, [1] = Hover de Botón
    public AudioClip[] sonidosMuerte;

    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement contenedorPrincipal;

    private Button btnReintentar;
    private Button btnMenuPrincipal;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        if (reproductorSFX == null)
        {
            reproductorSFX = gameObject.AddComponent<AudioSource>();
        }

        reproductorSFX.ignoreListenerPause = true;

        contenedorPrincipal = root.Q<VisualElement>("PantallaMuerteRoot");
        btnReintentar = root.Q<Button>("BtnReintentar");
        btnMenuPrincipal = root.Q<Button>("BtnMenuPrincipal");

        ConfigurarCallbacks();
        DesactivarPantallaImedatadamente();
    }

    private void ConfigurarCallbacks()
    {
        if (btnReintentar != null)
            btnReintentar.RegisterCallback<ClickEvent>(OnReintentarPressed);

        if (btnMenuPrincipal != null)
            btnMenuPrincipal.RegisterCallback<ClickEvent>(OnMenuPrincipalPressed);

        Button[] botonesGameOver = { btnReintentar, btnMenuPrincipal };
        foreach (Button btn in botonesGameOver)
        {
            if (btn != null)
            {
                btn.RegisterCallback<MouseEnterEvent>(OnBotonHoverIn);
            }
        }
    }

    private void ReproducirSFXLocal(int indice)
    {
        if (reproductorSFX != null && sonidosMuerte != null && indice >= 0 && indice < sonidosMuerte.Length)
        {
            if (sonidosMuerte[indice] != null)
            {
                reproductorSFX.PlayOneShot(sonidosMuerte[indice]);
            }
        }
    }

    private void OnBotonHoverIn(MouseEnterEvent evt)
    {
        ReproducirSFXLocal(1); // Sonido de Hover [1]
    }

    private void OnReintentarPressed(ClickEvent evt)
    {
        Debug.Log("Click detectado en Reintentar con TimeScale 0.");

        // ------Uso Aqui---- REANUDAR MÚSICA GLOBAL
        // Le avisamos al GameManager que despause la pista de una hora antes de cargar la escena
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReanudarMusicaGlobal();

            if (GameManager.Instance.sceneManager != null)
            {
                GameManager.Instance.sceneManager.ReiniciarNivel();
            }
        }
    }

    private void OnMenuPrincipalPressed(ClickEvent evt)
    {
        Debug.Log("Click detectado en Menú Principal.");

        // NOTA: Si va al menú principal, el GameManager en su estado "GameState.Menu" 
        // ya tiene la lógica para manejar el flujo general o podrías dejar que continúe.
        if (GameManager.Instance != null && GameManager.Instance.sceneManager != null)
        {
            GameManager.Instance.sceneManager.IrAMenu("MenuPrincipal");
        }
    }

    public void ActivarPantallaConFade()
    {
        if (contenedorPrincipal == null) return;
        contenedorPrincipal.style.display = DisplayStyle.Flex;

        // ------Uso Aqui---- PAUSAR MÚSICA GLOBAL Y TOCAR IMPACTO
        // 1. Pausamos la música de fondo para dar paso al silencio dramático
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PausarMusicaGlobal();
        }

        // 2. Tocamos el SFX de muerte local [0] en completa prioridad
        ReproducirSFXLocal(0);

        contenedorPrincipal.schedule.Execute(() => {
            contenedorPrincipal.AddToClassList("visible");
        }).StartingIn(10);
    }

    public void DesactivarPantallaImedatadamente()
    {
        if (contenedorPrincipal == null) return;
        contenedorPrincipal.RemoveFromClassList("visible");
        contenedorPrincipal.style.display = DisplayStyle.None;
    }
}