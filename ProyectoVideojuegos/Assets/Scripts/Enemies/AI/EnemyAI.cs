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


    // Sebas: Asigna el Animator del enemigo UGA en el Inspector.
    // *** LA ANIMACION QUE EXISTE ES PROVISIONAL ***
    // El Animator actual de UGA es temporal. El animador debe reemplazarlo
    // con las animaciones finales teniendo en cuenta los siguientes parámetros:
    //
    // PARAMETROS REQUERIDOS EN EL ANIMATOR DE UGA:
    //   - "IsWalking"  (bool)    -> true cuando patrulla o persigue
    //   - "Attacking"  (trigger) -> dispara la animación de ataque
    //   - "seMueve"    (bool)    -> false cuando el jugador muere
    //
    // ANIMATION EVENTS requeridos en la animación de ataque de UGA:
    //   - Al momento del impacto del golpe -> llamar: EjecutarLogicaDaño()
    //   - Al inicio del área de daño       -> llamar: ActivarHitbox()
    //   - Al final del área de daño        -> llamar: DesactivarHitbox()


    //Variable para anexar animacion
    [SerializeField] protected Animator animator;

    // HITBOX DE UGA: Se asigna el GameObject con el Collider2D del área de daño de Uga
    // Este hitbox es unicamente de Uga
    // El jefe final tiene sus propios hitboxes
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
        ComportamientoEnemigo();
    }

    // Virtual para que el jefe final la sobreescriba con su propia configuracion
    // sin depender del hitbox de Uga
    protected virtual void ComportamientoEnemigo()
    {
        //Si el jugador muere el enemigo solo patrulla


        if (GameManager.Instance != null && GameManager.Instance.EstadoJuego == GameState.GameOver)
        {
            if (estadoActual != Estado.Muerto) // Solo una vez
            {
                estadoActual = Estado.Muerto;
                patrulla.enabled = false;
                persecusion.enabled = false;
                if (ataque != null) ataque.enabled = false;
                if (animator != null) animator.SetBool("seMueve", false);
            }
            return;
        }

        // Si el jugador está vivo, comportamiento normal
        if (jugador == null) return;
        if (estadoActual == Estado.Muerto) return;

        DeterminarEstado();
        Comportamiento();
        Animaciones();
    }

    protected virtual void ConfigurarEnemigo()
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


    // ANIMATION EVENT de UGA --- llamar en el frame de impacto del ataque
    public void EjecutarLogicaDaño()
    {
        if (ataque != null && jugador != null)
        {
            // Esto le dice al AttackManager que ejecute el ataque actual
            ataque.Atacar(jugador);
            Debug.Log("¡La animación mandó la orden de hacer daño!");
        }
    }

    // ANIMATION EVENT de UGA --- llamar al inicio del frame de daño.
    public void ActivarHitbox()
    {
        if (hitbox != null) hitbox.SetActive(true);
    }

    // ANIMATION EVENT de UGA --- llamar al final del frame de daño
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