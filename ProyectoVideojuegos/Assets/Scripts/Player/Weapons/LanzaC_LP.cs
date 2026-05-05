using UnityEngine;

public class LanzaC_LP
{
    private int daño;
    private int nivel;
    private int experienciaAcumulada;

    public LanzaC_LP()
    {
        daño = 10;
        nivel = 1;
        experienciaAcumulada = 0;
    }
    public LanzaC_LP(int daño, int nivel, int experiencia)
    {
        this.daño = daño;
        this.nivel = nivel;
        this.experienciaAcumulada = experiencia;
    }

    public void calcularNivel()
    {
        if (experienciaAcumulada == 100)
        {
            daño += 10;
            nivel++;
        }

    }

    public int Daño { get; set; }
    public int Nivel { get; set; }
    public int ExperienciaAcumulada { get; set; }


}
