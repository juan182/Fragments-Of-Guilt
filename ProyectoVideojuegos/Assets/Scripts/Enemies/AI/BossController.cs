using System;
using UnityEngine;
using static GameManager;

public class BossController : EnemyAI
{
    // ---- EVENTOS ----
    // Estos eventos son invocados desde los Animation Events del jefe
    // El animador debe conectar los métodos públicos en cada
    // animación en el momento indicado abajo
    public static event Action OnMordida;
    public static event Action OnMordidaFin;
    public static event Action OnGolpeSuelo;
    public static event Action OnEmbestidaInicio;
    public static event Action OnEmbestidaFin;

    [Header("Entrada")]
    // ANIMACION: puntoEntrada es un Transform vacio colocado arriba de la escena
    // El jefe aparece ahí y cae al inicio del combate
    [SerializeField] private Transform puntoEntrada;
    [SerializeField] private float velocidadCaida = 8f;

    [Header("Ataques - Daño")]
    [SerializeField] private float dañoMordida = 25f;
    [SerializeField] private float dañoOndaExpansiva = 15f;
    [SerializeField] private float dañoEmbestida = 40f;

    [Header("HitBox del jefe")]
    // HITBOX MORDIDA: GameObject hijo del jefe con Collider2D en modo IsTrigger.
    // Colocarlo en la boca del sprite. Empieza desactivado.
    // ANIMATION EVENT: activar con EventoMordida() y desactivar con EventoMordidaFin()
    [SerializeField] private GameObject hitboxMordida;

    // HITBOX EMBESTIDA: GameObject hijo del jefe con Collider2D en modo IsTrigger.
    // Colocarlo en los cuernos/cuerpo frontal del sprite. Empieza desactivado.
    // ANIMATION EVENT: activar con EventoEmbestidaInicio() y desactivar con EventoEmbestidaFin()
    [SerializeField] private GameObject hitboxEmbestida;

    [Header("Onda Expansiva")]
    // ONDA EXPANSIVA: Prefab con OndaExpansiva.cs y Collider2D en modo IsTrigger.
    // Representa la onda que viaja por el suelo tras el golpe. Asignar en el Inspector.
    // ANIMATION EVENT: instanciar con EventoGolpeSuelo() en el frame de impacto al suelo.
    [SerializeField] private GameObject ondaExpansivaPrefab;
    [SerializeField] private float velocidadOnda = 5f;
    [SerializeField] private float duracionOnda = 3f;

    [Header("Embestida")]
    [SerializeField] private float velocidadEmbestida = 12f;
    [SerializeField] private float duracionEmbestida = 0.6f;

    [Header("Fases")]
    [SerializeField] private float umbralFase2 = 0.4f;
    [SerializeField] private float tiempoEntreAtaques = 2f;

    // Estados internos
    private bool estaEntrando = false;
    private bool jefeListo = false;
    private bool estaEnFase2 = false;
    private bool estaEmbistiendo = false;
    private bool estaMuerto = false;

    private float timerAtaque = 0f;
    private float timerEmbestida = 0f;

    

    private Health health;

    private enum TipoAtaque { Mordida, GolpeSuelo, Embestida }


    protected new void Start()
    {
        health = GetComponent<Health>();
        if (health == null)
            Debug.LogError("BossController requiere un componente Health en el mismo GameObject");

        // Hitboxes empiezan desactivados
        if (hitboxMordida != null) hitboxMordida.SetActive(false);
        if (hitboxEmbestida != null) hitboxEmbestida.SetActive(false);

        gameObject.SetActive(false);
    }

    // Sobreescribe ConfigurarEnemigo para que el jefe no dependa
    // del hitbox ni del AttackManager de Uga
    protected override void ConfigurarEnemigo() { }

    private void OnEnable()
    {
        OnMordida += ActivarHitboxMordida;
        OnMordidaFin += DesactivarHitboxMordida;
        OnGolpeSuelo += InstanciarOndaExpansiva;
        OnEmbestidaInicio += ActivarHitboxEmbestida;
        OnEmbestidaFin += DesactivarHitboxEmbestida;
    }

