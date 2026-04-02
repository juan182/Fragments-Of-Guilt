using UnityEngine;

public enum GameState { Gameplay, Paused, GameOver, Dialogue }
public class GameManager : MonoBehaviour
{
    //Agrego Awake e Instance para poder acceder al gameover al morir el jugador
    //La logica de muerte del jugador falta agregarla, hice esto solo para probar
    //Que la logica de Health tenga coherencia, Health es modular, puede ser usado
    //Por cualquier personaje

    public static GameManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void OnEnable()
    {
        PlayerStats.OnPlayerDeath += GameOver;
    }

    private void OnDisable()
    {
        PlayerStats.OnPlayerDeath -= GameOver;
    }

    //Cambié a public para poder acceder
    public void GameOver()
    {
        Debug.Log("Perdiste :,c");
    }
    //CurrentState = EstadoActual
    public GameState currentState;

    //Para que no se me olvide, Esta funcion la debe usar algun componente de la UI
    public void ChangeState(GameState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case GameState.Gameplay:
                Time.timeScale = 1f; //Reanudar juego
                break;
            case GameState.Paused:
                Time.timeScale = 0f; //Congela el juego
                break;
            case GameState.GameOver:
                Debug.Log("Mostrar Pantalla de derrota");
                break;
        }
    }

    public void LevelComplete()
    {
        //IMPLEMENTAR LOGICA DESPUES :v
        Debug.Log("Nivel Superado");
        // Deberiamos llamar al SceneLoader que haremos mas adelante :v
    }
}
