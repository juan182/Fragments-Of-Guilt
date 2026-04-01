using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Gestiona el movimiento patrulla de los enemigos entre 2 puntos específicos
/// </summary>

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;

    [SerializeField] private float velocidad; // Velocidad a la que se mueve el enemigo

    private Transform puntoObjetivo;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (puntoA == null || puntoB == null)
        {
            Debug.Log("No se han asignado los puntos de patrulla correctamente");
        }

        puntoObjetivo = puntoA;

    }

    // Update is called once per frame
    void Update()
    {
        // Se calcula la distancia entre el punto objetivo inicializado en Start y el enemigo
        float distancia = Vector3.Distance(transform.position, puntoObjetivo.position);

        if (distancia < 0.1f)
        {
            puntoObjetivo = (puntoObjetivo == puntoA) ? puntoB : puntoA;
        }

        Moverse();

    }

    /// <summary>
    /// Desplaza al enemigo hacia un punto fisico especifico
    /// </summary>

    private void Moverse()
    {
        transform.position = Vector3.MoveTowards(transform.position, puntoObjetivo.position, velocidad * Time.deltaTime);

    }

}
