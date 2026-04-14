using UnityEngine;

public class Triggers : MonoBehaviour
{
    [SerializeField] private string targetTag;
    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (targetTag)
        {
            case "Player":
                ChangeScene("Escena_2");
                break;
            case "SuMadre":
                Debug.Log("Caballo homosexual de las montañas, ningun animal le gana, Caballoo, Caballo");
                break;
        }
    }

    private void ChangeScene(string SceneName)
    {
        Debug.Log("Hola :p");
        GameManager.Instance.sceneManager.CargarNivelesDeJuego(SceneName);
    }
}
