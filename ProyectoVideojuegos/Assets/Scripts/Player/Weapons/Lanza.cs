using UnityEngine;

public class Lanza : MonoBehaviour
{
    public LanzaC_LP lanzaAsignada = null;
    public GameObject colisionadorDaño;
    public int dañoLanza;

    private void Start()
    {

        bool obtenido = GameManager.Instance.datosJugador.sessionSO.playerDATOS.GetLanza != null;
        if (obtenido)
        {
            lanzaAsignada = GameManager.Instance.datosJugador.sessionSO.playerDATOS.GetLanza;
            configurarLanza();
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
}
