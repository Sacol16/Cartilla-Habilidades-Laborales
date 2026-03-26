using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Module1Activity4Presenter : MonoBehaviour
{
    [Header("Option Buttons (4) - en el MISMO orden que las opciones")]
    [Tooltip("Arrastra los 4 botones correspondientes a las 4 opciones.")]
    public Button[] optionButtons = new Button[4];

    [Header("Selected/Unselected Colors")]
    public Color selectedGreen = Color.green;
    public Color unselectedColor = Color.white;

    [Header("Audio UI")]
    [Tooltip("AudioSource para reproducir el audio cargado.")]
    public AudioSource audioSource;

    [Tooltip("Botón para reproducir/pausar (opcional).")]
    public Button playButton;

    [Tooltip("Slider de progreso del audio (0..1). Si no lo asignas, se ignora.")]
    [SerializeField] private Slider progressSlider;

    [Tooltip("Texto de duración \"00:00 / 00:00\". Si no lo asignas, se ignora.")]
    [SerializeField] private TMP_Text durationText;

    [Header("Option IDs (texto EXACTO que llega del backend)")]
    [TextArea(2, 5)] public string option1Id;
    [TextArea(2, 5)] public string option2Id;
    [TextArea(2, 5)] public string option3Id;
    [TextArea(2, 5)] public string option4Id;

    [Header("Debug")]
    public bool debugLogs = true;

    private string[] _ids;
    private bool _isScrubbing;
    private bool _uiReady;

#if UNITY_WEBGL && !UNITY_EDITOR
[System.Runtime.InteropServices.DllImport("__Internal")]
private static extern void WA_LoadBase64(string base64);

[System.Runtime.InteropServices.DllImport("__Internal")]
private static extern void WA_PlayPause();

[System.Runtime.InteropServices.DllImport("__Internal")]
private static extern int WA_IsPlaying();

[System.Runtime.InteropServices.DllImport("__Internal")]
private static extern float WA_GetDuration();

[System.Runtime.InteropServices.DllImport("__Internal")]
private static extern float WA_GetTime();

[System.Runtime.InteropServices.DllImport("__Internal")]
private static extern void WA_Seek(float t);
#endif

    private void Awake()
    {
        _ids = new[] { option1Id, option2Id, option3Id, option4Id };

        // Botones de opciones no interactuables
        if (optionButtons != null)
        {
            foreach (var b in optionButtons)
                if (b != null) b.interactable = false;
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;

            progressSlider.onValueChanged.RemoveListener(OnSliderChanged);
            progressSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        UpdateDurationText(0f, 0f);
        _uiReady = true;
    }

    private void OnDisable()
    {
        // opcional: si desactivas el panel, evita que siga sonando
        // if (audioSource != null) audioSource.Stop();
    }

    private void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

    float total = WA_GetDuration();
    float current = WA_GetTime();

    if (total <= 0.01f)
    {
        UpdateDurationText(0f, 0f);
        if (progressSlider != null && !_isScrubbing) progressSlider.value = 0f;
        return;
    }

    if (progressSlider != null && !_isScrubbing)
        progressSlider.value = Mathf.Clamp01(current / total);

    UpdateDurationText(current, total);

#else
        // TU LÓGICA ORIGINAL (no tocar)
        if (!_uiReady) return;
        if (audioSource == null) return;
        if (audioSource.clip == null) return;

        float total = audioSource.clip.length;
        float current = audioSource.time;

        if (progressSlider != null && !_isScrubbing)
            progressSlider.value = Mathf.Clamp01(current / total);

        UpdateDurationText(current, total);
#endif
    }

    /// <summary>
    /// Aplica selección (pinta verde el botón correspondiente) y carga el audio.
    /// selectedOptionId debe ser uno de los 4 textos exactos.
    /// audioBase64 idealmente WAV/OGG en base64 (sin "data:...").
    /// </summary>
    public void Apply(string selectedOptionId, string audioBase64)
    {
        ApplySelection(selectedOptionId);
        LoadAudio(audioBase64);
    }

    public void ApplySelection(string selectedOptionId)
    {
        // Resetea colores
        for (int i = 0; i < optionButtons.Length; i++)
        {
            var btn = optionButtons[i];
            if (btn == null) continue;

            var img = btn.GetComponent<Image>();
            if (img != null) img.color = unselectedColor;
        }

        if (string.IsNullOrEmpty(selectedOptionId))
        {
            if (debugLogs) Debug.Log("[Activity4Presenter] selectedOptionId vacío -> no se marca ninguna opción.");
            return;
        }

        int index = FindOptionIndex(selectedOptionId);
        if (index < 0 || index >= optionButtons.Length)
        {
            Debug.LogWarning("[Activity4Presenter] selectedOptionId no coincide con ninguna opción. Revisa textos exactos.");
            if (debugLogs) Debug.Log("[Activity4Presenter] selectedOptionId recibido:\n" + selectedOptionId);
            return;
        }

        var selectedBtn = optionButtons[index];
        if (selectedBtn != null)
        {
            var img = selectedBtn.GetComponent<Image>();
            if (img != null) img.color = selectedGreen;
        }

        if (debugLogs) Debug.Log($"[Activity4Presenter] Selección aplicada index={index}");
    }

    private int FindOptionIndex(string selectedOptionId)
    {
        string s = Normalize(selectedOptionId);

        for (int i = 0; i < _ids.Length; i++)
        {
            if (string.IsNullOrEmpty(_ids[i])) continue;
            if (Normalize(_ids[i]) == s) return i;
        }

        return -1;
    }

    private string Normalize(string v)
    {
        return (v ?? "").Trim().Replace("\r", "").Replace("\n", "");
    }

    /// <summary>
    /// Carga audio desde base64.
    /// Nota: este decode implementa WAV PCM 16-bit. Si tu base64 es WEBM/MP3, no va a funcionar.
    /// </summary>
    public void LoadAudio(string audioBase64)
    {
        // Reset UI
        if (progressSlider != null) progressSlider.value = 0f;
        UpdateDurationText(0f, 0f);

        if (string.IsNullOrEmpty(audioBase64))
        {
            if (debugLogs) Debug.Log("[Activity4Presenter] audio vacío.");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR

    audioBase64 = StripDataUrlPrefixIfNeeded(audioBase64);

    WA_LoadBase64(audioBase64);

    if (debugLogs)
        Debug.Log("[Activity4Presenter] Audio WEBM cargado en JS");

#else
        Debug.LogWarning("[Activity4Presenter] Este flujo solo funciona en WebGL.");
#endif
    }

    private void OnPlayClicked()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    WA_PlayPause();
#else
        if (audioSource == null || audioSource.clip == null) return;

        if (audioSource.isPlaying)
            audioSource.Pause();
        else
            audioSource.Play();
#endif
    }

    private void OnSliderChanged(float value01)
    {
        _isScrubbing = true;

#if UNITY_WEBGL && !UNITY_EDITOR
    float total = WA_GetDuration();
    float target = Mathf.Clamp01(value01) * total;
    WA_Seek(target);

    UpdateDurationText(target, total);
#else
        if (audioSource == null || audioSource.clip == null) return;

        float total = audioSource.clip.length;
        audioSource.time = Mathf.Clamp01(value01) * total;

        UpdateDurationText(audioSource.time, total);
#endif

        CancelInvoke(nameof(EndScrub));
        Invoke(nameof(EndScrub), 0.05f);
    }

    private void EndScrub()
    {
        _isScrubbing = false;
    }

    private void UpdateDurationText(float current, float total)
    {
        if (durationText == null) return;
        durationText.text = $"{FormatTime(current)} / {FormatTime(total)}";
    }

    private string FormatTime(float seconds)
    {
        if (seconds < 0) seconds = 0;
        int s = Mathf.FloorToInt(seconds);
        int mm = s / 60;
        int ss = s % 60;
        return $"{mm:00}:{ss:00}";
    }

    private string StripDataUrlPrefixIfNeeded(string b64)
    {
        // Ej: "data:audio/wav;base64,AAAA..."
        int comma = b64.IndexOf(',');
        if (b64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0)
            return b64.Substring(comma + 1);

        return b64;
    }
}

