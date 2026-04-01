using UnityEngine;

/// <summary>
/// Gestiona la deteccion del jugador/ Puede ser usado por cualquier elemento del juego
/// No solo por enemigos
/// </summary>
public class DeteccionJugador : MonoBehaviour
{
    [SerializeField] private float distanciaRayCast = 2f;
    [SerializeField] private LayerMask capaDeteccion; //Seleccionar al jugador

    private Vector2 direccionRayCast= Vector2.left; //Direccion a la izquierda por default

    

    // Update is called once per frame
    void Update()
    {
        //Actualiza la direccion segun donde mire el sprite del enemigo
        //si localScale.x es negativo mira a la derecha
        direccionRayCast=(transform.localScale.x>0)? Vector2.left: Vector2.right;

        //Lanza el raycast
        RaycastHit2D hit=Physics2D.Raycast(transform.position,direccionRayCast,distanciaRayCast,capaDeteccion);

        //Checa que toco el raycast
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                //Logica de activar persecusion
            }
        }
    }
}
