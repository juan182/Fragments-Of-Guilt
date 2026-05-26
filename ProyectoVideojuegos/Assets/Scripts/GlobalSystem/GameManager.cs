using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Gameplay, Paused, GameOver, Menu }

    [Header("Referencias de Control")]
    public PlayerController datosJugador = null;
    public SceneManager_P sceneManager;
    public UI_Manager UI_Manager;
    public GameController1 gc1 = null;

    [Header("Sistema de Música Central")]
    [SerializeField] private AudioSource audioSourceMusica;
    [SerializeField] private AudioClip musicaPrincipalDeUnaHora; // ------Uso Aqui---- Tu pista de 1 hora

    [Header("Estado Actual")]
    [SerializeField] private GameState estadoDeJuego;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSourceMusica != null)
            {
                audioSourceMusica.loop = true;
                audioSourceMusica.ignoreListenerPause = true;
                IniciarMusicaGlobal();
            }
        }
        else if (Instance != this)
        {
            // ------Uso Aqui---- 
            // Desvinculamos el objeto de la jerarquía activa inmediatamente 
            // para que el Inspector de Unity no intente leer sus componentes de UI en este fotograma
            transform.SetParent(null);

            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (Instance != this) return;
        ChangeState(GameState.Menu); // El estado cambia aquí tranquilamente
    }

    private void OnEnable()
    {
        if (Instance == this || Instance == null) PlayerController.OnPlayerDeath += GameOver;
    }

    private void OnDisable()
    {
        if (Instance == this) PlayerController.OnPlayerDeath -= GameOver;
    }

    private void GameOver()
    {
        ChangeState(GameState.GameOver);
    }

    public void ChangeState(GameState newState)
    {
        estadoDeJuego = newState;
        switch (estadoDeJuego)
        {
            case GameState.Menu:
                EstadoEnPartida(true);

                if (UI_Manager != null)
                {
                    if (UI_Manager.ui_Menu != null) UI_Manager.ui_Menu.gameObject.SetActive(true);
                    if (UI_Manager.gameOverUI != null) UI_Manager.gameOverUI.DesactivarPantallaImedatadamente();
                }

                // NOTA: Ya no detenemos la música aquí, se mantiene sonando de fondo fluidamente.
                break;

            case GameState.Gameplay:
                EstadoEnPartida(true);

                if (UI_Manager != null)
                {
                    if (UI_Manager.ui_Menu != null) UI_Manager.ui_Menu.gameObject.SetActive(false);
                    if (UI_Manager.gameOverUI != null) UI_Manager.gameOverUI.DesactivarPantallaImedatadamente();
                    UI_Manager.Activar_o_DesactivarEstadisticas();
                }
                break;

            case GameState.Paused:
                EstadoEnPartida(false);
                break;

            case GameState.GameOver:
                EstadoEnPartida(false);

                if (UI_Manager != null && UI_Manager.gameOverUI != null)
                {
                    UI_Manager.gameOverUI.ActivarPantallaConFade();
                }
                // Si deseas que en Game Over baje un poco el volumen de la música para dar tensión, podrías modularlo aquí.
                break;
        }
    }

    private void EstadoEnPartida(bool condicion)
    {
        if (condicion)
        {
            if (Time.timeScale == 0f) Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    // ==========================================
    // CONTROLADORES DE AUDIO CENTRAL
    // ==========================================

    private void IniciarMusicaGlobal()
    {
        if (audioSourceMusica != null && musicaPrincipalDeUnaHora != null && !audioSourceMusica.isPlaying)
        {
            audioSourceMusica.clip = musicaPrincipalDeUnaHora;
            audioSourceMusica.volume = 0.5f; // Volumen seguro por defecto al arrancar
            audioSourceMusica.Play();
            Debug.Log("Música global iniciada correctamente.");
        }
    }

    // ------Uso Aqui---- Método público que llamará el Slider de la UI para ajustar el volumen (recibe de 0.0 a 1.0)
    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        if (audioSourceMusica != null)
        {
            audioSourceMusica.volume = Mathf.Clamp01(nuevoVolumen);
        }
    }

    public GameState EstadoJuego => estadoDeJuego;

    public void PausarMusicaGlobal()
    {
        if (audioSourceMusica != null && audioSourceMusica.isPlaying)
        {
            audioSourceMusica.Pause();
            Debug.Log("Música global pausada en el segundo: " + audioSourceMusica.time);
        }
    }

    // ------Uso Aqui---- Reanuda la música exactamente desde donde se pausó
    public void ReanudarMusicaGlobal()
    {
        if (audioSourceMusica != null && !audioSourceMusica.isPlaying)
        {
            audioSourceMusica.UnPause();
            Debug.Log("Música global reanudada.");
        }
    }
}