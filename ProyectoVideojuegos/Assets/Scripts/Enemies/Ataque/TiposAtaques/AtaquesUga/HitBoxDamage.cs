using UnityEngine;

public class HitboxDamage : MonoBehaviour
{
    [SerializeField] private AttackManager attackManager; // Ahora es visible en el Inspector
    private bool yaAplicoDaño = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !yaAplicoDaño)
        {
            if (attackManager != null)
            {
                attackManager.Atacar(other.transform);
                yaAplicoDaño = true;
            }
        }
    }

    private void OnDisable()
    {
        yaAplicoDaño = false;
    }
}