using UnityEngine;
using System;
using System.Collections;

public class Spear : MonoBehaviour
{
    [Header("Configuración")]
    public TipoHabilidadEnum habilidad;
    [SerializeField] private float pausaRecoleccion = 20f; // tiempo para la animación

    public static event Action<TipoHabilidadEnum> OnArmaObtenida;

    private bool jugadorCerca = false;
    private bool yaRecolectando = false; // evita doble ejecución
    private PlayerController player;
    private GameController1 gc1;

    private void Start()
    {
        gc1 = FindFirstObjectByType<GameController1>();
    }

    private void Update()
    {
        if (jugadorCerca && !yaRecolectando && Input.GetKeyDown(KeyCode.Y))
            StartCoroutine(SecuenciaRecoleccion());
    }

    private IEnumerator SecuenciaRecoleccion()
    {
        yaRecolectando = true;


        PlayerController jugadorLocal = player;
        if (jugadorLocal == null)
        {
            yaRecolectando = false;
            yield break;
        }

        // Deshabilitar controles del jugador
        jugadorLocal.HabilitarControles(false);

        // Oculta el aviso "Presiona E"
        gc1?.OcultarAvisoInteraccion();
        gc1.MostrarNotificacion();

        // Aquí puedes hacer la animación del personaje cuando recolecta si tiene
        // player.GetComponent<Animator>().SetTrigger("Recoger");

        // Pausa para que se vea la animación
        yield return new WaitForSeconds(pausaRecoleccion);

        // Desbloquea la habilidad
        jugadorLocal.sessionSO.playerDATOS.Unlock(habilidad);
        jugadorLocal.ActualizarHabilidades();

        // Notifica a GameController1 y otros sistemas
        OnArmaObtenida?.Invoke(habilidad);


        // Reactivar controles
        jugadorLocal.HabilitarControles(true);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        player = collision.GetComponent<PlayerController>();
        jugadorCerca = true;
        gc1?.MostrarAvisoInteraccion("Presiona E para obtener la Lanza");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (yaRecolectando) return; // No limpiar mientras recolectamos

        jugadorCerca = false;
        player = null;
        gc1?.OcultarAvisoInteraccion();
    }
}