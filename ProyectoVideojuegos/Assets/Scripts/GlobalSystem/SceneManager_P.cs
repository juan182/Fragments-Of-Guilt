using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

public class SceneManager_P : MonoBehaviour
{
    public void CargarNivelesDeJuego(string nombreNivel)
    {
        switch (nombreNivel)
        {
            case "Nivel1":
                GameManager.Instance.ChangeState(GameState.Gameplay);
                SceneManager.LoadScene(nombreNivel);

                // Aseguramos que el UI_Manager exista antes de llamarlo para evitar NullReferenceException
                if (GameManager.Instance.UI_Manager != null)
                {
                    GameManager.Instance.UI_Manager.Activar_o_DesactivarEstadisticas();
                }
                break;

            default:
                // Por si en el futuro cargas otros niveles ("Nivel2", "Nivel3", etc.) sin configurar reglas especiales
                GameManager.Instance.ChangeState(GameState.Gameplay);
                SceneManager.LoadScene(nombreNivel);
                break;
        }
    }

    // Método asociado a cualquier botón que requiera ir al menú principal
    public void IrAMenu(string nombre)
    {
        if (nombre == "Menu") // Añadida tolerancia por si cambia el string
        {
            GameManager.Instance.ChangeState(GameState.Menu);
            SceneManager.LoadScene(nombre);
        }
    }

    // Método que va en el botón de GameOver para reiniciar el nivel actual
    public void ReiniciarNivel()
    {
        //Forzamos el regreso al estado de juego para que el Time.timeScale vuelva a 1f nativamente
        GameManager.Instance.ChangeState(GameState.Gameplay);
        string escenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(escenaActual);
        Debug.Log("Escena " + escenaActual + " recargada con éxito.");
    }
}