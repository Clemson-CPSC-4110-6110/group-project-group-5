using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BuyScript : MonoBehaviour
{
    [SerializeField] TMP_Text textObj;
    [SerializeField] XRBaseInteractable xRInteractable;
    [SerializeField] float basePrice = 100;
    GameObject itemBeingSold;

    void Awake()
    {
        DisableTextVisibility();
        xRInteractable.hoverEntered.AddListener((args) => EnableTextVisibility());
        xRInteractable.hoverExited.AddListener((args) => DisableTextVisibility());
    }

    void DisableTextVisibility()
    {
        Color color = textObj.color;
        color.a = 0;
        textObj.color = color;
    }

    void EnableTextVisibility()
    {
        Color color = textObj.color;
        color.a = 100;
        textObj.color = color;
    }

    public void SetItemBeingSold(GameObject newObject)
    {
        itemBeingSold = newObject;
        newObject.TryGetComponent(out IngotToSword ingotScript);
        float price = basePrice;
        if (ingotScript) 
        { 
            price *= ingotScript.smithingMaterial.priceMultiplier; 
            price *= newObject.transform.localScale.x * newObject.transform.localScale.y * newObject.transform.localScale.z;
        }
        textObj.text = $"Buy for <color=yellow>{(int)price}</color> gold";
        itemBeingSold.GetComponent<XRGrabInteractable>().enabled = false;
    }
    public void PurchaseItemBeingSold()
    {
        if (!itemBeingSold) return;
        itemBeingSold.GetComponent<XRGrabInteractable>().enabled = true;
        itemBeingSold = null;
    }
    public void DeleteItemBeingSold()
    {
        if (!itemBeingSold) return;
        Destroy(itemBeingSold);
        itemBeingSold = null;
    }
}
