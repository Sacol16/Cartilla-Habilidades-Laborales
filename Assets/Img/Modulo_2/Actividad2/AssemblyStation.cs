using UnityEngine;
using UnityEngine.UI;

public class AssemblyStation : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image assembledImage;

    [Header("Order (3 pieces)")]
    [SerializeField] private string[] requiredPieceIds = new string[3];

    [Header("Progress Sprites (0..3)")]
    [SerializeField] private Sprite[] progressSprites = new Sprite[4];

    [Header("Feedback Messages")]
    [SerializeField] private string wrongPieceMessage = "Esa pieza no pertenece a este objeto.";
    [SerializeField] private string wrongOrderMessage = "Orden incorrecto. Intenta primero la siguiente pieza.";

    public bool IsComplete { get; private set; }
    public int CurrentStep { get; private set; } = 0;

    public System.Action<AssemblyStation> OnCompleted;

    private void Start()
    {
        RefreshVisual();
    }

    // NUEVO: devuelve ok y un mensaje si falla
    public bool TryPlacePiece(string pieceId, out string feedback)
    {
        feedback = "";

        if (IsComplete)
        {
            feedback = "Esta mesa ya está completa.";
            return false;
        }

        if (requiredPieceIds == null || requiredPieceIds.Length < 3)
        {
            feedback = "Configuración inválida de la estación.";
            return false;
        }

        // Si la pieza correcta para este paso NO coincide:
        string expected = requiredPieceIds[CurrentStep];

        if (pieceId != expected)
        {
            // Diferenciar: ¿es una pieza de este objeto pero en otro orden?
            bool belongsToThisObject = false;
            for (int i = 0; i < requiredPieceIds.Length; i++)
            {
                if (requiredPieceIds[i] == pieceId)
                {
                    belongsToThisObject = true;
                    break;
                }
            }

            feedback = belongsToThisObject ? wrongOrderMessage : wrongPieceMessage;
            return false;
        }

        // Correcto
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