using UnityEngine;

public class AttackManager : MonoBehaviour
{
    private IAttack ataqueActual;

    //Método que asigna el tipo de ataque desde EnemyAI
    public void SetAtaque(IAttack nuevoAtaque)
    {
        ataqueActual = nuevoAtaque;
    }

    public void Atacar(Transform objetivo)
    {
        if (ataqueActual != null)
        {
            ataqueActual.EjecutarAtaque(transform, objetivo);
        }
    }
}
