using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WebAudioRecorderUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI")]
    [SerializeField] private Button recordOrPlayButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Slider progressSlider;      // 0..1
    [SerializeField] private TMP_Text durationText;      // "00:00 / 00:00"
    [SerializeField] private TMP_Text recordButtonLabel; // opcional (texto del botón)

    [Header("Settings")]
    [SerializeField] private int maxSeconds = 60;

    private bool isScrubbing;
    private bool lastHasAudio = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    // ===== WebGL: JS bridge =====
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void WA_Init(int maxSeconds);
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void WA_StartRecord();
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void WA_StopRecord();
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void WA_PlayPause();
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void WA_Clear();
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern int WA_HasAudio();      // 0/1
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern int WA_IsRecording();   // 0/1
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern int WA_IsPlaying();     // 0/1
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern float WA_GetDuration(); // seconds
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern float WA_GetTime();     // seconds
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void WA_Seek(float t);  // seconds
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern string WA_GetAudioBase64(); // webm base64 (sin data:)
#else
    // ===== Editor/Standalone: Microphone (para probar local) =====
    [Header("Editor/Standalone test (ignored in WebGL)")]
    [SerializeField] private AudioSource playbackSource;
    [SerializeField] private int sampleRate = 44100;

    private string micDevice;
    private AudioClip recordedClip;
    private bool isRecording;
    private bool isPlaying;
    private float playStartTime;
#endif

    private void Awake()
    {
        if (recordOrPlayButton != null) recordOrPlayButton.onClick.AddListener(OnRecordOrPlayPressed);
        if (deleteButton != null) deleteButton.onClick.AddListener(DeleteAudio);

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.onValueChanged.AddListener(OnSliderChanged);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        WA_Init(maxSeconds);
#else
        if (Microphone.devices.Length > 0) micDevice = Microphone.devices[0];
#endif

        RefreshUI();
        PushAudioStateToManager(false); // al iniciar, no hay audio
    }

    private void Update()
    {
        if (progressSlider == null || durationText == null) return;

#if UNITY_WEBGL && !UNITY_EDITOR
        bool has = WA_HasAudio() == 1;
        bool rec = WA_IsRecording() == 1;

        // Sincroniza audio real al manager (y por tanto el gating del botón 4)
        PushAudioStateToManager(has);

        // UI tiempo durante grabación / reproducción
        float dur = rec ? maxSeconds : (has ? WA_GetDuration() : 0f);
        float t   = rec ? WA_GetTime() : (has ? WA_GetTime() : 0f);

        if (rec) t = Mathf.Clamp(t, 0f, dur);

        if (!isScrubbing)
        {
            float v = (dur > 0.01f) ? Mathf.Clamp01(t / dur) : 0f;
            progressSlider.SetValueWithoutNotify(v);
        }

        durationText.text = $"{FormatTime(t)} / {FormatTime(dur)}";
#else
        bool has = recordedClip != null;

        // Sincroniza al manager
        PushAudioStateToManager(has);

        float dur;
        float t;

        if (isRecording)
        {
            dur = maxSeconds;
            t = GetRecordingTimeStandalone();
        }
        else
        {
            dur = has ? recordedClip.length : 0f;
            t = 0f;
            if (has && playbackSource != null)
            {
                t = playbackSource.isPlaying ? playbackSource.time : Mathf.Clamp(playStartTime, 0f, dur);
                isPlaying = playbackSource.isPlaying;
            }
        }

        if (!isScrubbing)
        {
            float v = (dur > 0.01f) ? Mathf.Clamp01(t / dur) : 0f;
            progressSlider.SetValueWithoutNotify(v);
        }

        durationText.text = $"{FormatTime(t)} / {FormatTime(dur)}";
#endif
    }

    // ---------- Activity 4 integration (store real audio in manager) ----------
    private void PushAudioStateToManager(bool hasAudioNow)
    {
        var mgr = Module1ActivityManager.Instance;
        if (mgr == null) return;

        // 1) Siempre actualiza el flag de "hay audio" (esto activa el botón 4 aunque base64 tarde)
        mgr.SetActivity4HasAudio(hasAudioNow);

        // 2) Si NO hay audio, limpia todo y listo
        if (!hasAudioNow)
        {
            if (lastHasAudio)
            {
                lastHasAudio = false;
                mgr.ClearActivity4Audio(); // deja flag false + limpia base64/bytes
            }
            return;
        }

        // hasAudioNow == true
        if (!lastHasAudio) lastHasAudio = true;

#if UNITY_WEBGL && !UNITY_EDITOR
    // 3) Intenta guardar el base64 real (si aún no está).
    // Si por timing viene vacío, Update lo volverá a intentar en el siguiente frame.
    if (string.IsNullOrEmpty(mgr.Activity4AudioBase64))
    {
        string b64 = GetRecordedAudioBase64();
        if (!string.IsNullOrEmpty(b64))
            mgr.SetActivity4AudioBase64(b64);
    }
#endif
    }

    // ---------- Button behavior ----------
    private void OnRecordOrPlayPressed()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        bool has = WA_HasAudio() == 1;
        bool rec = WA_IsRecording() == 1;

        if (!has && !rec)
        {
            // Iniciar grabación
            WA_StartRecord();
            // mientras graba, aún no hay audio final
            if (Module1ActivityManager.Instance != null)
                Module1ActivityManager.Instance.ClearActivity4Audio();
        }
        else if (rec)
        {
            // Detener grabación
            WA_StopRecord();
            // Update() se encargará de detectar WA_HasAudio() y guardar base64 cuando esté listo.
        }
        else
        {
            // Play/Pause
            WA_PlayPause();
        }
