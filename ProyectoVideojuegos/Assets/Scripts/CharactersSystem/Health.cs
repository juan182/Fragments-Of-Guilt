using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float vidaMaxima = 100f;
    private float vidaActual;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        vidaActual = vidaMaxima;
    }

    public float VidaActual { get => vidaActual; }
    public float VidaMaxima { get => vidaMaxima; }

    public void Daño(float cantidad)
    {
        if (vidaActual <= 0) return;

        vidaActual -= cantidad;
        Debug.Log($"{name} recibio {cantidad} de daño, vida restante {vidaActual}");

        //Trigger de daño
        Animator anim=GetComponent<Animator>();
        if(anim != null)
        {
            anim.SetTrigger("daño");
        }


        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        Animator anim=GetComponent<Animator>();
        if(anim != null)
        {
            anim.SetTrigger("muerte"); //Funciona para enemigos y jugador
        }
        Destroy(gameObject, 1f); //1f es retraso para la ejecucion de la animacion antes de destruirse
    }
}
