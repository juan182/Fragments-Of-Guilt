using UnityEngine;

public enum GameState { Gameplay, Paused, GameOver, Dialogue }
public class GameManager : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerStats.OnPlayerDeath += GameOver;
    }

    private void OnDisable()
    {
        PlayerStats.OnPlayerDeath -= GameOver;
    }

    private void GameOver()
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
