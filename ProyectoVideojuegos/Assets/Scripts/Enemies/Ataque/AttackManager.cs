using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public IAttack ataqueActual;

    // Métodos que crean la estrategia con el daño indicado
    public void SetAtaqueFisico(float daño, Collider2D hitbox)
    {
        ataqueActual = new AtaqueFisico(daño, hitbox);
    }

    public void SetAtaqueDistancia(float daño, Collider2D hitbox)
    {
        ataqueActual = new AtaqueDistancia(daño, hitbox);
    }


    public void Atacar(Transform objetivo)
    {
        if (ataqueActual != null)
        {
            ataqueActual.EjecutarAtaque(transform, objetivo);
        }
    }

    public bool usaAtaqueDistancia() => ataqueActual is AtaqueDistancia;
    public bool usaAtaqueFisico() => ataqueActual is AtaqueFisico;
}
