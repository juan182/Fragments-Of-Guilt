using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Gameplay, Paused, GameOver, Menu }

    // Identificadores para organizar tu arreglo de audios sin usar números sueltos
    public enum TipoSonidoUI { SonidoMuerteImpacto, MusicaGameOver, ClickBoton, HoverBoton }

    [Header("Referencias de Control")]
    public PlayerController datosJugador = null;
    public SceneManager_P sceneManager;
    public UI_Manager UI_Manager;
    public GameController1 gc1 = null;

    [Header("Sistema de Audio")]
    [SerializeField] private AudioSource audioSourceUI;

    // ------Uso Aqui---- Arreglo para almacenar múltiples audios de la interfaz (Tamaño asignado en el Inspector)
    [SerializeField] private AudioClip[] sonidosUI;

    [Header("Estado Actual")]
    [SerializeField] private GameState estadoDeJuego;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSourceUI == null) audioSourceUI = gameObject.AddComponent<AudioSource>();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (Instance != this) return;
        ChangeState(GameState.Menu);
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

                // ------Uso Aqui---- Detener música fúnebre si el jugador vuelve al menú principal
                if (audioSourceUI.isPlaying) audioSourceUI.Stop();
                break;

            case GameState.Gameplay:
                EstadoEnPartida(true);

                if (UI_Manager != null)
                {
                    if (UI_Manager.ui_Menu != null) UI_Manager.ui_Menu.gameObject.SetActive(false);
                    if (UI_Manager.gameOverUI != null) UI_Manager.gameOverUI.DesactivarPantallaImedatadamente();
                    UI_Manager.Activar_o_DesactivarEstadisticas();
                }

                // ------Uso Aqui---- Detener música de Game Over inmediatamente al reintentar el nivel
                if (audioSourceUI.isPlaying) audioSourceUI.Stop();
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

                // ------Uso Aqui---- REPRODUCCIÓN DE EFECTOS SIMULTÁNEOS PARA EL GAME OVER
                // 1. Sonido de impacto seco/orquesta al morir instantáneamente
                ReproducirEfectoUI((int)TipoSonidoUI.SonidoMuerteImpacto);

                // 2. Música melancólica de fondo que se queda en bucle mientras decide si reintentar
                ReproducirMusicaFondoUI((int)TipoSonidoUI.MusicaGameOver, true);
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

    #region REPRODUCTORES DE AUDIO PERSONALIZADOS

    // ------Uso Aqui---- Disparar efectos cortos (clicks, hovers, golpes) sin interrumpir lo que ya está sonando
    public void ReproducirEfectoUI(int indiceClip)
    {
        if (sonidosUI != null && indiceClip >= 0 && indiceClip < sonidosUI.Length)
        {
            if (sonidosUI[indiceClip] != null)
            {
                audioSourceUI.PlayOneShot(sonidosUI[indiceClip]);
            }
        }
    }

    // ------Uso Aqui---- Cambiar la música o ambiente de fondo de la interfaz (como la de Game Over)
    public void ReproducirMusicaFondoUI(int indiceClip, bool loop)
    {
        if (sonidosUI != null && indiceClip >= 0 && indiceClip < sonidosUI.Length)
        {
            if (sonidosUI[indiceClip] != null)
            {
                audioSourceUI.clip = sonidosUI[indiceClip];
                audioSourceUI.loop = loop;
                audioSourceUI.ignoreListenerPause = true; // CRÍTICO: Permite que suene aunque el Time.timeScale sea 0
                audioSourceUI.Play();
            }
        }
    }
    #endregion

    public GameState EstadoJuego => estadoDeJuego;
}