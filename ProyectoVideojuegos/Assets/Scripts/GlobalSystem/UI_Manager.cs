using System.Collections;
using UnityEngine;
using static GameManager;

public class UI_Manager : MonoBehaviour
{
    public MenuScript ui_Menu;
    public UI_Inventario inventario;
    public UI_InGame ui_Estadisticas;

    private void Start()
    {
        ui_Estadisticas.root.style.opacity = 0;
    }

    private void Update()
    {
        VerificarInventario();
    }

    public void Activar_o_DesactivarEstadisticas()
    {
        ui_Menu.gameObject.SetActive(false);
        if (GameManager.Instance.EstadoJuego != GameState.Gameplay) return;
        ui_Estadisticas.root.style.opacity = 1;
    }
    
    public void VerificarInventario()
    {
        if (GameManager.Instance.EstadoJuego == GameState.Gameplay)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                inventario.ActivarUI();
            }
        } 
    }
}
