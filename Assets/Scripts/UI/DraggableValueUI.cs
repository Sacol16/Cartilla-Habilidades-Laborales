using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableUI3 : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("State")]
    public bool isPlaced = false;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Transform originalParent;
    private Vector2 originalAnchoredPos;

    // posición real de inicio (para volver abajo)
    private Transform startParent;
    private Vector2 startAnchoredPos;
    private Vector3 startScale;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) Debug.LogError("DraggableUI: No encuentro un Canvas padre.");

        startParent = rect.parent;
        startAnchoredPos = rect.anchoredPosition;
        startScale = rect.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        originalParent = rect.parent;
        originalAnchoredPos = rect.anchoredPosition;

        canvasGroup.blocksRaycasts = false;

        // traer al frente mientras arrastras
        rect.SetParent(rootCanvas.transform, true);
        rect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        rect.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        canvasGroup.blocksRaycasts = true;

        // si no se dropeó en slot, vuelve a donde estaba
        if (rect.parent == rootCanvas.transform)
        {
            rect.SetParent(originalParent, true);
            rect.anchoredPosition = originalAnchoredPos;
        }
    }

    // el slot llama esto cuando acepta el item
    public void SnapTo(Transform slot, bool lockInPlace = true, float placedScale = 0.75f)
    {
        rect.SetParent(slot, false);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one * placedScale; // 👈 más pequeño en slot

        if (lockInPlace)
        {
            isPlaced = true;
            canvasGroup.blocksRaycasts = false; // para que no se arrastre otra vez
        }
        else
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void ReturnToStart()
    {
        isPlaced = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        rect.SetParent(startParent, true);
        rect.anchoredPosition = startAnchoredPos;
        rect.localScale = startScale; // 👈 vuelve al tamaño original abajo
    }

    public void SetAlpha(float a)
    {
        canvasGroup.alpha = a;
    }
}