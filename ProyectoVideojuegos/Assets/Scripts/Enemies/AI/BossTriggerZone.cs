using UnityEngine;

public class BossTriggerZone : MonoBehaviour
{
    [SerializeField] private BossController boss;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!other.CompareTag("Player")) return;
            boss.ActivarJefe();
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
