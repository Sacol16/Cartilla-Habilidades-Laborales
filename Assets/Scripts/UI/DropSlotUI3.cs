using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlotUI3 : MonoBehaviour, IDropHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Index")]
    public int slotIndex = 0; // 0..4

    [Header("Config")]
    public bool onlyOneItem = true;

    [Header("Placed Visual")]
    [Range(0.4f, 1f)]
    public float placedScale = 0.75f;

    [Header("Manager")]
    public Module3ActivityManager manager;

    [Header("Hold-to-Undo")]
    public float holdSeconds = 3f;

    private bool occupied = false;
    private DraggableUI3 currentItem;

    private Coroutine holdRoutine;
    private bool isHolding = false;

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag;
        if (dragged == null) return;

        var draggable = dragged.GetComponent<DraggableUI3>();
        var data = dragged.GetComponent<ValueUIData>();

        if (draggable == null || data == null) return;
        if (draggable.isPlaced) return;

        // reemplazo
        if (occupied && onlyOneItem && currentItem != null)
        {
            var oldData = currentItem.GetComponent<ValueUIData>();
            if (manager != null && oldData != null)
                manager.RegisterActivity2Removal(slotIndex, oldData.id, oldData.isGood);

            currentItem.ReturnToStart();
            currentItem = null;
            occupied = false;
        }

        // snap + lock
        draggable.SnapTo(transform, lockInPlace: true, placedScale: placedScale);

        currentItem = draggable;
        occupied = true;

        if (manager != null)
            manager.RegisterActivity2Placement(slotIndex, data.id, data.isGood);
        else
            Debug.LogWarning("[DropSlotUI3] manager no asignado (Module3ActivityManager).");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!occupied || currentItem == null) return;

        CancelHold();
        isHolding = true;
        holdRoutine = StartCoroutine(HoldToUndoRoutine());
    }

    public void OnPointerUp(PointerEventData eventData) => CancelHold();
    public void OnPointerExit(PointerEventData eventData) => CancelHold();

    private IEnumerator HoldToUndoRoutine()
    {
        float t = 0f;
        currentItem.SetAlpha(1f);

        while (t < holdSeconds)
        {
            if (!isHolding || currentItem == null) yield break;

            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(1f, 0f, t / holdSeconds);
            currentItem.SetAlpha(a);

            yield return null;
        }

        DoUndo();
    }

    private void CancelHold()
    {
        if (!isHolding && holdRoutine == null) return;

        isHolding = false;

        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }

        if (currentItem != null)
            currentItem.SetAlpha(1f);
    }

    private void DoUndo()
    {
        isHolding = false;

        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }

        if (currentItem != null)
        {
            var data = currentItem.GetComponent<ValueUIData>();
            if (manager != null && data != null)
                manager.RegisterActivity2Removal(slotIndex, data.id, data.isGood);

            currentItem.ReturnToStart();
            currentItem = null;
        }

        occupied = false;
    }

    public void ResetSlot()
    {
        CancelHold();
        occupied = false;
        currentItem = null;
    }
}