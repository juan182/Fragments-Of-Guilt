using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    #region eventos del personaje
    // Este es importante::::
    // Este evento lo usara el GameManager para poder comunicarse con
    // UIManager y comunicar GameOver.
    public static event Action OnPlayerDeath; //GameManager se suscribe a este evento
    #endregion

    #region Acciones del Personaje
    [Header("Movimiento")]
    public float movementSpeed = 8f;
    private Rigidbody2D rb;
    private float horizontalInput;

    [Header("Salto")]
    public float jumpForce = 10f;
    public LayerMask capaSuelo;
    private bool isOnGround = false;
    private bool shouldJump = false;
    #endregion

    #region Estadisticas
    [Header("Estadisticas")]
    //Vida
    public float maxHealth = 200f;
    public float currentHealth;
    //Stamina
    public float maxStamina = 150f;
    public float currentStamina;
    public float staminaRegenRate = 5f;
    #endregion


    // ANIMATOR AQUI
    [Header("Estados de Movimiento")]
    private Animator animator;

    [Header("Estado de Combate")]
    public bool isBlocking = false;


    [Header("Habilidades, Armas activadas?")]
    public bool hasSpear = false;
    public bool hasMagic = false;
    public bool hasParry = false;
    public bool hasStrongAttack = false;
    public bool hasBook = false;



    Dictionary<string, bool> unlockItems = new Dictionary<string, bool>()
    {
        {"Lanza", false},
        {"FragementoStamina", false},
        {"FragementoMagia", false },
        {"LibroEGVA", false },
        {"FragmentoRecuerdo1", false }
    };


    private void Awake()
    {
        //BUSQUEDA DEL ANIMATOR AQUI.
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }


    //En Update iran todas las validaciones como Deteccion de teclas, Ejecucion de animaciones.
    private void Update()
    {
        //Validaciones
        ValidateMovement();
        ValidateJump();
        ValidateAtack();
        ValidateMagicAtack();

    }

    //FixedUpdate ejecuta la logica de fisicas, no meter nada que no sea de las fisicas aqui...
    private void FixedUpdate()
    {
        Movement();
        Jump();
    }

    #region Movimiento
    //_______
    void ValidateMovement()
    {

        //Horizontal registrar valores entre -1 0 1
        //Si el valor de horizontal cambia de 0 a valores entre -1 o 1 pues se ejecuta animacion de moverse.
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (horizontalInput != 0)
        {
            animator.SetBool("IsRunning", true);
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }

    }

    void Movement()
    {
        rb.linearVelocity = new Vector2(horizontalInput * movementSpeed, rb.linearVelocity.y);
    }
    //_________
    #endregion


    #region Salto
    //_______
    void ValidateJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            shouldJump = true;
            //animator.SetBool("isJumping", true); Si se implementa la logica para apagarlo esta medio complicada i Think.
        }
    }

    void Jump()
    {
        //Logica del salto
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.7f, capaSuelo);
        if (hit.collider != null)
        {
            isOnGround = true;
            Debug.DrawRay(transform.position, Vector2.down * 0.7f, Color.green);
        }
        else
        {
            isOnGround = false;
            Debug.DrawRay(transform.position, Vector2.down * 0.7f, Color.red);
        }

        if (shouldJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            shouldJump = false;
        } //Fin logica del salto 
    }
    //_________
    #endregion


    #region Ataques
    //---------
    void ValidateAtack()
    {   //Esta logica puede ser redundante, pero la implemento para evitar algun error o eventual cambio


        // Si esta presionando Q, pero no esta corriendo esta ejecutando ataque quieto
        bool canAtack = Input.GetKey(KeyCode.Q) && !animator.GetBool("IsRunning");
        // Si esta presionando Q y esta corriendo esta ejecutando ataque en movimiento
        bool canAtackWhileRun = Input.GetKey(KeyCode.Q) && animator.GetBool("IsRunning");

        if (canAtack || canAtackWhileRun)
        {//Si se cumple que una de las dos esta en ejecucion
         // QUE BASTARIA SOLO CON DEJAR EL PRIMERO : canAtack.
            animator.SetBool("IsAttacking", true);
        }
        else
        {
            animator.SetBool("IsAttacking", false);
        }
    }

    void ValidateMagicAtack()
    { //ES PRACTICAMENTE LO MISMO QUE EL DE ARRIBA
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

    public void AtaquePesado()
    {
        if (hasSpear && hasStrongAttack && currentStamina >= 20f)
        {
            // ¿Que pasa si se usa?
            // pasa lo siguiente...
            currentStamina -= 20f;
            Debug.Log("Hola mama estoy tirando ataques pesados");
        }
        else
        {
            Debug.Log("Ey care monda no cumplis las condiciones");
        }
    }
    //----------
    #endregion

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Esta gonorrea me pego una puñalada");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        OnPlayerDeath?.Invoke(); //Notificamos a los que esten suscritos a el metodo Die
    }

    public void Fragments(string fragment)
    {

        if (string.IsNullOrEmpty(fragment)) return;
        if (unlockItems.ContainsKey(fragment))
        {
            unlockItems[fragment] = true;

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
}
