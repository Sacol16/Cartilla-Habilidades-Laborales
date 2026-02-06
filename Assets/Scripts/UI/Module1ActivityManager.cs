using System.Collections.Generic;
using UnityEngine;

public class Module1ActivityManager : MonoBehaviour
{
    public static Module1ActivityManager Instance { get; private set; }

    [Header("Slots")]
    [Tooltip("Asigna aquí los 6 slots (DropSlotUI) en el orden que quieras guardar.")]
    [SerializeField] private DropSlotUI[] slots;

    [Header("UI")]
    [Tooltip("Botón o GameObject que se activa cuando todos los slots estén llenos.")]
    [SerializeField] private GameObject continueButton;

    // Guarda: slotIndex -> itemName
    private readonly Dictionary<int, string> placedBySlot = new Dictionary<int, string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (continueButton != null)
            continueButton.SetActive(false);

        // Inicializa el registro vacío
        placedBySlot.Clear();
        for (int i = 0; i < slots.Length; i++)
            placedBySlot[i] = "";
    }

    /// <summary>
    /// Llamado por un slot cuando se coloca un item exitosamente.
    /// </summary>
    public void RegisterPlacement(int slotIndex, string itemObjectName)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return;

        placedBySlot[slotIndex] = itemObjectName;

        // Si están todos llenos, activa botón
        if (AreAllSlotsFilled())
        {
            if (continueButton != null)
                continueButton.SetActive(true);

            Debug.Log("? Todos los slots llenos. Registros: " + DumpPlacements());
        }
    }

    public bool AreAllSlotsFilled()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (string.IsNullOrEmpty(placedBySlot[i]))
                return false;
        }
        return true;
    }

    public string GetItemInSlot(int slotIndex)
    {
        return placedBySlot.TryGetValue(slotIndex, out var val) ? val : "";
    }

    public string DumpPlacements()
    {
        // Para debug: "0:Hogar | 1:Comida | ..."
        List<string> parts = new List<string>();
        for (int i = 0; i < slots.Length; i++)
            parts.Add($"{i}:{placedBySlot[i]}");
        return string.Join(" | ", parts);
    }
}
