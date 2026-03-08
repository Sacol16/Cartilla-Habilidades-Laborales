using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Module4ActivityManager : MonoBehaviour, IActivity4Receiver
{
    public static Module4ActivityManager Instance { get; private set; }
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
    // Start is called before the first frame update
    void Start()
    {
        // ===== Actividad 4 =====
        if (continueButton4 != null)
            continueButton4.SetActive(false);

        // estado inicial act4
        ClearActivity4Selection();
        ClearActivity4Audio();
        UpdateContinue4();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
