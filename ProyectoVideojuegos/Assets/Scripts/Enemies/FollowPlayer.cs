using UnityEngine;

/// <summary>
/// Gestiona la persecusion del enemigo hacia el jugador
/// </summary>
public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private float velocidadPersecusion=3f;
    [SerializeField] private float distanciaAtaque = 1f; //Distancia para dejar de caminar y atacar al jugador

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
        transform.Translate(new Vector3(sentido * velocidadPersecusion * Time.deltaTime, 0, 0));

    }

    private void OrientacionSprite(float direccionX)
    {
        if (direccionX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

    }
}
