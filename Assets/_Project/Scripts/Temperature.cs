using System;
using UnityEngine;

public class TemperatureScript : MonoBehaviour
{
    float temp = 60;
    readonly float minTemp = 60;
    readonly float maxTemp = 500;
    float tempLostPerSecond;

    [SerializeField] Renderer[] targetRenderers;
    Color coldColor = Color.white;
    Color hotColor = new(255f/255f, 210f/255f, 0f/255f);
    Color coldEmissionColor = Color.black; 
    Color hotEmissionColor = new(255f/255f,80f/255f,50f/255f);

    public SmithingMaterial smithingMaterial;

    void Awake()
    {
        tempLostPerSecond = smithingMaterial.tempLostPerSecond;
        temp = minTemp;
    }

    void Update()
    {
        temp -= tempLostPerSecond * Time.deltaTime;
        temp = Mathf.Clamp(temp, minTemp, maxTemp);
        UpdateColor();
    }

    void UpdateColor()
    {
        float t = Mathf.InverseLerp(minTemp, maxTemp, temp);

        // First blend: cold → red
        Color color = Color.Lerp(coldColor, hotColor, t);
        Color emissionColor = Color.Lerp(coldEmissionColor, hotEmissionColor, t);
        // Debug.Log("Color: " + color);
        // Debug.Log("Emission Color: " + emissionColor);
        float emissionIntensity = Mathf.Lerp(0f, 4f, t); // tweak 8 → higher for more glow
        emissionColor *= emissionIntensity;

        foreach (Renderer targetRenderer in targetRenderers)
        {
            targetRenderer.material.color = color;
            targetRenderer.material.SetColor("_EmissionColor", emissionColor);
        }
    }

    public void AddTemp(float amountAdded)
    {
        temp += amountAdded;
    }
    public void SetTemp(float newTemp)
    {
        temp = newTemp;
    }
    public float GetTemp()
    {
        return temp;
    }
    public float GetPercentMaxTemp()
    {
        return Math.Max(0, (temp - smithingMaterial.minWorkingTemp) / (smithingMaterial.maxWorkingTemp - smithingMaterial.minWorkingTemp));
    }
}
