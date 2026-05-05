using System.Collections;
using TMPro;
using UnityEngine;
using static GameManager;

public class GameController1 : MonoBehaviour
{
    [Header("Referencias de Escena")]
    [SerializeField] private GameObject panelArmaObtenida; // ejemplo de UI

    [Header("Aviso de interacción")]
    [SerializeField] private GameObject avisoInteraccion;
    [SerializeField] private TextMeshProUGUI textoAviso;

    [Header("Notificación temporal")]
    [SerializeField] private GameObject panelNotificacion;
    [SerializeField] private TextMeshProUGUI textoNotificacion;
    [SerializeField] private float duracionNotificacion = 2f;

    private Coroutine coroutineNotificacion;

    private void Start()
    {
        // Con esto hacemos que el GameManager pueda tomar la referencia del PlayerController
        if (GameManager.Instance != null)
        {
            bool enGameplay = GameManager.Instance.EstadoJuego == GameState.Gameplay;
            if (enGameplay)
            {
                GameManager.Instance.gc1 = this;
            }
        }
    }
    //private void OnEnable()
    //{
    //    Spear.OnArmaObtenida += HandleArmaObtenida;
    //}

    //private void OnDisable()
    //{
    //    Spear.OnArmaObtenida -= HandleArmaObtenida;
    //}

    private void HandleArmaObtenida(TipoHabilidadEnum habilidad)
    {
        Debug.Log($"Arma obtenida: {habilidad}");
        // Aquí puedes mostrar UI, activar siguiente fase del nivel, etc.
        //GameManager.Instance.RegistrarArmaObtenida();
    }

    //Presiona E para obtener la lanza UI

    public void MostrarAvisoInteraccion(string mensaje)
    {
        if (avisoInteraccion == null) return;
        textoAviso.text = mensaje;
        avisoInteraccion.SetActive(true);
    }

    public void OcultarAvisoInteraccion()
    {
        if (avisoInteraccion != null)
            avisoInteraccion.SetActive(false);
    }

    // --- Notificación temporal ---

    public void MostrarNotificacion()
    {
        if (panelNotificacion == null) return;

        if (coroutineNotificacion != null)
            StopCoroutine(coroutineNotificacion);

        panelNotificacion.SetActive(true);
        coroutineNotificacion = StartCoroutine(OcultarNotificacion());
    }

    private IEnumerator OcultarNotificacion()
    {
        yield return new WaitForSeconds(duracionNotificacion);
        panelNotificacion.SetActive(false);
    }
}