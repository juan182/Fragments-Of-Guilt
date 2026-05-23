using System;
using System.Collections;
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
    [SerializeField] private GameObject hitboxTorso; // Cambiado por consistencia de Embestida

    [Header("Embestida")]
    [SerializeField] private float velocidadEmbestida = 15f;
    [SerializeField] private float duracionEmbestida = 0.8f;

    [Header("Distancias")]
    [SerializeField] private float distanciaCerca = 2.5f;
    [SerializeField] private float distanciaLejos = 4f;

    [Header("Fases")]
    [SerializeField] private float umbralFase2 = 0.4f;
    [SerializeField] private float tiempoEntreAtaques = 1.8f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource musicaSource;
    [SerializeField] private AudioClip musicaBoss;
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
    private bool estaEmbistiendo = false;
    private bool estaMuerto = false;

    private float timerAtaque = 0f;
    private float timerEmbestida = 0f;

    private Health health;
    private Rigidbody2D rb;

    private enum TipoAtaque { Mordida, GolpeSuelo, Embestida }

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
        jefeListo = false;
    }

    private void Update()
    {
        ComportamientoJefe();
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

    public void ActivarJefe()
    {
        gameObject.SetActive(true);
        transform.position = puntoEntrada.position;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        estaEntrando = true;
        ResetearBools();
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

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            0.1f,
            LayerMask.GetMask("Suelo")
        );

        if (hit.collider != null)
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + 1f, transform.position.z);
            estaEntrando = false;
            jefeListo = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            if (animator != null) animator.SetBool("IsJump", false);
            InstanciarOndaExpansiva();
        }
    }

    private void EscogerAtaque()
    {
        if (jugador == null) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);
        TipoAtaque ataqueTipo;

        if (distancia >= distanciaLejos)
        {
            ataqueTipo = TipoAtaque.Embestida;
        }
        else if (distancia <= distanciaCerca)
        {
            ataqueTipo = UnityEngine.Random.value > 0.5f ? TipoAtaque.Mordida : TipoAtaque.GolpeSuelo;
        }
        else
        {
            ataqueTipo = UnityEngine.Random.value > 0.5f ? TipoAtaque.Mordida : TipoAtaque.GolpeSuelo;
        }

        EjecutarAtaque(ataqueTipo);
    }

    private void EjecutarAtaque(TipoAtaque tipo)
    {
        if (tipo == TipoAtaque.Embestida)
        {
            estaEmbistiendo = true;
            timerEmbestida = 0f;
            if (audioSource != null && sfxEmbestida != null) audioSource.PlayOneShot(sfxEmbestida);
        }
        else if (tipo == TipoAtaque.Mordida)
        {
            if (animator != null) animator.SetTrigger("Attack");
            if (audioSource != null && sfxMordida != null) audioSource.PlayOneShot(sfxMordida);
        }
        else if (tipo == TipoAtaque.GolpeSuelo)
        {
            if (animator != null) animator.SetTrigger("Stomp");
        }
    }

    private void InstanciarOndaExpansiva()
    {
        if (audioSource != null && sfxOnda != null) audioSource.PlayOneShot(sfxOnda);

        // Comunica el impacto al sistema central del suelo usando el nuevo método unificado
        SueloOndulante suelo = FindFirstObjectByType<SueloOndulante>();
        if (suelo != null)
        {
            suelo.ActivarOndaExpansiva(transform.position);
        }
    }

    private void ActivarHitboxMordida() { if (hitboxMordida != null) hitboxMordida.SetActive(true); }
    private void DesactivarHitboxMordida() { if (hitboxMordida != null) hitboxMordida.SetActive(false); }
    private void ActivarHitboxCabezazo() { if (hitboxCabezazo != null) hitboxCabezazo.SetActive(true); }
    private void DesactivarHitboxCabezazo() { if (hitboxCabezazo != null) hitboxCabezazo.SetActive(false); }
    private void ActivarHitboxEmbestida() { if (hitboxTorso != null) hitboxTorso.SetActive(true); }
    private void DesactivarHitboxEmbestida() { if (hitboxTorso != null) hitboxTorso.SetActive(false); }

    private void VerificarFase2()
    {
        if (!estaEnFase2 && health != null && health.VidaMaxima > 0 && (health.VidaActual / health.VidaMaxima) <= umbralFase2)
        {
            estaEnFase2 = true;
            if (audioSource != null && sfxFase2 != null) audioSource.PlayOneShot(sfxFase2);
        }
    }

    private void ProcesarEmbestida()
    {
        timerEmbestida += Time.deltaTime;
        if (timerEmbestida >= duracionEmbestida)
        {
            estaEmbistiendo = false;
            return;
        }
        if (jugador != null)
        {
            float direccion = jugador.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direccion * velocidadEmbestida, rb.linearVelocity.y);
        }
    }

    private void MorirJefe()
    {
        estaMuerto = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        if (audioSource != null && sfxMuerte != null) audioSource.PlayOneShot(sfxMuerte);
        DesactivarHitboxMordida(); DesactivarHitboxEmbestida();
        gameObject.SetActive(false);
    }

    private void ResetearBools()
    {
        estaMuerto = false; estaEnFase2 = false; estaEmbistiendo = false;
    }
}
