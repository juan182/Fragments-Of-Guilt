using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    public enum GameState { Gameplay, Paused, GameOver, Menu }
    public PlayerController datosJugador = null;
    public SceneManager_P sceneManager;
    public UI_Manager UI_Manager;

    public GameController1 gc1 = null;
    
    //ESTO NO VA AQUI.
    public bool tieneArma { get; private set; } = false;

    //
    [SerializeField]private GameState estadoDeJuego;
    //


    //Esto no deberia estar aqui//
    //Revisar porfavor el comportamiento de muerte del PlayerController.
    //Cuando en el playerController el jugador muere este ejecuta un metodo y ese metodo notifica a los metodos suscritos
    //El metodo suscrito a esa notificacion es el metodo de aqui llamado GameOver que cambia el estado de juego.
    //Entonces como sabemos cuando el jugador muere? Cuando el estadoDeJuego es GameOver.

    //Verificar
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

    

    #region COMPONENTES DE ESTADO DE JUEGO
    private void GameOver()
    {
        //IsPlayerDead = true;
        //ActivarUIGameOver();
        //Este metodo cambia el estado de juego
        ChangeState(GameState.GameOver);
        Debug.Log("Perdiste :,c");

        // Inicia la corrutina para reiniciar la escena después de 10 segundos
        //Esto es provicional, la logica cambia segun como quieras
        //StartCoroutine(ReiniciarEscenaConDelay(10f));
    }




    private void ActivarUIGameOver()
    {
        //Se debera ActivarUIGameOver
        //Se accedera al UI_Manager para poder activar el "GameOverUI" | Aun no se ha creado
        //"GameOverUI" Tiene dos opciones, Volver a el menu principal o Reiniciar nivel | Nos enfocaremos por ahora en Reiniciar nivel
        //Para poder acceder a el reinicio del Nivel, deberemos acceder a el SceneManager_P (P de Propio, no estamos usando el que Unity Incluye)
        //A este le enviaremos la scena en la que estamos y esa nos recargara.


        //Cuando se activa el GameOver Tengamos en cuenta lo siguiente.
        //Cambia en estado del juego que es manejado unica y exclusivamente por el GameManager.
        //ChangeState cambia el estado de juego y este estado del Juego estara en la variable estadoDeJuego que
        //puede ser accedida a traves de un metodo get.


        //Esta interfaz precisamente en el boton de reiniciar nivel accede a el SceneManager y tiene asociado el metodo del SceneManager : Reiniciar Nivel.
    }

    //Este changeState analiza que cambio vamos a hacer, dependiendo del tipo de estado ejecutaremos una accion.
    //Si es GameOver accedemos a el metodo Estado en Partida que puede parar algunos comportamientos.
    public void ChangeState(GameState newState)
    {
        estadoDeJuego = newState;
        switch (estadoDeJuego)
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
        get {return estadoDeJuego;}
    }

    //public void RegistrarArmaObtenida()
    //{
    //    tieneArma = true;
    //}


    //Logica de reincio solo para pruebas
    //private System.Collections.IEnumerator ReiniciarEscenaConDelay(float delay)
    //{
    //    yield return new WaitForSecondsRealtime(delay); // Usa tiempo real porque Time.timeScale podría ser 0
    //    ReiniciarEscenaActual();
    //}

    //private void ReiniciarEscenaActual()
    //{
    //    // Resetea el estado antes de recargar para que la nueva escena comience limpia
    //    IsPlayerDead = false;
    //    tieneArma = false;

    //    // Recarga la escena activa
    //    string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    //    UnityEngine.SceneManagement.SceneManager.LoadScene(escenaActual);
    //}

    //Termina logica de reinicio solo para pruebas

    // Opcional método público para reiniciar manualmente (por si un botón)
    //public void ReiniciarNivel()
    //{
    //    ReiniciarEscenaActual();
    //}

}
