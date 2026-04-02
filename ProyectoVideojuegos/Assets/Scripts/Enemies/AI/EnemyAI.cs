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

    [SerializeField] private float rangoAtaque=2f;

    [SerializeField] protected float dañoFisico = 10f;
    [SerializeField] protected float dañoDistancia = 20f;

    [SerializeField] private float tiempoEntreAtaques = 0.8f;

    //Variable para anexar animacion
    [SerializeField] private Animator animator;

    private float tiempoUltimoAtaque;

    protected Transform jugador;

    private void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;

        ConfigurarEnemigo(); 

        
    }

    private void Update()
    {
        //Si el jugador muere todo se detiene
        if(jugador == null)
        {
            patrulla.enabled= false;
            persecusion.enabled= false;
            return;
        }

        if (estadoActual == Estado.Muerto) return;

        DeterminarEstado();
        Comportamiento();
        Animaciones(); //Metodo de las animaciones
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

            if (distancia < rangoAtaque)
            {
                estadoActual = Estado.Atacando;

                // Elegir el tipo de ataque segun la distancia y si puede atacar a distancia
                if (puedeAtacarADistancia && distancia > 2f)
                {
                    ataque.SetAtaqueDistancia(dañoDistancia);
                }
                else
                {
                    ataque.SetAtaqueFisico(dañoFisico);
                }
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

    //Método para control de animaciones
    private void Animaciones()
    {
        if (animator == null) return;

        //bool de movimiento de patrulla o persecusion llamado "seMueve"
        bool estadoMovimiento = (estadoActual == Estado.Persiguiendo ||
            estadoActual == Estado.Patrullando);
        animator.SetBool("seMueve", estadoMovimiento);

        //trigger para activar cuando inicia el ataque
        if (estadoActual == Estado.Atacando && Time.time >= tiempoUltimoAtaque + tiempoEntreAtaques)
        {
            animator.SetTrigger("Attack");
        }

        //segun tipo de ataque 0 para fisico, 1 para distancia
        if (ataque != null&&ataque.ataqueActual!=null)
        {
            int tipoAtaque = (ataque.ataqueActual is AtaqueDistancia) ? 1 : 0;
            animator.SetInteger("tipoAtaque",tipoAtaque);

        }
    }

    
}