    private void OnDisable()
    {
        OnMordida -= ActivarHitboxMordida;
        OnMordidaFin -= DesactivarHitboxMordida;
        OnGolpeSuelo -= InstanciarOndaExpansiva;
        OnEmbestidaInicio -= ActivarHitboxEmbestida;
        OnEmbestidaFin -= DesactivarHitboxEmbestida;
    }

    protected override void ComportamientoEnemigo()
    {
        if (estaMuerto) return;

        if (GameManager.Instance != null && GameManager.Instance.EstadoJuego == GameState.GameOver)
            return;

        if (health != null && health.VidaActual <= 0)
        {
            MorirJefe();
            return;
        }

        if (estaEntrando)
        {
            ProcesarEntrada();
            return;
        }

        if (!jefeListo) return;

        VerificarFase2();

        if (estaEmbistiendo)
        {
            ProcesarEmbestida();
            return;
        }

        base.ComportamientoEnemigo();

        timerAtaque += Time.deltaTime;
        if (timerAtaque >= tiempoEntreAtaques)
        {
            timerAtaque = 0f;
            EscogerAtaque();
        }
    }

    // ---- ENTRADA ----

    public void ActivarJefe()
    {
        transform.position = puntoEntrada.position;
        gameObject.SetActive(true);
        estaEntrando = true;
        Debug.Log("El jefe aparece");
    }

