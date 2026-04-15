using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private string nombreEscenaDestino;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(CargarConDelay(1f));
        }
    }

    IEnumerator CargarConDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nombreEscenaDestino);
    }
}
