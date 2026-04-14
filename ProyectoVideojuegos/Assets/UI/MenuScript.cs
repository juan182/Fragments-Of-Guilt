using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuScript : MonoBehaviour
{
    private UIDocument menuDocument;
    VisualElement root;

    Button play;
    Button menu;
    Button salir;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        menuDocument = GetComponent<UIDocument>();
        root = menuDocument.rootVisualElement;
        ConfigurarBotones();
    }

    private void ConfigurarBotones()
    {
        play = root.Q<Button>("Play");
        play.RegisterCallback<PointerDownEvent>(Jugar, TrickleDown.TrickleDown);
    }

    private void Jugar(PointerDownEvent evt)
    {
        Debug.Log("Hola hola hola");
        GameManager.Instance.sceneManager.CargarNivelesDeJuego("Nivel1");
    }
}
