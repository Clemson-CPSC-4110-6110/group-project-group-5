using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("Sphere")]
    public Renderer sphereRenderer;
    public float fadeSpeed = 2f;

    [Header("Canvas Content")]
    public GameObject transitionCanvas;
    public TMP_Text dayText;
    public TMP_Text subtitleText;
    public GameObject restartButton;

    [Header("Follow Target")]
    public Transform cameraTarget;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip dayTransitionSound;
    public AudioClip loseSound;

    Material sphereMat;
    bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Start fully transparent and hidden
        sphereMat = sphereRenderer.material;
        SetAlpha(0f);
        transitionCanvas.SetActive(false);
        restartButton.SetActive(false);
    }

    void Update()
    {
        // Sphere always follows player head
        if (cameraTarget != null)
            transform.position = cameraTarget.position;
    }

    // Called by QuotaManager between days
    public void ShowDayTransition(int day, string subtitle, float holdSeconds)
    {
        if (!isTransitioning)
            StartCoroutine(DayTransitionRoutine(day, subtitle, holdSeconds));
    }

    // Called by QuotaManager on lose
    public void ShowLoseScreen(float shortfall)
    {
        StartCoroutine(LoseScreenRoutine(shortfall));
    }

    IEnumerator DayTransitionRoutine(int day, string subtitle, float holdSeconds)
    {
        isTransitioning = true;

        dayText.text = $"Day {day}";
        subtitleText.text = subtitle;
        restartButton.SetActive(false);
        transitionCanvas.SetActive(true);

        if (audioSource != null && dayTransitionSound != null)
            audioSource.PlayOneShot(dayTransitionSound);

        yield return StartCoroutine(FadeSphere(0f, 1f));
        yield return new WaitForSeconds(holdSeconds);
        yield return StartCoroutine(FadeSphere(1f, 0f));

        transitionCanvas.SetActive(false);
        isTransitioning = false;

        QuotaManager.Instance.OnTransitionComplete();
    }

    IEnumerator LoseScreenRoutine(float shortfall)
    {
        isTransitioning = true;

        dayText.text = "You've Been Fired.";
        subtitleText.text = $"You were ${shortfall:F0} short of your quota.";
        restartButton.SetActive(true);
        transitionCanvas.SetActive(true);

        if (audioSource != null && loseSound != null)
            audioSource.PlayOneShot(loseSound);

        yield return StartCoroutine(FadeSphere(0f, 1f));
    }

    IEnumerator FadeSphere(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * fadeSpeed;
            SetAlpha(Mathf.Lerp(from, to, elapsed));
            yield return null;
        }
        // SetAlpha(to);
        SetAlpha(0);
    }

    void SetAlpha(float alpha)
    {
        Color c = sphereMat.color;
        c.a = alpha;
        sphereMat.color = c;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}