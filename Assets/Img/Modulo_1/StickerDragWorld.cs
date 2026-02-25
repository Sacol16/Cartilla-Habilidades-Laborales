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

    [Header("Lock after drop")]
    [Tooltip("Si es true, al soltar dentro del área el sticker queda fijo y no se puede mover más.")]
    [SerializeField] private bool lockAfterSuccessfulDrop = true;

    [Tooltip("Si es true, desactiva raycast del Image al quedar fijo (para que no bloquee clicks).")]
    [SerializeField] private bool disableRaycastWhenLocked = true;

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
        // Si este sticker ya quedó fijo, no permitir drag
        if (!enabled) return;

        originalPos = rect.anchoredPosition;
        originalParent = transform.parent;

        if (cloneOnDrag)
        {
            draggedObj = Instantiate(gameObject, canvas.transform);
            draggedObj.name = gameObject.name + "_Placed";

            var cloneScript = draggedObj.GetComponent<StickerDragUI>();
            if (cloneScript != null)
            {
                // El clon NO debe generar más clones
                cloneScript.cloneOnDrag = false;
                cloneScript.destroyIfDroppedOutside = false;
                cloneScript.uiDrawArea = uiDrawArea;
                cloneScript.canvas = canvas;
                cloneScript.lockAfterSuccessfulDrop = lockAfterSuccessfulDrop;
                cloneScript.disableRaycastWhenLocked = disableRaycastWhenLocked;
            }

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

        // Que se vea por encima mientras arrastras
        draggedRect.SetAsLastSibling();
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

        if (inside)
        {
            // ? Registrar Undo cuando se coloca correctamente
            if (Linea.Instance != null && draggedObj != null)
            {
                Linea.Instance.RegisterUndo(draggedObj);
            }

            // ? Bloquear para que no se pueda mover después de colocarlo
            if (lockAfterSuccessfulDrop && draggedObj != null)
            {
                var dragScript = draggedObj.GetComponent<StickerDragUI>();
                if (dragScript != null)
                    dragScript.enabled = false;

                if (disableRaycastWhenLocked)
                {
                    var img = draggedObj.GetComponent<Image>();
                    if (img != null) img.raycastTarget = false;
                }
            }
        }
        else
        {
            // Fuera del área: mismo comportamiento que ya tenías
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