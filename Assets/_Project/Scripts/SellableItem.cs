using UnityEngine;

public class SellableItem : MonoBehaviour
{
    // Hardcoded for now, will be set by crafting system later (line 8)
    public string itemName = "Sword";
    public float basePrice = 50f;

    // These properties are useless rn, but will be used later to calculate how much the item can sell for
    public float sharpness;
    public string material;
    public float buildQuality;

    public float GetSellPrice()
    {
        // For now just return base price, will change later (line 8)
        // Later we can do: return basePrice * sharpnessMultiplier * materialMultiplier etc.
        return basePrice * GetComponent<TemperatureScript>().smithingMaterial.priceMultiplier;
    }
}