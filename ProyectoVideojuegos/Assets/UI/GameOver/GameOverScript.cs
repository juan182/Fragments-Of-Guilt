using UnityEngine;
using UnityEngine.UIElements;

public class GameOverScript : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement contenedorPrincipal;

    private Button btnReintentar;
    private Button btnMenuPrincipal;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        contenedorPrincipal = root.Q<VisualElement>("PantallaMuerteRoot");

        btnReintentar = root.Q<Button>("BtnReintentar");
        btnMenuPrincipal = root.Q<Button>("BtnMenuPrincipal");

        // CAMBIO CRÍTICO: Registramos 'ClickEvent' en lugar de 'PointerDownEvent'
        // Este evento ignora por completo si el Time.timeScale está en 0
        if (btnReintentar != null)
            btnReintentar.RegisterCallback<ClickEvent>(OnReintentarPressed);

        if (btnMenuPrincipal != null)
            btnMenuPrincipal.RegisterCallback<ClickEvent>(OnMenuPrincipalPressed);

        DesactivarPantallaImedatadamente();
    }

    // Cambiamos el parámetro aquí también a ClickEvent
    private void OnReintentarPressed(ClickEvent evt)
    {
        Debug.Log("Click detectado en Reintentar con TimeScale 0.");
        if (GameManager.Instance != null && GameManager.Instance.sceneManager != null)
        {
            GameManager.Instance.sceneManager.ReiniciarNivel();
        }
    }

    // Cambiamos el parámetro aquí también a ClickEvent
    private void OnMenuPrincipalPressed(ClickEvent evt)
    {
        Debug.Log("Click detectado en Menú Principal.");
        if (GameManager.Instance != null && GameManager.Instance.sceneManager != null)
        {
            GameManager.Instance.sceneManager.IrAMenu("MenuPrincipal");
        }
    }

    public void ActivarPantallaConFade()
    {
        if (contenedorPrincipal == null) return;
        contenedorPrincipal.style.display = DisplayStyle.Flex;

        contenedorPrincipal.schedule.Execute(() => {
            contenedorPrincipal.AddToClassList("visible");
        }).StartingIn(10);
    }

    public void DesactivarPantallaImedatadamente()
    {
        if (contenedorPrincipal == null) return;
        contenedorPrincipal.RemoveFromClassList("visible");
        contenedorPrincipal.style.display = DisplayStyle.None;
    }
}