using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LanzaLuz : MonoBehaviour
{
    public float baseIntensity = 0.8f;
    public float pulseAmount = 0.3f;
    public float pulseSpeed = 2.5f;

    private Light2D _light;

    void Start()
    {
        _light = GetComponent<Light2D>();
    }

    void Update()
    {
        // Pulso suave tipo "respiración"
        float pulse = Mathf.Sin(Time.time * pulseSpeed);
        _light.intensity = baseIntensity + pulse * pulseAmount;
    }
}
