using UnityEngine;

/// <summary>
/// Gestiona Ataque a distancia solo jefe final
/// </summary>
public class AtaqueDistancia : IAttack
{
    private float daño;
    private Collider2D hitboxCollider; // Referencia al PolygonCollider2D (hitbox)

    // Constructor ahora recibe tambi�n el hitbox
    public AtaqueDistancia(float daño, Collider2D hitbox)
    {
        this.daño = daño;
        this.hitboxCollider = hitbox;
    }

    public void EjecutarAtaque(Transform controller, Transform objetivo)
    {
        Debug.Log($"Ataque magico con daño {daño}");

        // Opcional: instanciar un proyectil visual (no necesario para la detecci�n)
        // GameObject.Instantiate(orbePrefab, controller.position, Quaternion.identity);

        // Verifica si el hitbox est� activo y toca al jugador
        if (hitboxCollider == null || !hitboxCollider.gameObject.activeInHierarchy) return;

        // Detecta todos los colliders que solapan con el hitbox
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        Collider2D[] results = new Collider2D[5];
        int count = hitboxCollider.Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            if (results[i].CompareTag("Player"))
            {
                PlayerController player = results[i].GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(Mathf.RoundToInt(daño));
                    Debug.Log($"Hitbox de ataque distancia aplic� {daño} de daño");
                    break;
                }
            }
        }
    }
}
