using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NuevaSesion", menuName = "Sistema/Sesion de Juego")]
public class GameSessionSO : ScriptableObject
{
    [Header("DATOS DEL JUGADOR")]
    public Player playerDATOS;

    


    /// <summary>
    /// Este metodo se ejecuta automaticamente apenas se inicie el juego
    /// Lo que hace es basicamente agregar a la listadeHabilidades cada tipo existente por cada valor del enum tipoHabilidadEnum
    /// y actualiza de haber agregado otra.
    /// </summary>
    private void OnValidate()
    {
        if (playerDATOS == null) return;


        // Generacion de un arreglo por cada tipo de Habilidad en el Enum
        TipoHabilidadEnum[] todosLosTipos = (TipoHabilidadEnum[])System.Enum.GetValues(typeof(TipoHabilidadEnum)); 

        // Aqui estamos validando que | Si la lista de mi player es diferente a la lista de enums necesitaActualizar = true
        // Si esto es true quiere decir que la lista de enums no ha cambiado.
        bool necesitaActualizar = playerDATOS.ListaHabilidades_ReadOnly.Count != todosLosTipos.Length;

        // Okey, pero que pasa si por ejemplo un enum cambia de valor ej: MagicFragment a Hola123 :v
        // Necesitamos poner ese nuevo valor en nuestra listaHabilidades
        if (!necesitaActualizar)//Entonces si necesitaActualizar no es "true" hagamos la siguiente validacion.
        {
            for (int i = 0; i< todosLosTipos.Length; i++) // Recorremos el tamaño total del arreglo de enums
            {
                // si en el indice de iteracion que estoy, el valor de listaHabilidades no cuadro con el del enum
                if (playerDATOS.ListaHabilidades_ReadOnly[i].tipo != todosLosTipos[i]) 
                {//Necesitamos hacer un cambio y con ese cambio que se ejecute, con que solo uno sea diferente ya acomoda los demas
                    necesitaActualizar = true;
                    break;
                }
            }
        }

        //Aqui re corrige en caso de que los enums cambien.
        if (necesitaActualizar)
        {
            // Creamos una lista temporal para no perder lo que ya estaba marcado
            List<NodoHabilidad> listaLimpia = new List<NodoHabilidad>();

            // Recorremos cada valor del Array
            foreach (TipoHabilidadEnum tipoEnum in todosLosTipos)
            {
                // Buscamos si ya existía en la lista de nuestro player.
                var nodoExistente = playerDATOS.ListaHabilidades_ReadOnly.Find(s => s.tipo == tipoEnum);
                // Si el valor del array lo tenemos en nuestra lista.
                if (nodoExistente != null) // Simplemente lo agregamos en nuestra lista sin modificar nada
                {
                    listaLimpia.Add(nodoExistente); // Mantenemos el estado (true/false)
                }
                // Pero si no lo esta agregamos el valor del Enum en falso
                else
                {
                    listaLimpia.Add(new NodoHabilidad { tipo = tipoEnum, unlock = false }); // Nuevo
                }
            }
            //ACTUALIZAMOS ESTA MONDA Y SOBELO :P
            playerDATOS.ListaHabilidades_ReadOnly = listaLimpia;
        }
    }

    [ContextMenu("Resetear Datos")]
    public void Resetear()
    {
        playerDATOS = new Player();
        OnValidate(); // Refresca la lista de skills
        Debug.Log("Datos del jugador reseteados.");
    }
}