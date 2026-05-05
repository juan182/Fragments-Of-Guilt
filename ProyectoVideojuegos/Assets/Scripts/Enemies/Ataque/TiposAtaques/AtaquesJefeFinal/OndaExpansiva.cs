using UnityEngine;

public class OndaExpansiva : MonoBehaviour
{
    private float daño;
    private float velocidad;
    private float duracion;
    private float timerVida;
    private int direccion;

    public void Inicializar(float daño, float velocidad, float duracion, int direccion)
    {
        this.daño = daño;
        this.velocidad = velocidad;
        this.duracion = duracion;
        this.direccion = direccion;
        timerVida = duracion;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Movimiento horizontal
        transform.position += Vector3.right * direccion * velocidad * Time.deltaTime;

        timerVida -= Time.deltaTime;
        if (timerVida <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(Mathf.RoundToInt(daño));
                Debug.Log($"Onda expansiva golpeó al jugador: {daño} daño");
            }
            Destroy(gameObject);
        }
    }

}
