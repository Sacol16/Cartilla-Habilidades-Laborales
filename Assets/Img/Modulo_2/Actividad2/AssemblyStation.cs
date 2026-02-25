// ===========================
// AssemblyStation.cs
// - Define el orden de 3 piezas
// - Actualiza sprite por progreso (0,1,2,3)
// ===========================

using UnityEngine;
using UnityEngine.UI;

public class AssemblyStation : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image assembledImage; // Imagen del objeto armado

    [Header("Order (3 pieces)")]
    [Tooltip("IDs de las piezas en orden correcto. Ej: RobotHead, RobotBody, RobotLegs")]
    [SerializeField] private string[] requiredPieceIds = new string[3];

    [Header("Progress Sprites (0..3)")]
    [Tooltip("sprites[0]=vacío, [1]=paso1, [2]=paso2, [3]=completo")]
    [SerializeField] private Sprite[] progressSprites = new Sprite[4];

    public bool IsComplete { get; private set; }
    public int CurrentStep { get; private set; } = 0;

    public System.Action<AssemblyStation> OnCompleted;

    private void Start()
    {
        RefreshVisual();
    }

    public bool TryPlacePiece(string pieceId)
    {
        if (IsComplete) return false;
        if (requiredPieceIds == null || requiredPieceIds.Length < 3) return false;

        // Validar orden
        if (pieceId != requiredPieceIds[CurrentStep])
            return false;

        CurrentStep++;

        if (CurrentStep >= 3)
        {
            IsComplete = true;
            CurrentStep = 3;
            RefreshVisual();
            OnCompleted?.Invoke(this);
        }
        else
        {
            RefreshVisual();
        }

        return true;
    }

    private void RefreshVisual()
    {
        if (assembledImage == null) return;

        int idx = Mathf.Clamp(CurrentStep, 0, 3);
        if (progressSprites != null && progressSprites.Length > idx && progressSprites[idx] != null)
        {
            assembledImage.sprite = progressSprites[idx];
            assembledImage.enabled = true;
        }
    }
}