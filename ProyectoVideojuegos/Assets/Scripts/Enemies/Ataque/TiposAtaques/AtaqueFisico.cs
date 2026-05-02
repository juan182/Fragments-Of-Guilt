using UnityEngine;

/// <summary>
/// Gestiona el ataque cuerpo a cuerpo de uga y jefe final
/// </summary>
public class AtaqueFisico : IAttack
{
    private float daño;
    private Collider2D hitboxCollider; // Referencia al PolygonCollider2D

    public AtaqueFisico(float daño, Collider2D hitbox)
    {
        this.daño = daño;
        this.hitboxCollider = hitbox;
    }

    public void EjecutarAtaque(Transform controller, Transform objetivo)
    {
        if (hitboxCollider == null || !hitboxCollider.gameObject.activeInHierarchy) return;

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
                    Debug.Log($"Hitbox aplica {daño} de daño");
                    break;
                }
            }
        }
    }
}
