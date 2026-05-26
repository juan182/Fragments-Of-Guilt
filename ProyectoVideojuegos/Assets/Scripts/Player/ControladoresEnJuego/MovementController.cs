using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public PlayerController playerController;

    [Header("Estados de Movimiento")]
    private Animator animator;

    [Header("Movimiento")]
    public float movementSpeed;
    private Rigidbody2D rb;
    private float horizontalInput;

    [Header("Salto")]
    public float jumpForce;
    public LayerMask capaSuelo;
    private bool estaEnElSuelo = false;
    private bool puedeSaltar = false;

    [Header("Estado de Combate")]
    public bool isBlocking = false;

    [Header("Costo de Habilidades")]
    [SerializeField] private int costoStaminaMagia = 20; // Cuánta stamina consume cada disparo de magia

    [Header("Sistema de Audio Local (SFX)")]
    [SerializeField] private AudioSource reproductorSFX;
    [SerializeField] private AudioClip sonidoSalto;
    [SerializeField] private AudioClip sonidoMagia;
    [SerializeField] private AudioClip[] sonidosAtaqueVariados;

    [Header("Configuración de Pasos Orgánicos")]
    [SerializeField] private AudioClip[] sonidosPasosVariados;

    // Tiempo en segundos entre cada paso
    [SerializeField] private float tiempoEntrePasos = 0.35f;
    private float cronometroPasos;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        if (reproductorSFX == null)
        {
            reproductorSFX = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        ValidarMovimiento();
        ValidarSalto();
        ValidateAtack();
        ValidateMagicAtack();
        DetectarCaidayAscenso();
        GestionarPasosRealistas();
    }

    private void FixedUpdate()
    {
        Movement();
        Saltar();
    }

    #region Movimiento
    void ValidarMovimiento()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (horizontalInput > 0) transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
        else if (horizontalInput < 0) transform.localScale = new Vector3(-0.18f, 0.18f, 0.18f);
        animator.SetBool("IsRunning", horizontalInput != 0);
    }

    void Movement()
    {
        rb.linearVelocity = new Vector2(horizontalInput * movementSpeed, rb.linearVelocity.y);
    }

    void GestionarPasosRealistas()
    {
        bool estaCaminando = horizontalInput != 0 && estaEnElSuelo;

        if (estaCaminando)
        {
            cronometroPasos += Time.deltaTime;

            if (cronometroPasos >= tiempoEntrePasos)
            {
                ReproducirPasoAleatorio();
                cronometroPasos = 0f;
            }
        }
        else
        {
            cronometroPasos = tiempoEntrePasos;
        }
    }

    private void ReproducirPasoAleatorio()
    {
        if (sonidosPasosVariados != null && sonidosPasosVariados.Length > 0)
        {
            int indiceAleatorio = UnityEngine.Random.Range(0, sonidosPasosVariados.Length);
            AudioClip clipSeleccionado = sonidosPasosVariados[indiceAleatorio];

            if (clipSeleccionado != null)
            {
                reproductorSFX.pitch = UnityEngine.Random.Range(0.88f, 1.12f);
                float volumenAleatorio = UnityEngine.Random.Range(0.85f, 1.0f);
                reproductorSFX.PlayOneShot(clipSeleccionado, volumenAleatorio);
            }
        }
    }
    #endregion

    #region Salto
    void ValidarSalto()
    {
        if (Input.GetKeyDown(KeyCode.Space) && estaEnElSuelo)
        {
            puedeSaltar = true;

            if (sonidoSalto != null)
            {
                reproductorSFX.pitch = 1.0f;
                reproductorSFX.PlayOneShot(sonidoSalto);
            }
        }
    }

    void Saltar()
    {
        Vector2 posicionn = (Vector2)transform.position + new Vector2(0.1f, 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(posicionn, Vector2.down, 0.5f, capaSuelo);
        if (hit.collider != null)
        {
            estaEnElSuelo = true;
            Debug.DrawRay(posicionn, Vector2.down * 0.3f, Color.green);
        }
        else
        {
            estaEnElSuelo = false;
            Debug.DrawRay(posicionn, Vector2.down * 0.3f, Color.red);
        }

        if (puedeSaltar)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            puedeSaltar = false;
        }
    }
    #endregion

    #region Caida
    void DetectarCaidayAscenso()
    {
        if (!estaEnElSuelo)
        {
            animator.SetBool("IsJumping", rb.linearVelocityY > 0.05f);
            animator.SetBool("IsFalling", rb.linearVelocityY < -0.1f);
        }
        else
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
        }
    }
    #endregion

    #region Ataques
    void ValidateAtack()
    {
        if (playerController.tieneLanza)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                animator.SetBool("IsAttacking", true);
                ReproducirAtaqueAleatorio();
            }

            if (Input.GetKeyUp(KeyCode.Q))
            {
                animator.SetBool("IsAttacking", false);
            }
        }
    }

    private void ReproducirAtaqueAleatorio()
    {
        if (sonidosAtaqueVariados != null && sonidosAtaqueVariados.Length > 0)
        {
            int indiceAleatorio = UnityEngine.Random.Range(0, sonidosAtaqueVariados.Length);
            if (sonidosAtaqueVariados[indiceAleatorio] != null)
            {
                reproductorSFX.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                reproductorSFX.PlayOneShot(sonidosAtaqueVariados[indiceAleatorio]);
            }
        }
    }

    void ValidateMagicAtack()
    {
        // MODIFICADO: Comprobamos si se presiona la tecla de magia
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Verificamos si el jugador cuenta con la stamina suficiente para castear
            if (playerController.Stamina >= costoStaminaMagia)
            {
                // Restamos la stamina a través de la propiedad del PlayerController
                playerController.Stamina -= costoStaminaMagia;

                if (sonidoMagia != null)
                {
                    reproductorSFX.pitch = 1.0f;
                    reproductorSFX.PlayOneShot(sonidoMagia);
                }

                DisparoLanzaMagica scriptLanza = GetComponentInChildren<DisparoLanzaMagica>(true);
                if (scriptLanza != null)
                {
                    scriptLanza.DispararProyectil();
                }
            }
            else
            {
                Debug.LogWarning("No tienes suficiente Stamina para usar magia.");
            }
        }

        // El estado visual de la animación solo se mantiene activo si el jugador tiene suficiente energía para el casteo
        bool tieneSuficienteStamina = playerController.Stamina >= costoStaminaMagia;
        bool canUseMagic = Input.GetKey(KeyCode.E) && !animator.GetBool("IsRunning") && tieneSuficienteStamina;
        bool canUseMagicWhileRun = Input.GetKey(KeyCode.E) && animator.GetBool("IsRunning") && tieneSuficienteStamina;

        if (canUseMagic || canUseMagicWhileRun)
        {
            animator.SetBool("IsMagic", true);
        }
        else
        {
            animator.SetBool("IsMagic", false);
        }
    }
    #endregion   
}