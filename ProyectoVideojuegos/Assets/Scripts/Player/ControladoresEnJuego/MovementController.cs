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

    [Header("Sistema de Audio Local (SFX)")]
    [SerializeField] private AudioSource reproductorSFX;
    [SerializeField] private AudioClip sonidoSalto;
    [SerializeField] private AudioClip sonidoMagia;
    [SerializeField] private AudioClip[] sonidosAtaqueVariados;

    [Header("Configuración de Pasos Orgánicos")]
    // ------Uso Aqui---- Arreglo para tus 2 o 3 clips de pasos cortitos separados
    [SerializeField] private AudioClip[] sonidosPasosVariados;

    // Tiempo en segundos entre cada paso (Ajústalo para que calce con tu animación)
    [SerializeField] private float tiempoEntrePasos = 0.35f;
    private float cronometroPasos;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // Validación defensiva por si no se arrastra el AudioSource en el inspector
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

        // ------Uso Aqui---- Control de pasos por tiempo directo en Update
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

    // ------Uso Aqui---- Lógica corregida: Depende de horizontalInput para evitar retrasos de física
    void GestionarPasosRealistas()
    {
        // El personaje camina si el usuario presiona teclas de dirección Y el Raycast confirma que toca el suelo
        bool estaCaminando = horizontalInput != 0 && estaEnElSuelo;

        if (estaCaminando)
        {
            cronometroPasos += Time.deltaTime;

            if (cronometroPasos >= tiempoEntrePasos)
            {
                ReproducirPasoAleatorio();
                cronometroPasos = 0f; // Reiniciamos el segundero para el siguiente paso
            }
        }
        else
        {
            // Al detenerse, dejamos el cronómetro listo para que el primer paso suene instantáneamente al arrancar
            cronometroPasos = tiempoEntrePasos;
        }
    }

    private void ReproducirPasoAleatorio()
    {
        if (sonidosPasosVariados != null && sonidosPasosVariados.Length > 0)
        {
            // 1. Elegimos un archivo de paso al azar
            int indiceAleatorio = UnityEngine.Random.Range(0, sonidosPasosVariados.Length);
            AudioClip clipSeleccionado = sonidosPasosVariados[indiceAleatorio];

            if (clipSeleccionado != null)
            {
                // 2. MODULACIÓN: Alteramos ligeramente el Pitch y Volumen para romper el efecto robótico
                reproductorSFX.pitch = UnityEngine.Random.Range(0.88f, 1.12f);
                float volumenAleatorio = UnityEngine.Random.Range(0.85f, 1.0f);

                // 3. Lanzamos el SFX sin cortar los sonidos previos
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

            // ------Uso Aqui---- Sonido de Salto Único
            if (sonidoSalto != null)
            {
                reproductorSFX.pitch = 1.0f; // Reseteamos el pitch a la normalidad para el salto
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
            // Cambiado a GetKeyDown para registrar un único impacto de sonido y animación por pulsación
            if (Input.GetKeyDown(KeyCode.Q))
            {
                animator.SetBool("IsAttacking", true);

                // ------Uso Aqui---- Selección aleatoria de efectos de espada/lanza
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
                // Variación sutil en el pitch del ataque para que se sienta dinámico
                reproductorSFX.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                reproductorSFX.PlayOneShot(sonidosAtaqueVariados[indiceAleatorio]);
            }
        }
    }

    void ValidateMagicAtack()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // ------Uso Aqui---- Sonido de lanzamiento mágico único
            if (sonidoMagia != null)
            {
                reproductorSFX.pitch = 1.0f; // Reseteamos el pitch para el hechizo
                reproductorSFX.PlayOneShot(sonidoMagia);
            }

            DisparoLanzaMagica scriptLanza = GetComponentInChildren<DisparoLanzaMagica>(true);
            if (scriptLanza != null)
            {
                scriptLanza.DispararProyectil();
            }
        }

        bool canUseMagic = Input.GetKey(KeyCode.E) && !animator.GetBool("IsRunning");
        bool canUseMagicWhileRun = Input.GetKey(KeyCode.E) && animator.GetBool("IsRunning");

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