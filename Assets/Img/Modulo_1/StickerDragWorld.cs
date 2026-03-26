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
    [SerializeField] private bool lockAfterSuccessfulDrop = true;
    [SerializeField] private bool disableRaycastWhenLocked = true;

    // ?? NUEVO: Configuración para mundo
    [Header("World Sticker")]
    [SerializeField] private GameObject worldStickerPrefab;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private float worldZDepth = 5f; // ajusta según tu escena

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

        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
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
                cloneScript.cloneOnDrag = false;
                cloneScript.destroyIfDroppedOutside = false;
                cloneScript.uiDrawArea = uiDrawArea;
                cloneScript.canvas = canvas;
                cloneScript.lockAfterSuccessfulDrop = lockAfterSuccessfulDrop;
                cloneScript.disableRaycastWhenLocked = disableRaycastWhenLocked;

                // ?? pasar config mundo al clon
                cloneScript.worldStickerPrefab = worldStickerPrefab;
                cloneScript.worldCamera = worldCamera;
                cloneScript.worldZDepth = worldZDepth;
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
            // ?? Crear versión en el mundo (VISIBLE EN CAPTURA)
            // ?? Crear versión en el mundo (VISIBLE EN CAPTURA)
            CreateWorldSticker(eventData.position);

            // Undo (opcional: ahora sobre el world sticker si quieres luego lo ajustamos)
            if (Linea.Instance != null && draggedObj != null)
            {
                Linea.Instance.RegisterUndo(draggedObj);
            }

            // ?? ELIMINAR EL STICKER DE UI
            if (draggedObj != null)
            {
                Destroy(draggedObj);
            }
        }
        else
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

    // ?? FUNCIÓN CLAVE
    private void CreateWorldSticker(Vector2 screenPosition)
    {
        if (worldStickerPrefab == null || worldCamera == null) return;

        Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, worldZDepth);
        Vector3 worldPos = worldCamera.ScreenToWorldPoint(screenPoint);

        GameObject sticker = Instantiate(worldStickerPrefab, worldPos, Quaternion.identity);

        // Copiar sprite desde UI ? mundo
        var uiImage = draggedObj.GetComponent<Image>();
        var spriteRenderer = sticker.GetComponent<SpriteRenderer>();

        if (uiImage != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = uiImage.sprite;
        }
    }
}