using System;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Esta es la base de datos central del Player, es donde vamos a estar consultando los
    /// diferentes atributos del player, tambien sirve para la recuperacion de datos en caso de errores entre scenas
    /// </summary>
    [SerializeField]
    public GameSessionSO sessionSO;


    //Evento que notifica a el GameManager
    //GameManager se suscribe a este evento
    public static event Action OnPlayerDeath; 


    #region Estadisticas
    [Header("Estadisticas")]
    public float vidaActual; //VidaActual
    public float staminaActual; //StaminaActual
    public float staminaRegenRate = 5f; // Regeneracion de Stamina.
    #endregion

    #region ActualizarHabilidades
    // Esto permite dos cosas:
    // Conocer si el usuario ha desbloqueado una habilidad
    // Validar el uso de ciertas funciones.
    private bool tieneLanza;
    private bool tieneMagia;
    private bool tieneParry;
    private bool tieneLibro;
    #endregion

    [Header("Configuracion de Deteccion")]
    public float radioDeteccion = 5f;
    public LayerMask capaItems;
    public LayerMask capaFragmentos;

    private void Awake()
    {
        // Con esto hacemos que el GameManager pueda tomar la referencia del PlayerController
        if (GameManager.Instance != null)
        {
            bool enGameplay = GameManager.Instance.EstadoJuego == GameState.Gameplay;
            if (enGameplay)
            {
                GameManager.Instance.datosJugador = this;
                GameManager.Instance.ChangeState(GameState.Gameplay);   
            }
        }
    }
    private void Start()
    {
        // Validaciones
        if (sessionSO == null || sessionSO.playerDATOS == null)
        {
            Debug.LogError("Verifique que el scriptableObject del jugador este asignado");
            return;
        }

        // Validacion que nuestro player haya inicializado su inventario.
        if (sessionSO.playerDATOS.Inventario == null)
        {
            Debug.LogError("Inventario Vacio");
            Debug.Log("CreandoInventario");
            //En caso de no tener una instancia generada, fuerza la inicializacion del inventario.
            sessionSO.playerDATOS.Inventario.ObtenerSlotsVacios();
        }

        vidaActual = sessionSO.playerDATOS.Health;

        

        


        //Logs para verificacion de creacion.
        Debug.Log("PlayerController inicializado correctamente");
        Debug.Log($"Slots de inventario disponibles: {sessionSO.playerDATOS.Inventario.ListaItemsIn_ReadOnly.Count}");
        Debug.Log($"Slots vacíos: {sessionSO.playerDATOS.Inventario.ObtenerSlotsVacios()}");
    }

    private void Update()
    {
        //Validación defensiva
        if (sessionSO == null || sessionSO.playerDATOS == null)
        {
            return;
        }
        RecolectarItems();
    }

    private void ActualizarValoresEnControlador()
    {
        vidaActual = sessionSO.playerDATOS.Health;
        // Si la vida es igual o menor a cero eso significa que es una nueva partida.
        if (vidaActual <= 0)
        {
            vidaActual = 100f;
            GuardarVidaEnScriptableObject();
        }
        // ActualizamosHabilidades
        ActualizarHabilidades();
    }
    /// <summary>
    /// Este metodo debemos usarlo cada que aumentemos o disminuyamos la vida en el jugador.
    /// Para tener actualizado siempre el valor de la base de datos.
    /// </summary>
    private void GuardarVidaEnScriptableObject()
    {
        if(vidaActual > 100)
        {
            sessionSO.playerDATOS.Health = 100f;
        }
        if (vidaActual <= 0) sessionSO.playerDATOS.Health = 0;
        else
        {
            sessionSO.playerDATOS.Health = vidaActual;
        }
        
    }


    public void ActualizarHabilidades()
    {
        if (sessionSO == null || sessionSO.playerDATOS == null) return;

        tieneLanza = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.Lanza);
        tieneMagia = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.FragmentoMagia);
        tieneParry = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.FragmentoParry_AtaqueFuerte);
        tieneLibro = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.libroEGVA);
    }

    private void TakeDamage(int damage)
    {
        vidaActual -= damage;
        GuardarVidaEnScriptableObject();
        Debug.Log("El jugador recibió " + damage + " de daño. Vida actual: " + vidaActual);

        if (vidaActual <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnPlayerDeath?.Invoke(); 
    }


    // Dibujar el círculo en el Editor para pruebas
    private void OnDrawGizmos()
    {
        // Chequeo rápido para el color del Gizmo en 2D
        bool detectado = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaItems);

        Gizmos.color = detectado ? Color.green : Color.red;

        // Dibujamos el círculo (usamos la posición del transform)
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
    
    private void RecolectarItems()
    {
        ItemContainer item = GetItemMasCercano();

        if (item != null)
        {

            if (Input.GetKeyDown(KeyCode.Y))
            {
                Debug.Log($"Item recolectado: {item.ItemDATA.Nombre} cantidad : {item.Cantidad}");
                bool exito = sessionSO.playerDATOS.Inventario.AgregarItem(item);

                    if (exito)
                    {
                        Debug.Log($"Total de items en inventario: {sessionSO.playerDATOS.Inventario.ObtenerCantidadTotalItems()}");
                    }
                    else
                    {
                        Debug.LogWarning($"Inventario está LLENO ({sessionSO.playerDATOS.Inventario.ObtenerSlotsVacios()} slots disponibles)");
                    }
             }
            
        }
    }

    private ItemContainer GetItemMasCercano()
    {
        // Detectar todos los colliders en el radio
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radioDeteccion, capaItems);

        ItemContainer itemMasCercano = null;
        float distanciaMinima = Mathf.Infinity; // Empezamos con una distancia infinita

        foreach (Collider2D col in colliders)
        {
            //Verificacion si el objeto tiene ItemContainer
            if (col.TryGetComponent<ItemContainer>(out ItemContainer item))
            {
                //Calcular la distancia entre yo(player) y el objeto
                float distancia = Vector2.Distance(transform.position, col.transform.position);

                // 4. Si esta distancia es menor a la anterior, este es nuestro nuevo "ganador"
                if (distancia < distanciaMinima)
                {
                    distanciaMinima = distancia;
                    itemMasCercano = item;
                }
            }
        }

        return itemMasCercano;
    }

    private void RecolectarFragmentos()
    {
        Collider2D collider = Physics2D.OverlapCircle(transform.position, radioDeteccion,capaFragmentos);

        if (collider != null)
        {
            if(Input.GetKeyDown(KeyCode.Y))
            {
                if (collider.TryGetComponent<FragmentContainer>(out FragmentContainer fragmento))
                {
                    FragmentsScriptable.TipoFragmento tipo = fragmento.fragmento.tipoFragmento;
                    switch (tipo)
                    {
                        case FragmentsScriptable.TipoFragmento.Lanza:
                            sessionSO.playerDATOS.Unlock(TipoHabilidadEnum.Lanza);
                            tieneLanza = true;
                            break;
                        case FragmentsScriptable.TipoFragmento.FragmentoMagia:
                            sessionSO.playerDATOS.Unlock(TipoHabilidadEnum.Lanza);
                            tieneMagia = true;
                            break;
                        case FragmentsScriptable.TipoFragmento.FragmentoParry_AtaqueFuerte:
                            sessionSO.playerDATOS.Unlock(TipoHabilidadEnum.Lanza);
                            tieneParry = true;
                            break;
                        case FragmentsScriptable.TipoFragmento.libroEGVA:
                            sessionSO.playerDATOS.Unlock(TipoHabilidadEnum.Lanza);
                            tieneLibro = true;
                            break;
                    }
                }
            }
        }
    }
}