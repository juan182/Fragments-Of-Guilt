using System;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class PlayerController : MonoBehaviour
{
    public static event Action OnPlayerDeath;
    public event Action<bool, bool, bool, bool> OnHabilidadesChanged;
    public event Action<int, int> OnVidaChanged;
    public event Action<int, int> OnStaminaChanged;

    private const int VIDA_MAXIMA = 100;
    private const int STAMINA_MAXIMA = 100;

    [Header("Bases de Datos y Componentes")]
    public GameSessionSO sessionSO;
    public GameObject lanza;
    private MovementController movementController;

    [Header("Estadísticas Locales (Encapsuladas)")]
    [SerializeField] private int _vidaActual;
    [SerializeField] private int _staminaActual;
    public int staminaRegenRate = 5;

    public int VidaJugador
    {
        get => _vidaActual;
        set
        {
            _vidaActual = Mathf.Clamp(value, 0, VIDA_MAXIMA);
            GuardarVidaEnScriptableObject();
            if (_vidaActual <= 0) Die();
        }
    }

    public int Stamina
    {
        get => _staminaActual;
        set
        {
            _staminaActual = Mathf.Clamp(value, 0, STAMINA_MAXIMA);
            GuardarStaminaEnScriptableObject();
        }
    }

    [Header("Estados de Desbloqueo (Habilidades)")]
    public bool tieneLanza;
    public bool tieneMagia;
    public bool tieneParry;
    public bool tieneLibro;

    [Header("Parámetros de Detección")]
    public float radioDeteccion = 2f;
    public LayerMask capaItems;
    public LayerMask capaFragmentos;
    private bool controlesHabilitados = true;

    [Header("Referencias de Interfaz")]
    [SerializeField] private UI_Inventario interfazUI;

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.datosJugador = this;
        }
        movementController = GetComponent<MovementController>();

        if (interfazUI == null) interfazUI = FindFirstObjectByType<UI_Inventario>();
    }

    private void Start()
    {
        if (sessionSO == null || sessionSO.playerDATOS == null)
        {
            Debug.LogError("ScriptableObject del jugador no asignado en PlayerController.");
            return;
        }

        if (sessionSO.playerDATOS.Inventario == null)
        {
            Debug.Log("Inicializando inventario vacío en base de datos.");
            sessionSO.playerDATOS.Inventario.ObtenerSlotsVacios();
        }

        ActualizarValoresEnControlador();
    }

    private void Update()
    {
        if (sessionSO == null || sessionSO.playerDATOS == null) return;

        RecolectarItems();
        RecolectarFragmentos();
        ProcesarCartelDeDeteccion();
    }

    private void ProcesarCartelDeDeteccion()
    {
        if (interfazUI == null) return;

        Vector2 posicionAjuste = (Vector2)transform.position + new Vector2(0f, 1.2f);

        ItemContainer itemCercano = GetItemMasCercano();
        if (itemCercano != null)
        {
            interfazUI.AlternarCartelInteraccion(true, "Presiona [Y] para Recolectar Ítem");
            return;
        }

        Collider2D fragmentoCercano = Physics2D.OverlapCircle(posicionAjuste, radioDeteccion, capaFragmentos);
        if (fragmentoCercano != null)
        {
            string nombreCapa = LayerMask.LayerToName(fragmentoCercano.gameObject.layer);

            // =========================================================================
            // METER AQUÍ: Esta línea te imprimirá en la consola el error real del objeto
            Debug.Log($"[DETECCIÓN] Objeto: {fragmentoCercano.gameObject.name} | Layer real en Unity: {nombreCapa}");
            // =========================================================================

            string mensajeFragmento = "Presiona [R] para Absorber";

            switch (nombreCapa)
            {
                case "LanzaEstatica": mensajeFragmento = "Presiona [R] para reclamar la Lanza Celestial"; break;
                case "FragmentoMagia": mensajeFragmento = "Presiona [R] para absorber Fragmento de Magia"; break;
                case "FragmentoParry_AtaqueFuerte": mensajeFragmento = "Presiona [R] para dominar el Contraataque"; break;
                case "libroEGVA": mensajeFragmento = "Presiona [R] para descifrar el Libro de EGVA"; break;
            }

            interfazUI.AlternarCartelInteraccion(true, mensajeFragmento);
            return;
        }

        interfazUI.AlternarCartelInteraccion(false);
    }

    private void OnDrawGizmos()
    {
        Vector2 posicionAjuste = (Vector2)transform.position + new Vector2(0f, 1.2f);
        bool detectado = Physics2D.OverlapCircle(posicionAjuste, radioDeteccion, capaItems) || Physics2D.OverlapCircle(posicionAjuste, radioDeteccion, capaFragmentos);
        Gizmos.color = detectado ? Color.green : Color.red;
        Gizmos.DrawWireSphere(posicionAjuste, radioDeteccion);
    }

    private void ActualizarValoresEnControlador()
    {
        VidaJugador = sessionSO.playerDATOS.VidaJugador;
        Stamina = sessionSO.playerDATOS.Stamina;

        if (VidaJugador <= 0) VidaJugador = VIDA_MAXIMA;
        if (Stamina <= 0) Stamina = STAMINA_MAXIMA;

        ActualizarHabilidades();
    }

    private void GuardarVidaEnScriptableObject()
    {
        if (sessionSO != null && sessionSO.playerDATOS != null)
        {
            sessionSO.playerDATOS.VidaJugador = _vidaActual;
        }
        OnVidaChanged?.Invoke(_vidaActual, VIDA_MAXIMA);
    }

    private void GuardarStaminaEnScriptableObject()
    {
        if (sessionSO != null && sessionSO.playerDATOS != null)
        {
            sessionSO.playerDATOS.Stamina = _staminaActual;
        }
        OnStaminaChanged?.Invoke(_staminaActual, STAMINA_MAXIMA);
    }

    public void ActualizarHabilidades()
    {
        if (sessionSO == null || sessionSO.playerDATOS == null) return;
        tieneLanza = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.Lanza);
        if (tieneLanza) lanza.SetActive(true);
        tieneMagia = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.FragmentoMagia);
        tieneParry = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.FragmentoParry_AtaqueFuerte);
        tieneLibro = sessionSO.playerDATOS.IsUnlocked(TipoHabilidadEnum.libroEGVA);
        OnHabilidadesChanged?.Invoke(tieneLanza, tieneMagia, tieneParry, tieneLibro);
    }

    public void TakeDamage(int damage)
    {
        VidaJugador -= damage;
    }

    private void Die()
    {
        HabilitarControles(false);
        OnPlayerDeath?.Invoke();
    }

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
        if (collider != null && Input.GetKeyDown(KeyCode.R))
        {
            string nombreCapaItem = LayerMask.LayerToName(collider.gameObject.layer);   
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
                    Destroy(collider.gameObject);
                    tieneMagia = true;
                    break;
                case "FragmentoParry_AtaqueFuerte":
                    sessionSO.playerDATOS.Unlock(TipoHabilidadEnum.FragmentoParry_AtaqueFuerte);
                    Destroy(collider.gameObject);
                    tieneParry = true;
                    break;
                case "libroEGVA":
                    sessionSO.playerDATOS.Unlock(TipoHabilidadEnum.libroEGVA);
                    Destroy(collider.gameObject);
                    tieneLibro = true;
                    break;
            }
            ActualizarHabilidades();
        }
    }

    public void HabilitarControles(bool enabled)
    {
        controlesHabilitados = enabled;
        if (movementController != null) movementController.enabled = enabled;
        if (!enabled)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }
}