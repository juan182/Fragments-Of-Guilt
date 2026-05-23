using UnityEngine;

public class DetectorOndaFisica : MonoBehaviour
{
    private SueloOndulante scriptPrincipal;

    void Start()
    {
        // Busca el script en el objeto padre
        scriptPrincipal = GetComponentInParent<SueloOndulante>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (scriptPrincipal != null)
        {
            scriptPrincipal.AplicarDañoOEmpuje(collision);
        }
    }
}
