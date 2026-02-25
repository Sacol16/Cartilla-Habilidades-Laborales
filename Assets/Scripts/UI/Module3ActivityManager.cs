using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Module3ActivityManager : MonoBehaviour
{
    public static Module3ActivityManager Instance { get; private set; }

    [Header("Slots (arrástralos todos aquí)")]
    [SerializeField] private DropSlot[] slots;

    [Header("Regla de empleados")]
    [SerializeField] private string employeesGroupId = "EMPLOYEES";
    [SerializeField] private int requiredEmployees = 4;

    [Header("Botón continuar")]
    [SerializeField] private Button continueButton1;
    [SerializeField] private bool hideButtonInstead = false;

    // Guardamos SOLO los slots no-empleado por slotIndex (DG/DF/DP)
    private readonly HashSet<int> requiredUniqueSlotIndices = new HashSet<int>();
    private readonly Dictionary<int, string> placedUniqueBySlotIndex = new Dictionary<int, string>();

    // Guardamos slots de empleados
    private readonly List<DropSlot> employeeSlots = new List<DropSlot>();

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
        requiredUniqueSlotIndices.Clear();
        placedUniqueBySlotIndex.Clear();
        employeeSlots.Clear();

        if (slots != null)
        {
            foreach (var s in slots)
            {
                if (s == null) continue;

                if (s.groupId == employeesGroupId)
                {
                    employeeSlots.Add(s);
                }
                else
                {
                    requiredUniqueSlotIndices.Add(s.slotIndex);
                    placedUniqueBySlotIndex[s.slotIndex] = ""; // vacío al inicio
                }
            }
        }

        // ✅ Botón desactivado por defecto al iniciar
        SetContinueEnabled(false);

        Debug.Log($"[Module3ActivityManager] Unique required={requiredUniqueSlotIndices.Count} | employeeSlots={employeeSlots.Count}");

        // ✅ Revisión inicial por si la escena carga con slots ya ocupados
        // (si no aplica, igual no molesta)
        ForceRecheck();
    }

    /// <summary>
    /// Esto lo llama MinigameOrgManager en cada acierto.
    /// </summary>
    public void RegisterPlacement(int slotIndex, string itemValue)
    {
        // Si este slotIndex es de un slot único, lo registramos
        if (requiredUniqueSlotIndices.Contains(slotIndex))
        {
            placedUniqueBySlotIndex[slotIndex] = itemValue ?? "";
        }

        bool complete = IsActivityComplete();
        SetContinueEnabled(complete);

        Debug.Log($"[Module3ActivityManager] Registro slotIndex={slotIndex} item={itemValue} | complete={complete} | {DumpStatus()}");
    }

    /// <summary>
    /// ✅ Completado = (DG/DF/DP registrados) + (4 empleados ocupando slots del grupo)
    /// </summary>
    public bool IsActivityComplete()
    {
        // 1) Todos los slots únicos llenos (según registro)
        foreach (var idx in requiredUniqueSlotIndices)
        {
            if (!placedUniqueBySlotIndex.ContainsKey(idx)) return false;
            if (string.IsNullOrEmpty(placedUniqueBySlotIndex[idx])) return false;
        }

        // 2) Empleados: cantidad ocupada en el grupo (estado real)
        int occupiedEmployees = 0;
        for (int i = 0; i < employeeSlots.Count; i++)
        {
            var s = employeeSlots[i];
            if (s != null && s.IsOccupied) occupiedEmployees++;
        }

        return occupiedEmployees >= requiredEmployees;
    }

    /// <summary>
    /// ✅ Activa / desactiva el botón según el estado.
    /// </summary>
    private void SetContinueEnabled(bool value)
    {
        if (continueButton1 == null)
        {
            Debug.LogWarning("[Module3ActivityManager] continueButton1 no asignado.");
            return;
        }

        if (hideButtonInstead)
        {
            continueButton1.gameObject.SetActive(value);
        }
        else
        {
            continueButton1.gameObject.SetActive(true);
            continueButton1.interactable = value;

            var img = continueButton1.GetComponent<Image>();
            if (img != null) img.color = value ? Color.white : new Color(1f, 1f, 1f, 0.5f);
        }

        Debug.Log("[Module3ActivityManager] Botón interactable = " +
                  (continueButton1 != null ? continueButton1.interactable.ToString() : "null"));
    }

    /// <summary>
    /// ✅ Útil si quieres forzar re-cálculo (por ejemplo al cargar escena o si cambias algo sin registrar).
    /// </summary>
    public void ForceRecheck()
    {
        bool complete = IsActivityComplete();
        SetContinueEnabled(complete);
    }

    private string DumpStatus()
    {
        var parts = new List<string>();
        foreach (var idx in requiredUniqueSlotIndices)
        {
            placedUniqueBySlotIndex.TryGetValue(idx, out var val);
            parts.Add($"{idx}:{val}");
        }

        int occupiedEmployees = 0;
        for (int i = 0; i < employeeSlots.Count; i++)
            if (employeeSlots[i] != null && employeeSlots[i].IsOccupied) occupiedEmployees++;

        return $"Unique[{string.Join(" | ", parts)}] EmployeesOccupied={occupiedEmployees}/{requiredEmployees}";
    }

    // ✅ Para el submitter
    public Dictionary<int, string> GetActivity1PlacementsCopy()
    {
        var result = new Dictionary<int, string>();

        foreach (var kv in placedUniqueBySlotIndex)
            result[kv.Key] = kv.Value ?? "";

        foreach (var s in employeeSlots)
        {
            if (s == null) continue;
            result[s.slotIndex] = s.IsOccupied ? "Empleado" : "";
        }

        return result;
    }
}