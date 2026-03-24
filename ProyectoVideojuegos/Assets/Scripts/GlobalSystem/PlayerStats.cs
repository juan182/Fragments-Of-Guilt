using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // Este es importante::::
    // Este evento lo usara el GameManager para poder comunicarse con
    // UIManager y comunicar GameOver.
    public static event Action OnPlayerDeath; //GameManager se suscribe a este evento
    // ------

    [Header("Movimiento")]
    public float movementSpeed = 8f;
    private Rigidbody2D rb;
    private float horizontalInput;

    [Header("Salto")]
    public float jumpForce= 10f;
    public LayerMask capaSuelo;
    private bool isOnGround = false;
    private bool shouldJump = false;

    [Header("Estadisticas")]
    //Vida
    public float maxHealth = 200f;
    public float currentHealth;
    //Stamina
    public float maxStamina = 150f;
    public float currentStamina;
    public float staminaRegenRate = 5f;

    [Header("Estado de Combate")]
    public bool isBlocking= false;
    
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
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            shouldJump = true;
        }
        
    }
    private void FixedUpdate()
    {
        Movement();

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
           shouldJump= false; 
        } //Fin logica del salto 
        
    }
    void Movement()
    {
        rb.linearVelocity = new Vector2 (horizontalInput*movementSpeed, rb.linearVelocity.y);
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
}
