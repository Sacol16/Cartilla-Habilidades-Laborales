using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(RawImage))]
public class ClickToPauseVideoOnRawImage : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage rawImage;

    [Header("Play UI (icono encima del video)")]
    [SerializeField] private GameObject play;          // el objeto que se activa/desactiva
    [SerializeField] private Image playImage;          // el componente Image del objeto play
    [SerializeField] private Sprite playSprite;        // icono normal (play)
    [SerializeField] private Sprite replaySprite;      // icono replay
    [SerializeField] private GameObject next; 

    [Header("Overlay")]
    [SerializeField] private Color playingColor = Color.white;
    [SerializeField] private Color pausedColor = Color.gray;

    [Range(0f, 1f)]
    [SerializeField] private float pausedAlpha = 0.55f;
    [Range(0f, 1f)]
    [SerializeField] private float playingAlpha = 1f;

    private bool ended = false;

    private void Reset()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoEnded;
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnded;
    }

    private void Awake()
    {
        if (!rawImage) rawImage = GetComponent<RawImage>();
        SetPlayingState(); // arranca "reproduciendo"
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!videoPlayer) return;

        // Si terminó, cualquier click hace replay
        if (ended)
        {
            Replay();
            return;
        }

        // Si está reproduciendo: pausa
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            SetPausedState();
        }
        else
        {
            // Si está pausado (pero no terminado): sigue
            videoPlayer.Play();
            SetPlayingState();
        }
    }

    // Llama a esto desde el botón "play" también si quieres (OnClick)
    public void Replay()
    {
        if (!videoPlayer) return;

        ended = false;

        // Reinicia y reproduce
        videoPlayer.Stop();          // asegura reset interno
        videoPlayer.time = 0;
        videoPlayer.frame = 0;
        videoPlayer.Play();

        SetPlayingState();
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        ended = true;
        next.SetActive(true);

        // Estado visual “oscuro como pausado” + replay icon
        SetPausedState(isReplay: true);

        // Dejamos el video detenido en el último frame (ya terminó)
        // Si prefieres que quede en negro, podrías hacer vp.Stop();
    }

    private void SetPlayingState()
    {
        ApplyVisual(isPaused: false);

        if (play != null) play.SetActive(false);
        if (playImage != null && playSprite != null) playImage.sprite = playSprite;
        ended = false;
    }

    private void SetPausedState(bool isReplay = false)
    {
        ApplyVisual(isPaused: true);

        if (play != null) play.SetActive(true);

        if (playImage != null)
        {
            if (isReplay && replaySprite != null) playImage.sprite = replaySprite;
            else if (playSprite != null) playImage.sprite = playSprite;
        }
    }

    private void ApplyVisual(bool isPaused)
    {
        if (!rawImage) return;

        Color c = isPaused ? pausedColor : playingColor;
        c.a = isPaused ? pausedAlpha : playingAlpha;
        rawImage.color = c;
    }
}
