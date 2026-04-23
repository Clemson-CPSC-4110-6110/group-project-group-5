using UnityEngine;

[CreateAssetMenu(fileName = "SmithingMaterial", menuName = "Scriptable Objects/SmithingMaterial")]
public class SmithingMaterial : ScriptableObject
{
    public Material material;
    public float density;
    public string materialName;
    public float minWorkingTemp = 60;
    public float maxWorkingTemp = 300;
    public float tempLostPerSecond;
    public float priceMultiplier = 1;
}
