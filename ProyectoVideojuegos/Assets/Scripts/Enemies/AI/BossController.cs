using System;
using System.Collections;
using UnityEngine;
using static GameManager;

public class BossController : MonoBehaviour
{
    // ---- EVENTOS ----
    // Estos eventos son invocados desde los Animation Events del jefe
    // El animador debe conectar los metodos publicos en cada
    // animacion en el momento indicado abajo
    public static event Action OnMordida;
    public static event Action OnMordidaFin;
    public static event Action OnGolpeSuelo;
    public static event Action OnEmbestidaInicio;
    public static event Action OnEmbestidaFin;
    public static event Action OnCabezazo;
    public static event Action OnCabezazoFin;

    [Header("Referencias")]
    [SerializeField] private DeteccionJugador deteccionJugador;
    [SerializeField] private AttackManager ataque;
    [SerializeField] private Animator animator;

    [Header("Entrada")]
    // puntoEntrada es un Transform vacio colocado arriba de la escena
    // El jefe aparece ahi y cae al inicio del combate
    [SerializeField] private Transform puntoEntrada;
    [SerializeField] private float velocidadCaida = 8f;

    [Header("Ataques - Daño")]
    [SerializeField] private float dañoMordida = 25f;
    [SerializeField] private float dañoOndaExpansiva = 15f;
    [SerializeField] private float dañoEmbestida = 40f;

    [Header("HitBox del jefe")]
    // HitboxMordida: GameObject hijo con Collider2D en modo IsTrigger
    // Colocarlo en la boca del sprite. Empieza desactivado.
    // Se activa con EventoMordida() y desactiva con EventoMordidaFin()
    [SerializeField] private GameObject hitboxMordida;

    // HitboxEmbestida: GameObject hijo con Collider2D en modo IsTrigger
    // Colocarlo en el cuerpo frontal del sprite. Empieza desactivado.
    // Se activa con EventoEmbestidaInicio() y desactiva con EventoEmbestidaFin()
    [SerializeField] private GameObject hitboxEmbestida;
    [SerializeField] private GameObject hitboxCabezazo;

    [Header("Onda Expansiva")]
    // Prefab con OndaExpansiva.cs y Collider2D en modo IsTrigger
    // Se instancia al aterrizar y con EventoGolpeSuelo()
    [SerializeField] private GameObject ondaExpansivaPrefab;
    [SerializeField] private float velocidadOnda = 5f;
    [SerializeField] private float duracionOnda = 3f;

    [Header("Embestida")]
    [SerializeField] private float velocidadEmbestida = 15f;
    [SerializeField] private float duracionEmbestida = 0.8f;

    [Header("Distancias")]
    [SerializeField] private float distanciaCerca = 2.5f;
    [SerializeField] private float distanciaLejos = 4f;

    [Header("Fases")]
    [SerializeField] private float umbralFase2 = 0.4f;
    [SerializeField] private float tiempoEntreAtaques = 1.8f;

    private Transform jugador;
    private bool estaEntrando = false;
    private bool jefeListo = false;
    private bool estaEnFase2 = false;
    private bool estaEmbistiendo = false;
    private bool estaMuerto = false;

    private float timerAtaque = 0f;
    private float timerEmbestida = 0f;

    private Health health;

    private enum TipoAtaque { Mordida, GolpeSuelo, Embestida }

    // ---- INICIALIZACION ----

    private void Start()
    {
        health = GetComponent<Health>();
        if (health == null)
            Debug.LogError("BossController requiere un componente Health en el mismo GameObject");

        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        if (jugador == null)
            Debug.LogError("No se encontro el jugador, verifica que tenga el tag Player");

        if (hitboxMordida != null) hitboxMordida.SetActive(false);
        if (hitboxEmbestida != null) hitboxEmbestida.SetActive(false);

        gameObject.SetActive(false);
    }

    private void Update()
    {
        ComportamientoJefe();
    }

