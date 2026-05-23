using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private void Awake() => Instance = this;

    public void Sacudir(float duracion, float fuerza)
    {
        StartCoroutine(ProcesarSacudida(duracion, fuerza));
    }

    private IEnumerator ProcesarSacudida(float duracion, float fuerza)
    {
        Vector3 posicionOriginal = transform.localPosition;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            float x = Random.Range(-1f, 1f) * fuerza;
            float y = Random.Range(-1f, 1f) * fuerza;

            transform.localPosition = new Vector3(posicionOriginal.x + x, posicionOriginal.y + y, posicionOriginal.z);
            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = posicionOriginal;
    }
}