/// <summary>
/// WAV utility mínima (PCM 16-bit little-endian) para convertir bytes -> AudioClip.
/// Funciona si el audioBase64 es WAV estándar.
/// </summary>
public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavBytes, string clipName = "wav")
    {
        if (wavBytes == null || wavBytes.Length < 44)
            throw new Exception("WAV demasiado corto.");

        // RIFF header check
        if (wavBytes[0] != 'R' || wavBytes[1] != 'I' || wavBytes[2] != 'F' || wavBytes[3] != 'F')
            throw new Exception("No es RIFF/WAV.");

        int channels = BitConverter.ToInt16(wavBytes, 22);
        int sampleRate = BitConverter.ToInt32(wavBytes, 24);
        int bitsPerSample = BitConverter.ToInt16(wavBytes, 34);
        if (bitsPerSample != 16)
            throw new Exception("Solo soporta WAV PCM 16-bit.");

        int dataChunkOffset = FindDataChunkOffset(wavBytes);
        int dataSize = BitConverter.ToInt32(wavBytes, dataChunkOffset + 4);
        int dataStart = dataChunkOffset + 8;

        if (dataStart + dataSize > wavBytes.Length)
            dataSize = wavBytes.Length - dataStart;

        int sampleCount = dataSize / 2; // 16-bit -> 2 bytes
        float[] samples = new float[sampleCount];

        int offset = dataStart;
        for (int i = 0; i < sampleCount; i++)
        {
            short s = BitConverter.ToInt16(wavBytes, offset);
            samples[i] = s / 32768f;
            offset += 2;
        }

        int lengthSamplesPerChannel = sampleCount / Mathf.Max(1, channels);

        var clip = AudioClip.Create(clipName, lengthSamplesPerChannel, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static int FindDataChunkOffset(byte[] bytes)
    {
        for (int i = 12; i < bytes.Length - 4; i++)
        {
            if (bytes[i] == 'd' && bytes[i + 1] == 'a' && bytes[i + 2] == 't' && bytes[i + 3] == 'a')
                return i;
        }
        throw new Exception("Chunk 'data' no encontrado.");
    }
}