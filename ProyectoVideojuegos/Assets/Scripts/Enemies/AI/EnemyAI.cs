using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum Estado { Patrullando, Persiguiendo, Atacando, Muerto}
    public Estado estadoActual=Estado.Patrullando; // Estado inicial del enemigo

    [SerializeField] protected EnemyPatrol patrulla;
    [SerializeField] protected FollowPlayer persecusion;
    [SerializeField] protected DeteccionJugador deteccionJugador;
    [SerializeField] protected AttackManager ataque;
    [SerializeField] private bool puedeAtacarADistancia = false; //Activar o desactivar en el inspector

    [SerializeField] private float rangoAtaque=1.2f;

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
        if (GameManager.Instance != null&&GameManager.Instance.IsPlayerDead) 
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
        if(ataque==null)
        {
            ataque = GetComponent<AttackManager>();
        }
    }

    private void DeterminarEstado()
    {
        if (deteccionJugador.VeAlJugador)
        {
            float distancia = Vector3.Distance(transform.position, jugador.position);
            Debug.Log($"Distancia al jugador: {distancia}, rangoAtaque: {rangoAtaque}");


            if (distancia < rangoAtaque)
            {
                estadoActual = Estado.Atacando;
                Collider2D hitboxCollider = hitbox.GetComponent<Collider2D>(); // hitbox es el GameObject hijo

                if (puedeAtacarADistancia && distancia > 2f)
                    ataque.SetAtaqueDistancia(dañoDistancia, hitboxCollider);
                else
                    ataque.SetAtaqueFisico(dañoFisico, hitboxCollider);
            }
            else
            {
                // Ve al jugador pero está lejos persigue
                estadoActual = Estado.Persiguiendo;
                persecusion.SetObjetivo(jugador);
            }
        }
        else
        {
            // No ve al jugador va a patrullar
            estadoActual = Estado.Patrullando;
        }
    }

    private void Comportamiento()
    {
        patrulla.enabled = (estadoActual == Estado.Patrullando);
        persecusion.enabled=(estadoActual==Estado.Persiguiendo);

        if (estadoActual == Estado.Atacando)
        {
            if (Time.time >= tiempoUltimoAtaque + tiempoEntreAtaques)
            {
                ataque.Atacar(jugador);
                tiempoUltimoAtaque=Time.time;
            }
            
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

        //bool de movimiento de patrulla o persecusion llamado "seMueve"
        bool estadoMovimiento = (estadoActual == Estado.Persiguiendo ||
            estadoActual == Estado.Patrullando);
      //  animator.SetBool("seMueve", estadoMovimiento);

        //trigger para activar cuando inicia el ataque
        if (estadoActual == Estado.Atacando && Time.time >= tiempoUltimoAtaque + tiempoEntreAtaques)
        {
            animator.SetTrigger("Attack");
        }

        //segun tipo de ataque 0 para fisico, 1 para distancia
        if (ataque != null&&ataque.ataqueActual!=null)
        {
            int tipoAtaque = (ataque.ataqueActual is AtaqueDistancia) ? 1 : 0;
           // animator.SetInteger("tipoAtaque",tipoAtaque);

        }
    }

    
}
