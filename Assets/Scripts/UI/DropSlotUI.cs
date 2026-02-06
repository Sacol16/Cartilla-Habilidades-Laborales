using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlotUI : MonoBehaviour, IDropHandler
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
    public ParticleSystem confettiFx;          // Asigna aquí el ParticleSystem (o uno global compartido)
    public bool spawnAtSlotCenter = true;      // Si está activo, lo posiciona en el centro del slot (world)

    private bool occupied = false;

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

        if (onlyOneItem) occupied = true;

        // ? Avisar al manager (guardar qué item quedó en este slot)
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

    // (Opcional) Para que el Manager pueda reiniciar el estado del slot si luego haces "Reset"
    public void ResetSlot()
    {
        occupied = false;
    }
}
