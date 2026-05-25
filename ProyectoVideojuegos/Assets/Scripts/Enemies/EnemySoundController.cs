using UnityEngine;

public class EnemySoundController : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip vuelo;
    [SerializeField] AudioClip ataque; //
    [SerializeField] AudioClip detectaJugador; //
    [SerializeField] AudioClip muerte; //
    
    public void PlayDetectaJugador()
    {
        audioSource.PlayOneShot(detectaJugador);
    }
    public void PlayAtaque()
    {
        audioSource.PlayOneShot(ataque);
    }
    public void PlayMuerte()
    {
        audioSource.PlayOneShot(muerte);
    }
}
