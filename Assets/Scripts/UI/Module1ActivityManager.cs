using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    [Header("Actividad 4")]
    [SerializeField] private GameObject continueButton4;

    // =========================
    // Actividad 1: slotIndex -> itemName
    // =========================
    private readonly Dictionary<int, string> placedBySlot = new Dictionary<int, string>();

    // =========================
    // Actividad 2: inputIndex -> respuesta
    // =========================
    private readonly Dictionary<int, string> answersByInput = new Dictionary<int, string>();
    private bool activity2WasLogged = false;
    private TMP_InputField[] inputs;

    // =========================
    // Actividad 4: selección + audio
    // =========================
    public string Activity4SelectedOptionId { get; private set; } = "";
    public string Activity4AudioBase64 { get; private set; } = "";
    public byte[] Activity4AudioBytes { get; private set; }

    // ? NUEVO: flag para “audio confirmado” (sirve para WebGL timing)
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

        for (int i = 0; i < inputs.Length; i++)
        {
            if (inputs[i] == null)
            {
                Debug.LogWarning($"[Module1ActivityManager] Falta asignar inputField{i + 1} en el Inspector.");
                continue;
            }

            int index = i;

            inputs[i].onValueChanged.AddListener(value =>
            {
                RegisterInputAnswer(index, value);
                CheckInputsAndToggleContinue2();
            });

            inputs[i].onEndEdit.AddListener(_ =>
            {
                TryLogActivity2Once();
            });

            RegisterInputAnswer(index, inputs[i].text);
        }

        // ===== Actividad 3 =====
        if (continueButton3 != null)
            continueButton3.SetActive(false);

        StartCoroutine(AttachToLineaWhenReady());

        // ===== Actividad 4 =====
        if (continueButton4 != null)
            continueButton4.SetActive(false);

        // estado inicial act4
        ClearActivity4Selection();
        ClearActivity4Audio();

        CheckInputsAndToggleContinue2();
        TryLogActivity2Once();
        UpdateContinue4();
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

    // =========================
    // Actividad 3 (Dibujo)
    // =========================
    private void HandleStrokeCountChanged(int count)
    {
        bool canContinue = count >= minLinesForContinue3;

        if (continueButton3 != null)
            continueButton3.SetActive(canContinue);
    }

    private void OnDestroy()
    {
        if (Linea.Instance != null)
            Linea.Instance.OnStrokeCountChanged -= HandleStrokeCountChanged;
    }

    public void SetActivity3Drawing(byte[] pngBytes)
    {
        Activity3PngBytes = pngBytes;
        Debug.Log($"? Actividad 3 guardada en memoria. Bytes: {(pngBytes != null ? pngBytes.Length : 0)}");
    }

    private System.Collections.IEnumerator AttachToLineaWhenReady()
    {
        while (Linea.Instance == null)
            yield return null;

        Linea.Instance.OnStrokeCountChanged += HandleStrokeCountChanged;
        HandleStrokeCountChanged(Linea.Instance.StrokeCount);

        Debug.Log("[Manager] Suscrito a Linea.OnStrokeCountChanged");
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

        // ? Si ya llegó base64, confirmamos flag también
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

    // ? Usa el flag como fuente de verdad (y también soporta base64/bytes)
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
        if (continueButton4 != null)
            continueButton4.SetActive(Activity4IsComplete());
    }

    // ? IMPORTANTE: este método vuelve a ser el “gatillo” para WebAudioRecorderUI
    // Cuando detectes WA_HasAudio()==1, llama SetActivity4HasAudio(true)
    // y el botón se activa aunque base64 llegue un frame después.
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

    public System.Collections.Generic.Dictionary<int, string> GetActivity1PlacementsCopy()
    {
        return new System.Collections.Generic.Dictionary<int, string>(placedBySlot);
    }

    public string[] GetActivity2AnswersArray()
    {
        var arr = new string[5];
        for (int i = 0; i < 5; i++)
            arr[i] = answersByInput.ContainsKey(i) ? (answersByInput[i] ?? "") : "";
        return arr;
    }
}