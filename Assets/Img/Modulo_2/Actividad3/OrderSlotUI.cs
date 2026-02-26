// ===========================
// OrderSlotUI.cs
// - Muestra icono del producto
// - Se marca como entregado visualmente
// ===========================

using UnityEngine;
using UnityEngine.UI;

public class OrderSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;       // icono del producto
    [SerializeField] private Image deliveredOverlay; // overlay check (opcional)
    [SerializeField] private CanvasGroup canvasGroup; // opcional para opacidad

    public string ProductId { get; private set; }
    public bool Delivered { get; private set; }

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        ResetSlot();
    }

    public void SetProduct(string productId, Sprite icon)
    {
        ProductId = productId;
        Delivered = false;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = (icon != null);
        }

        if (deliveredOverlay != null)
            deliveredOverlay.enabled = false;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    public void MarkDelivered()
    {
        Delivered = true;

        if (deliveredOverlay != null)
            deliveredOverlay.enabled = true;

        if (canvasGroup != null)
            canvasGroup.alpha = 0.6f; // “relleno/entregado”
    }

    public void ResetSlot()
    {
        ProductId = "";
        Delivered = false;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (deliveredOverlay != null)
            deliveredOverlay.enabled = false;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
}