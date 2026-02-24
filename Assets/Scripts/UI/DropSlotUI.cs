using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlotUI : MonoBehaviour, IDropHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Index")]
    [Tooltip("Índice de este slot (0..5). Debe coincidir con el orden que maneja el Manager.")]
    public int slotIndex = 0;

    [Header("Config")]
    public string acceptedItemId = "";
    public bool onlyOneItem = true;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip placedSfx;

    [Header("Particles")]
    public ParticleSystem confettiFx;
    public bool spawnAtSlotCenter = true;

    [Header("Hold-to-Undo")]
    [Tooltip("Segundos que debes mantener presionado para deshacer (undo).")]
    public float holdSeconds = 3f;

    private bool occupied = false;
    private DraggableUI currentItem;

    private Coroutine holdRoutine;
    private bool isHolding = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (occupied && onlyOneItem) return;

        var dragged = eventData.pointerDrag;
        if (dragged == null) return;

        var draggable = dragged.GetComponent<DraggableUI>();
        if (draggable == null || draggable.isPlaced) return;

        // Validación opcional por ID/nombre
        if (!string.IsNullOrEmpty(acceptedItemId) && dragged.name != acceptedItemId)
            return;

        // Snap
        draggable.SnapTo(transform, lockInPlace: true);

        currentItem = draggable;
        if (onlyOneItem) occupied = true;

        // Avisar al manager
        if (Module1ActivityManager.Instance != null)
        {
            Module1ActivityManager.Instance.RegisterPlacement(slotIndex, dragged.name);
        }
        else
        {
            Debug.LogWarning("DropSlotUI: No hay Module1ActivityManager.Instance en la escena.");
        }

        // Sonido
        if (audioSource != null && placedSfx != null)
            audioSource.PlayOneShot(placedSfx);

        // Confetti
        if (confettiFx != null)
        {
            if (spawnAtSlotCenter)
                confettiFx.transform.position = transform.position;

            confettiFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            confettiFx.Play();
        }
    }

    // ? LONG PRESS START
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!occupied || currentItem == null) return;

        // iniciar hold
        CancelHold(); // por si quedaba uno colgado
        isHolding = true;
        holdRoutine = StartCoroutine(HoldToUndoRoutine());
    }

    // ? LONG PRESS CANCEL
    public void OnPointerUp(PointerEventData eventData) => CancelHold();
    public void OnPointerExit(PointerEventData eventData) => CancelHold();

    private IEnumerator HoldToUndoRoutine()
    {
        float t = 0f;

        // Asegura que arranque en 1
        currentItem.SetAlpha(1f);

        while (t < holdSeconds)
        {
            if (!isHolding || currentItem == null)
                yield break;

            t += Time.unscaledDeltaTime; // UI: mejor con unscaled por si usas Time.timeScale
            float a = Mathf.Lerp(1f, 0f, t / holdSeconds);
            currentItem.SetAlpha(a);

            yield return null;
        }

        // Completó los 3s -> UNDO
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

        // Si no llegó a 3s, vuelve a 100% opacidad
        if (currentItem != null)
            currentItem.SetAlpha(1f);
    }

    private void DoUndo()
    {
        // detener rutina
        isHolding = false;
        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }

        if (currentItem != null)
        {
            currentItem.ReturnToStart();
            currentItem = null;
        }

        occupied = false;

        // Avisar al manager que se vació el slot (ajusta según tu manager)
        if (Module1ActivityManager.Instance != null)
        {
            // Opción simple: guardar vacío
            Module1ActivityManager.Instance.RegisterPlacement(slotIndex, "");
        }
    }

    public void ResetSlot()
    {
        CancelHold();
        occupied = false;
        currentItem = null;
    }
}