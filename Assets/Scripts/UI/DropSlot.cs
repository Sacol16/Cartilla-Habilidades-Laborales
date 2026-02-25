using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropSlot : MonoBehaviour, IDropHandler
{
    [Header("Config")]
    public int slotIndex = 0;
    public CargoType acceptsCargo;

    [Tooltip("Usa 'EMPLOYEES' para los slots de empleados. Vacío para slots únicos.")]
    public string groupId = "";

    public MinigameOrgManager manager;

    [Header("Visual (opcional)")]
    public Image slotFrame;

    [Header("State (read-only)")]
    [SerializeField] private bool isOccupied = false;

    private RectTransform rect;
    public RectTransform Rect => rect;
    public bool IsOccupied => isOccupied;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (slotFrame == null) slotFrame = GetComponent<Image>();
    }

    public void SetOccupied(bool value) => isOccupied = value;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        var piece = eventData.pointerDrag.GetComponent<PieceDragUI>();
        if (piece == null) return;

        if (manager == null)
        {
            piece.ReturnToStart();
            return;
        }

        manager.HandleDrop(piece, this);
    }
}