using UnityEngine;

public class TemperatureScript : MonoBehaviour
{
    float temp = 60;
    readonly float minTemp = 60;
    readonly float maxTemp = 500;
    float tempLostPerSecond;

    [SerializeField] Renderer[] targetRenderers;
    Color coldColor = Color.white; // gray-blue
    Color hotColor = Color.orange;    // red
    // [SerializeField] Color maxHotColor = Color.white;  
    [SerializeField] Color coldEmissionColor = Color.black; 
    [SerializeField] Color hotEmissionColor = Color.red;

    public SmithingMaterial smithingMaterial;

    void Awake()
    {
        tempLostPerSecond = smithingMaterial.tempLostPerSecond;
        temp = minTemp;
    }

    void Update()
    {
        // Decrease temp
        temp -= tempLostPerSecond * Time.deltaTime;
        temp = Mathf.Clamp(temp, minTemp, maxTemp);
        // Debug.Log("Temp: " + temp);
        UpdateColor();
    }

    void UpdateColor()
    {
        // Debug.Log("Updating Color");
        // Normalize temp (0 → 1)
        float t = Mathf.InverseLerp(minTemp, maxTemp, temp);

        // First blend: cold → red
        Color midColor = Color.Lerp(coldColor, hotColor, t);
        Color midEmissionColor = Color.Lerp(coldEmissionColor, hotEmissionColor, t);

        // // Optional: push toward white at very high temps
        // if (t > 0.8f)
        // {
        //     float whiteBlend = (t - 0.8f) / 0.2f;
        //     midColor = Color.Lerp(midColor, maxHotColor, whiteBlend);
        // }
        
        foreach (Renderer targetRenderer in targetRenderers)
        {
            targetRenderer.material.color = midColor;
            targetRenderer.material.SetColor("_EmissionColor", midEmissionColor);
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
        return (temp - smithingMaterial.minWorkingTemp) / (smithingMaterial.maxWorkingTemp - smithingMaterial.minWorkingTemp);
    }
}
