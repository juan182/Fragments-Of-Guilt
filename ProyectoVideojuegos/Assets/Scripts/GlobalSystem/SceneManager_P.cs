using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

public class SceneManager_P : MonoBehaviour
{
    //public enum ScenaActual { Menu, Escena1, Escena2, Escena3, Escena4 }
    //public ScenaActual scenaActual;


    public void CargarNivelesDeJuego(string nombreNivel)
    {
        switch (nombreNivel)
        {
            case "Nivel1":
                GameManager.Instance.ChangeState(GameState.Gameplay);
                SceneManager.LoadScene(nombreNivel);
                GameManager.Instance.UI_Manager.Activar_o_DesactivarEstadisticas();
                break;
           
        }
        SceneManager.LoadScene(nombreNivel);
    }

    //Metodo que ira asociado a cualquier boton que requiera ir a el menu principal.
    public void IrAMenu(string nombre)
    {
        if (nombre == "Menu")
        {
            GameManager.Instance.ChangeState(GameState.Menu);
            SceneManager.LoadScene(nombre);
        }
        
    }
    
    //Metodo que ira en boton de GameOver para reiniciar Nivel
    public void ReiniciarNivel()
    {
        if (GameManager.Instance.EstadoJuego == GameState.GameOver)
        {
            string escenaActual = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(escenaActual);
        } 
    }
}
