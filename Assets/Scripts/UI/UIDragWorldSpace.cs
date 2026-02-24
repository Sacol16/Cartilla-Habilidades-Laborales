using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragWorldSpace : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Canvas canvas; // asigna el Canvas padre (World Space)
    private RectTransform rect;
    private RectTransform canvasRect;

    private Vector2 pointerOffset; // offset para que no salte al centro

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Convertir mouse a local del canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,   // CLAVE en World Space
            out var localPointerPos
        );

        pointerOffset = rect.anchoredPosition - localPointerPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out var localPointerPos
        );

        rect.anchoredPosition = localPointerPos + pointerOffset;
    }

    public void OnEndDrag(PointerEventData eventData) { }
}