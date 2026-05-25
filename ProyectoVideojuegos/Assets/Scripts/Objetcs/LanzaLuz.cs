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
        //_light = GetComponent<Light2D>();
        //if (_light == null)
        //    Debug.LogError("LanzaLuz requiere un componente Light2D en " + gameObject.name);
    }

    void Update()
    {
        // Pulso suave tipo "respiración"
        if (_light == null) return;
        float pulse = Mathf.Sin(Time.time * pulseSpeed);
        _light.intensity = baseIntensity + pulse * pulseAmount;
    }
}
