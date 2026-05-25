using UnityEngine;

public class quitarvida : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out PlayerController jugador))
        {
            jugador.TakeDamage(5);
        }
    }
}
