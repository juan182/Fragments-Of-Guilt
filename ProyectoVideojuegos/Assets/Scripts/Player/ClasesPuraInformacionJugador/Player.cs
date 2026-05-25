using System;
using System.Collections.Generic;
using UnityEngine;

public enum TipoHabilidadEnum { Lanza, FragmentoMagia, FragmentoParry_AtaqueFuerte, libroEGVA }

[System.Serializable]
public class NodoHabilidad
{
    public TipoHabilidadEnum tipo; // Identificador único de la habilidad.
    public bool unlock; // Estado actual de desbloqueo (True/False).
}

[System.Serializable]
public class Player
{
    // VARIABLES Y CAMPOS SERIALIZADOS (CAMPOS)
    [SerializeField] private int vidaJugador;// Valor persistente de salud del personaje.
    [SerializeField] private int stamina;// Valor persistente de energía del personaje.
    [SerializeField] private Inventario inventario;// Referencia a la instancia del sistema de inventario.
    [SerializeField] private LanzaC_LP lanzaObjectInfo = null; // Información lógica y de estado del arma Lanza.
    [SerializeField] private List<NodoHabilidad> listaHabilidades = new List<NodoHabilidad>(); // Contenedor de estados de habilidades.

   
    // PROPIEDADES DE ACCESO (GETTERS Y SETTERS)
    public int VidaJugador
    {
        get { return vidaJugador; }
        set { vidaJugador = value; }
    }

    public int Stamina
    {
        get { return stamina; }
        set { stamina = value; }
    }

    public Inventario Inventario
    {
        get
        {
            if (inventario == null) inventario = new Inventario(); // Inicialización diferida (Lazy Initialization) si es nulo.
            return inventario;
        }
        set { inventario = value; }
    }

    public List<NodoHabilidad> ListaHabilidades_ReadOnly
    {
        get { return listaHabilidades; }
        set { listaHabilidades = value; }
    }

    public LanzaC_LP GetLanza
    {
        get { return lanzaObjectInfo; }
        set { lanzaObjectInfo = value; }
    }

    // MÉTODOS DE LÓGICA Y GESTIÓN DE HABILIDADES
    public void Unlock(TipoHabilidadEnum habilidad)
    {
        var node = listaHabilidades.Find(s => s.tipo == habilidad);

        if (habilidad == TipoHabilidadEnum.Lanza)
        {
            lanzaObjectInfo = new LanzaC_LP();// Instancia los datos lógicos específicos de la lanza al obtenerla.
        }

        if (node != null) node.unlock = true; // Modifica el flag en la lista persistente a desbloqueado.
    }

    public bool IsUnlocked(TipoHabilidadEnum habilidad)
    {
        return listaHabilidades.Exists(s => s.tipo == habilidad && s.unlock); // Verifica existencia y estado de desbloqueo en un paso.
    }
}