// ===========================
// DraggableUI.cs
// - Draggable UI con ID (name/id) para validación del slot
// - SnapBack a su posición original si cae mal
// - Compatible con DropSlotUI (bloquea raycasts mientras arrastras)
// ===========================

using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DraggableUI2 : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("ID")]
    [Tooltip("Debe coincidir con el acceptedId del slot. Ej: 'pan', 'pieza1', 'RobotHead'.")]
    public string draggableId;

    [Header("Refs")]
    [SerializeField] private Canvas canvas;

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Vector2 originalAnchoredPos;
    private Vector3 originalLocalScale;
    private Quaternion originalLocalRot;

    private bool droppedOnValidZone;

    private DropSlotUI2 currentSlot; // slot donde quedó colocado (si aplica)

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        droppedOnValidZone = false;

        // Si estaba en un slot, lo liberamos al empezar a arrastrar
        if (currentSlot != null)
        {
            currentSlot.ClearIfCurrent(this);
            currentSlot = null;
        }

        originalParent = rect.parent;
        originalAnchoredPos = rect.anchoredPosition;
        originalLocalScale = rect.localScale;
        originalLocalRot = rect.localRotation;

        // Mientras arrastras, no bloquea raycasts para que el slot reciba el drop
        canvasGroup.blocksRaycasts = false;

        // Traer al frente
        rect.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        RectTransform canvasRect = canvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            rect.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Volver a bloquear raycasts
        canvasGroup.blocksRaycasts = true;

        if (!droppedOnValidZone)
            SnapBack();
    }

    public void MarkDroppedValid()
    {
        droppedOnValidZone = true;
    }

    public void SetPlacedInSlot(DropSlotUI2 slot)
    {
        currentSlot = slot;
    }

    public void SnapBack()
    {
        droppedOnValidZone = true;

        rect.SetParent(originalParent, true);
        rect.anchoredPosition = originalAnchoredPos;
        rect.localScale = originalLocalScale;
        rect.localRotation = originalLocalRot;
    }
}