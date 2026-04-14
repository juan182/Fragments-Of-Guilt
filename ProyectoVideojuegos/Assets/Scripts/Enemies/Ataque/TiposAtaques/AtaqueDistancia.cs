using UnityEngine;

/// <summary>
/// Gestiona Ataque a distancia solo jefe final
/// </summary>
public class AtaqueDistancia : IAttack
{
    private float daño;

    public AtaqueDistancia(float daño)
    {
        this.daño = daño;
    }

    public void EjecutarAtaque(Transform controller, Transform objetivo)
    {
        Debug.Log($"Ataque mágico con daño {daño}");
        // Instance al prefab orbe de magia
        // GameObject.Instantiate(orbePrefab, controller.position, Quaternion.identity)
        
        
        MovementController playerStats=objetivo.GetComponent<MovementController>();
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
