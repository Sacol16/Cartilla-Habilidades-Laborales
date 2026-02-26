using UnityEngine;
using UnityEngine.EventSystems;

public class CustomerDropZoneUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private CustomerServiceGameManager manager;

    public void OnDrop(PointerEventData eventData)
    {
        if (manager == null) return;

        var dragged = eventData.pointerDrag;
        if (dragged == null) return;

        var item = dragged.GetComponent<ProductItemUI>();
        if (item == null) return;

        bool accepted = manager.TryDeliverProduct(item.productId);

        item.MarkDroppedValid();

        if (!accepted)
            item.SnapBack();
        else
            item.SnapBack(); // o Destroy(item.gameObject) si quieres que se consuma
    }
}