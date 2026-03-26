using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Module1Activity4Presenter : MonoBehaviour
{
    [Header("Option Buttons")]
    public Button[] optionButtons = new Button[4];

    [Header("Colors")]
    public Color selectedGreen = Color.green;
    public Color unselectedColor = Color.white;

    [Header("Audio UI")]
    public AudioSource audioSource; // solo para editor
    public Button playButton;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text durationText;

    [Header("Option IDs")]
    [TextArea] public string option1Id;
    [TextArea] public string option2Id;
    [TextArea] public string option3Id;
    [TextArea] public string option4Id;

    [Header("Debug")]
    public bool debugLogs = true;

    private string[] _ids;
    private bool _isScrubbing;
    private bool _hasAudioLoaded;

    // Tiempo máximo en segundos para esperar que el audio esté "ready"
    private float _readyCheckTimer = 0f;
    private const float ReadyCheckTimeout = 5f;
    private bool _waitingForReady = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    // Namespace separado WAP_ para no colisionar con el recorder (WA_)
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void WAP_LoadBase64(string base64);
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void WAP_PlayPause();
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern int  WAP_IsReady();      // 0/1
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern int  WAP_IsPlaying();    // 0/1
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern float WAP_GetDuration();
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern float WAP_GetTime();
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void WAP_Seek(float t);
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void WAP_Stop();
#endif

    private void Awake()
    {
        _ids = new[] { option1Id, option2Id, option3Id, option4Id };

        foreach (var b in optionButtons)
            if (b != null) b.interactable = false;

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            progressSlider.onValueChanged.RemoveAllListeners();
            progressSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        UpdateDurationText(0, 0);
    }

    private void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Esperar a que el audio esté listo (oncanplaythrough)
        if (_waitingForReady)
        {
            if (WAP_IsReady() == 1)
            {
                _hasAudioLoaded = true;
                _waitingForReady = false;
                _readyCheckTimer = 0f;
                if (debugLogs) Debug.Log("[Activity4Presenter] Audio listo para reproducir.");
            }
            else
            {
                _readyCheckTimer += Time.deltaTime;
                if (_readyCheckTimer >= ReadyCheckTimeout)
                {
                    _waitingForReady = false;
                    _readyCheckTimer = 0f;
                    Debug.LogWarning("[Activity4Presenter] Timeout esperando que el audio esté listo.");
                }
            }
            return; // No actualizar slider hasta que esté listo
        }

        if (!_hasAudioLoaded) return;

        float total = WAP_GetDuration();
        float current = WAP_GetTime();

        if (total <= 0.01f)
        {
            UpdateDurationText(0, 0);
            return;
        }

        if (progressSlider != null && !_isScrubbing)
            progressSlider.value = Mathf.Clamp01(current / total);

        UpdateDurationText(current, total);

#else
        if (audioSource == null || audioSource.clip == null) return;

        float total = audioSource.clip.length;
        float current = audioSource.time;

        if (progressSlider != null && !_isScrubbing)
            progressSlider.value = Mathf.Clamp01(current / total);

        UpdateDurationText(current, total);
#endif
    }

    // ---------- API pública ----------

    public void Apply(string selectedOptionId, string audioBase64)
    {
        ApplySelection(selectedOptionId);
        LoadAudio(audioBase64);
    }

    public void ApplySelection(string selectedOptionId)
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            var img = optionButtons[i]?.GetComponent<Image>();
            if (img != null) img.color = unselectedColor;
        }

        if (string.IsNullOrEmpty(selectedOptionId)) return;

        int index = FindOptionIndex(selectedOptionId);

        if (index >= 0 && index < optionButtons.Length)
        {
            var img = optionButtons[index]?.GetComponent<Image>();
            if (img != null) img.color = selectedGreen;
        }
    }

    public void LoadAudio(string audioBase64)
    {
        // Reset completo
        _hasAudioLoaded = false;
        _waitingForReady = false;
        _readyCheckTimer = 0f;

        if (progressSlider != null) progressSlider.value = 0f;
        UpdateDurationText(0, 0);

        if (string.IsNullOrEmpty(audioBase64))
        {
            if (debugLogs) Debug.Log("[Activity4Presenter] LoadAudio: base64 vacío, nada que cargar.");
            return;
        }

        audioBase64 = StripDataUrlPrefixIfNeeded(audioBase64);

        if (string.IsNullOrEmpty(audioBase64))
        {
            Debug.LogWarning("[Activity4Presenter] LoadAudio: base64 quedó vacío después de strip.");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WAP_LoadBase64(audioBase64);
            // No ponemos _hasAudioLoaded = true aquí.
            // Esperamos a que JS dispare oncanplaythrough (WAP_IsReady() == 1).
            _waitingForReady = true;
            _readyCheckTimer = 0f;

            if (debugLogs) Debug.Log("[Activity4Presenter] WAP_LoadBase64 llamado. Esperando oncanplaythrough...");
        }
        catch (Exception e)
        {
            Debug.LogError("[Activity4Presenter] Error llamando WAP_LoadBase64: " + e.Message);
        }
#else
        Debug.LogWarning("[Activity4Presenter] LoadAudio solo funciona en WebGL build.");
#endif
    }

    // ---------- Botones ----------

    private void OnPlayClicked()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (_waitingForReady)
        {
            if (debugLogs) Debug.Log("[Activity4Presenter] Aún esperando que el audio esté listo...");
            return;
        }

        if (!_hasAudioLoaded)
        {
            if (debugLogs) Debug.Log("[Activity4Presenter] No hay audio cargado.");
            return;
        }

        WAP_PlayPause();
#else
        if (audioSource == null || audioSource.clip == null) return;

        if (audioSource.isPlaying)
            audioSource.Pause();
        else
            audioSource.Play();
#endif
    }

    // ---------- Slider scrubbing ----------

    private void OnSliderChanged(float value01)
    {
        _isScrubbing = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_hasAudioLoaded) return;

        float total = WAP_GetDuration();
        float target = value01 * total;
        WAP_Seek(target);
        UpdateDurationText(target, total);
#else
        if (audioSource == null || audioSource.clip == null) return;

        float total = audioSource.clip.length;
        audioSource.time = value01 * total;
        UpdateDurationText(audioSource.time, total);
#endif

        CancelInvoke(nameof(EndScrub));
        Invoke(nameof(EndScrub), 0.05f);
    }

    private void EndScrub() => _isScrubbing = false;

    // ---------- Helpers ----------

    private int FindOptionIndex(string selectedOptionId)
    {
        string s = Normalize(selectedOptionId);
        for (int i = 0; i < _ids.Length; i++)
            if (Normalize(_ids[i]) == s)
                return i;
        return -1;
    }

    private string Normalize(string v) =>
        (v ?? "").Trim().Replace("\r", "").Replace("\n", "");

    private void UpdateDurationText(float current, float total)
    {
        if (durationText == null) return;
        durationText.text = $"{FormatTime(current)} / {FormatTime(total)}";
    }

    private string FormatTime(float seconds)
    {
        int s = Mathf.FloorToInt(Mathf.Max(0, seconds));
        return $"{s / 60:00}:{s % 60:00}";
    }

    private string StripDataUrlPrefixIfNeeded(string b64)
    {
        int comma = b64.IndexOf(',');
        if (b64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0)
            return b64.Substring(comma + 1);
        return b64;
    }
}