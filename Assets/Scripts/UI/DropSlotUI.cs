using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropSlotUI : MonoBehaviour, IDropHandler
{
    [Header("Config")]
    public string acceptedItemId = ""; // opcional: filtrar qué item entra aquí (ej: "Item_1")
    public bool onlyOneItem = true;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip placedSfx;
    public Animator slotAnimator; // opcional (si tienes anim)
    public string animTriggerName = "Placed";

    private bool occupied = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (occupied && onlyOneItem) return;

        var dragged = eventData.pointerDrag;
        if (dragged == null) return;

        var draggable = dragged.GetComponent<DraggableUI>();
        if (draggable == null || draggable.isPlaced) return;

        // Si quieres que cada slot acepte un item específico
        if (!string.IsNullOrEmpty(acceptedItemId) && dragged.name != acceptedItemId)
            return;

        // Snap
        draggable.SnapTo(transform, lockInPlace: true);

        // Marca ocupado
        if (onlyOneItem) occupied = true;

        // Sonido
        if (audioSource != null && placedSfx != null)
            audioSource.PlayOneShot(placedSfx);

        // Mini animación
        if (slotAnimator != null && !string.IsNullOrEmpty(animTriggerName))
            slotAnimator.SetTrigger(animTriggerName);
        else
            StartCoroutine(PunchScale(dragged.transform));
    }

    System.Collections.IEnumerator PunchScale(Transform t)
    {
        // animación simple sin Animator: “pop”
        Vector3 baseScale = t.localScale;
        t.localScale = baseScale * 1.12f;
        yield return new WaitForSeconds(0.08f);
        t.localScale = baseScale;
    }
}
