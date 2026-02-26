using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class AssemblyDropZoneUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private AssemblyStation station;

    [Header("Feedback UI")]
    [SerializeField] private TMP_Text feedbackText;      // puede ser global o por estación
    [SerializeField] private float feedbackSeconds = 1.2f;

    private Coroutine feedbackRoutine;

    public void OnDrop(PointerEventData eventData)
    {
        if (station == null) return;

        var dragged = eventData.pointerDrag;
        if (dragged == null) return;

        var piece = dragged.GetComponent<ConveyorPieceDraggableUI>();
        if (piece == null) return;

        bool ok = station.TryPlacePiece(piece.pieceId, out string msg);

        if (ok)
        {
            ShowFeedback("¡Bien!", isError: false);
            piece.MarkDroppedValid();
            piece.Consume();
        }
        else
        {
            ShowFeedback(string.IsNullOrEmpty(msg) ? "No va ahí." : msg, isError: true);
            piece.MarkDroppedValid();
            piece.SnapBack();
        }
    }

    private void ShowFeedback(string msg, bool isError)
    {
        if (feedbackText == null) return;

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(FeedbackCoroutine(msg));
    }

    private IEnumerator FeedbackCoroutine(string msg)
    {
        feedbackText.text = msg;
        yield return new WaitForSeconds(feedbackSeconds);
        feedbackText.text = "";
    }
}