#else
        if (recordedClip == null && !isRecording)
        {
            StartRecordingStandalone();
        }
        else if (isRecording)
        {
            StopRecordingStandalone(); // recorta y deja recordedClip != null (si grabó)
        }
        else
        {
            PlayPauseStandalone();
        }
#endif
        RefreshUI();
    }

    public void DeleteAudio()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WA_Clear();
#else
        if (playbackSource != null) playbackSource.Stop();
        recordedClip = null;
        isRecording = false;
        isPlaying = false;
        playStartTime = 0f;
#endif
        if (progressSlider != null) progressSlider.SetValueWithoutNotify(0f);

        // Limpia audio en manager (Actividad 4)
        if (Module1ActivityManager.Instance != null)
            Module1ActivityManager.Instance.ClearActivity4Audio();

        lastHasAudio = false;

        RefreshUI();
    }

    // ---------- Slider scrubbing ----------
    public void OnPointerDown(PointerEventData eventData)
    {
        isScrubbing = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isScrubbing = false;

#if UNITY_WEBGL && !UNITY_EDITOR
        if (WA_HasAudio() == 1 && WA_IsRecording() == 0)
        {
            float dur = WA_GetDuration();
            float target = Mathf.Clamp01(progressSlider.value) * dur;
            WA_Seek(target);
        }
#else
        if (recordedClip != null && playbackSource != null && !isRecording)
        {
            float target = Mathf.Clamp01(progressSlider.value) * recordedClip.length;
            playbackSource.time = target;
            playStartTime = target;
        }
#endif
    }

    private void OnSliderChanged(float value01)
    {
        if (!isScrubbing) return;

#if UNITY_WEBGL && !UNITY_EDITOR
        if (WA_HasAudio() == 1 && WA_IsRecording() == 0)
        {
            float dur = WA_GetDuration();
            float target = Mathf.Clamp01(value01) * dur;
            WA_Seek(target);
        }
#else
        if (recordedClip != null && playbackSource != null && !isRecording)
        {
            float target = Mathf.Clamp01(value01) * recordedClip.length;
            playbackSource.time = target;
            playStartTime = target;
        }
#endif
    }

    // ---------- UI state ----------
    private void RefreshUI()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        bool has = WA_HasAudio() == 1;
        bool rec = WA_IsRecording() == 1;
        bool playing = WA_IsPlaying() == 1;

        if (deleteButton != null) deleteButton.interactable = has && !rec;
        if (progressSlider != null) progressSlider.interactable = has && !rec;

        if (recordButtonLabel != null)
        {
            recordButtonLabel.text = !has && !rec ? "Grabar" :
                                     rec ? "Detener" :
                                     (playing ? "Pausar" : "Reproducir");
        }
