using UnityEngine;

public class DisparoLanzaMagica : MonoBehaviour
{
    [Header("Referencias del Proyectil")]
    [SerializeField] private GameObject prefabBalaMagica;
    [SerializeField] private Transform puntoDisparo;

    public void DispararProyectil()
    {
        Debug.Log("DispararProyectil llamado");

        if (prefabBalaMagica == null)
        {
            Debug.LogError("prefabBalaMagica no asignado en " + gameObject.name);
            return;
        }

        if (puntoDisparo == null)
        {
            Debug.LogError("puntoDisparo no asignado en " + gameObject.name);
            return;
        }

        Debug.Log("Instanciando proyectil en " + puntoDisparo.position);

        GameObject proyectil = Instantiate(prefabBalaMagica, puntoDisparo.position, Quaternion.identity);

        ProyectilMagico comp = proyectil.GetComponent<ProyectilMagico>();
        if (comp != null)
        {
            int direccion = transform.root.localScale.x > 0 ? 1 : -1;
            Debug.Log("Inicializando proyectil direccion: " + direccion);
            comp.Inicializar(direccion, 20f);
        }
        else
        {
            Debug.LogError("El prefab no tiene ProyectilMagico.cs");
        }
    }
}