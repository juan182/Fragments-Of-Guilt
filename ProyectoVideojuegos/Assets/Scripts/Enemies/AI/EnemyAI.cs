using UnityEngine;
using static GameManager;

public class EnemyAI : MonoBehaviour
{
    public enum Estado { Patrullando, Persiguiendo, Atacando, Muerto }
    public Estado estadoActual = Estado.Patrullando;

    [SerializeField] protected EnemyPatrol patrulla;
    [SerializeField] protected FollowPlayer persecusion;
    [SerializeField] protected DeteccionJugador deteccionJugador;
    [SerializeField] protected AttackManager ataque;
    [SerializeField] private bool puedeAtacarADistancia = false;

    [SerializeField] private float rangoAtaque = 2f;

    [SerializeField] protected float dañoFisico = 10f;
    [SerializeField] protected float dañoDistancia = 20f;

    [SerializeField] private float tiempoEntreAtaques = 0.8f;

    [Header("Audio")]
    [SerializeField] private EnemySoundController enemySoundController;

    [SerializeField] protected Animator animator;
    [SerializeField] private GameObject hitbox;

    private float tiempoUltimoAtaque;
    protected Transform jugador;

    private bool yaSonóAtaque = false;

    private void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        ConfigurarEnemigo();

    }

    private void Update()
    {
        ComportamientoEnemigo();
    }

    protected virtual void ComportamientoEnemigo()
    {
        if (GameManager.Instance != null && GameManager.Instance.EstadoJuego == GameState.GameOver)
        {
            if (estadoActual != Estado.Muerto)
            {
                estadoActual = Estado.Muerto;
                patrulla.enabled = false;
                persecusion.enabled = false;
                if (ataque != null) ataque.enabled = false;
                if (animator != null) animator.SetBool("seMueve", false);
                enemySoundController.StopVuelo();
            }
            return;
        }

        if (jugador == null) return;
        if (estadoActual == Estado.Muerto) return;

        DeterminarEstado();
        Comportamiento();
        Animaciones();
    }

    protected virtual void ConfigurarEnemigo()
    {
        if (ataque == null)
            ataque = GetComponent<AttackManager>();

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
                enemySoundController.StopVuelo();
            }
            else
            {
                if (estadoActual == Estado.Atacando)
                    animator.ResetTrigger("Attacking");

                estadoActual = Estado.Persiguiendo;
                persecusion.SetObjetivo(jugador);
                enemySoundController.PlayVuelo();
            }
        }
        else
        {
            if (estadoActual == Estado.Atacando)
                animator.ResetTrigger("Attacking");

            estadoActual = Estado.Patrullando;
            enemySoundController.PlayVuelo();
        }
    }

    private void OrientarHaciaJugador()
    {
        float direccionX = jugador.position.x - transform.position.x;
        Vector3 escala = transform.localScale;

        if (direccionX < 0)
            escala.x = Mathf.Abs(escala.x);
        else if (direccionX > 0)
            escala.x = -Mathf.Abs(escala.x);

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
                animator.ResetTrigger("Attacking");
                animator.SetTrigger("Attacking");
                tiempoUltimoAtaque = Time.time;

                yaSonóAtaque = false;
            }
        }
        else
        {
            animator.ResetTrigger("Attacking");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }

    // ANIMATION EVENT --- llamar en el frame de impacto del ataque
    public void EjecutarLogicaDaño()
    {
        if (!yaSonóAtaque)   
        {
            enemySoundController.PlayAtaque();
            yaSonóAtaque = true;
        }
    }

    // ANIMATION EVENT --- llamar al inicio del frame de daño
    public void ActivarHitbox()
    {
        if (hitbox != null) hitbox.SetActive(true);

    }

    // ANIMATION EVENT --- llamar al final del frame de daño
    public void DesactivarHitbox()
    {
        if (hitbox != null) hitbox.SetActive(false);
    }

    private void Animaciones()
    {
        if (animator == null) return;

        if (estadoActual != Estado.Atacando)
        {
            bool estaMoviendose = (estadoActual == Estado.Patrullando ||
                                   estadoActual == Estado.Persiguiendo);
            animator.SetBool("IsWalking", estaMoviendose);
        }
    }
}