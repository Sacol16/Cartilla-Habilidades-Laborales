using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Module2ActivityManager
/// SOLO Actividad 3 y 4 (según tu Module1ActivityManager).
/// - Act.3: habilita continuar cuando StrokeCount >= minLinesForContinue3 y guarda PNG en memoria
/// - Act.4: habilita continuar cuando hay selección + audio (base64 o bytes o flag)
/// </summary>
public class Module2ActivityManager : MonoBehaviour, IActivity4Receiver
{
    public static Module2ActivityManager Instance { get; private set; }

    // =========================
    // Actividad 3 (Dibujo)
    // =========================
    public byte[] Activity3PngBytes { get; private set; }

    [Header("Actividad 3")]
    [SerializeField] private GameObject continueButton3;
    [SerializeField] private int minLinesForContinue3 = 10;

    // =========================
    // Actividad 4 (Selección + Audio)
    // =========================
    [Header("Actividad 4")]
    [SerializeField] private GameObject continueButton4;

    public string Activity4SelectedOptionId { get; private set; } = "";
    public string Activity4AudioBase64 { get; private set; } = "";
    public byte[] Activity4AudioBytes { get; private set; }

    // Flag para confirmar audio (útil WebGL / timing)
    public bool Activity4HasAudioFlag { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // ===== Actividad 3 =====
        if (continueButton3 != null)
            continueButton3.SetActive(false);

        StartCoroutine(AttachToLineaWhenReady());

        // ===== Actividad 4 =====
        if (continueButton4 != null)
            continueButton4.SetActive(false);

        // estado inicial act4
        ClearActivity4Selection();
        ClearActivity4Audio();
        UpdateContinue4();
    }

    // =========================
    // Actividad 3 (Dibujo)
    // =========================
    private void HandleStrokeCountChanged(int count)
    {
        bool canContinue = count >= minLinesForContinue3;

        if (continueButton3 != null)
            continueButton3.SetActive(canContinue);
    }

    private void OnDestroy()
    {
        if (Linea.Instance != null)
            Linea.Instance.OnStrokeCountChanged -= HandleStrokeCountChanged;
    }

    public void SetActivity3Drawing(byte[] pngBytes)
    {
        Activity3PngBytes = pngBytes;
        Debug.Log($"? [Module2] Actividad 3 guardada en memoria. Bytes: {(pngBytes != null ? pngBytes.Length : 0)}");
    }

    private IEnumerator AttachToLineaWhenReady()
    {
        while (Linea.Instance == null)
            yield return null;

        Linea.Instance.OnStrokeCountChanged += HandleStrokeCountChanged;
        HandleStrokeCountChanged(Linea.Instance.StrokeCount);

        Debug.Log("[Module2] Suscrito a Linea.OnStrokeCountChanged");
    }

    // =========================
    // Actividad 4 (Selección + Audio)
    // =========================
    public void SetActivity4SelectedOption(string optionId)
    {
        Activity4SelectedOptionId = optionId == null ? "" : optionId.Trim();
        UpdateContinue4();
    }

    public void SetActivity4AudioBase64(string base64)
    {
        Activity4AudioBase64 = base64 ?? "";
        Activity4AudioBytes = null;

        Activity4HasAudioFlag = !string.IsNullOrEmpty(Activity4AudioBase64);

        UpdateContinue4();
    }

    public void SetActivity4AudioBytes(byte[] bytes)
    {
        Activity4AudioBytes = bytes;
        Activity4AudioBase64 = "";

        Activity4HasAudioFlag = (Activity4AudioBytes != null && Activity4AudioBytes.Length > 0);

        UpdateContinue4();
    }

    public void ClearActivity4Audio()
    {
        Activity4AudioBase64 = "";
        Activity4AudioBytes = null;
        Activity4HasAudioFlag = false;
        UpdateContinue4();
    }

    public void ClearActivity4Selection()
    {
        Activity4SelectedOptionId = "";
        UpdateContinue4();
    }

    public bool Activity4HasAudio()
    {
        if (Activity4HasAudioFlag) return true;

        bool hasBase64 = !string.IsNullOrEmpty(Activity4AudioBase64);
        bool hasBytes = Activity4AudioBytes != null && Activity4AudioBytes.Length > 0;
        return hasBase64 || hasBytes;
    }

    public bool Activity4HasSelection()
    {
        return !string.IsNullOrEmpty(Activity4SelectedOptionId);
    }

    public bool Activity4IsComplete()
    {
        return Activity4HasSelection() && Activity4HasAudio();
    }

    private void UpdateContinue4()
    {
        bool sel = Activity4HasSelection();
        bool aud = Activity4HasAudio();
        bool complete = sel && aud;

        Debug.Log($"[Module2] Act4 sel={sel} ('{Activity4SelectedOptionId}') aud={aud} flag={Activity4HasAudioFlag} -> complete={complete}");

        if (continueButton4 != null)
            continueButton4.SetActive(complete);
    }

    /// <summary>
    /// Gatillo extra (WebGL): si detectas WA_HasAudio()==1, llama esto y activa el botón
    /// aunque el base64 llegue un frame después.
    /// </summary>
    public void SetActivity4HasAudio(bool hasAudio)
    {
        Activity4HasAudioFlag = hasAudio;

        if (!hasAudio)
        {
            Activity4AudioBase64 = "";
            Activity4AudioBytes = null;
        }

        UpdateContinue4();
    }
}