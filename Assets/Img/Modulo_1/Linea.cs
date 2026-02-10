// Linea.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class Linea : MonoBehaviour
{
    public static Linea Instance { get; private set; }

    public int StrokeCount { get; private set; } = 0;
    public System.Action<int> OnStrokeCountChanged;

    [Header("Prefab / line settings")]
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private float minDistance = 0.1f;

    [Header("Brush size")]
    [SerializeField, Range(0.01f, 2f)] private float width = 0.2f;
    [SerializeField] private bool applyWidthToExistingStrokes = false;

    [Header("Brush color (next stroke)")]
    [SerializeField] private Color nextColor = Color.black;

    [Header("Limit drawing area (choose one)")]
    [Tooltip("Opción A: limita a un área UI (Panel)")]
    [SerializeField] private RectTransform uiDrawArea;

    [Tooltip("Opción B: limita a un área en mundo 2D (BoxCollider2D)")]
    [SerializeField] private BoxCollider2D worldDrawArea;

    [Header("Sorting (2D)")]
    [SerializeField] private int startOrderInLayer = 1;
    private int nextOrderInLayer;

    [Header("Optional UI")]
    [SerializeField] private Slider widthSlider;

    private readonly List<LineRenderer> strokes = new();
    private readonly Stack<GameObject> undoStack = new Stack<GameObject>(); // ? líneas + stickers

    private LineRenderer currentLine;
    private Vector3 previousPosition;
    private Camera cam;

    private void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    private void Start()
    {
        nextOrderInLayer = startOrderInLayer;

        if (widthSlider != null)
        {
            widthSlider.value = width;
            widthSlider.onValueChanged.AddListener(SetBrushSize);
        }

        previousPosition = Vector3.positiveInfinity;
    }

    private void Update()
    {
        if (!cam) return;

        bool canDrawNow = IsPointerInsideDrawArea(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (!canDrawNow) return;

            StartNewLine();
            AddPoint(GetMouseWorld());
        }

        if (Input.GetMouseButton(0) && currentLine != null)
        {
            if (!canDrawNow) return; // si se sale del área, pausa el trazo

            Vector3 p = GetMouseWorld();
            if (Vector3.Distance(p, previousPosition) > minDistance)
                AddPoint(p);
        }

        if (Input.GetMouseButtonUp(0))
        {
            currentLine = null;
        }
    }

    private void StartNewLine()
    {
        if (linePrefab == null) return;

        currentLine = Instantiate(linePrefab);
        currentLine.useWorldSpace = true;
        currentLine.positionCount = 0;
        currentLine.startWidth = currentLine.endWidth = width;

        // ? sorting incremental: 1,2,3...
        currentLine.sortingLayerID = linePrefab.sortingLayerID;
        currentLine.sortingOrder = nextOrderInLayer;
        nextOrderInLayer++;

        // ? aplicar color al siguiente trazo
        ApplyColorToLine(currentLine, nextColor);

        strokes.Add(currentLine);

        // ? registrar para Undo global (líneas + stickers)
        RegisterUndo(currentLine.gameObject);

        StrokeCount++;
        OnStrokeCountChanged?.Invoke(StrokeCount);
        Debug.Log($"[Linea] StrokeCount incrementado a: {StrokeCount} | strokes.Count={strokes.Count} | undoStack.Count={undoStack.Count} | currentLine={currentLine?.name}");
        Debug.Log($"[Linea] OnStrokeCountChanged Invoke -> {StrokeCount} | listeners? {(OnStrokeCountChanged == null ? "NO" : "SI")}");

        previousPosition = Vector3.positiveInfinity;
    }

    private void AddPoint(Vector3 p)
    {
        currentLine.positionCount++;
        currentLine.SetPosition(currentLine.positionCount - 1, p);
        previousPosition = p;
    }

    private Vector3 GetMouseWorld()
    {
        Vector3 mp = Input.mousePosition;

        // Dibuja en el plano Z=0
        float distance = -cam.transform.position.z;
        mp.z = distance;

        Vector3 w = cam.ScreenToWorldPoint(mp);
        w.z = 0f;
        return w;
    }

    private void ApplyColorToLine(LineRenderer lr, Color c)
    {
        lr.startColor = c;
        lr.endColor = c;

        // Si el material soporta _Color, también lo ajustamos
        if (lr.material != null && lr.material.HasProperty("_Color"))
            lr.material.color = c;
    }

    // =========================
    //  UNDO / CLEAR (GLOBAL)
    // =========================

    /// <summary>Registra cualquier objeto (línea o sticker) para que Undo lo elimine.</summary>
    public void RegisterUndo(GameObject go)
    {
        if (go != null) undoStack.Push(go);
    }

    /// <summary>Deshace lo último: puede ser trazo o sticker.</summary>
    public void UndoLast()
    {
        currentLine = null;
        if (undoStack.Count == 0) return;

        var go = undoStack.Pop();
        if (go == null) return;

        var lr = go.GetComponent<LineRenderer>();
        if (lr != null)
        {
            strokes.Remove(lr);
            StrokeCount = Mathf.Max(0, StrokeCount - 1);
            OnStrokeCountChanged?.Invoke(StrokeCount);
        }

        Destroy(go);
    }

    /// <summary>Borra TODO (trazos + stickers).</summary>
    public void ClearAll()
    {
        currentLine = null;

        while (undoStack.Count > 0)
        {
            var go = undoStack.Pop();
            if (go != null) Destroy(go);
        }

        strokes.Clear();
        StrokeCount = 0;
        OnStrokeCountChanged?.Invoke(StrokeCount);

        nextOrderInLayer = startOrderInLayer;
    }

    // =========================
    //  BRUSH SIZE
    // =========================

    public void SetBrushSize(float newWidth)
    {
        width = Mathf.Max(0.001f, newWidth);

        if (currentLine != null)
            currentLine.startWidth = currentLine.endWidth = width;

        if (applyWidthToExistingStrokes)
        {
            for (int i = 0; i < strokes.Count; i++)
            {
                if (strokes[i] != null)
                    strokes[i].startWidth = strokes[i].endWidth = width;
            }
        }
    }

    // =========================
    //  COLORS (NEXT STROKE)
    // =========================

    public void SetColorRed() => nextColor = Color.red;
    public void SetColorGreen() => nextColor = Color.green;
    public void SetColorYellow() => nextColor = Color.yellow;
    public void SetColorBlue() => nextColor = Color.blue;
    public void SetColorBlack() => nextColor = Color.black;
    public void SetColorWhite() => nextColor = Color.white;

    // =========================
    //  LIMIT DRAW AREA
    // =========================

    private bool IsPointerInsideDrawArea(Vector3 screenPos)
    {
        // UI Draw Area
        if (uiDrawArea != null)
        {
            // Si Canvas es Overlay, camera null. Si es Camera/World, pon la cámara del canvas si quieres.
            Camera uiCam = null;
            return RectTransformUtility.RectangleContainsScreenPoint(uiDrawArea, screenPos, uiCam);
        }

        // World Draw Area (2D)
        if (worldDrawArea != null)
        {
            Vector3 mp = screenPos;
            float distance = -cam.transform.position.z;
            mp.z = distance;

            Vector3 world = cam.ScreenToWorldPoint(mp);
            return worldDrawArea.OverlapPoint(world);
        }

        return true;
    }

    // ? Devuelve y reserva el siguiente sortingOrder para cualquier cosa (líneas o stickers)
    public int AllocateNextOrderInLayer()
    {
        int order = nextOrderInLayer;
        nextOrderInLayer++;
        return order;
    }

    // ? Opcional: si quieres que los stickers usen la misma sorting layer del prefab de línea
    public int GetSortingLayerID()
    {
        return linePrefab != null ? linePrefab.sortingLayerID : 0;
    }
}