using UnityEngine;

public class SellTable : MonoBehaviour
{
    public SellTableUI tableUI;
    private SellableItem currentItem;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sellSound;
    public AudioClip itemDetectedSound;

    void OnTriggerEnter(Collider other)
    {
        other.TryGetComponent<SellableItem>(out var item);
        if (item == null)
        {
            item = other.GetComponentInParent<SellableItem>();
        }
        if (item)
        {
            currentItem = item;
            tableUI.ShowItem(item);

            if (audioSource != null && itemDetectedSound != null)
                audioSource.PlayOneShot(itemDetectedSound);

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

        if (audioSource != null && sellSound != null)
            audioSource.PlayOneShot(sellSound);

        if (audioSource != null && sellSound != null)
        {
            Debug.Log("[SellTable] Playing sell sound.");
            audioSource.PlayOneShot(sellSound);
        }
        else
        {
            Debug.LogWarning($"[SellTable] Audio missing � source: {audioSource}, clip: {sellSound}");
        }

        Destroy(currentItem.gameObject);
        tableUI.HideUI();


    }
}