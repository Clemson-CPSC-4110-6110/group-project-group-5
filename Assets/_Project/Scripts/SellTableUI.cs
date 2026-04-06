using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellTableUI : MonoBehaviour
{
    [Header("Table")]
    public SellTable sellTable;

    [Header("Panel")]
    public GameObject uiPanel;

    [Header("Text")]
    public TMP_Text itemNameText;
    public TMP_Text priceText;

    [Header("Debug Slider")]
    public Slider debugPriceSlider;
    public float sliderMin = 0f;
    public float sliderMax = 500f;

    [Header("Button")]
    public Button sellButton;

    private SellableItem currentItem;

    void Start()
    {
        uiPanel.SetActive(false);

        debugPriceSlider.minValue = sliderMin;
        debugPriceSlider.maxValue = sliderMax;
        debugPriceSlider.onValueChanged.AddListener(OnSliderChanged);

        sellButton.onClick.AddListener(OnSellButtonPressed);
    }

    public void ShowItem(SellableItem item)
    {
        currentItem = item;
        uiPanel.SetActive(true);
        itemNameText.text = item.itemName;
        debugPriceSlider.value = item.GetSellPrice();
        priceText.text = $"${debugPriceSlider.value:F0}";
    }

    public void HideUI()
    {
        currentItem = null;
        uiPanel.SetActive(false);
    }

    void OnSliderChanged(float value)
    {
        priceText.text = $"${value:F0}";
    }

    public float GetCurrentPrice()
    {
        return debugPriceSlider.value;
    }

    void OnSellButtonPressed()
    {
        if (sellTable != null)
            sellTable.SellCurrentItem();
        else
            Debug.LogWarning("[SellTableUI] SellTable reference is missing!");
    }
}