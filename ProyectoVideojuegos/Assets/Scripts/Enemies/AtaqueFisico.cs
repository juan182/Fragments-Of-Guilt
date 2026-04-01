using UnityEngine;

/// <summary>
/// Gestiona el ataque cuerpo a cuerpo de uga y jefe final
/// </summary>
public class AtaqueFisico : IAttack
{
    public void EjecutarAtaque(Transform controller, Transform objetivo)
    {
        Debug.Log("Ataque cuerpo a cuerpo");
        //Logica daño al jugador; objetivo.GetComponent<Health>.Daño(10)
    }
}
