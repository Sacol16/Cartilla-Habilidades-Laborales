using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("State")]
    public bool isPlaced = false;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Transform originalParent;
    private Vector2 originalAnchoredPos;

    // ? “posición de inicio” real (la primera al cargar la escena)
    private Transform startParent;
    private Vector2 startAnchoredPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) Debug.LogError("DraggableUI: No encuentro un Canvas padre.");

        // Guardar posición inicial (la que quieres para “volver a inicio”)
        startParent = rect.parent;
        startAnchoredPos = rect.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        originalParent = rect.parent;
        originalAnchoredPos = rect.anchoredPosition;

        // Para que el slot reciba el drop
        canvasGroup.blocksRaycasts = false;

        // Opcional: traer al frente mientras arrastras
        rect.SetParent(rootCanvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        // Mover con el mouse/touch (compensa escala del canvas)
        rect.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        // Restauro raycasts siempre al final del drag
        canvasGroup.blocksRaycasts = true;

        // Solo vuelve al origen si todavía está en el "drag layer"
        if (rect.parent == rootCanvas.transform)
        {
            rect.SetParent(originalParent, true);
            rect.anchoredPosition = originalAnchoredPos;
        }
    }

    // Lo llama el slot cuando acepta el item
    public void SnapTo(Transform slot, bool lockInPlace = true)
    {
        rect.SetParent(slot, false);
        rect.anchoredPosition = Vector2.zero;

        if (lockInPlace)
        {
            isPlaced = true;
            canvasGroup.blocksRaycasts = false; // ya no se vuelve a arrastrar (por eso el slot detecta el hold)
        }
        else
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    // ? Para el “undo”: volver a donde arrancó el objeto
    public void ReturnToStart()
    {
        isPlaced = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        rect.SetParent(startParent, true);
        rect.anchoredPosition = startAnchoredPos;
    }

    // ? Utilidad para el fade del “borrado”
    public void SetAlpha(float a)
    {
        canvasGroup.alpha = a;
    }
}