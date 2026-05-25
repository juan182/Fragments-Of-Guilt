using System.Collections;
using UnityEngine;
using static GameManager;

public class UI_Manager : MonoBehaviour
{
    public MenuScript ui_Menu;
    public UI_Inventario inventario = null;

    private void Start()
    {

    }

    private void Update()
    {
        //VerificarInventario();
    }

    public void Activar_o_DesactivarEstadisticas()
    {
        ui_Menu.gameObject.SetActive(false);
        if (GameManager.Instance.EstadoJuego != GameState.Gameplay) return;
    }
    
    //public void VerificarInventario()
    //{
    //    if (GameManager.Instance.EstadoJuego == GameState.Gameplay)
    //    {
    //        if (Input.GetKeyDown(KeyCode.Tab))
    //        {
    //            inventario.ActivarUI();
    //        }
    //    } 
    //}
}
