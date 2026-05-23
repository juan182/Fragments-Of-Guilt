using UnityEngine;

public class Lanza : MonoBehaviour
{
    public LanzaC_LP lanzaAsignada = null;
    public GameObject colisionadorDaño;
    public int dañoLanza;

    [Header("Magia")]
    [SerializeField] private GameObject proyectilMagicoPrefab;
    [SerializeField] private Transform puntaLanza;
    [SerializeField] private float cooldownMagia = 0.5f;
    private float timerCooldown = 0f;

    private PlayerController playerController;

    private void Start()
    {

        bool obtenido = GameManager.Instance.datosJugador.sessionSO.playerDATOS.GetLanza != null;
        if (obtenido)
        {
            lanzaAsignada = GameManager.Instance.datosJugador.sessionSO.playerDATOS.GetLanza;
            configurarLanza();
        }
        playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        timerCooldown -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.E) && timerCooldown <= 0f && playerController != null && playerController.tieneMagia)
        {
            LanzarMagia();
            timerCooldown = cooldownMagia;
        }
    }


    private void configurarLanza()
    {
        dañoLanza = lanzaAsignada.Daño;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemigo"))
        {
            //if (collision.collider.TryGetComponent(out EnemigoVida vida))
            //{
            //    vida.TomarDaño(dañoLanza);
            //}
        }
    }
    void Activar_DesactivarColisionadorDeDaño()
    {
        //Aqui es para activar o desactivar el colisionador para cuando se active la animacion
    }

    private void LanzarMagia()
    {
        if (proyectilMagicoPrefab == null || puntaLanza == null)
        {
            Debug.LogWarning("Falta asignar proyectilMagicoPrefab o puntaLanza en el Inspector");
            return;
        }

        int direccion = transform.root.localScale.x > 0 ? 1 : -1;

        GameObject proyectil = Instantiate(proyectilMagicoPrefab, puntaLanza.position, Quaternion.identity);

        ProyectilMagico comp = proyectil.GetComponent<ProyectilMagico>();
        if (comp != null)
            comp.Inicializar(direccion, dañoLanza);
    }
}
