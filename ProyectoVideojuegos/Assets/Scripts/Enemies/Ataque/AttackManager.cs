using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public IAttack ataqueActual;

    // Métodos que crean la estrategia con el daño indicado
    public void SetAtaqueFisico(float daño)
    {
        ataqueActual = new AtaqueFisico(daño);
    }

    public void SetAtaqueDistancia(float daño)
    {
        ataqueActual = new AtaqueDistancia(daño);
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
