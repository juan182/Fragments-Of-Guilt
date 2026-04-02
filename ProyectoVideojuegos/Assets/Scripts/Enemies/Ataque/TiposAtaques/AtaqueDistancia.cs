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
        
        
        Health salud = objetivo.GetComponent<Health>();
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
