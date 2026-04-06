using UnityEngine;
using TMPro;

public class QuotaManager : MonoBehaviour
{
    public static QuotaManager Instance;

    [Header("Quota Settings")]
    public float baseQuotaPerItem = 35f;
    public float quotaVariance = 0.15f;
    public float quotaScalingPerCycle = 1.25f;

    [Header("Item Count Settings")]
    public int minItemsPerDay = 2;
    public int maxItemsPerDay = 4;

    [Header("UI")]
    public TMP_Text moneyText;
    public TMP_Text quotaText;
    public TMP_Text daysText;
    public TMP_Text itemsRemainingText;

    public float CurrentMoney { get; private set; } = 0f;
    public int ItemsRequiredToday { get; private set; }
    public int ItemsSoldToday { get; private set; }

    float quotaTarget;
    int currentDay = 1;
    int cycleCount = 0;
    bool gameOver = false;

    // Day flavor text — add more as you like
    string[] daySubtitles = new string[]
    {
        "Time to get to work...",
        "The forge awaits.",
        "Another day, another blade.",
        "Make it count.",
        "The customers are waiting."
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Show Day 1 intro then begin
        TransitionManager.Instance.ShowDayTransition(
            currentDay,
            daySubtitles[Random.Range(0, daySubtitles.Length)],
            2.5f
        );
    }

    // Called by TransitionManager when fade is complete
    public void OnTransitionComplete()
    {
        if (!gameOver)
            StartNewDay();
    }

    void StartNewDay()
    {
        ItemsSoldToday = 0;
        ItemsRequiredToday = Random.Range(minItemsPerDay, maxItemsPerDay + 1);

        float cycleMultiplier = Mathf.Pow(quotaScalingPerCycle, cycleCount);
        float rawQuota = baseQuotaPerItem * ItemsRequiredToday * cycleMultiplier;
        float variance = Random.Range(-quotaVariance, quotaVariance);
        quotaTarget = Mathf.Round(rawQuota * (1f + variance));

        Debug.Log($"[QuotaManager] Day {currentDay} | Items: {ItemsRequiredToday} | Quota: ${quotaTarget}");
        UpdateUI();
    }

    public void AddMoney(float amount)
    {
        if (gameOver) return;

        CurrentMoney += amount;
        ItemsSoldToday++;

        Debug.Log($"[QuotaManager] +${amount} | {ItemsSoldToday}/{ItemsRequiredToday} sold | Total: ${CurrentMoney}");
        UpdateUI();

        if (ItemsSoldToday >= ItemsRequiredToday)
            EvaluateDay();
    }

    void EvaluateDay()
    {
        if (CurrentMoney >= quotaTarget)
        {
            currentDay++;
            cycleCount++;
            CurrentMoney = 0f;

            // Show day transition before starting next day
            TransitionManager.Instance.ShowDayTransition(
                currentDay,
                daySubtitles[Random.Range(0, daySubtitles.Length)],
                2.5f
            );
        }
        else
        {
            float shortfall = quotaTarget - CurrentMoney;
            gameOver = true;
            TransitionManager.Instance.ShowLoseScreen(shortfall);
        }
    }

    void UpdateUI()
    {
        if (moneyText) moneyText.text = $"${CurrentMoney:F0}";
        if (quotaText) quotaText.text = $"Quota: ${quotaTarget:F0}";
        if (daysText) daysText.text = $"Day {currentDay}";
        if (itemsRemainingText)
            itemsRemainingText.text = $"Items to sell: {ItemsRequiredToday - ItemsSoldToday}";
    }
}