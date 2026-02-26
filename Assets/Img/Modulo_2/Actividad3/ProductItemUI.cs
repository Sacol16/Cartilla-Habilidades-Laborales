// ===========================
// ProductItemUI.cs
// - Arrastrable con ID de producto
// ===========================

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ProductItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Product")]
    public string productId; // "pan", "camiseta", "cuaderno"

    [Header("Refs")]
    [SerializeField] private Canvas canvas;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalAnchoredPos;

    public bool DroppedOnValidZone { get; private set; }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DroppedOnValidZone = false;
        originalParent = rect.parent;
        originalAnchoredPos = rect.anchoredPosition;

        canvasGroup.blocksRaycasts = false; // clave para drop
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
        canvasGroup.blocksRaycasts = true;

        if (!DroppedOnValidZone)
            SnapBack();
    }

    public void MarkDroppedValid() => DroppedOnValidZone = true;

    public void SnapBack()
    {
        DroppedOnValidZone = true;
        rect.SetParent(originalParent, true);
        rect.anchoredPosition = originalAnchoredPos;
    }
}