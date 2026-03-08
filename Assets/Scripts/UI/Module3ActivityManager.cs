using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Module3ActivityManager : MonoBehaviour, IActivity4Receiver
{
    public static Module3ActivityManager Instance { get; private set; }

    // =========================================================
    // ACTIVITY 1 (Organigrama) - TU LÓGICA ORIGINAL
    // =========================================================
    [Header("ACTIVITY 1 - Slots (arrástralos todos aquí)")]
    [SerializeField] private DropSlot[] slots;

    [Header("ACTIVITY 1 - Regla de empleados")]
    [SerializeField] private string employeesGroupId = "EMPLOYEES";
    [SerializeField] private int requiredEmployees = 4;

    [Header("ACTIVITY 1 - Botón continuar")]
    [SerializeField] private Button continueButton1;
    [SerializeField] private bool hideButton1Instead = false;

    // Guardamos SOLO los slots no-empleado por slotIndex (DG/DF/DP)
    private readonly HashSet<int> requiredUniqueSlotIndices = new HashSet<int>();
    private readonly Dictionary<int, string> placedUniqueBySlotIndex = new Dictionary<int, string>();

    // Guardamos slots de empleados
    private readonly List<DropSlot> employeeSlots = new List<DropSlot>();


    // =========================================================
    // ACTIVITY 2 (Camino de valores)
    // =========================================================
    [Header("ACTIVITY 2 - Character UI")]
    [SerializeField] private Image activity2CharacterImage;

    [Tooltip("11 sprites: 0..4 malos, 5 mid, 6..10 buenos")]
    [SerializeField] private Sprite[] activity2States; // size 11

    [SerializeField] private int activity2MidIndex = 5;

    [Header("ACTIVITY 2 - Slots (los 5 slots de valores)")]
    [Tooltip("Arrastra los 5 DropSlotUI3 aquí. Deben tener slotIndex 0..4.")]
    [SerializeField] private DropSlotUI3[] activity2Slots;

    [Header("ACTIVITY 2 - Win")]
    [SerializeField] private int activity2RequiredGoodToWin = 5;

    [Header("ACTIVITY 2 - Botón continuar")]
    [SerializeField] private Button continueButton2;
    [SerializeField] private bool hideButton2Instead = false;

    private int a2CurrentIndex;
    private bool a2Complete = false;

    // slotIndex -> valueId
    private readonly Dictionary<int, string> a2PlacedIdBySlot = new Dictionary<int, string>();
    // valueId (bueno) colocados
    private readonly HashSet<string> a2GoodPlacedIds = new HashSet<string>();
    // valueId -> isGood (para revertir al quitar)
    private readonly Dictionary<string, bool> a2IsGoodById = new Dictionary<string, bool>();

    // =========================
    // Actividad 4 (Selección + Audio)
    // =========================
    [Header("Actividad 4")]
    [SerializeField] private GameObject continueButton4;

    public string Activity4SelectedOptionId { get; private set; } = "";
    public string Activity4AudioBase64 { get; private set; } = "";
    public byte[] Activity4AudioBytes { get; private set; }

    // Flag para confirmar audio (útil WebGL / timing)
    public bool Activity4HasAudioFlag { get; private set; } = false;


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
        // ======================
        // INIT ACTIVITY 1
        // ======================
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

        SetContinue1Enabled(false);
        ForceRecheckActivity1();

        // ======================
        // INIT ACTIVITY 2
        // ======================
        a2PlacedIdBySlot.Clear();
        a2GoodPlacedIds.Clear();
        a2IsGoodById.Clear();
        a2Complete = false;

        // conectar slots de activity2 a este manager
        if (activity2Slots != null)
        {
            foreach (var s in activity2Slots)
            {
                if (s == null) continue;
                s.manager = this; // ✅ DropSlotUI3 llamará a este manager
            }
        }

        SetContinue2Enabled(false);
        A2SetState(activity2MidIndex);
        ForceRecheckActivity2();
    }

    // =========================================================
    // ACTIVITY 1 API
    // =========================================================
    public void RegisterPlacement(int slotIndex, string itemValue)
    {
        if (requiredUniqueSlotIndices.Contains(slotIndex))
        {
            placedUniqueBySlotIndex[slotIndex] = itemValue ?? "";
        }

        bool complete = IsActivity1Complete();
        SetContinue1Enabled(complete);

        Debug.Log($"[Module3ActivityManager] A1 Registro slotIndex={slotIndex} item={itemValue} | complete={complete}");
    }

    public bool IsActivity1Complete()
    {
        foreach (var idx in requiredUniqueSlotIndices)
        {
            if (!placedUniqueBySlotIndex.ContainsKey(idx)) return false;
            if (string.IsNullOrEmpty(placedUniqueBySlotIndex[idx])) return false;
        }

        int occupiedEmployees = 0;
        for (int i = 0; i < employeeSlots.Count; i++)
        {
            var s = employeeSlots[i];
            if (s != null && s.IsOccupied) occupiedEmployees++;
        }

        return occupiedEmployees >= requiredEmployees;
    }

    private void SetContinue1Enabled(bool value)
    {
        if (continueButton1 == null)
        {
            Debug.LogWarning("[Module3ActivityManager] continueButton1 no asignado.");
            return;
        }

        if (hideButton1Instead)
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
    }

    public void ForceRecheckActivity1()
    {
        bool complete = IsActivity1Complete();
        SetContinue1Enabled(complete);
    }

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

    // =========================================================
    // ACTIVITY 2 API (lo llama DropSlotUI3)
    // =========================================================
    public void RegisterActivity2Placement(int slotIndex, string valueId, bool isGood)
    {
        if (a2Complete) return;
        if (string.IsNullOrEmpty(valueId)) return;

        // si el slot ya tenía algo, removerlo antes (revert)
        if (a2PlacedIdBySlot.TryGetValue(slotIndex, out var oldId) && !string.IsNullOrEmpty(oldId))
        {
            bool oldIsGood = a2IsGoodById.TryGetValue(oldId, out var og) && og;
            RegisterActivity2Removal(slotIndex, oldId, oldIsGood);
        }

        a2PlacedIdBySlot[slotIndex] = valueId;
        a2IsGoodById[valueId] = isGood;

        if (isGood)
        {
            a2GoodPlacedIds.Add(valueId);
            A2Step(+1);
        }
        else
        {
            A2Step(-1);
        }

        a2Complete = IsActivity2Complete();
        SetContinue2Enabled(a2Complete);

        Debug.Log($"[Module3ActivityManager] A2 Place slot={slotIndex} id={valueId} good={isGood} goodCount={a2GoodPlacedIds.Count} state={a2CurrentIndex} complete={a2Complete}");
    }

    public void RegisterActivity2Removal(int slotIndex, string valueId, bool isGood)
    {
        if (string.IsNullOrEmpty(valueId)) return;

        // limpiar slot
        if (a2PlacedIdBySlot.ContainsKey(slotIndex))
            a2PlacedIdBySlot[slotIndex] = "";

        // revertir efecto
        if (isGood)
        {
            a2GoodPlacedIds.Remove(valueId);
            A2Step(-1);
        }
        else
        {
            A2Step(+1);
        }

        a2Complete = IsActivity2Complete();
        SetContinue2Enabled(a2Complete);

        Debug.Log($"[Module3ActivityManager] A2 Remove slot={slotIndex} id={valueId} good={isGood} goodCount={a2GoodPlacedIds.Count} state={a2CurrentIndex} complete={a2Complete}");
    }

    public bool IsActivity2Complete()
    {
        return a2GoodPlacedIds.Count >= activity2RequiredGoodToWin;
    }

    public void ForceRecheckActivity2()
    {
        bool complete = IsActivity2Complete();
        SetContinue2Enabled(complete);
    }

    private void SetContinue2Enabled(bool value)
    {
        if (continueButton2 == null)
        {
            Debug.LogWarning("[Module3ActivityManager] continueButton2 no asignado.");
            return;
        }

        if (hideButton2Instead)
        {
            continueButton2.gameObject.SetActive(value);
        }
        else
        {
            continueButton2.gameObject.SetActive(true);
            continueButton2.interactable = value;

            var img = continueButton2.GetComponent<Image>();
            if (img != null) img.color = value ? Color.white : new Color(1f, 1f, 1f, 0.5f);
        }
    }

    // ===== Activity2 sprites =====
    private void A2SetState(int idx)
    {
        if (activity2CharacterImage == null) return;
        if (activity2States == null || activity2States.Length == 0) return;

        a2CurrentIndex = Mathf.Clamp(idx, 0, activity2States.Length - 1);
        var sp = activity2States[a2CurrentIndex];
        if (sp == null) return;

        activity2CharacterImage.sprite = sp;
        activity2CharacterImage.enabled = true;
        activity2CharacterImage.preserveAspect = true;
    }

    private void A2Step(int delta) => A2SetState(a2CurrentIndex + delta);

    // ===== Para Submitter =====
    public Dictionary<int, string> GetActivity2PlacementsCopy()
    {
        var result = new Dictionary<int, string>();

        if (activity2Slots == null) return result;

        foreach (var s in activity2Slots)
        {
            if (s == null) continue;
            int idx = s.slotIndex;

            a2PlacedIdBySlot.TryGetValue(idx, out var val);
            result[idx] = val ?? "";
        }

        return result;
    }

    // ✅ COMPATIBILIDAD: MinigameOrgManager llama ForceRecheck()
    public void ForceRecheck()
    {
        ForceRecheckActivity1();
        ForceRecheckActivity2();
    }

    // =========================
    // Actividad 4 (Selección + Audio)
    // =========================
    public void SetActivity4SelectedOption(string optionId)
    {
        Activity4SelectedOptionId = optionId == null ? "" : optionId.Trim();
        UpdateContinue4();
    }

    public void SetActivity4AudioBase64(string base64)
    {
        Activity4AudioBase64 = base64 ?? "";
        Activity4AudioBytes = null;

        Activity4HasAudioFlag = !string.IsNullOrEmpty(Activity4AudioBase64);

        UpdateContinue4();
    }

    public void SetActivity4AudioBytes(byte[] bytes)
    {
        Activity4AudioBytes = bytes;
        Activity4AudioBase64 = "";

        Activity4HasAudioFlag = (Activity4AudioBytes != null && Activity4AudioBytes.Length > 0);

        UpdateContinue4();
    }

    public void ClearActivity4Audio()
    {
        Activity4AudioBase64 = "";
        Activity4AudioBytes = null;
        Activity4HasAudioFlag = false;
        UpdateContinue4();
    }

    public void ClearActivity4Selection()
    {
        Activity4SelectedOptionId = "";
        UpdateContinue4();
    }

    public bool Activity4HasAudio()
    {
        if (Activity4HasAudioFlag) return true;

        bool hasBase64 = !string.IsNullOrEmpty(Activity4AudioBase64);
        bool hasBytes = Activity4AudioBytes != null && Activity4AudioBytes.Length > 0;
        return hasBase64 || hasBytes;
    }

    public bool Activity4HasSelection()
    {
        return !string.IsNullOrEmpty(Activity4SelectedOptionId);
    }

    public bool Activity4IsComplete()
    {
        return Activity4HasSelection() && Activity4HasAudio();
    }

    private void UpdateContinue4()
    {
        bool sel = Activity4HasSelection();
        bool aud = Activity4HasAudio();
        bool complete = sel && aud;

        Debug.Log($"[Module2] Act4 sel={sel} ('{Activity4SelectedOptionId}') aud={aud} flag={Activity4HasAudioFlag} -> complete={complete}");

        if (continueButton4 != null)
            continueButton4.SetActive(complete);
    }

    /// <summary>
    /// Gatillo extra (WebGL): si detectas WA_HasAudio()==1, llama esto y activa el botón
    /// aunque el base64 llegue un frame después.
    /// </summary>
    public void SetActivity4HasAudio(bool hasAudio)
    {
        Activity4HasAudioFlag = hasAudio;

        if (!hasAudio)
        {
            Activity4AudioBase64 = "";
            Activity4AudioBytes = null;
        }

        UpdateContinue4();
    }
}