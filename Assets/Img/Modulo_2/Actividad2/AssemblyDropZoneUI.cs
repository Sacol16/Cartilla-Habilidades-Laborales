// ===========================
// AssemblyDropZoneUI.cs
// - Zona UI donde sueltas piezas
// - Si es correcto: consume la pieza
// - Si es incorrecto: la devuelve a la cinta (snap back)
// ===========================

using UnityEngine;
using UnityEngine.EventSystems;

public class AssemblyDropZoneUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private AssemblyStation station;

    public void OnDrop(PointerEventData eventData)
    {
        if (station == null) return;

        var dragged = eventData.pointerDrag;
        if (dragged == null) return;

        var piece = dragged.GetComponent<ConveyorPieceDraggableUI>();
        if (piece == null) return;

        bool ok = station.TryPlacePiece(piece.pieceId);

        if (ok)
        {
            piece.Consume(); // se destruye o se desactiva
        }
        else
        {
            piece.SnapBack();
        }
    }
}