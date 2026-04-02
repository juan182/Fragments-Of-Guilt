using UnityEngine;

/// <summary>
/// Gestiona el ataque cuerpo a cuerpo de uga y jefe final
/// </summary>
public class AtaqueFisico : IAttack
{
    private float daño;

    public AtaqueFisico(float daño)
    {
        this.daño = daño;
    }
    public void EjecutarAtaque(Transform controller, Transform objetivo)
    {
        Debug.Log($"Ataque cuerpo a cuerpo con daño: {daño}");
        //Logica daño 

        Health salud=objetivo.GetComponent<Health>();
        if (salud != null) 
        {
            salud.Daño(daño);
        }
        else
        {
            Debug.LogWarning($"El objetivo {objetivo.name} no tiene componente Health");
        }
    }
}