    private void ProcesarEntrada()
    {
        transform.position += Vector3.down * velocidadCaida * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f);
        if (hit.collider != null
            && !hit.collider.CompareTag("Player")
            && !hit.collider.CompareTag("Enemy"))
        {
            estaEntrando = false;
            jefeListo = true;
            // El aterrizaje genera onda expansiva automáticamente como primer ataque
            InstanciarOndaExpansiva();
            Debug.Log("El jefe aterrizó");
        }
    }

    // ---- SELECCIÓN DE ATAQUE ----

    private void EscogerAtaque()
    {
        float distancia = Vector2.Distance(transform.position, jugador.position);
        bool cerca = distancia <= 2.5f;
        TipoAtaque ataque;

        if (estaEnFase2)
        {
            // Fase 2: cerca -> Mordida o GolpeSuelo | lejos -> Embestida o GolpeSuelo
            ataque = cerca
                ? (UnityEngine.Random.value > 0.7f ? TipoAtaque.Mordida : TipoAtaque.GolpeSuelo)
                : (UnityEngine.Random.value > 0.6f ? TipoAtaque.Embestida : TipoAtaque.GolpeSuelo);
        }
        else
        {
            // Fase 1: cerca -> Mordida | lejos -> GolpeSuelo
            ataque = cerca ? TipoAtaque.Mordida : TipoAtaque.GolpeSuelo;
        }

        EjecutarAtaque(ataque);
    }

    private void EjecutarAtaque(TipoAtaque tipo)
    {
        // ANIMACION: El Animator del jefe debe tener estos triggers:
        //   - "Mordida"    -> animación de ataque con la boca
        //   - "GolpeSuelo" -> animación de golpe al suelo
        //   - "Embestida"  -> animación de carga horizontal

        switch (tipo)
        {
            case TipoAtaque.Mordida:
                animator.SetTrigger("Mordida");
                break;
            case TipoAtaque.GolpeSuelo:
                animator.SetTrigger("GolpeSuelo");
                break;
            case TipoAtaque.Embestida:
                animator.SetTrigger("Embestida");
                IniciarEmbestida();
                break;
        }
    }

    // ---- HITBOXES ----

    // Suscrito a OnMordida
    // ANIMATION EVENT: en "Mordida" -> frame donde la boca impacta -> EventoMordida()
    private void ActivarHitboxMordida()
    {
        if (hitboxMordida != null) hitboxMordida.SetActive(true);
    }

    // Suscrito a OnMordidaFin
    // ANIMATION EVENT: en "Mordida" -> frame donde termina el impacto -> EventoMordidaFin()
    private void DesactivarHitboxMordida()
    {
        if (hitboxMordida != null) hitboxMordida.SetActive(false);
    }

    // Suscrito a OnEmbestidaInicio
    // ANIMATION EVENT: en "Embestida" -> frame donde inicia el impulso -> EventoEmbestidaInicio()
    private void ActivarHitboxEmbestida()
    {
        if (hitboxEmbestida != null) hitboxEmbestida.SetActive(true);
    }


    // Suscrito a OnEmbestidaFin
    // ANIMATION EVENT: en "Embestida" -> frame donde termina el impulso -> EventoEmbestidaFin()
    private void DesactivarHitboxEmbestida()
    {
        if (hitboxEmbestida != null) hitboxEmbestida.SetActive(false);
    }


    // ---- ONDA EXPANSIVA ----

    // Suscrito a OnGolpeSuelo y también se llama al aterrizar
    // ANIMATION EVENT: en "GolpeSuelo" -> frame de impacto al suelo -> EventoGolpeSuelo()
    public void InstanciarOndaExpansiva()
    {
        if (ondaExpansivaPrefab == null)
        {
            Debug.LogWarning("Falta asignar ondaExpansivaPrefab en el Inspector");
            return;
        }

        // Instancia dos ondas: una hacia la izquierda (-1) y otra hacia la derecha (1)
        for (int dir = -1; dir <= 1; dir += 2)
        {
            GameObject onda = Instantiate(ondaExpansivaPrefab, transform.position, Quaternion.identity);
            onda.GetComponent<OndaExpansiva>().Inicializar(dañoOndaExpansiva, velocidadOnda, duracionOnda, dir);
        }
    }


    // ---- EMBESTIDA ----
    private void IniciarEmbestida()
    {
        estaEmbistiendo = true;
        timerEmbestida = duracionEmbestida;
    }

    private void ProcesarEmbestida()
    {
        timerEmbestida -= Time.deltaTime;

        if (jugador != null)
        {
            float direccionX = Mathf.Sign(jugador.position.x - transform.position.x);
            transform.position += new Vector3(direccionX * velocidadEmbestida * Time.deltaTime, 0, 0);
        }

        if (timerEmbestida <= 0f)
            estaEmbistiendo = false;
    }

    // El hitboxEmbestida maneja el daño por trigger, no por colision directa
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!estaEmbistiendo) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
                player.TakeDamage(Mathf.RoundToInt(dañoEmbestida));
        }
    }


    // ---- MUERTE ----
    private void MorirJefe()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        estaEmbistiendo = false;
        estaEntrando = false;

        if (hitboxMordida != null) hitboxMordida.SetActive(false);
        if (hitboxEmbestida != null) hitboxEmbestida.SetActive(false);

        // ANIMACION: El trigger "muerte" debe estar en el Animator del jefe.
        // Health.cs ya lo dispara automáticamente, pero se refuerza aquí por seguridad.
        // La destrucción del GameObject la maneja Health.cs con delay de 1f
        // para que la animación de muerte se reproduzca completa.
        if (animator != null)
            animator.SetTrigger("muerte");

        if (GameManager.Instance != null)
            GameManager.Instance.LevelComplete();

        Debug.Log("El jefe fue derrotado");
    }

    // ---- FASE 2 ----
    private void VerificarFase2()
    {
        if (health == null) return;

        if (!estaEnFase2 && health.VidaActual / health.VidaMaxima <= umbralFase2)
        {
            estaEnFase2 = true;
            tiempoEntreAtaques = 1.5f;
            Debug.Log("El jefe entro en fase 2");
        }
    }

    // ---- ANIMATION EVENTS ----
    // Estos son los métodos que el animador conecta en el Animator del jefe.
    // NO modificar los nombres.

    // Conectar en: "Mordida" -> frame de impacto de la boca
    public void EventoMordida() => OnMordida?.Invoke();

    // Conectar en: "Mordida" -> frame donde termina el impacto
    public void EventoMordidaFin() => OnMordidaFin?.Invoke();

    // Conectar en: "GolpeSuelo" -> frame de impacto en el suelo
    public void EventoGolpeSuelo() => OnGolpeSuelo?.Invoke();

    // Conectar en: "Embestida" -> frame donde inicia el impulso
    public void EventoEmbestidaInicio() => OnEmbestidaInicio?.Invoke();

    // Conectar en: "Embestida" -> frame donde termina el impulso
    public void EventoEmbestidaFin() => OnEmbestidaFin?.Invoke();

    // ---- GIZMOS ----
    private new void OnDrawGizmos()
    {
        // Círculo magenta = radio cercano/lejano para selección de ataque
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 2.5f);
    }
}
