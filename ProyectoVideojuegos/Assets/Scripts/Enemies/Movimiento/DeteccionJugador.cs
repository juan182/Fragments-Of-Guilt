using UnityEngine;

/// <summary>
/// Gestiona la deteccion del jugador/ Puede ser usado por cualquier elemento del juego
/// No solo por enemigos
/// </summary>
public class DeteccionJugador : MonoBehaviour
{
    [SerializeField] private float radioDeteccion = 3f;
    [SerializeField] private LayerMask capaJugador; // Asigna la capa donde está el jugador (ej: "Player" o "Default")

    public bool VeAlJugador { get; private set; }

    private void Update()
    {
        VeAlJugador = false;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radioDeteccion, capaJugador);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                VeAlJugador = true;
                break;
            }
        }
    }

    // Dibuja el área de detección en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = VeAlJugador ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}


