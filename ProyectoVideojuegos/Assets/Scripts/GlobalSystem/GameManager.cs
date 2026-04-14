using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    public enum GameState { Gameplay, Paused, GameOver, Menu }
    public PlayerController datosJugador = null;
    public SceneManager_P sceneManager;
    public UI_Manager UI_Manager;

    //
    [SerializeField]private GameState currentState;
    //
    
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
        ActivarUIGameOver();
        ChangeState(GameState.GameOver);
        Debug.Log("Perdiste :,c");
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
}
