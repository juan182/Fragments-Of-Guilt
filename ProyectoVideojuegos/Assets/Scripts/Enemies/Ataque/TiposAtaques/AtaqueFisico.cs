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

        MovementController playerStats = objetivo.GetComponent<MovementController>();
        if (playerStats != null)
        {
            int dañoInt = Mathf.RoundToInt(daño); //Convertimos la vidaMaxima del jugador (float) a enteros
            //playerStats.TakeDamage(dañoInt);
            return;
        }
        else
        {
            Debug.Log($"El objetivo {objetivo.name} no encuentra o tiene PlayerStats");
        }
    }
}
