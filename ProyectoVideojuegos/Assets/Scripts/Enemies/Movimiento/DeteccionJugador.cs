using UnityEngine;

public class DeteccionJugador : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private EnemySoundController enemySoundController;
    [Header("Cooldown")]
    [SerializeField] private float cooldownDeteccion = 2f;

    public bool VeAlJugador { get; private set; }
    private float tiempoUltimoSonido = -100f;
    private int jugadoresDentro = 0; // Contador por si hay múltiples players

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadoresDentro++;
            VeAlJugador = true;

            if (Time.time >= tiempoUltimoSonido + cooldownDeteccion)
            {
                enemySoundController?.PlayDetectaJugador();
                tiempoUltimoSonido = Time.time;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Aseguramos que siga siendo true mientras esté dentro
            VeAlJugador = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadoresDentro--;
            if (jugadoresDentro <= 0)
            {
                VeAlJugador = false;
                jugadoresDentro = 0;
            }
        }
    }
}