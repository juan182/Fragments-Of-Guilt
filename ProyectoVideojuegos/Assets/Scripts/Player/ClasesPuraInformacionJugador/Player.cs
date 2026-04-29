using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;



public enum TipoHabilidadEnum { Lanza, FragmentoMagia, FragmentoParry_AtaqueFuerte, libroEGVA }

[System.Serializable]
public class NodoHabilidad
{
    public TipoHabilidadEnum tipo;
    public bool unlock;
}


[System.Serializable]
public class Player
{

    // Atributos basicos del player
    //[SerializeField]private string name;
    [SerializeField]private float health;
    [SerializeField]private float stamina; 


    // Atributos de posicion.
    public Vector2 posicionActual;

    // Lista de Habilidades
    // La lista tiene un espacio por cada enum presente en SkillType
    // Maneja dos estado True o False | Si es true podemos "acceder" o simplemente sirve como condicionador si algo(habilidad, ataque) se puede usar o no
    [SerializeField] private List<NodoHabilidad> listaHabilidades = new List<NodoHabilidad>(); 
    [SerializeField] private Inventario inventario;

    

    public void Unlock(TipoHabilidadEnum habilidad) 
    {
        var node = listaHabilidades.Find(s => s.tipo == habilidad);
        if (node != null) node.unlock = true;
    }

    public bool IsUnlocked(TipoHabilidadEnum habilidad)
    {
        return listaHabilidades.Exists(s => s.tipo == habilidad && s.unlock);
    }

    //public string Name
    //{
    //    get { return name; }
    //    set { name = value; }
    //}

    public float Health
    {
        get { return health; }
        set { health = value; }
    }

    public Vector2 PosicionActual
    {
        get { return posicionActual; }
        set { posicionActual = value; }
    }

    public float Stamina
    {
        get { return stamina; }
        set { stamina = value; }
    }
   
    public Inventario Inventario
    {
        get // Cuando alguien intente acceder a los datos de inventario a la clase pura en especifico salta condicion 
            //Debido a mi :v "get"
        {
            if (inventario == null) // si inventario no tiene referencia asociada
            {
                inventario = new Inventario(); // Creamos una instancia, con esto ejeuctamos el constructos, Analizar constructor para revisar comportamiento.
            }
            return inventario;
        }
        set { inventario = value; }
    }

    //Metod get y set para la lista de habilidades
    public List<NodoHabilidad> ListaHabilidades_ReadOnly
    {
        get { return listaHabilidades; }
        set { listaHabilidades = value; }
    }
}