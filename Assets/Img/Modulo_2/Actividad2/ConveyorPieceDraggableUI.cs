// ===========================
// ConveyorPieceDraggableUI.cs
// - Drag & drop de una pieza
// - SnapBack a su parent/pos original si no sirve
// - Consume() si se colocó bien
// ===========================

using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ConveyorPieceDraggableUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Piece")]
    public string pieceId; // Ej: "RobotHead", "CarWheel", etc.

    [Header("Refs")]
    [SerializeField] private Canvas canvas;

    private RectTransform rect;
    private Transform originalParent;
    private Vector2 originalAnchoredPos;
    private bool droppedOnValidZone;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        droppedOnValidZone = false;

        originalParent = rect.parent;
        originalAnchoredPos = rect.anchoredPosition;

        // Traer al frente (para que quede sobre todo)
        rect.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Si no fue consumida por una zona válida -> vuelve
        if (!droppedOnValidZone)
            SnapBack();
    }

    // Llamado por drop zone si fue correcto
    public void Consume()
    {
        droppedOnValidZone = true;
        Destroy(gameObject);
    }

    public void SnapBack()
    {
        droppedOnValidZone = true; // para que OnEndDrag no lo re-procese
        rect.SetParent(originalParent, true);
        rect.anchoredPosition = originalAnchoredPos;
    }
}