using UnityEngine;

/// <summary>
/// Gestiona la persecusion del enemigo hacia el jugador
/// </summary>
public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private float velocidadPersecusion=3f;
    [SerializeField] private float distanciaAtaque = 1.8f; //Distancia para dejar de caminar y atacar al jugador

    

    private Transform jugador;

    /// <summary>
    /// Define a quien debe seguir, este metodo es llamado desde EnemyAI
    /// </summary>
    public void SetObjetivo(Transform Objetivo)
    {
        jugador = Objetivo;
    }

    private void Update()
    {
        if (jugador != null)
        {
            float direccionX=jugador.position.x-transform.position.x; //Calcula la direccion hacia el jugador en el eje x

            OrientacionSprite(direccionX); //Gira el sprite del enemigo hacia la direccion del jugador

            //Se mueve unicamente si no esta pegado al jugador
            float distanciaActual = Mathf.Abs(direccionX);

            if (distanciaActual > distanciaAtaque)
            {
                MoverseHaciaObjetivo(direccionX);
            }
            else
            {
                //Metodo ataque por EnemyAI
            }
        }
    }

    private void MoverseHaciaObjetivo(float direccionX)
    {
        // 1 es hacia la derecha, -1 hacia la izquierda
        float sentido=Mathf.Sign(direccionX);
        Vector3 nuevaPosicion = transform.position;
        nuevaPosicion.x += sentido * velocidadPersecusion * Time.deltaTime;
        transform.position = nuevaPosicion;
    }

    private void OrientacionSprite(float direccionX)
    {
        Vector3 escala=transform.localScale; //Escala actual del sprite en caso que su escala sea distinta a 1
        if (direccionX < 0)
        {
            escala.x=Mathf.Abs(escala.x); //izq
        }
        else if(direccionX > 0)
        {
            escala.x=-Mathf.Abs(escala.x); //der
            
        }
        transform.localScale = escala;
        Debug.Log($"Dirección: {direccionX}, Nueva escala X: {escala.x}");
    }
}
