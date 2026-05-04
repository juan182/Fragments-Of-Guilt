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
    public GameSessionSO sessionSO;
    public GameObject lanza;

    //Evento que notifica a el GameManager
    //GameManager se suscribe a este evento
    public static event Action OnPlayerDeath; 


    [Header("Estadisticas")]
    public int vidaActual; 
    public int staminaActual; 
    public int staminaRegenRate = 5; //Esto aun no se utiliza.

    [Header("Habilidades")]
    public bool tieneLanza;
    public bool tieneMagia;
    public bool tieneParry;
    public bool tieneLibro;
  

    [Header("Configuracion de Deteccion de Fragmentos e Items")]
    public float radioDeteccion = 2f;
    public LayerMask capaItems;
    public LayerMask capaFragmentos;

    //Pongo esta variable para poder deshabilitar el movimiento por un momento al capturar la lanza
    private bool controlesHabilitados = true;
    private MovementController movementController; //Perfecto aqui te me adelantaste con esto. by: Miguel

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

        movementController = GetComponent<MovementController>();

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
        ActualizarValoresEnControlador();
        
        ///Esto no es necesario puesto que quien le dice que tiene armas o no es la clase base Player.
        ///el player controller atraves de Actualizar Habilidades pregunta a la lista del player y si esta simplemente activa la lanza que ya tiene el personaje 
        ///Asignada, asi como solo es un nivel es mas facil el manejo entre scenas, no es lo mas optimo, pero es mas breve por ahora.
       
        //if (GameManager.Instance.tieneArma==true)
        //{
        //    sessionSO.playerDATOS.Unlock(TipoHabilidadEnum.Lanza);
        //    ActualizarHabilidades();
        //}
    }

    private void Update()
    {
        //Validación defensiva
        if (sessionSO == null || sessionSO.playerDATOS == null)
        {
            return;
        }
        RecolectarItems();
        RecolectarFragmentos();
    }


    //Cada que por ejemplo reiniciemos una scena, este script playerController se reinicia
    //Como este script se reinicia se ejecuta este metodo.
    //Que rellena la vida del enemigo aqui como en la base de datos del personaje
    private void ActualizarValoresEnControlador()
    {
        vidaActual = sessionSO.playerDATOS.VidaJugador;
        // Si la vida es igual o menor a cero eso significa que es una nueva partida.
        if (vidaActual <= 0)
        {
            vidaActual = 100;
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
            sessionSO.playerDATOS.VidaJugador = 100;
        }
        if (vidaActual <= 0) sessionSO.playerDATOS.VidaJugador = 0;
        else
        {
            sessionSO.playerDATOS.VidaJugador = vidaActual;
        }
        
    }

    public void ActualizarHabilidades()
    {
        if (sessionSO == null || sessionSO.playerDATOS == null) return;

        tieneLanza = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.Lanza);
        if (tieneLanza == true) lanza.gameObject.SetActive(true);
        tieneMagia = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.FragmentoMagia);
        tieneParry = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.FragmentoParry_AtaqueFuerte);
        tieneLibro = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.libroEGVA);
    }

    public void TakeDamage(int damage)
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
        HabilitarControles(false);
        OnPlayerDeath?.Invoke(); 
    }


    // Dibujar el círculo en el Editor para pruebas
    private void OnDrawGizmos()
    {
        Vector2 posicionAjuste = (Vector2)transform.position + new Vector2(0f, 1.2f);
        // Chequeo rápido para el color del Gizmo en 2D
        bool detectado = Physics2D.OverlapCircle(posicionAjuste, radioDeteccion, capaItems);

        Gizmos.color = detectado ? Color.green : Color.red;

        // Dibujamos el círculo (usamos la posición del transform)
        Gizmos.DrawWireSphere(posicionAjuste, radioDeteccion);
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
        Vector2 posicionAjuste = (Vector2)transform.position + new Vector2(0f, 1.2f);
        // Detectar todos los colliders en el radio
        Collider2D[] colliders = Physics2D.OverlapCircleAll(posicionAjuste, radioDeteccion, capaItems);

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
        Vector2 posicionAjuste = (Vector2)transform.position + new Vector2(0f, 1.2f);
        Collider2D collider = Physics2D.OverlapCircle(posicionAjuste, radioDeteccion, capaFragmentos);

        if (collider != null)
        {
            string nombreCapaItem = LayerMask.LayerToName(collider.gameObject.layer);
            if (Input.GetKeyDown(KeyCode.R))
            {
                switch (nombreCapaItem)
                {
                    case "LanzaEstatica":
                        sessionSO.playerDATOS.Unlock(TipoHabilidadEnum.Lanza);
                        Destroy(collider.gameObject);
                        lanza.SetActive(true);
                        tieneLanza = true;
                        break;
                    case "FragmentoMagia":
                        sessionSO.playerDATOS.Unlock(TipoHabilidadEnum.FragmentoMagia);
                        tieneMagia = true;
                        break;
                    case "FragmentoParry_AtaqueFuerte":
                        sessionSO.playerDATOS.Unlock(TipoHabilidadEnum.FragmentoParry_AtaqueFuerte);
                        tieneParry = true;
                        break;
                    case "libroEGVA":
                        sessionSO.playerDATOS.Unlock(TipoHabilidadEnum.libroEGVA);
                        tieneLibro = true;
                        break;
                }
            }
        }
    }

    //Metodo publico para habilitar o deshabilitar controles
    public void HabilitarControles(bool enabled)
    {
        controlesHabilitados = enabled;

        Debug.Log($"SetControlEnabled({enabled}), movementController = {movementController}");
        if (movementController != null)
        {
            movementController.enabled = enabled;
            Debug.Log($"MovementController.enabled ahora es {movementController.enabled}");
        }
        else
        {
            Debug.LogError("MovementController NO encontrado en el mismo GameObject que PlayerController");
        }

        if (!enabled)
        {
            // frena la velocidad
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;


            }
        }
    }
}