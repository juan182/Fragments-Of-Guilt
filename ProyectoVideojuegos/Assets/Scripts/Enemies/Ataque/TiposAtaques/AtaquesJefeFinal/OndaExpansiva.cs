using UnityEngine;

public class OndaExpansiva : MonoBehaviour
{
    private float velocidad;
    private float duracion;
    private float daño;
    private int direccion;
    private float timer;
    private bool yaGolpeo = false;

    public void Inicializar(float daño, float velocidad, float duracion, int direccion)
    {
        this.daño = daño;
        this.velocidad = velocidad;
        this.duracion = duracion;
        this.direccion = direccion;

        // Voltea el particle system segun la direccion
        if (direccion < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Mueve la onda horizontalmente pegada al suelo
        transform.position += new Vector3(direccion * velocidad * Time.deltaTime, 0, 0);

        // Se destruye al terminar su duracion
        if (timer >= duracion)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaGolpeo) return;
        if (!other.CompareTag("Player")) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.Daño(daño);
            yaGolpeo = true;
        }
    }
}