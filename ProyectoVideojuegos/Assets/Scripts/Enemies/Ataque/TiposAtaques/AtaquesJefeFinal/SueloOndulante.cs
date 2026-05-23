using UnityEngine;

public class SueloOndulante : MonoBehaviour
{
    [Header("Configuración Visual (Shader)")]
    public Material materialSuelo2D;
    public float velocidadOnda = 8f;
    public float duracionEfecto = 1.2f;
    public float grosorOnda = 0.4f;

    [Header("Configuración Física (Colliders)")]
    public GameObject colliderOndaIzquierda;
    public GameObject colliderOndaDerecha;
    public float fuerzaEmpujeVertical = 10f;

    private float radioActual = 0f;
    private float tiempo = 0f;
    private bool ondaActiva = false;
    private Vector2 puntoImpacto;

    void Start()
    {
        // Inicializar el shader apagado (oculto)
        if (materialSuelo2D != null)
        {
            materialSuelo2D.SetFloat("_WaveRadius", -10f);
            materialSuelo2D.SetFloat("_WaveThickness", grosorOnda);
        }

        // Asegurar que los físicos empiecen desactivados
        if (colliderOndaIzquierda) colliderOndaIzquierda.SetActive(false);
        if (colliderOndaDerecha) colliderOndaDerecha.SetActive(false);
    }

    // Este método es público para que lo llame el Jugador o el Jefe
    public void ActivarOndaExpansiva(Vector2 posicionImpacto)
    {
        puntoImpacto = posicionImpacto;
        if (materialSuelo2D != null)
        {
            materialSuelo2D.SetVector("_WaveCenter", puntoImpacto);
        }

        radioActual = 0f;
        tiempo = 0f;
        ondaActiva = true;

        // Activar los triggers físicos
        if (colliderOndaIzquierda) colliderOndaIzquierda.SetActive(true);
        if (colliderOndaDerecha) colliderOndaDerecha.SetActive(true);
    }

    void Update()
    {
        if (!ondaActiva) return;

        tiempo += Time.deltaTime;
        if (tiempo >= duracionEfecto)
        {
            ApagarOnda();
            return;
        }

        // 1. CONTROL VISUAL: Expandir el radio en el shader
        radioActual += velocidadOnda * Time.deltaTime;
        if (materialSuelo2D != null)
        {
            materialSuelo2D.SetFloat("_WaveRadius", radioActual);

            // Atenuar la fuerza de la onda para que disminuya sutilmente y no se corte feo
            float fuerzaAtenuada = Mathf.Lerp(0.08f, 0f, tiempo / duracionEfecto);
            materialSuelo2D.SetFloat("_WaveForce", fuerzaAtenuada);
        }

        // 2. CONTROL FÍSICO: Mover los colliders invisibles al mismo ritmo que el shader
        if (colliderOndaDerecha)
        {
            colliderOndaDerecha.transform.position = new Vector2(puntoImpacto.x + radioActual, puntoImpacto.y);
        }
        if (colliderOndaIzquierda)
        {
            colliderOndaIzquierda.transform.position = new Vector2(puntoImpacto.x - radioActual, puntoImpacto.y);
        }
    }

    void ApagarOnda()
    {
        ondaActiva = false;
        if (materialSuelo2D != null)
        {
            materialSuelo2D.SetFloat("_WaveRadius", -10f);
        }
        if (colliderOndaIzquierda) colliderOndaIzquierda.SetActive(false);
        if (colliderOndaDerecha) colliderOndaDerecha.SetActive(false);
    }

    // Aplica daño o empuja a la entidad que no saltó a tiempo
    public void AplicarDañoOEmpuje(Collider2D collision)
    {
        // Si la onda la generó el jugador, debe golpear al enemigo (Boss), y viceversa
        if (collision.CompareTag("Player") || collision.CompareTag("Enemy"))
        {
            Rigidbody2D rbAfectado = collision.GetComponent<Rigidbody2D>();
            if (rbAfectado != null)
            {
                // Empujón hacia arriba para simular la sacudida del terreno
                rbAfectado.linearVelocity = new Vector2(rbAfectado.linearVelocity.x, fuerzaEmpujeVertical);
                Debug.Log("¡Entidad golpeada por la onda expansiva!");
            }

            PlayerController jugador = collision.GetComponent<PlayerController>();
            if (jugador != null)
            {
                // Invocamos TakeDamage  
                jugador.TakeDamage(15);

                Debug.Log("¡La onda expansiva golpeó al jugador!");
            }
        }
    }
}
