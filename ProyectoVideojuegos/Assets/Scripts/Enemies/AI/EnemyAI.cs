using UnityEngine;
using static GameManager;

public class EnemyAI : MonoBehaviour
{
    public enum Estado { Patrullando, Persiguiendo, Atacando, Muerto }
    public Estado estadoActual = Estado.Patrullando; // Estado inicial del enemigo

    [SerializeField] protected EnemyPatrol patrulla;
    [SerializeField] protected FollowPlayer persecusion;
    [SerializeField] protected DeteccionJugador deteccionJugador;
    [SerializeField] protected AttackManager ataque;
    [SerializeField] private bool puedeAtacarADistancia = false; //Activar o desactivar en el inspector

    [SerializeField] private float rangoAtaque = 2f;

    [SerializeField] protected float dañoFisico = 10f;
    [SerializeField] protected float dañoDistancia = 20f;

    [SerializeField] private float tiempoEntreAtaques = 0.8f;

    //Variable para anexar animacion
    [SerializeField] private Animator animator;

    [SerializeField] private GameObject hitbox;


    private float tiempoUltimoAtaque;

    protected Transform jugador;

    private void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;

        ConfigurarEnemigo();


    }

    private void Update()
    {
        //Si el jugador muere el enemigo solo patrulla
        /// Correccion by Miguel: Este apartado podrias cambiar el IsPlayedead, porque si no de que sirve poner un GameStage puesto
        /// Todos los managers y en general todo actuan dependiendo del estado de Juego, entonces estadoDeJuego.GameOver representa la muerte del jugador
        /// Asi que es redundante el IsPlayerDead, tambien es importante revisar PlayerController tiene un callback que notifica a el 
        /// GameManager para cambiar el estado a GameOver entonces todo los scripts que esten preguntando el estado del GameManager ejecutaran cierta condicion si es estadoDeJuego es .GameOver.


        //if (GameManager.Instance != null && GameManager.Instance.EstadoJuego == GameState.GameOver) ->Para que se pueda usar la variable GameState.GameOver o cualquiera de los estados los scripts deben usar: using static GameManager;
        //{

        //}

        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDead)
        {
            if (estadoActual != Estado.Muerto) // Solo una vez
            {
                estadoActual = Estado.Muerto;
                patrulla.enabled = false;
                persecusion.enabled = false;
                if (ataque != null) ataque.enabled = false;
                if (animator != null) animator.SetBool("seMueve", false);
            }
            return; // No ejecutar más lógica
        }

        // Si el jugador está vivo, comportamiento normal
        if (jugador == null) return;
        if (estadoActual == Estado.Muerto) return;

        DeterminarEstado();
        Comportamiento();
        Animaciones();
    }

    private void ConfigurarEnemigo()
    {
        if (ataque == null)
        {
            ataque = GetComponent<AttackManager>();
        }
        Collider2D hitboxCollider = hitbox.GetComponent<Collider2D>();
        ataque.SetAtaqueFisico(dañoFisico, hitboxCollider);
    }

    private void DeterminarEstado()
    {
        if (deteccionJugador.VeAlJugador)
        {
            OrientarHaciaJugador();

            float distancia = Vector3.Distance(transform.position, jugador.position);

            if (distancia <= rangoAtaque)
            {
                estadoActual = Estado.Atacando;
                persecusion.enabled = false;
            }
            else
            {
                // Si venía de atacar, limpiar el trigger acumulado
                if (estadoActual == Estado.Atacando)
                {
                    animator.ResetTrigger("Attacking");
                }
                estadoActual = Estado.Persiguiendo;
                persecusion.SetObjetivo(jugador);
            }
        }
        else
        {
            if (estadoActual == Estado.Atacando)
            {
                animator.ResetTrigger("Attacking");
            }
            estadoActual = Estado.Patrullando;
        }
    }

    private void OrientarHaciaJugador()
    {
        float direccionX = jugador.position.x - transform.position.x;
        Vector3 escala = transform.localScale;

        if (direccionX < 0)
            escala.x = Mathf.Abs(escala.x);   // izquierda
        else if (direccionX > 0)
            escala.x = -Mathf.Abs(escala.x);  // derecha

        transform.localScale = escala;
    }

    private void Comportamiento()
    {
        patrulla.enabled = (estadoActual == Estado.Patrullando);
        persecusion.enabled = (estadoActual == Estado.Persiguiendo);

        if (estadoActual == Estado.Atacando)
        {
            animator.SetBool("IsWalking", false);

            if (Time.time >= tiempoUltimoAtaque + tiempoEntreAtaques)
            {
                animator.ResetTrigger("Attacking"); // Limpia antes de disparar
                animator.SetTrigger("Attacking");
                tiempoUltimoAtaque = Time.time;
            }
        }
        else
        {
            // Si salimos del estado atacando, limpiar cualquier trigger pendiente
            animator.ResetTrigger("Attacking");
        }

    
    }

    private void OnDrawGizmos()
    {
        // Dibuja un círculo azul que representa el rango de ataque
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }

    public void EjecutarLogicaDaño()
    {
        if (ataque != null && jugador != null)
        {
            // Esto le dice al AttackManager que ejecute el ataque actual
            ataque.Atacar(jugador);
            Debug.Log("¡La animación mandó la orden de hacer daño!");
        }
    }

    // Métodos para Animation Events
    public void ActivarHitbox()
    {
        if (hitbox != null) hitbox.SetActive(true);
    }

    public void DesactivarHitbox()
    {
        if (hitbox != null) hitbox.SetActive(false);
    }


    //Método para control de animaciones
    private void Animaciones()
    {
        if (animator == null) return;

        // Evaluamos si el enemigo se está moviendo (Patrulla o Persecución)

        if (estadoActual != Estado.Atacando)
        {
            bool estaMoviendose = (estadoActual == Estado.Patrullando ||
                                   estadoActual == Estado.Persiguiendo);
            animator.SetBool("IsWalking", estaMoviendose);
        }

    }
}