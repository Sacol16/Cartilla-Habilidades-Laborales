using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StickerDragUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drop area (UI)")]
    [SerializeField] private RectTransform uiDrawArea;
    [SerializeField] private Canvas canvas;

    [Header("Behavior")]
    [SerializeField] private bool cloneOnDrag = true;
    [SerializeField] private bool destroyIfDroppedOutside = true;

    private RectTransform rect;
    private RectTransform canvasRect;

    private GameObject draggedObj;
    private RectTransform draggedRect;

    private Vector2 pointerOffset;
    private Vector2 originalPos;
    private Transform originalParent;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        canvasRect = canvas.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPos = rect.anchoredPosition;
        originalParent = transform.parent;

        if (cloneOnDrag)
        {
            draggedObj = Instantiate(gameObject, canvas.transform);
            draggedObj.name = gameObject.name + "_Placed";

            var cloneScript = draggedObj.GetComponent<StickerDragUI>();
            cloneScript.cloneOnDrag = false;
            cloneScript.destroyIfDroppedOutside = false;
            cloneScript.uiDrawArea = uiDrawArea;
            cloneScript.canvas = canvas;

            draggedRect = draggedObj.GetComponent<RectTransform>();
        }
        else
        {
            draggedObj = gameObject;
            draggedRect = rect;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out var localPointerPos
        );

        pointerOffset = draggedRect.anchoredPosition - localPointerPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedRect == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out var localPointerPos
        );

        draggedRect.anchoredPosition = localPointerPos + pointerOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedRect == null || uiDrawArea == null) return;

        bool inside = RectTransformUtility.RectangleContainsScreenPoint(
            uiDrawArea,
            eventData.position,
            eventData.pressEventCamera
        );

        if (!inside)
        {
            if (cloneOnDrag)
            {
                Destroy(draggedObj);
            }
            else
            {
                if (destroyIfDroppedOutside)
                {
                    Destroy(draggedObj);
                }
                else
                {
                    draggedRect.anchoredPosition = originalPos;
                    draggedRect.SetParent(originalParent, true);
                }
            }
        }

        draggedObj = null;
        draggedRect = null;
    }
}