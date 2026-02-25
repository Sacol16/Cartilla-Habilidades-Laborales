// ===========================
// MiniGameManager1.cs
// - Reinicia SOLO el minijuego (sin recargar escena)
// - Limpia frutas/basuras via spawner.ClearSpawned() (no tags extra)
// ===========================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameManager1 : MonoBehaviour
{
    [Header("Time")]
    [SerializeField] private float gameDuration = 30f;

    [SerializeField] private Slider timeSlider;
    [SerializeField] private Image timeSliderFillImage;
    [SerializeField] private TMP_Text timeText;

    [Header("Time Colors")]
    [SerializeField] private Color fullTimeColor = Color.green;
    [SerializeField] private Color lowTimeColor = Color.red;

    [SerializeField] private bool useGradient = true;
    [SerializeField] private Gradient timeGradient;

    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float feedbackShowSeconds = 1.0f;

    [Header("End Screen")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private Button playAgainButton;

    [Header("Minijuego - referencias para reset")]
    [SerializeField] private FruitSpawner2D spawner;
    [SerializeField] private Transform basket;

    [Tooltip("Scripts a habilitar/deshabilitar al iniciar/terminar (canasta, etc).")]
    [SerializeField] private MonoBehaviour[] toDisableOnEnd;

    private Vector3 basketStartPos;

    private float timeLeft;
    private bool gameRunning;

    private int score = 0;
    private int combo = 0;
    private bool last5sMessageShown = false;

    private Coroutine feedbackRoutine;

    private const string BEST_SCORE_KEY = "MINIGAME_BEST_SCORE";

    private void Awake()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(RestartMiniGame);
        }

        if (endPanel != null)
            endPanel.SetActive(false);

        if (feedbackText != null) feedbackText.text = "";
        if (comboText != null) comboText.text = "";

        if (basket != null)
            basketStartPos = basket.position;

        if (useGradient && (timeGradient == null || timeGradient.colorKeys.Length == 0))
        {
            timeGradient = new Gradient();
            timeGradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.red, 0f),
                    new GradientColorKey(Color.yellow, 0.5f),
                    new GradientColorKey(Color.green, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
        }
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        timeLeft = gameDuration;
        score = 0;
        combo = 0;
        last5sMessageShown = false;

        gameRunning = true;

        if (endPanel != null) endPanel.SetActive(false);
        if (feedbackText != null) feedbackText.text = "";

        UpdateScoreUI();
        UpdateTimerUI();

        EnableGameplay(true);

        if (spawner != null)
        {
            spawner.ResetSpawner();
            spawner.SetRunning(true);
        }
    }

    private void Update()
    {
        if (!gameRunning) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft < 0f) timeLeft = 0f;

        if (!last5sMessageShown && timeLeft <= 5f && timeLeft > 0f)
        {
            last5sMessageShown = true;
            ShowFeedback("¡Último empujón!");
        }

        UpdateTimerUI();

        if (timeLeft <= 0f)
            EndGame();
    }

    public void RegisterCorrect()
    {
        if (!gameRunning) return;

        score += 1;
        combo += 1;

        UpdateScoreUI();
        ShowFeedback("¡Buena cosecha!");

        if (combo > 0 && combo % 5 == 0)
            ShowFeedback("¡Súper cosecha!");
    }

    public void RegisterWrong()
    {
        if (!gameRunning) return;

        score -= 1;
        if (score < 0) score = 0;

        combo = 0;

        UpdateScoreUI();
        ShowFeedback("¡Uy! Eso no era");
    }

    private void UpdateTimerUI()
    {
        float t = Mathf.Clamp01(timeLeft / gameDuration);

        if (timeSlider != null) timeSlider.value = t;

        if (timeSliderFillImage != null)
        {
            if (useGradient) timeSliderFillImage.color = timeGradient.Evaluate(t);
            else timeSliderFillImage.color = Color.Lerp(lowTimeColor, fullTimeColor, t);
        }

        if (timeText != null) timeText.text = FormatTimeMMSS(timeLeft);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = $"Puntos: {score}";
        if (comboText != null) comboText.text = $"Racha: {combo}";
    }

    private void ShowFeedback(string msg)
    {
        if (feedbackText == null) return;

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(FeedbackCoroutine(msg));
    }

    private IEnumerator FeedbackCoroutine(string msg)
    {
        feedbackText.text = msg;
        yield return new WaitForSeconds(feedbackShowSeconds);
        feedbackText.text = "";
    }

    private void EndGame()
    {
        gameRunning = false;

        if (spawner != null)
            spawner.SetRunning(false);

        EnableGameplay(false);

        int best = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        if (score > best)
        {
            best = score;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, best);
            PlayerPrefs.Save();
        }

        if (endPanel != null) endPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = $"Tu puntaje: {score}";
        if (bestScoreText != null) bestScoreText.text = $"Mejor puntaje: {best}";
    }

    private void EnableGameplay(bool enabled)
    {
        if (toDisableOnEnd == null) return;
        foreach (var mb in toDisableOnEnd)
            if (mb != null) mb.enabled = enabled;
    }

    private string FormatTimeMMSS(float seconds)
    {
        int s = Mathf.CeilToInt(seconds);
        int mm = s / 60;
        int ss = s % 60;
        return $"{mm:00}:{ss:00}";
    }

    // ====== Reinicio SOLO del minijuego ======
    public void RestartMiniGame()
    {
        // detener feedback
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        // parar spawner y limpiar items
        if (spawner != null)
        {
            spawner.SetRunning(false);
            spawner.ClearSpawned();
        }

        // reset canasta
        if (basket != null)
            basket.position = basketStartPos;

        // iniciar de nuevo
        StartGame();
    }
}