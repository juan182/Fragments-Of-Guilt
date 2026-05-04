using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_InGame : MonoBehaviour
{
    public GameSessionSO sesion;
    private UIDocument menu;
    

    ProgressBar vida_bar;
    ProgressBar stamina_bar;
    public VisualElement root;

    private void Awake()
    {
        menu = GetComponent<UIDocument>();
        root = menu.rootVisualElement;
        ConfigurarBarras();
        
    }

    private void ConfigurarBarras()
    {
        vida_bar = root.Q<ProgressBar>("Vida_bar");
        stamina_bar = root.Q<ProgressBar>("Stamina_bar");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.SetActive(true);  
        ActualizarVidaEnUI();
    }

    public void ActualizarVidaEnUI()
    {
        vida_bar.lowValue = sesion.playerDATOS.VidaJugador;
        stamina_bar.lowValue= sesion.playerDATOS.Stamina;
    }

    private void Update()
    {
        ActualizarVidaEnUI();
    }
}
