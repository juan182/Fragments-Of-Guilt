using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float vidaMaxima = 100f;
    private float vidaActual;

    [Header("Muerte")]
    [SerializeField] private float tiempoParpadeo = 1.5f;  // Duración del parpadeo antes de destruir

    private EnemySoundController enemySoundController;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D[] colliders;

    private SpriteRenderer[] sprites;

    public float VidaActual => vidaActual;
    public float VidaMaxima => vidaMaxima;

    void Start()
    {
        vidaActual = vidaMaxima;
        enemySoundController = GetComponent<EnemySoundController>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();
    }

    public void Daño(float cantidad)
    {
        if (vidaActual <= 0) return;
        vidaActual -= cantidad;
        Debug.Log($"{name} recibió {cantidad} de daño");

        if (animator != null) animator.SetTrigger("daño");

        if (vidaActual <= 0) Morir();
    }

    private void Morir()
    {
        if (enemySoundController != null)
            enemySoundController.PlayMuerte();
        else
            Debug.LogWarning("EnemySoundController no encontrado en " + name);

        // 1. Desactivar scripts de movimiento y ataque
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null) enemyAI.enabled = false;

        FollowPlayer follow = GetComponent<FollowPlayer>();
        if (follow != null) follow.enabled = false;

        EnemyPatrol patrol = GetComponent<EnemyPatrol>();
        if (patrol != null) patrol.enabled = false;

        AttackManager attack = GetComponent<AttackManager>();
        if (attack != null) attack.enabled = false;

        // 2. Detener físicamente
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // 3. Activar animación de muerte (si existe)
        if (animator != null)
            animator.SetBool("IsDead", true);

        // 4. Iniciar efecto de parpadeo y destrucción
        StartCoroutine(ParpadearYMorir());
    }

    private IEnumerator ParpadearYMorir()
    {
        Debug.Log("Iniciando rutina de muerte...");

        if (colliders != null)
        {
            foreach (var col in colliders)
                if (col != null) col.isTrigger = true;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
                rb.simulated = false;
            }

            float esperaInicial = 0.5f;
            yield return new WaitForSeconds(esperaInicial);

            // --- 3. Obtener los sprites (si no los tenemos ya) ---
            SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
            if (sprites.Length == 0) Debug.LogWarning("No se encontraron SpriteRenderer");

            float duracion = tiempoParpadeo;
            float inicio = Time.time;

            while (Time.time - inicio < duracion)
            {
                bool visible = Mathf.FloorToInt((Time.time - inicio) * 10) % 2 == 0; // 10 veces por segundo
                foreach (var sp in sprites)
                    if (sp != null) sp.enabled = visible;
                yield return new WaitForSeconds(0.1f);
            }

            foreach (var sp in sprites)
                if (sp != null) sp.enabled = false;

            Debug.Log("Destruyendo objeto");
            Destroy(gameObject);
        }
    }
}