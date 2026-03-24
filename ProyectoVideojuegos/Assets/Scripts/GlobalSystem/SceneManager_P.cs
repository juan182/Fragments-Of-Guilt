using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager_P : MonoBehaviour
{
    public void CargarNivel(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }
    
}