    private void OnEnable()
    {
        OnMordida += ActivarHitboxMordida;
        OnMordidaFin += DesactivarHitboxMordida;
        OnGolpeSuelo += InstanciarOndaExpansiva;
        OnEmbestidaInicio += ActivarHitboxEmbestida;
        OnEmbestidaFin += DesactivarHitboxEmbestida;
        OnCabezazo += ActivarHitboxCabezazo;     
        OnCabezazoFin += DesactivarHitboxCabezazo;
    }

    private void OnDisable()
    {
        OnMordida -= ActivarHitboxMordida;
        OnMordidaFin -= DesactivarHitboxMordida;
        OnGolpeSuelo -= InstanciarOndaExpansiva;
        OnEmbestidaInicio -= ActivarHitboxEmbestida;
        OnEmbestidaFin -= DesactivarHitboxEmbestida;
        OnCabezazo -= ActivarHitboxCabezazo;    
        OnCabezazoFin -= DesactivarHitboxCabezazo;
    }

    // ---- COMPORTAMIENTO PRINCIPAL ----

    private void ComportamientoJefe()
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
        ResetearBools();
        animator.SetBool("IsJump", true);
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
            animator.SetBool("IsJump", false);
            InstanciarOndaExpansiva();
            Debug.Log("El jefe aterrizo");
        }
    }

    // ---- SELECCION DE ATAQUE ----

    private void EscogerAtaque()
    {
        if (jugador == null) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);
        TipoAtaque ataqueTipo;

        if (distancia >= distanciaLejos)
        {
            // Jugador huyo, embestida para alcanzarlo
            ataqueTipo = TipoAtaque.Embestida;
        }
        else if (distancia <= distanciaCerca)
        {
            // Jugador cerca, alternar mordida y cabezazo
            ataqueTipo = UnityEngine.Random.value > 0.5f
                ? TipoAtaque.Mordida
                : TipoAtaque.GolpeSuelo;

            // En fase 2 ocasionalmente onda expansiva para presionar
            if (estaEnFase2 && UnityEngine.Random.value > 0.7f)
                InstanciarOndaExpansiva();
        }
        else
        {
            // Distancia media, cabezazo
            ataqueTipo = TipoAtaque.GolpeSuelo;
        }

        EjecutarAtaque(ataqueTipo);
    }

    // ---- EJECUCION DE ATAQUE ----

    private void EjecutarAtaque(TipoAtaque tipo)
    {
        ResetearBools();

        switch (tipo)
        {
            case TipoAtaque.Mordida:
                OrientarHaciaJugador();
                animator.SetBool("IsBitting", true);
                StartCoroutine(DesactivarBoolTrasAnimacion("IsBitting"));
                break;

            case TipoAtaque.GolpeSuelo:
                OrientarHaciaJugador();
                animator.SetBool("IsHeadbut", true);
                StartCoroutine(DesactivarBoolTrasAnimacion("IsHeadbut"));
                break;

            case TipoAtaque.Embestida:
                OrientarHaciaJugador();
                animator.SetBool("Ischarged", true);
                IniciarEmbestida();
                StartCoroutine(DesactivarBoolTrasAnimacion("Ischarged"));
                break;
        }
    }

    private void OrientarHaciaJugador()
    {
        if (jugador == null) return;
        float direccionX = jugador.position.x - transform.position.x;
        Vector3 escala = transform.localScale;

        if (direccionX < 0)
            escala.x = Mathf.Abs(escala.x);
        else if (direccionX > 0)
            escala.x = -Mathf.Abs(escala.x);

        transform.localScale = escala;
    }

    private void ResetearBools()
    {
        animator.SetBool("IsBitting", false);
        animator.SetBool("IsHeadbut", false);
        animator.SetBool("IsJump", false);
        animator.SetBool("Ischarged", false);
    }

    private IEnumerator DesactivarBoolTrasAnimacion(string paramNombre)
    {
        yield return null;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float duracion = stateInfo.length;
        yield return new WaitForSeconds(duracion);
        animator.SetBool(paramNombre, false);
    }

    // ---- HITBOXES ----
    // Activados y desactivados directamente desde los Animation Events
    // El daño lo maneja el Collider2D de cada hitbox via OnTriggerEnter2D

    // ANIMATION EVENT: en Cabezazo -> frame de impacto de la cabeza
    private void ActivarHitboxCabezazo()
    {
        if (hitboxCabezazo != null) hitboxCabezazo.SetActive(true);
    }

    // ANIMATION EVENT: en Cabezazo -> frame donde termina el impacto
    private void DesactivarHitboxCabezazo()
    {
        if (hitboxCabezazo != null) hitboxCabezazo.SetActive(false);
    }

    // ANIMATION EVENT: en mordida -> frame de impacto de la boca
    private void ActivarHitboxMordida()
    {
        if (hitboxMordida != null) hitboxMordida.SetActive(true);
    }

    // ANIMATION EVENT: en mordida -> frame donde termina el impacto
    private void DesactivarHitboxMordida()
    {
        if (hitboxMordida != null) hitboxMordida.SetActive(false);
    }

    // ANIMATION EVENT: en embestir -> frame donde inicia el impulso
    private void ActivarHitboxEmbestida()
    {
        if (hitboxEmbestida != null) hitboxEmbestida.SetActive(true);
    }

    // ANIMATION EVENT: en embestir -> frame donde termina el impulso
    private void DesactivarHitboxEmbestida()
    {
        if (hitboxEmbestida != null) hitboxEmbestida.SetActive(false);
    }

    // ---- ONDA EXPANSIVA ----
    // ANIMATION EVENT: en Cabezazo -> frame de impacto en el suelo

    public void InstanciarOndaExpansiva()
    {
        if (ondaExpansivaPrefab == null)
        {
            Debug.LogWarning("Falta asignar ondaExpansivaPrefab en el Inspector");
            return;
        }

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
            OrientarHaciaJugador();
            float direccionX = Mathf.Sign(jugador.position.x - transform.position.x);
            transform.position += new Vector3(direccionX * velocidadEmbestida * Time.deltaTime, 0, 0);
        }

        if (timerEmbestida <= 0f)
        {
            estaEmbistiendo = false;
            animator.SetBool("Ischarged", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!estaEmbistiendo) return;
        if (!other.CompareTag("Player")) return;

        Health healthJugador = other.GetComponent<Health>();
        if (healthJugador != null)
            healthJugador.Daño(dañoEmbestida);
    }

    // ---- MUERTE ----

    private void MorirJefe()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        estaEmbistiendo = false;
        estaEntrando = false;

        StopAllCoroutines();
        ResetearBools();

        if (hitboxMordida != null) hitboxMordida.SetActive(false);
        if (hitboxEmbestida != null) hitboxEmbestida.SetActive(false);

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
            tiempoEntreAtaques = 1.2f;
            distanciaLejos = 3f;
            Debug.Log("El jefe entro en fase 2");
        }
    }

    // ---- ANIMATION EVENTS ----
    // Conectar estos metodos en el Animator del jefe en los frames indicados
    // No modificar los nombres

    // Conectar en: mordida -> frame de impacto de la boca
    public void EventoMordida() => OnMordida?.Invoke();

    // Conectar en: mordida -> frame donde termina el impacto
    public void EventoMordidaFin() => OnMordidaFin?.Invoke();

    // Conectar en: Cabezazo -> frame de impacto en el suelo
    public void EventoGolpeSuelo() => OnGolpeSuelo?.Invoke();

    // Conectar en: Embestir -> frame donde inicia el impulso
    public void EventoEmbestidaInicio() => OnEmbestidaInicio?.Invoke();

    // Conectar en: Embestir -> frame donde termina el impulso
    public void EventoEmbestidaFin() => OnEmbestidaFin?.Invoke();

    // Conectar en: Cabezazo -> frame de impacto de la cabeza
    public void EventoCabezazo() => OnCabezazo?.Invoke();

    // Conectar en: Cabezazo -> frame donde termina el impacto
    public void EventoCabezazoFin() => OnCabezazoFin?.Invoke();

    // ---- GIZMOS ----

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaCerca);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaLejos);
    }
}