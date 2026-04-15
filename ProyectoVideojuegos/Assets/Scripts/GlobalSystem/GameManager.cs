using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    public enum GameState { Gameplay, Paused, GameOver, Menu }
    public PlayerController datosJugador = null;
    public SceneManager_P sceneManager;
    public UI_Manager UI_Manager;

    public GameController1 gc1;

    public bool tieneArma { get; private set; } = false;

    //
    [SerializeField]private GameState currentState;
    //

    public bool IsPlayerDead { get; private set; } = false; //Para saber si el jugador ha muerto

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this) // Si ya existe uno y no soy yo...
        {
            Debug.Log("Clon detectado y destruido en: " + gameObject.scene.name);
            Destroy(gameObject);
            return; // ¡CRÍTICO! Detiene la ejecución del resto del Awake
        }
    }

    private void Start()
    {
        if (Instance != this) return;
        ChangeState(GameState.Menu);
    }

    private void OnEnable()
    {
        if (!Instance == this)
        {
            PlayerController.OnPlayerDeath += GameOver;
        }
        
    }

    private void OnDisable()
    {
        if(Instance == this)
        {
            PlayerController.OnPlayerDeath -= GameOver;
        }
    }

    public GameState CurrentState
    {
        get { return currentState; }
    }

    #region COMPONENTES DE ESTADO DE JUEGO
    private void GameOver()
    {
        IsPlayerDead = true;
        ActivarUIGameOver();
        ChangeState(GameState.GameOver);
        Debug.Log("Perdiste :,c");

        // Inicia la corrutina para reiniciar la escena después de 10 segundos
        //Esto es provicional, la logica cambia segun como quieras
        StartCoroutine(ReiniciarEscenaConDelay(10f));

    }

    //Logica de reincio solo para pruebas
    private System.Collections.IEnumerator ReiniciarEscenaConDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Usa tiempo real porque Time.timeScale podría ser 0
        ReiniciarEscenaActual();
    }

    private void ReiniciarEscenaActual()
    {
        // Resetea el estado antes de recargar para que la nueva escena comience limpia
        IsPlayerDead = false;
        tieneArma = false;

        // Recarga la escena activa
        string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(escenaActual);
    }

    //Termina logica de reinicio solo para pruebas

    // Opcional método público para reiniciar manualmente (por si un botón)
    public void ReiniciarNivel()
    {
        ReiniciarEscenaActual();
    }


    private void ActivarUIGameOver()
    {
        //Aqui logica para activar UI
    }
    

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case GameState.Gameplay: 
                EnJuego();
                EstadoEnPartida(true);
                break;
            case GameState.Paused: 
                EstadoEnPartida(false);
                break;
            case GameState.GameOver: 
                ActivarUIGameOver();
                EstadoEnPartida(false);
                break;
            case GameState.Menu:
                EstadoEnPartida(true);
                break;
        }
    }

    private void EnJuego()
    {

    }

    private void EstadoEnPartida(bool condicion)
    {
        if (condicion)
        {
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }
        }
        else
        {
            Time.timeScale = 0f;
        }
    }
    #endregion

    public void LevelComplete()
    {
        //IMPLEMENTAR LOGICA DESPUES :v
        Debug.Log("Nivel Superado");
        // Deberiamos llamar al SceneLoader que haremos mas adelante :v
    }

    //Metodo get obtener estado del juegos
    public GameState EstadoJuego
    {
        get {return currentState;}
    }

    public void RegistrarArmaObtenida()
    {
        tieneArma = true;
    }

}
