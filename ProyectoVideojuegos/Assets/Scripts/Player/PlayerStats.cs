using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    #region Eventos del Personaje
    // Este evento lo usara el GameManager para poder comunicarse con
    // UIManager y comunicar GameOver.
    public static event Action OnPlayerDeath; // GameManager se suscribe a este evento
    #endregion

    #region Acciones del Personaje
    [Header("Movimiento")]
    public float movementSpeed = 8f;        // Velocidad de movimiento horizontal
    private Rigidbody2D rb;                 // Referencia al Rigidbody2D del personaje
    private float horizontalInput;          // Valor entre -1, 0 y 1 segun input horizontal

    [Header("Salto")]
    public float jumpForce = 5f;            // Fuerza aplicada al saltar
    public LayerMask capaSuelo;             // Capa que define que es suelo para el Raycast
    private bool isOnGround = false;        // True si el personaje esta tocando el suelo
    private bool shouldJump = false;        // Flag para ejecutar el salto en FixedUpdate
    #endregion

    #region Estadisticas
    [Header("Estadisticas")]
    // Vida
    public float maxHealth = 200f;          // Vida maxima del personaje
    public float currentHealth;             // Vida actual, se inicializa en Start()
    // Stamina
    public float maxStamina = 150f;         // Stamina maxima del personaje
    public float currentStamina;            // Stamina actual, se inicializa en Start()
    public float staminaRegenRate = 5f;     // Cuanta stamina regenera por segundo
    #endregion

    #region Componentes y Estado
    [Header("Componentes")]
    private Animator animator;              // Referencia al Animator del personaje

    [Header("Estado de Combate")]
    public bool isBlocking = false;         // True si el personaje esta bloqueando (sin implementar aun)
    private bool isDead = false;            // Flag para bloquear inputs cuando el personaje muere

    [Header("Habilidades y Armas")]
    public bool hasSpear = false;           // Tiene la lanza desbloqueada
    public bool hasMagic = false;           // Tiene la magia desbloqueada
    public bool hasParry = false;           // Tiene el parry desbloqueado
    public bool hasStrongAttack = false;    // Tiene el ataque fuerte desbloqueado
    public bool hasBook = false;            // Tiene el libro EGVA
    #endregion

    #region Inventario de Fragmentos
    // Diccionario que registra que fragmentos/items han sido recogidos
    Dictionary<string, bool> unlockItems = new Dictionary<string, bool>()
    {
        {"Lanza", false},
        {"FragementoStamina", false},
        {"FragementoMagia", false},
        {"LibroEGVA", false},
        {"FragmentoRecuerdo1", false}
    };
    #endregion

    private void Awake()
    {
        // Obtenemos los componentes al iniciar antes que cualquier otra cosa
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // Evita que el Rigidbody rote por fisicas
    }

    private void Start()
    {
        // Inicializamos vida y stamina a sus valores maximos al arrancar
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    private void Update()
    {
        // Si el personaje esta muerto bloqueamos cualquier input
        if (isDead) return;

        // Validaciones de input (deteccion de teclas y animaciones)
        ValidateMovement();
        ValidateJump();
        ValidateAtack();
        ValidateMagicAtack();
        AtaquePesado();
        StaminaRegen();
    }

    private void FixedUpdate()
    {
        // Si el personaje esta muerto bloqueamos las fisicas
        if (isDead) return;

        // Logica de fisicas
        Movement();
        Jump();
    }

    #region Movimiento
    //_______
    void ValidateMovement()
    {
        // Capturamos el input horizontal: -1 izquierda, 0 quieto, 1 derecha
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Verificamos si alguna animacion de ataque esta activa
        bool isAttacking = animator.GetBool("IsAttacking") ||
                           animator.GetBool("IsMagic") ||
                           animator.GetBool("IsHeavy");

        if (horizontalInput != 0 && !isAttacking && isOnGround)
        {
            // Solo corremos si hay input, no estamos atacando y estamos en el suelo
            animator.SetBool("IsRunning", true);

            // Giramos el sprite segun la direccion de movimiento
            // Abs evita acumulacion de escala, Sign devuelve 1 o -1
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(horizontalInput);
            transform.localScale = scale;
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }
    }

    void Movement()
    {
        // Verificamos si alguna animacion de ataque esta activa
        bool isAttacking = animator.GetBool("IsAttacking") ||
                           animator.GetBool("IsMagic") ||
                           animator.GetBool("IsHeavy");

        if (isAttacking)
        {
            // Detenemos el movimiento horizontal al atacar, pero preservamos la velocidad Y (gravedad/salto)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            // Movimiento normal horizontal, preservando velocidad Y
            rb.linearVelocity = new Vector2(horizontalInput * movementSpeed, rb.linearVelocity.y);
        }
    }
    //_________
    #endregion

    #region Salto
    //_______
    void ValidateJump()
    {
        // Solo podemos saltar si estamos en el suelo
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            shouldJump = true; // Le decimos a FixedUpdate que ejecute el salto
        }
    }

    void Jump()
    {
        // Raycast hacia abajo para detectar si estamos en el suelo
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.7f, capaSuelo);

        if (hit.collider != null)
        {
            // Tocamos suelo: reseteamos flags y animaciones de aire
            isOnGround = true;
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
            Debug.DrawRay(transform.position, Vector2.down * 0.7f, Color.green);
        }
        else
        {
            isOnGround = false;
            Debug.DrawRay(transform.position, Vector2.down * 0.7f, Color.red);

            // Diferenciamos entre subir (salto) y bajar (caida) por la velocidad Y
            if (rb.linearVelocity.y > 0)
            {
                animator.SetBool("IsJumping", true);
                animator.SetBool("IsFalling", false);
            }
            else
            {
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsFalling", true);
            }
        }

        if (shouldJump)
        {
            // Aplicamos la fuerza de salto y reseteamos el flag
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            shouldJump = false;
        }
    }
    //_________
    #endregion

    #region Ataques
    //---------
    void ValidateAtack()
    {
        // Click izquierdo: ataque normal
        // GetMouseButton mantiene el bool activo mientras se sostiene el click
        bool canAtack = Input.GetMouseButton(0);
        if (canAtack)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetBool("IsRunning", false); // Cancela la animacion de correr
        }
        else
        {
            animator.SetBool("IsAttacking", false);
        }
    }

    void ValidateMagicAtack()
    {
        // Tecla E: ataque magico
        bool canUseMagic = Input.GetKey(KeyCode.E);
        if (canUseMagic)
        {
            animator.SetBool("IsMagic", true);
            animator.SetBool("IsRunning", false); // Cancela la animacion de correr
        }
        else
        {
            animator.SetBool("IsMagic", false);
        }
    }

    public void AtaquePesado()
    {
        // Click derecho: ataque pesado (requiere lanza, ataque fuerte desbloqueado y 20 de stamina)
        if (Input.GetMouseButtonDown(1))
        {
            if (hasSpear && hasStrongAttack && currentStamina >= 20f)
            {
                currentStamina -= 20f;              // Consumimos stamina
                animator.SetBool("IsHeavy", true);
                animator.SetBool("IsRunning", false); // Cancela la animacion de correr
                Debug.Log("Hola mama estoy tirando ataques pesados");
            }
            else
            {
                animator.SetBool("IsHeavy", false);
                Debug.Log("Ey care monda no cumplis las condiciones");
            }
        }
        else if (!Input.GetMouseButton(1))
        {
            // Apagamos IsHeavy cuando se suelta el click derecho
            animator.SetBool("IsHeavy", false);
        }
    }
    //---------
    #endregion

    #region Vida y Daño
    //---------
    public void TakeDamage(int damage)
    {
        // No recibimos daño si ya estamos muertos
        if (isDead) return;

        currentHealth -= damage;
        animator.SetBool("IsDamage", true); // Activa animacion de daño
        Debug.Log("Esta gonorrea me pego una puñalada");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Llamar este metodo como Animation Event al terminar la animacion de daño
    public void ResetDamageAnim()
    {
        animator.SetBool("IsDamage", false);
    }

    public void Die()
    {
        isDead = true;                              // Bloqueamos todos los inputs
        animator.SetBool("IsDead", true);           // Activa animacion de muerte
        rb.linearVelocity = Vector2.zero;           // Detenemos al personaje
        OnPlayerDeath?.Invoke();                    // Notificamos al GameManager
    }
    //---------
    #endregion

    #region Stamina
    //---------
    void StaminaRegen()
    {
        // Regeneramos stamina gradualmente hasta el maximo
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina); // Clamp para no pasarse del maximo
        }
    }
    //---------
    #endregion

    #region Fragmentos e Items
    //---------
    public void Fragments(string fragment)
    {
        // Validamos que el string no este vacio y que exista en el diccionario
        if (string.IsNullOrEmpty(fragment)) return;

        if (unlockItems.ContainsKey(fragment))
        {
            unlockItems[fragment] = true; // Marcamos el fragmento como recogido

            switch (fragment)
            {
                case "Lanza":
                    hasSpear = true;
                    Debug.Log("Lanza :v");
                    break;
                case "FragmentoStamina":
                    hasStrongAttack = true;
                    Debug.Log("Ataque duro contra el mundo activao mi socio");
                    break;
                case "FragmentoMagia":
                    hasMagic = true;
                    hasParry = true;
                    Debug.Log("Bomba Molotov univalluna Activada");
                    break;
                case "LibroEGVA":
                    hasBook = true;
                    Debug.Log("Libro EGVA obtenido ña");
                    break;
            }
        }
        else
        {
            Debug.Log("No hay asignacion");
        }
    }
    //---------
    #endregion
}