using UnityEngine;

public class ProyectilMagico : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 12f;
    [SerializeField] private float duracion = 3f;

    [Header("Daño")]
    [SerializeField] private float dañoProyectil = 10f;

    [Header("Particulas")]
    [SerializeField] private ParticleSystem particulasBala;
    [SerializeField] private ParticleSystem particulasImpacto;

    private int direccion = 1;
    private float timer = 0f;
    private bool yaImpacto = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D miCollider;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        miCollider = GetComponent<Collider2D>();

        // Asegurar que la explosión empiece apagada
        if (particulasImpacto != null)
        {
            particulasImpacto.Stop();
        }

        // Encender el rastro de vuelo
        if (particulasBala != null)
        {
            particulasBala.Play();
        }
    }

    public void Inicializar(int dir, float dañoBase)
    {
        direccion = dir;
        dañoProyectil = dañoBase;

        Vector3 escala = transform.localScale;
        escala.x = dir > 0 ? Mathf.Abs(escala.x) : -Mathf.Abs(escala.x);
        transform.localScale = escala;
    }

    private void Update()
    {
        if (yaImpacto) return;

        timer += Time.deltaTime;

        // Trayectoria lineal constante y limpia en horizontal
        transform.position += new Vector3(direccion * velocidad * Time.deltaTime, 0, 0);

        if (timer >= duracion)
            Impactar();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Regla de seguridad 1: Si ya impactó en este frame, ignoramos duplicados
        if (yaImpacto) return;

        // Regla de seguridad 2: Nunca estallar contra el propio jugador que lo dispara
        if (other.CompareTag("Player")) return;

        // Regla de seguridad 3: Ignorar otros triggers invisibles del mapa (zonas de carga, etc.) 
        // Si el objeto tocado es un Trigger y no es un enemigo, lo dejamos pasar de largo
        if (other.isTrigger && !other.CompareTag("Enemy")) return;

        // ---- LOGICA DE DAÑO DINÁMICA ----
        // No nos importa el Tag ni el Layer que tenga. Buscamos si tiene el script de Vida
        // en el objeto que tocamos o en alguno de sus padres en la jerarquía.
        Health sistemaVida = other.GetComponentInParent<Health>();
        if (sistemaVida == null)
        {
            sistemaVida = other.GetComponent<Health>();
        }

        if (sistemaVida != null)
        {
            sistemaVida.Daño(dañoProyectil);
            Debug.Log($"¡Impacto confirmado en {other.name}! Daño: {dañoProyectil}");
        }

        // ---- IMPACTO UNIVERSAL ----
        // Al tocar CUALQUIER COSA sólida (suelo, pared, cajas, enemigos, Boss), estalla
        Impactar();
    }

    private void Impactar()
    {
        if (yaImpacto) return;
        yaImpacto = true;

        // Desactivar físicas del orbe para evitar dobles choques seguidos
        if (miCollider != null) miCollider.enabled = false;

        // Ocultar la bola de inmediato
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        // Apagar el rastro de humo del vuelo
        if (particulasBala != null) particulasBala.Stop();

        // Disparar la explosión justo en su coordenada actual
        if (particulasImpacto != null)
        {
            particulasImpacto.transform.SetParent(null); // Desacoplar para que sobreviva en la escena
            particulasImpacto.Play();

            // Limpieza del objeto suelto de chispas
            Destroy(particulasImpacto.gameObject, particulasImpacto.main.duration + 0.5f);
        }

        // Destrucción inmediata del contenedor lógico del proyectil
        Destroy(gameObject, 0.02f);
    }
}
