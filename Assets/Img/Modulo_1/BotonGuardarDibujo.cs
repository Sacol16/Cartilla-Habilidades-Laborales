using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BotonGuardarDibujo : MonoBehaviour
{
    public GameObject canva1;
    public GameObject canva2;

    [Header("Refs")]
    [SerializeField] private DrawingSaver drawingSaver; // arrastra aquí el objeto que tiene DrawingSaver

    [Header("Activity Manager (Assign in Inspector)")]
    [Tooltip("Arrastra aquí Module1ActivityManager o Module2ActivityManager (según el módulo donde estés).")]
    [SerializeField] private MonoBehaviour activityManager;

    [Header("UI")]
    [SerializeField] private Button saveButton;         // el botón que se clickea
    [SerializeField] private TMP_Text buttonLabelTMP;   // texto del botón (TMP)
    [SerializeField] private Text buttonLabelLegacy;    // texto del botón (Text normal)
    [SerializeField] private string savingText = "Guardando...";
    [SerializeField] private float waitSeconds = 5f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool _busy = false;
    private string _originalText = "";

    public void SaveSwitch()
    {
        if (_busy) return;
        StartCoroutine(SaveAndSwitchRoutine());
    }

    private IEnumerator SaveAndSwitchRoutine()
    {
        _busy = true;

        if (saveButton != null) saveButton.interactable = false;

        CacheOriginalTextIfNeeded();
        SetButtonText(savingText);

        // Validar DrawingSaver
        if (drawingSaver == null)
        {
            Debug.LogError("[BotonGuardarDibujo] Falta asignar DrawingSaver.");
            RestoreUI();
            yield break;
        }

        // Capturar PNG
        byte[] png = drawingSaver.CapturePngBytes();
        if (png == null || png.Length == 0)
        {
            Debug.LogError("[BotonGuardarDibujo] No se pudo capturar el PNG.");
            RestoreUI();
            yield break;
        }

        // Guardar en el manager asignado
        if (!PushPngToManager(png))
        {
            Debug.LogError("[BotonGuardarDibujo] No se pudo guardar el PNG: ActivityManager no asignado o tipo no soportado.");
            RestoreUI();
            yield break;
        }

        if (debugLogs) Debug.Log($"[BotonGuardarDibujo] PNG guardado. bytes={png.Length}");

        // Espera antes de cambiar de canvas
        yield return new WaitForSeconds(waitSeconds);

        if (canva2 != null) canva2.SetActive(true);
        if (canva1 != null) canva1.SetActive(false);

        // Si quieres permitir volver y reusar el botón, descomenta:
        // RestoreUI();
    }

    private bool PushPngToManager(byte[] png)
    {
        if (activityManager == null)
        {
            Debug.LogError("[BotonGuardarDibujo] ActivityManager no asignado en el Inspector.");
            return false;
        }

        // Module2
        if (activityManager is Module2ActivityManager m2)
        {
            m2.SetActivity3Drawing(png);
            return true;
        }

        // Module1
        if (activityManager is Module1ActivityManager m1)
        {
            m1.SetActivity3Drawing(png);
            return true;
        }

        // Si quieres soportar más módulos luego, agregas aquí
        Debug.LogError("[BotonGuardarDibujo] ActivityManager asignado no es Module1ActivityManager ni Module2ActivityManager.");
        return false;
    }

    private void CacheOriginalTextIfNeeded()
    {
        if (!string.IsNullOrEmpty(_originalText)) return;

        if (buttonLabelTMP != null) _originalText = buttonLabelTMP.text;
        else if (buttonLabelLegacy != null) _originalText = buttonLabelLegacy.text;
    }

    private void SetButtonText(string txt)
    {
        if (buttonLabelTMP != null) buttonLabelTMP.text = txt;
        if (buttonLabelLegacy != null) buttonLabelLegacy.text = txt;
    }

    private void RestoreUI()
    {
        SetButtonText(string.IsNullOrEmpty(_originalText) ? "Guardar" : _originalText);
        if (saveButton != null) saveButton.interactable = true;
        _busy = false;
    }
}