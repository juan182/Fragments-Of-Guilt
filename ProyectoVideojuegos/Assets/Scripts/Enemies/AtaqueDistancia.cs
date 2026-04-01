using UnityEngine;

/// <summary>
/// Gestiona Ataque a distancia solo jefe final
/// </summary>
public class AtaqueDistancia : IAttack
{
    public void EjecutarAtaque(Transform controller, Transform objetivo)
    {
        Debug.Log("Ataque mágico");
        // Instance al prefab orbe de magia
        // GameObject.Instantiate(orbePrefab, controller.position, Quaternion.identity)
    }
}
