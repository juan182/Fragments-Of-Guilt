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
    public void IrAMenu(string nombre)
    {
        if (nombre == "Menu")
        {
            GameManager.Instance.ChangeState(GameState.Menu);
            SceneManager.LoadScene(nombre);
        }
        
    }
    
}
