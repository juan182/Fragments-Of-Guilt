using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float vidaMaxima = 100f;
    private float vidaActual;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    //[SerializeField] private AudioClip sfxDaño;
    [SerializeField] private AudioClip sfxMuerte;

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

       // if (audioSource != null && sfxDaño != null)
         //   audioSource.PlayOneShot(sfxDaño);

        Animator anim = GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("daño");

        if (vidaActual <= 0)
            Morir();
    }

    private void Morir()
    {
        if (audioSource != null && sfxMuerte != null)
            audioSource.PlayOneShot(sfxMuerte);

        Animator anim = GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("muerte");

        Destroy(gameObject, 1f);
    }
}