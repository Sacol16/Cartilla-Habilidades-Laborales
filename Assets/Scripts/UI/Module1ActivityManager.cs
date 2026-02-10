using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Module1ActivityManager : MonoBehaviour
{
    public static Module1ActivityManager Instance { get; private set; }

    public byte[] Activity3PngBytes { get; private set; }

    [Header("Actividad 1")]
    [SerializeField] private DropSlotUI[] slots;
    [SerializeField] private GameObject continueButton1;

    [Header("Actividad 2")]
    [SerializeField] private TMP_InputField inputField1;
    [SerializeField] private TMP_InputField inputField2;
    [SerializeField] private TMP_InputField inputField3;
    [SerializeField] private TMP_InputField inputField4;
    [SerializeField] private TMP_InputField inputField5;
    [SerializeField] private GameObject continueButton2;

    [Header("Actividad 3")]
    [SerializeField] private GameObject continueButton3;
    [SerializeField] private int minLinesForContinue3 = 10;

    // Actividad 1: slotIndex -> itemName
    private readonly Dictionary<int, string> placedBySlot = new Dictionary<int, string>();

    // Actividad 2: inputIndex -> respuesta
    private readonly Dictionary<int, string> answersByInput = new Dictionary<int, string>();
    private bool activity2WasLogged = false;

    private TMP_InputField[] inputs;

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
        // ===== Actividad 1 =====
        if (continueButton1 != null)
            continueButton1.SetActive(false);

        placedBySlot.Clear();
        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
                placedBySlot[i] = "";
        }

        // ===== Actividad 2 =====
        if (continueButton2 != null)
            continueButton2.SetActive(false);

        inputs = new TMP_InputField[] { inputField1, inputField2, inputField3, inputField4, inputField5 };

        answersByInput.Clear();
        for (int i = 0; i < 5; i++)
            answersByInput[i] = "";

        activity2WasLogged = false;

        // Listeners: guardar + toggle botón (onValueChanged)
        // y log UNA sola vez cuando terminen de editar (onEndEdit)
        for (int i = 0; i < inputs.Length; i++)
        {
            if (inputs[i] == null)
            {
                Debug.LogWarning($"[Module1ActivityManager] Falta asignar inputField{i + 1} en el Inspector.");
                continue;
            }

            int index = i;

            // Guardar constantemente
            inputs[i].onValueChanged.AddListener(value =>
            {
                RegisterInputAnswer(index, value);
                CheckInputsAndToggleContinue2();
            });

            // Log solo cuando el usuario termine de editar un campo
            inputs[i].onEndEdit.AddListener(_ =>
            {
                TryLogActivity2Once();
            });

            // Inicializa desde el texto actual (si venía precargado)
            RegisterInputAnswer(index, inputs[i].text);
        }

        if (continueButton3 != null)
            continueButton3.SetActive(false);

        // Suscribirse al evento del dibujo (si Linea ya existe)
        if (Linea.Instance != null)
        {
            Linea.Instance.OnStrokeCountChanged += HandleStrokeCountChanged;
            HandleStrokeCountChanged(Linea.Instance.StrokeCount); // estado inicial
        }

        CheckInputsAndToggleContinue2();
        TryLogActivity2Once(); // por si ya venía todo lleno
    }

    // =========================
    // Actividad 1 (Slots)
    // =========================
    public void RegisterPlacement(int slotIndex, string itemObjectName)
    {
        if (slots == null) return;
        if (slotIndex < 0 || slotIndex >= slots.Length) return;

        placedBySlot[slotIndex] = itemObjectName;

        if (AreAllSlotsFilled())
        {
            if (continueButton1 != null)
                continueButton1.SetActive(true);

            Debug.Log("? Todos los slots llenos. Registros: " + DumpPlacements());
        }
    }

    public bool AreAllSlotsFilled()
    {
        if (slots == null) return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (string.IsNullOrEmpty(placedBySlot[i]))
                return false;
        }
        return true;
    }

    public string DumpPlacements()
    {
        if (slots == null) return "";

        List<string> parts = new List<string>();
        for (int i = 0; i < slots.Length; i++)
            parts.Add($"{i}:{placedBySlot[i]}");
        return string.Join(" | ", parts);
    }

    // =========================
    // Actividad 2 (Inputs)
    // =========================
    private void RegisterInputAnswer(int inputIndex, string value)
    {
        string clean = value == null ? "" : value.Trim();
        answersByInput[inputIndex] = clean;

        // Si se borra algo, permite volver a loguear al completar
        if (!AreAllInputsFilled())
            activity2WasLogged = false;
    }

    private void CheckInputsAndToggleContinue2()
    {
        bool allFilled = AreAllInputsFilled();

        if (continueButton2 != null)
            continueButton2.SetActive(allFilled);
    }

    private void TryLogActivity2Once()
    {
        if (activity2WasLogged) return;

        if (AreAllInputsFilled())
        {
            activity2WasLogged = true;
            Debug.Log("? Todos los inputs llenos. Respuestas: " + DumpInputAnswers());
        }
    }

    private bool AreAllInputsFilled()
    {
        for (int i = 0; i < 5; i++)
        {
            if (!answersByInput.ContainsKey(i)) return false;
            if (string.IsNullOrWhiteSpace(answersByInput[i])) return false;
        }
        return true;
    }

    public string DumpInputAnswers()
    {
        List<string> parts = new List<string>();
        for (int i = 0; i < 5; i++)
            parts.Add($"{i}:{answersByInput[i]}");
        return string.Join(" | ", parts);
    }

    private void HandleStrokeCountChanged(int count)
    {
        bool canContinue = count >= minLinesForContinue3;

        if (continueButton3 != null)
            continueButton3.SetActive(canContinue);

        // (Opcional) Debug
        // if (canContinue) Debug.Log($"? Actividad 3 completa: {count} trazos.");
    }

    private void OnDestroy()
    {
        // si ya tienes OnDestroy, integra esto dentro
        if (Linea.Instance != null)
            Linea.Instance.OnStrokeCountChanged -= HandleStrokeCountChanged;
    }

    public void SetActivity3Drawing(byte[] pngBytes)
    {
        Activity3PngBytes = pngBytes;
        Debug.Log($"? Actividad 3 guardada en memoria. Bytes: {(pngBytes != null ? pngBytes.Length : 0)}");
    }
}