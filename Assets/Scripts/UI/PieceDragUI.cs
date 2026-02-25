using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PieceDragUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CargoType cargo;

    [Header("Auto")]
    [SerializeField] private Canvas canvas;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Image image;

    private Vector2 startAnchoredPos;
    private Transform startParent;

    public RectTransform Rect => rect;
    public Image Image => image;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        image = GetComponent<Image>();

        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
    }

    private void Start() => SaveStart();

    public void SaveStart()
    {
        startAnchoredPos = rect.anchoredPosition;
        startParent = rect.parent;
    }

    public void ReturnToStart()
    {
        rect.SetParent(startParent, false);
        rect.anchoredPosition = startAnchoredPos;
        rect.SetAsLastSibling();
    }

    public void SnapToSlot(RectTransform slot)
    {
        rect.SetParent(slot, false);
        rect.anchoredPosition = Vector2.zero;
        rect.SetAsLastSibling();
    }

    public void LockInPlace()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        enabled = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ✅ clave para que el slot reciba OnDrop
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.9f;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // ✅ Si NO soltó sobre un DropSlot (o hijo), vuelve
        GameObject hit = eventData.pointerCurrentRaycast.gameObject;
        bool droppedOnSlot = hit != null && hit.GetComponentInParent<DropSlot>() != null;

        if (!droppedOnSlot)
            ReturnToStart();
        // Si sí es slot, el slot/manager acepta o rechaza.
    }
}