#else
        bool has = recordedClip != null;

        if (deleteButton != null) deleteButton.interactable = has && !isRecording;
        if (progressSlider != null) progressSlider.interactable = has && !isRecording;

        if (recordButtonLabel != null)
        {
            recordButtonLabel.text = !has && !isRecording ? "Grabar" :
                                     isRecording ? "Detener" :
                                     (isPlaying ? "Pausar" : "Reproducir");
        }
#endif
    }

    private static string FormatTime(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }

    // ---------- Get audio for final POST ----------
    // En WebGL te devuelve base64 de webm (opus). En standalone no lo implemento aquí.
    public string GetRecordedAudioBase64()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return WA_HasAudio() == 1 ? WA_GetAudioBase64() : "";
#else
        Debug.LogWarning("GetRecordedAudioBase64: En standalone usarías WAV/bytes. En WebGL se usa JS.");
        return "";
#endif
    }

#if !(UNITY_WEBGL && !UNITY_EDITOR)
    // ===== Standalone test (Microphone) =====
    private void StartRecordingStandalone()
    {
        if (string.IsNullOrEmpty(micDevice))
        {
            Debug.LogError("No hay micrófono disponible (Standalone).");
            return;
        }

        if (playbackSource != null) playbackSource.Stop();
        recordedClip = Microphone.Start(micDevice, false, maxSeconds, sampleRate);
        isRecording = true;
        isPlaying = false;
        playStartTime = 0f;

        // al iniciar grabación, limpiamos audio en el manager
        if (Module1ActivityManager.Instance != null)
            Module1ActivityManager.Instance.ClearActivity4Audio();
    }

    private void StopRecordingStandalone()
    {
        if (string.IsNullOrEmpty(micDevice) || recordedClip == null) return;

        // 1) Tomar cuántas muestras reales se grabaron
        int samplesRecorded = Microphone.GetPosition(micDevice);
        Microphone.End(micDevice);
        isRecording = false;

        if (samplesRecorded <= 0)
        {
            recordedClip = null;
            if (Module1ActivityManager.Instance != null)
                Module1ActivityManager.Instance.ClearActivity4Audio();
            return;
        }

        // 2) Recortar: crear un nuevo clip solo con la duración grabada
        int channels = recordedClip.channels;
        float[] data = new float[samplesRecorded * channels];
        recordedClip.GetData(data, 0);

        AudioClip trimmed = AudioClip.Create(
            "RecordedTrimmed",
            samplesRecorded,
            channels,
            recordedClip.frequency,
            false
        );

        trimmed.SetData(data, 0);
        recordedClip = trimmed;

        // Si algún día conviertes a WAV bytes, aquí sería el lugar:
        // Module1ActivityManager.Instance.SetActivity4AudioBytes(wavBytes);
    }

    private void PlayPauseStandalone()
    {
        if (recordedClip == null || playbackSource == null) return;

        if (playbackSource.isPlaying)
        {
            playbackSource.Pause();
            isPlaying = false;
        }
        else
        {
            playbackSource.clip = recordedClip;
            playbackSource.Play();
            isPlaying = true;
        }
    }

    private float GetRecordingTimeStandalone()
    {
        if (string.IsNullOrEmpty(micDevice)) return 0f;
        int pos = Microphone.GetPosition(micDevice);
        if (pos < 0) return 0f;
        return Mathf.Clamp(pos / (float)sampleRate, 0f, maxSeconds);
    }
#endif
}