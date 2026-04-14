using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{

    [Header("Estados de Movimiento")]
    private Animator animator;

    [Header("Movimiento")]
    public float movementSpeed;
    private Rigidbody2D rb;
    private float horizontalInput;

    [Header("Salto")]
    public float jumpForce;
    public LayerMask capaSuelo;
    private bool isOnGround = false; //Esta en el suelo?
    private bool shouldJump = false; //Puede saltar?
    
    [Header("Estado de Combate")]
    public bool isBlocking = false;


    private void Awake()
    {
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
        rb.linearVelocity = new Vector2 (horizontalInput*movementSpeed, rb.linearVelocity.y);
    }
    #endregion


    #region Salto
    //Este registra la tecla
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
        Vector2 posicionn = (Vector2) transform.position + new Vector2(0f, 0.5f); 
        //Logica del salto
        RaycastHit2D hit = Physics2D.Raycast(posicionn, Vector2.down, 0.3f, capaSuelo);
        if (hit.collider != null)
        {
            isOnGround = true;
            Debug.DrawRay(posicionn, Vector2.down * 0.3f, Color.green);
        }
        else
        {
            isOnGround = false;
            Debug.DrawRay(posicionn, Vector2.down * 0.3f, Color.red);
        }

        if (shouldJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            shouldJump = false;
        } //Fin logica del salto 
    }
    #endregion


    #region Ataques
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
    #endregion

    
}
