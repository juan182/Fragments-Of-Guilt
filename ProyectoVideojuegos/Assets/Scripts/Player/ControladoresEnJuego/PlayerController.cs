using System;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class PlayerController : MonoBehaviour
{
    // DELEGADOS Y EVENTOS (ESTÁTICOS Y DE INSTANCIA)
    public static event Action OnPlayerDeath;            // Notifica al GameManager el deceso del jugador.
    public event Action<bool, bool, bool, bool> OnHabilidadesChanged; // Sincroniza las 4 habilidades con la UI.
    public event Action<int, int> OnVidaChanged;        // Actualiza el valor de salud en los medidores.
    public event Action<int, int> OnStaminaChanged;// Actualiza el valor de energía en los medidores.

    // CONFIGURACIÓN CONSTANTE Y PARÁMETROS BASE
    private const int VIDA_MAXIMA = 100;// Límite superior de salud del personaje.
    private const int STAMINA_MAXIMA = 100;// Límite superior de energía del personaje.

    [Header("Bases de Datos y Componentes")]
    public GameSessionSO sessionSO; // ScriptableObject con la persistencia de datos.
    public GameObject lanza;// Referencia del objeto físico de la lanza.
    private MovementController movementController; // Componente encargado de las físicas de movimiento.

    [Header("Estadísticas Locales")]
    public int vidaActual;// Registro de salud en tiempo real.
    public int staminaActual;// Registro de energía en tiempo real.
    public int staminaRegenRate = 5; // Tasa de recuperación de energía (pendiente de uso).

    [Header("Estados de Desbloqueo (Habilidades)")]
    public bool tieneLanza;// Estado de posesión de la Lanza.
    public bool tieneMagia;// Estado de posesión del Fragmento de Magia.
    public bool tieneParry;// Estado de posesión del Fragmento de Parry.
    public bool tieneLibro;// Estado de posesión del Libro EGVA.

    [Header("Parámetros de Detección")]
    public float radioDeteccion = 2f; // Rango del círculo de interacción.
    public LayerMask capaItems; // Filtro de colisión para consumibles u objetos.
    public LayerMask capaFragmentos;// Filtro de colisión para fragmentos de historia/habilidad.
    private bool controlesHabilitados = true;  // Flag de control de inputs del periférico.

    // CICLO DE VIDA DE UNITY (LIFECYCLE)
    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            // Vinculamos de manera segura la instancia del jugador al mánager central
            GameManager.Instance.datosJugador = this;
        }
        movementController = GetComponent<MovementController>();
    }

    private void Start()
    {
        if (sessionSO == null || sessionSO.playerDATOS == null)
        {
            Debug.LogError("ScriptableObject del jugador no asignado en PlayerController.");
            return;
        }

        if (sessionSO.playerDATOS.Inventario == null) // Inicialización forzada del inventario defensivo.
        {
            Debug.Log("Inicializando inventario vacío en base de datos.");
            sessionSO.playerDATOS.Inventario.ObtenerSlotsVacios();
        }

        ActualizarValoresEnControlador(); // Carga la información persistente al arrancar escena.
    }

    private void Update()
    {
        if (sessionSO == null || sessionSO.playerDATOS == null) return;
        RecolectarItems(); // Escucha el input para agarrar consumibles.
        RecolectarFragmentos();// Escucha el input para absorber habilidades.
    }

    private void OnDrawGizmos()
    {
        Vector2 posicionAjuste = (Vector2)transform.position + new Vector2(0f, 1.2f); // Desfase vertical del centro.
        bool detectado = Physics2D.OverlapCircle(posicionAjuste, radioDeteccion, capaItems);
        Gizmos.color = detectado ? Color.green : Color.red;
        Gizmos.DrawWireSphere(posicionAjuste, radioDeteccion); // Dibuja el área de interacción en el Editor.
    }

    // MÉTODOS DE CONTROL DE ESTADO Y PERSISTENCIA
    private void ActualizarValoresEnControlador()
    {
        vidaActual = sessionSO.playerDATOS.VidaJugador;
        staminaActual = sessionSO.playerDATOS.Stamina;

        if (vidaActual <= 0)// Reseteo de salud si es una sesión nueva o corrupta.
        {
            vidaActual = VIDA_MAXIMA;
            GuardarVidaEnScriptableObject();
        }

        if (staminaActual <= 0)// Reseteo de energía en las mismas condiciones.
        {
            staminaActual = STAMINA_MAXIMA;
            GuardarVidaEnScriptableObject();
        }

        ActualizarHabilidades(); // Sincroniza estados visuales de las armas e interfaz.
        OnVidaChanged?.Invoke(vidaActual, VIDA_MAXIMA);
        OnStaminaChanged?.Invoke(staminaActual, STAMINA_MAXIMA);
    }

    private void GuardarVidaEnScriptableObject()
    {
        if (vidaActual > VIDA_MAXIMA) vidaActual = VIDA_MAXIMA;
        sessionSO.playerDATOS.VidaJugador = vidaActual <= 0 ? 0 : vidaActual;

        OnVidaChanged?.Invoke(vidaActual, VIDA_MAXIMA);  // Dispara actualización síncrona a la interfaz.
    }

    public void ActualizarHabilidades()
    {
        if (sessionSO == null || sessionSO.playerDATOS == null) return;

        tieneLanza = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.Lanza);
        if (tieneLanza) lanza.SetActive(true);           // Activa el objeto gráfico de la lanza en el personaje.

        tieneMagia = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.FragmentoMagia);
        tieneParry = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.FragmentoParry_AtaqueFuerte);
        tieneLibro = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.libroEGVA);

        OnHabilidadesChanged?.Invoke(tieneLanza, tieneMagia, tieneParry, tieneLibro);
    }

    // SISTEMA DE COMBATE, DAÑO Y DECESO
    public void TakeDamage(int damage)
    {
        vidaActual -= damage;
        GuardarVidaEnScriptableObject();

        if (vidaActual <= 0) Die();
    }

    private void Die()
    {
        HabilitarControles(false);// Corta las físicas y comandos de entrada del jugador.
        OnPlayerDeath?.Invoke();// Delega la secuencia de muerte al GameManager.
    }

    // SISTEMA DE DETECCIÓN E INTERACCIÓN (ITEMS/FRAGMENTS)
    private void RecolectarItems()
    {
        ItemContainer item = GetItemMasCercano();
        if (item != null && Input.GetKeyDown(KeyCode.Y))
        {
            bool exito = sessionSO.playerDATOS.Inventario.AgregarItem(item);
            if (!exito) Debug.LogWarning("Inventario lleno. No se pudo agregar el ítem.");
        }
    }

    private ItemContainer GetItemMasCercano()
    {
        Vector2 posicionAjuste = (Vector2)transform.position + new Vector2(0f, 1.2f);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(posicionAjuste, radioDeteccion, capaItems);

        ItemContainer itemMasCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (Collider2D col in colliders)
        {
            if (col.TryGetComponent<ItemContainer>(out ItemContainer item))
            {
                float distancia = Vector2.Distance(transform.position, col.transform.position);
                if (distancia < distanciaMinima)// Filtra de forma iterativa el objeto a menor distancia.
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

        if (collider != null && Input.GetKeyDown(KeyCode.R))
        {
            string nombreCapaItem = LayerMask.LayerToName(collider.gameObject.layer);
            switch (nombreCapaItem)// Desbloqueo lógico según la capa del objeto detectado.
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
            ActualizarHabilidades(); //Fuerza el refresco inmediato de los cambios en la UI.
        }
    }

    public void HabilitarControles(bool enabled)
    {
        controlesHabilitados = enabled;
        if (movementController != null)
        {
            movementController.enabled = enabled;// Enciende o apaga el procesamiento del script de movimiento.
        }
        else
        {
            Debug.LogError("MovementController no configurado en este GameObject.");
        }

        if (!enabled)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero; // Anula la inercia del cuerpo rígido al bloquear.
        }
    }
}