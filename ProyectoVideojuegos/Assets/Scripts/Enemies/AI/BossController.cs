using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using static GameManager;

public class BossController : MonoBehaviour
{
    public static event Action OnMordida;
    public static event Action OnMordidaFin;
    public static event Action OnCabezazo;
    public static event Action OnCabezazoFin;
    public static event Action OnGolpeSuelo;
    public static event Action OnEmbestidaInicio;
    public static event Action OnEmbestidaFin;

    [Header("Referencias")]
    [SerializeField] private DeteccionJugador deteccionJugador;
    [SerializeField] private AttackManager ataque;
    [SerializeField] private Animator animator;

    [Header("Entrada")]
    [SerializeField] private Transform puntoEntrada;
    [SerializeField] private float velocidadCaida = 8f;

    [Header("Ataques - Daño")]
    [SerializeField] private float dañoMordida = 25f;
    [SerializeField] private float dañoOndaExpansiva = 15f;
    [SerializeField] private float dañoEmbestida = 40f;
    [SerializeField] private float dañoCabezazo = 20f;

    [Header("HitBox del jefe")]
    [SerializeField] private GameObject hitboxMordida;
    [SerializeField] private GameObject hitboxCabezazo;
    [SerializeField] private GameObject hitboxTorso;

    [Header("Embestida")]
    [SerializeField] private float velocidadEmbestida = 15f;
    [SerializeField] private float duracionEmbestida = 0.8f;
    [SerializeField] private float pausaAntesEmbestida = 2f;

    [Header("Retirada")]
    [SerializeField] private float velocidadRetirada = 6f;
    [SerializeField] private float distanciaRetirada = 5f;
    [SerializeField] private float tiempoEsperaJugador = 3f;

    [Header("Distancias")]
    [SerializeField] private float distanciaCerca = 2.5f;
    [SerializeField] private float distanciaLejos = 4f;

    [Header("Fases")]
    [SerializeField] private float umbralFase2 = 0.4f;
    [SerializeField] private float tiempoEntreAtaques = 1.8f;

    [Header("Salto")]
    [SerializeField] private float velocidadSalto = 10f;
    [SerializeField] private float alturaMaximaSalto = 4f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource musicaSource;
    [SerializeField] private AudioClip musicaBoss;
    [SerializeField] private AudioClip sfxEntrada;
    [SerializeField] private AudioClip sfxDaño;
    [SerializeField] private AudioClip sfxFase2;
    [SerializeField] private AudioClip sfxMuerte;
    [SerializeField] private AudioClip sfxMordida;
    [SerializeField] private AudioClip sfxCabezazo;
    [SerializeField] private AudioClip sfxEmbestida;
    [SerializeField] private AudioClip sfxOnda;

    private Transform jugador;
    private bool estaEntrando = false;
    private bool jefeListo = false;
    private bool estaEnFase2 = false;
    private bool estaMuerto = false;

    private Health health;
    private Rigidbody2D rb;

    // Radio aproximado del boss para medir distancia desde el contorno
    private const float radioBoss = 1.92f;

    private enum EstadoBoss
    {
        Inactivo,
        Entrando,
        Pausado,        // espera antes de embestir
        Embestida,
        Retirada,
        EsperandoJugador,
        AtaqueCuerpoACuerpo,
        SaltandoOnda
    }

    private EstadoBoss estadoActual = EstadoBoss.Inactivo;

