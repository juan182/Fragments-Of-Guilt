using UnityEngine;

public class EnemySoundController : MonoBehaviour
{
    [Header("Uga")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip vuelo;
    [SerializeField] AudioClip ataque; //
    [SerializeField] AudioClip detectaJugador; //

    [Header("Ambos")]
    [SerializeField] AudioClip muerte; //

    [Header("Boss")]
    [SerializeField] AudioClip entrada;
    [SerializeField] AudioClip fase2;
    [SerializeField] AudioClip mordida;
    [SerializeField] AudioClip cabezazo;
    [SerializeField] AudioClip embestida;
    [SerializeField] AudioClip ondaExpansiva;
    [SerializeField] AudioClip paso;

    public void PlayVuelo()
    {
        if (audioSource.isPlaying) return; // si ya suena no lo reinicia
        audioSource.clip = vuelo;
        audioSource.loop = true;
        audioSource.pitch = 0.8f;
        audioSource.volume = 0.3f;
        audioSource.Play();
    }

    public void StopVuelo()
    {
        audioSource.Stop();
    }
    public void PlayAtaque() => audioSource.PlayOneShot(ataque);
    public void PlayDetectaJugador() => audioSource.PlayOneShot(detectaJugador);
    public void PlayMuerte() => audioSource.PlayOneShot(muerte);


    public void PlayEntrada() => audioSource.PlayOneShot(entrada);
    public void PlayFase2() => audioSource.PlayOneShot(fase2);
    public void PlayMordida() => audioSource.PlayOneShot(mordida);
    public void PlayCabezazo() => audioSource.PlayOneShot(cabezazo);
    public void PlayEmbestida() => audioSource.PlayOneShot(embestida);
    public void PlayOndaExpansiva() => audioSource.PlayOneShot(ondaExpansiva);
    public void PlayPaso() => audioSource.PlayOneShot(paso);
}
