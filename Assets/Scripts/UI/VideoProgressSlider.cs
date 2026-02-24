using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoProgressSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Slider progressSlider;

    [Header("Behavior")]
    [SerializeField] private bool pauseWhileScrubbing = true;
    [SerializeField] private float uiUpdateRate = 0.05f; // segundos

    private bool isScrubbing;
    private bool wasPlayingBeforeScrub;
    private double cachedLength; // segundos
    private float nextUiUpdate;

    private void Awake()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.loopPointReached += OnVideoEnded;

            // Por si ya está listo
            TryCacheLength();
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.loopPointReached -= OnVideoEnded;
        }

        if (progressSlider != null)
            progressSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void Update()
    {
        if (videoPlayer == null || progressSlider == null) return;
        if (isScrubbing) return;

        // Throttle UI updates
        if (Time.unscaledTime < nextUiUpdate) return;
        nextUiUpdate = Time.unscaledTime + uiUpdateRate;

        if (!TryCacheLength()) return;

        double t = videoPlayer.time;
        double len = cachedLength;

        if (len > 0.01)
            progressSlider.SetValueWithoutNotify((float)(t / len));
        else
            progressSlider.SetValueWithoutNotify(0f);
    }

    private void OnPrepared(VideoPlayer vp)
    {
        TryCacheLength();
        // sincroniza el slider al preparar
        UpdateSliderImmediate();
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        // Al terminar, slider al 100%
        if (progressSlider != null)
            progressSlider.SetValueWithoutNotify(1f);
    }

    private bool TryCacheLength()
    {
        if (videoPlayer == null) return false;

        // VideoPlayer.length a veces es 0 hasta que está preparado
        double len = videoPlayer.length;
        if (len > 0.01)
        {
            cachedLength = len;
            return true;
        }

        // fallback por frames si está disponible
        if (videoPlayer.frameRate > 0 && videoPlayer.frameCount > 0)
        {
            cachedLength = videoPlayer.frameCount / videoPlayer.frameRate;
            return cachedLength > 0.01;
        }

        return false;
    }

    private void UpdateSliderImmediate()
    {
        if (videoPlayer == null || progressSlider == null) return;
        if (!TryCacheLength()) return;

        double len = cachedLength;
        double t = videoPlayer.time;

        progressSlider.SetValueWithoutNotify(len > 0.01 ? (float)(t / len) : 0f);
    }

    // Se llama mientras el usuario mueve el slider (y también cuando lo actualizamos por código,
    // por eso usamos isScrubbing para que solo haga seek cuando el usuario está interactuando).
    private void OnSliderValueChanged(float value01)
    {
        if (!isScrubbing) return;
        if (videoPlayer == null) return;
        if (!TryCacheLength()) return;

        double targetTime = Mathf.Clamp01(value01) * cachedLength;
        videoPlayer.time = targetTime;

        // Si usas audio del VideoPlayer, esto ayuda a que el seek se refleje mejor:
        videoPlayer.Play();
        videoPlayer.Pause();
    }

    // Detecta inicio de arrastre/click en el slider
    public void OnPointerDown(PointerEventData eventData)
    {
        if (videoPlayer == null || progressSlider == null) return;

        isScrubbing = true;

        wasPlayingBeforeScrub = videoPlayer.isPlaying;
        if (pauseWhileScrubbing && wasPlayingBeforeScrub)
            videoPlayer.Pause();
    }

    // Al soltar, aplica seek final y reanuda si corresponde
    public void OnPointerUp(PointerEventData eventData)
    {
        if (videoPlayer == null || progressSlider == null) return;

        // Aplicar seek final
        if (TryCacheLength())
            videoPlayer.time = Mathf.Clamp01(progressSlider.value) * cachedLength;

        isScrubbing = false;

        if (pauseWhileScrubbing && wasPlayingBeforeScrub)
            videoPlayer.Play();
    }
}