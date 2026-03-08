// ===========================
// DropSlotUI2.cs
// - Slot UI que solo acepta 1 Draggable (Primario/Secundario/Terciario)
// - SNAP al slot
// - Feedback guiado según el draggable que el jugador arrastra
// - Sonidos correcto / error
// - Usa TMP RichText (<b></b>) en vez de Markdown (** **)
// ===========================

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DropSlotUI2 : MonoBehaviour, IDropHandler
{
    [Header("Rule")]
    [Tooltip("ID que este slot acepta: 'Primario' 'Secundario' 'Terciario'")]
    [SerializeField] private string acceptedId;

    [Header("Snap")]
    [SerializeField] private RectTransform snapPoint;
    [SerializeField] private bool keepWorldPositionOnSnap = false;

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float feedbackSeconds = 1.2f;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip correctSfx;
    [SerializeField] private AudioClip errorSfx;

    [Header("Slot Context (para guiar mejor)")]
    [Tooltip("Nombre del minijuego/actividad de este slot. Ej: 'Cosecha', 'Ensamblaje', 'Atención al cliente'")]
    [SerializeField] private string activityName = "esta actividad";

    [Tooltip("Descripción corta de la actividad. Ej: 'cosechar frutas', 'ensamblar juguetes', 'vender y atender clientes'")]
    [SerializeField] private string activityHint = "la actividad";

    [Header("Messages (plantillas)")]
    [SerializeField] private string occupiedMsg = "Este espacio ya está ocupado.";
    [SerializeField] private string correctMsg = "¡Correcto!";

    [Header("Guided Feedback by dragged sector")]
    [TextArea]
    [SerializeField]
    private string primarioWrongHint =
        "Elegiste <b>Primario</b>. Recuerda: el sector primario extrae recursos de la naturaleza (agricultura, ganadería, pesca, minería).";

    [TextArea]
    [SerializeField]
    private string secundarioWrongHint =
        "Elegiste <b>Secundario</b>. Recuerda: el sector secundario transforma materias primas en productos (fabricación, ensamblaje, industria).";

    [TextArea]
    [SerializeField]
    private string terciarioWrongHint =
        "Elegiste <b>Terciario</b>. Recuerda: el sector terciario ofrece servicios (comercio, transporte, atención al cliente, ventas).";

    private DraggableUI2 current;
    private Coroutine feedbackRoutine;

    private void Awake()
    {
        if (snapPoint == null) snapPoint = transform as RectTransform;
        ClearFeedbackImmediate();
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedGO = eventData.pointerDrag;
        if (draggedGO == null) return;

        var drag = draggedGO.GetComponent<DraggableUI2>();
        if (drag == null) return;

        if (current != null && current != drag)
        {
            Play(errorSfx);
            ShowFeedback(occupiedMsg);
            drag.MarkDroppedValid();
            drag.SnapBack();
            return;
        }

        if (!string.IsNullOrEmpty(acceptedId) && drag.draggableId != acceptedId)
        {
            Play(errorSfx);
            ShowFeedback(BuildGuidedWrongMessage(drag.draggableId));
            drag.MarkDroppedValid();
            drag.SnapBack();
            return;
        }

        current = drag;
        SnapDraggableToSlot(drag);

        Play(correctSfx);
        ShowFeedback(BuildCorrectMessage(drag.draggableId));
    }

    private string BuildGuidedWrongMessage(string draggedId)
    {
        string baseMsg =
            $"No es <b>{draggedId}</b> para <b>{activityName}</b>.\n" +
            $"Pista: piensa en <b>{activityHint}</b>.\n\n";

        switch (draggedId)
        {
            case "Primario":
                return baseMsg + primarioWrongHint;

            case "Secundario":
                return baseMsg + secundarioWrongHint;

            case "Terciario":
                return baseMsg + terciarioWrongHint;

            default:
                return $"No es correcto para <b>{activityName}</b>. Pista: {activityHint}.";
        }
    }

    private string BuildCorrectMessage(string draggedId)
    {
        return $"{correctMsg} <b>{activityName}</b> corresponde al sector <b>{draggedId}</b>.";
    }

    private void SnapDraggableToSlot(DraggableUI2 drag)
    {
        drag.MarkDroppedValid();

        RectTransform dRect = drag.GetComponent<RectTransform>();
        RectTransform target = snapPoint != null ? snapPoint : (transform as RectTransform);

        dRect.SetParent(target, keepWorldPositionOnSnap);

        dRect.anchorMin = new Vector2(0.5f, 0.5f);
        dRect.anchorMax = new Vector2(0.5f, 0.5f);
        dRect.pivot = new Vector2(0.5f, 0.5f);
        dRect.anchoredPosition = Vector2.zero;
        dRect.localRotation = Quaternion.identity;
        dRect.localScale = Vector3.one;

        drag.SetPlacedInSlot(this);
    }

    public void ClearIfCurrent(DraggableUI2 drag)
    {
        if (current == drag) current = null;
    }

    public bool IsOccupied => current != null;

    private void Play(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private void ShowFeedback(string msg)
    {
        if (feedbackText == null) return;

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(FeedbackCoroutine(msg, feedbackSeconds));
    }

    private IEnumerator FeedbackCoroutine(string msg, float seconds)
    {
        feedbackText.text = msg;

        if (seconds > 0f)
        {
            yield return new WaitForSeconds(seconds);

            if (feedbackText.text == msg)
                feedbackText.text = "";
        }
    }

    private void ClearFeedbackImmediate()
    {
        if (feedbackText != null) feedbackText.text = "";
    }
}