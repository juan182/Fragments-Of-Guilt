using UnityEngine;


/// <summary>
/// Gestiona la comunicacion entre scripts y la logica de la escena,
/// En este caso la comunicacion entre enemyAI y PlayStats, tambien se recomienda
/// ser implementado directamente en GameManager debido a que GameController no
/// puede ser persistente entre escenas
/// </summary>
public class GameController : MonoBehaviour
{
    public static GameController Instance; //Variable publica que permite instanciar entre escenas

    //Bool que comunica si el metodo die de playstats se ejecutó
    public bool playerDied=false;

    //Metodo que permite a esta clase ser persistente
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //Busca al jugador y se suscribe al evento de muerte
        
        
            //PlayerStats.OnPlayerDeath += PlayerDied;
        
    }

    private void PlayerDied()
    {
        playerDied = true;
        Debug.Log("Jugador muere - notificando a EnemyAI");
        // Aqui puede mostrar UI de muerte
    }

    //Desuscripcion del evento por si reinicia o el jugador se destruye al morir
    private void OnDestroy()
    {
        //PlayerStats.OnPlayerDeath -= PlayerDied;
    }
}