    private float timerEmbestida = 0f;
    private float timerEspera = 0f;
    private float timerAtaqueCuerpoACuerpo = 0f;
    private int contadorAtaquesCuerpoACuerpo = 0;
    private int maxAtaquesCuerpoACuerpo = 3;
    private Vector3 posicionRetirada;
    private bool saltandoArriba = false;
    private float posYInicioSalto = 0f;
    private bool ondaGeneradaEnSalto = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) jugador = playerObj.transform;

        if (hitboxMordida != null) hitboxMordida.SetActive(false);
        if (hitboxCabezazo != null) hitboxCabezazo.SetActive(false);
        if (hitboxTorso != null) hitboxTorso.SetActive(false);

        transform.position = puntoEntrada.position;
        rb.bodyType = RigidbodyType2D.Kinematic;
        estadoActual = EstadoBoss.Inactivo;
    }

    private void Update()
    {
        if (estaMuerto) return;

        if (GameManager.Instance != null && GameManager.Instance.EstadoJuego == GameState.GameOver)
            return;

        if (health != null && health.VidaActual <= 0)
        {
            MorirJefe();
            return;
        }

        VerificarFase2();

        switch (estadoActual)
        {
            case EstadoBoss.Entrando:
                ProcesarEntrada();
                break;
            case EstadoBoss.Embestida:
                ProcesarEmbestida();
                break;
            case EstadoBoss.Retirada:
                ProcesarRetirada();
                break;
            case EstadoBoss.EsperandoJugador:
                ProcesarEsperaJugador();
                break;
            case EstadoBoss.AtaqueCuerpoACuerpo:
                ProcesarAtaqueCuerpoACuerpo();
                break;
            case EstadoBoss.SaltandoOnda:
                ProcesarSaltoOnda();
                break;
                // EstadoBoss.Pausado lo maneja la corrutina, Update no hace nada
        }
    }

    private void OnEnable()
    {
        OnMordida += ActivarHitboxMordida;
        OnMordidaFin += DesactivarHitboxMordida;
        OnCabezazo += ActivarHitboxCabezazo;
        OnCabezazoFin += DesactivarHitboxCabezazo;
        OnGolpeSuelo += InstanciarOndaExpansiva;
        OnEmbestidaInicio += ActivarHitboxEmbestida;
        OnEmbestidaFin += DesactivarHitboxEmbestida;
    }

    private void OnDisable()
    {
        OnMordida -= ActivarHitboxMordida;
        OnMordidaFin -= DesactivarHitboxMordida;
        OnCabezazo -= ActivarHitboxCabezazo;
        OnCabezazoFin -= DesactivarHitboxCabezazo;
        OnGolpeSuelo -= InstanciarOndaExpansiva;
        OnEmbestidaInicio -= ActivarHitboxEmbestida;
        OnEmbestidaFin -= DesactivarHitboxEmbestida;
    }

    // ---- DISTANCIA DESDE EL CONTORNO ----

    private float DistanciaAlJugador()
    {
        if (jugador == null) return float.MaxValue;
        float distanciaCentros = Vector2.Distance(transform.position, jugador.position);
        return Mathf.Max(0, distanciaCentros - radioBoss);
    }

    // ---- ENTRADA ----

    public void ActivarJefe()
    {
        gameObject.SetActive(true);
        transform.position = puntoEntrada.position;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        estadoActual = EstadoBoss.Entrando;
        if (animator != null) animator.SetBool("IsJump", true);

        if (musicaSource != null && musicaBoss != null)
        {
            musicaSource.clip = musicaBoss;
            musicaSource.Play();
        }
    }

    private void ProcesarEntrada()
    {
        transform.position += Vector3.down * velocidadCaida * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Suelo"));

        if (hit.collider != null)
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + 1f, transform.position.z);
            rb.bodyType = RigidbodyType2D.Dynamic;
            if (animator != null) animator.SetBool("IsJump", false);
            InstanciarOndaExpansiva();

            if (audioSource != null && sfxEntrada != null)
                audioSource.PlayOneShot(sfxEntrada);

            // Al aterrizar pausa y luego embestida
            estadoActual = EstadoBoss.Pausado;
            StartCoroutine(PausaYEjecutar(pausaAntesEmbestida, IniciarEmbestida));
        }
    }

    // ---- PAUSA GENERICA ----

    private IEnumerator PausaYEjecutar(float segundos, Action accion)
    {
        estadoActual = EstadoBoss.Pausado;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(segundos);
        accion?.Invoke();
    }

    // ---- EMBESTIDA ----

    private void IniciarEmbestida()
    {
        estadoActual = EstadoBoss.Embestida;
        timerEmbestida = 0f;
        OrientarHaciaJugador();
        if (animator != null) animator.SetBool("Ischarged", true);
        if (audioSource != null && sfxEmbestida != null) audioSource.PlayOneShot(sfxEmbestida);
    }

    private void ProcesarEmbestida()
    {
        timerEmbestida += Time.deltaTime;

        if (jugador != null)
        {
            OrientarHaciaJugador();
            float direccion = jugador.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direccion * velocidadEmbestida, rb.linearVelocity.y);
        }

        if (timerEmbestida >= duracionEmbestida)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (animator != null) animator.SetBool("Ischarged", false);
            IniciarRetirada();
        }
    }

    // ---- RETIRADA ----

    private void IniciarRetirada()
    {
        estadoActual = EstadoBoss.Retirada;

        if (jugador != null)
        {
            float direccionRetirada = transform.position.x > jugador.position.x ? 1f : -1f;
            posicionRetirada = new Vector3(
                transform.position.x + direccionRetirada * distanciaRetirada,
                transform.position.y,
                transform.position.z
            );
        }
    }

    private void ProcesarRetirada()
    {
        float distancia = Mathf.Abs(transform.position.x - posicionRetirada.x);

        if (distancia > 0.2f)
        {
            float direccion = posicionRetirada.x > transform.position.x ? 1f : -1f;
            OrientarHaciaJugador();
            rb.linearVelocity = new Vector2(direccion * velocidadRetirada, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            IniciarEsperaJugador();
        }
    }

    // ---- ESPERA AL JUGADOR ----

    private void IniciarEsperaJugador()
    {
        estadoActual = EstadoBoss.EsperandoJugador;
        timerEspera = 0f;
    }

    private void ProcesarEsperaJugador()
    {
        timerEspera += Time.deltaTime;
        OrientarHaciaJugador();

        float distancia = DistanciaAlJugador();

        // Jugador se acerca: ataques cuerpo a cuerpo
        if (distancia <= distanciaCerca)
        {
            IniciarAtaquesCuerpoACuerpo();
            return;
        }

        // Jugador se aleja demasiado: salto y embestida
        if (distancia >= distanciaLejos)
        {
            IniciarSaltoOnda();
            return;
        }

        // Jugador tarda demasiado: salto y embestida
        if (timerEspera >= tiempoEsperaJugador)
        {
            IniciarSaltoOnda();
        }
    }

    // ---- ATAQUES CUERPO A CUERPO ----

    private void IniciarAtaquesCuerpoACuerpo()
    {
        estadoActual = EstadoBoss.AtaqueCuerpoACuerpo;
        contadorAtaquesCuerpoACuerpo = 0;
        maxAtaquesCuerpoACuerpo = estaEnFase2 ? 4 : 3;
        timerAtaqueCuerpoACuerpo = tiempoEntreAtaques;
    }

    private void ProcesarAtaqueCuerpoACuerpo()
    {
        float distancia = DistanciaAlJugador();

        // Si el jugador se aleja durante los ataques salta
        if (distancia > distanciaLejos)
        {
            IniciarSaltoOnda();
            return;
        }

        timerAtaqueCuerpoACuerpo += Time.deltaTime;

        if (timerAtaqueCuerpoACuerpo >= tiempoEntreAtaques)
        {
            timerAtaqueCuerpoACuerpo = 0f;
            EjecutarAtaqueCuerpoACuerpo();
            contadorAtaquesCuerpoACuerpo++;

            if (contadorAtaquesCuerpoACuerpo >= maxAtaquesCuerpoACuerpo)
            {
                IniciarSaltoOnda();
            }
        }
    }

    private void EjecutarAtaqueCuerpoACuerpo()
    {
        OrientarHaciaJugador();

        // Aleatorio en vez de alternar
        if (UnityEngine.Random.value > 0.5f)
        {
            if (animator != null) animator.SetBool("IsBitting", true);
            StartCoroutine(DesactivarBoolTrasAnimacion("IsBitting"));
            if (audioSource != null && sfxMordida != null) audioSource.PlayOneShot(sfxMordida);
        }
        else
        {
            if (animator != null) animator.SetBool("IsHeadbut", true);
            StartCoroutine(DesactivarBoolTrasAnimacion("IsHeadbut"));
            if (audioSource != null && sfxCabezazo != null) audioSource.PlayOneShot(sfxCabezazo);
        }
    }

    // ---- SALTO CON ONDA ----

    private void IniciarSaltoOnda()
    {
        // Evita entrar en loop si ya esta saltando
        if (estadoActual == EstadoBoss.SaltandoOnda) return;

        estadoActual = EstadoBoss.SaltandoOnda;
        saltandoArriba = true;
        ondaGeneradaEnSalto = false;
        posYInicioSalto = transform.position.y;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetBool("IsJump", true);
    }

    private void ProcesarSaltoOnda()
    {
        if (saltandoArriba)
        {
            transform.position += Vector3.up * velocidadSalto * Time.deltaTime;

            if (transform.position.y >= posYInicioSalto + alturaMaximaSalto)
                saltandoArriba = false;
        }
        else
        {
            transform.position += Vector3.down * velocidadSalto * Time.deltaTime;

            // Daño al jugador si esta debajo durante la caida
            if (jugador != null)
            {
                float distanciaX = Mathf.Abs(jugador.position.x - transform.position.x);
                float distanciaY = transform.position.y - jugador.position.y;

                // Si el jugador esta debajo y cerca horizontalmente
                if (distanciaX < radioBoss && distanciaY > 0 && distanciaY < 1.5f)
                {
                    Health healthJugador = jugador.GetComponent<Health>();
                    if (healthJugador != null)
                        healthJugador.Daño(dañoCabezazo);
                }
            }

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Suelo"));

            if (hit.collider != null && !ondaGeneradaEnSalto)
            {
                ondaGeneradaEnSalto = true;
                transform.position = new Vector3(transform.position.x, hit.point.y + 1f, transform.position.z);
                rb.bodyType = RigidbodyType2D.Dynamic;
                if (animator != null) animator.SetBool("IsJump", false);
                InstanciarOndaExpansiva();
                StartCoroutine(PausaYEjecutar(pausaAntesEmbestida, IniciarEmbestida));
            }
        }
    }

    // ---- ORIENTACION ----

    private void OrientarHaciaJugador()
    {
        if (jugador == null) return;
        float direccionX = jugador.position.x - transform.position.x;
        Vector3 escala = transform.localScale;
        escala.x = direccionX < 0 ? Mathf.Abs(escala.x) : -Mathf.Abs(escala.x);
        transform.localScale = escala;
    }

    private IEnumerator DesactivarBoolTrasAnimacion(string paramNombre)
    {
        yield return null;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float duracion = stateInfo.length;
        yield return new WaitForSeconds(duracion);
        if (animator != null) animator.SetBool(paramNombre, false);
    }

    // ---- HITBOXES ----

    private void ActivarHitboxMordida() { if (hitboxMordida != null) hitboxMordida.SetActive(true); }
    private void DesactivarHitboxMordida() { if (hitboxMordida != null) hitboxMordida.SetActive(false); }
    private void ActivarHitboxCabezazo() { if (hitboxCabezazo != null) hitboxCabezazo.SetActive(true); }
    private void DesactivarHitboxCabezazo() { if (hitboxCabezazo != null) hitboxCabezazo.SetActive(false); }
    private void ActivarHitboxEmbestida() { if (hitboxTorso != null) hitboxTorso.SetActive(true); }
    private void DesactivarHitboxEmbestida() { if (hitboxTorso != null) hitboxTorso.SetActive(false); }

    // ---- ONDA EXPANSIVA ----

    private void InstanciarOndaExpansiva()
    {
        if (audioSource != null && sfxOnda != null) audioSource.PlayOneShot(sfxOnda);

        CinemachineImpulseSource emisorImpulso = GetComponent<CinemachineImpulseSource>();
        if (emisorImpulso != null)
            emisorImpulso.GenerateImpulse();

        SueloOndulante suelo = FindFirstObjectByType<SueloOndulante>();
        if (suelo != null)
            suelo.ActivarOndaExpansiva(transform.position);
    }

    // ---- FASE 2 ----

    private void VerificarFase2()
    {
        if (!estaEnFase2 && health != null && health.VidaMaxima > 0
            && (health.VidaActual / health.VidaMaxima) <= umbralFase2)
        {
            estaEnFase2 = true;
            tiempoEntreAtaques = 1.2f;
            distanciaLejos = 3f;
            if (audioSource != null && sfxFase2 != null) audioSource.PlayOneShot(sfxFase2);
            Debug.Log("El jefe entro en fase 2");
        }
    }

    // ---- MUERTE ----

    private void MorirJefe()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        if (hitboxMordida != null) hitboxMordida.SetActive(false);
        if (hitboxCabezazo != null) hitboxCabezazo.SetActive(false);
        if (hitboxTorso != null) hitboxTorso.SetActive(false);

        if (musicaSource != null) musicaSource.Stop();
        if (audioSource != null && sfxMuerte != null) audioSource.PlayOneShot(sfxMuerte);

        if (GameManager.Instance != null)
            GameManager.Instance.LevelComplete();

        gameObject.SetActive(false);
    }

    // ---- ANIMATION EVENTS ----

    public void EventoMordida() { OnMordida?.Invoke(); }
    public void EventoMordidaFin() => OnMordidaFin?.Invoke();
    public void EventoCabezazo() { OnCabezazo?.Invoke(); }
    public void EventoCabezazoFin() => OnCabezazoFin?.Invoke();
    public void EventoGolpeSuelo() => OnGolpeSuelo?.Invoke();
    public void EventoEmbestidaInicio() => OnEmbestidaInicio?.Invoke();
    public void EventoEmbestidaFin() => OnEmbestidaFin?.Invoke();

    // ---- GIZMOS ----

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaCerca);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaLejos);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaRetirada);
    }
}