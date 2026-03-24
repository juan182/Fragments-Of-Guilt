using Unity.VisualScripting;
using UnityEngine;

public class PersistentRoot : MonoBehaviour
{
    public static PersistentRoot Instance;

    [Header("Sistema Globales")]
    public GameManager gameManager;
    public SceneManager_P sceneManager;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            sceneManager = GetComponent<SceneManager_P>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
}
