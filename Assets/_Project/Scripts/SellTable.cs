using UnityEngine;

public class SellTable : MonoBehaviour
{
    public SellTableUI tableUI;
    private SellableItem currentItem;

    void OnTriggerEnter(Collider other)
    {
        SellableItem item = other.GetComponent<SellableItem>();
        if (item != null)
        {
            currentItem = item;
            tableUI.ShowItem(item);
            Debug.Log($"[SellTable] Detected item: {item.itemName} at ${item.GetSellPrice()}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        SellableItem item = other.GetComponent<SellableItem>();
        if (item == currentItem)
        {
            currentItem = null;
            tableUI.HideUI();
            Debug.Log("[SellTable] Item removed from table.");
        }
    }

    public void SellCurrentItem()
    {
        if (currentItem == null)
        {
            Debug.LogWarning("[SellTable] No item to sell.");
            return;
        }

        float price = tableUI.GetCurrentPrice();
        QuotaManager.Instance.AddMoney(price);

        Debug.Log($"[SellTable] Sold {currentItem.itemName} for ${price}");
        Destroy(currentItem.gameObject);
        tableUI.HideUI();
    }
}