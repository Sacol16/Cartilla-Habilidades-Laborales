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

    [Header("UI")]
    [SerializeField] private Button saveButton;         // el botón que se clickea
    [SerializeField] private TMP_Text buttonLabelTMP;   // texto del botón (TMP)
    [SerializeField] private Text buttonLabelLegacy;    // texto del botón (Text normal)
    [SerializeField] private string savingText = "Guardando...";
    [SerializeField] private float waitSeconds = 5f;

    private bool _busy = false;
    private string _originalText = "";

    public void SaveSwitch()
    {
        if (_busy) return; // evita doble click
        StartCoroutine(SaveAndSwitchRoutine());
    }

    private IEnumerator SaveAndSwitchRoutine()
    {
        _busy = true;

        // Deshabilitar botón (evita clicks)
        if (saveButton != null) saveButton.interactable = false;

        // Guardar texto original y cambiar a "Guardando..."
        CacheOriginalTextIfNeeded();
        SetButtonText(savingText);

        // Validaciones
        if (drawingSaver == null)
        {
            Debug.LogError("[BotonGuardarDibujo] Falta asignar DrawingSaver.");
            RestoreUI();
            yield break;
        }

        byte[] png = drawingSaver.CapturePngBytes();
        if (png == null || png.Length == 0)
        {
            Debug.LogError("[BotonGuardarDibujo] No se pudo capturar el PNG.");
            RestoreUI();
            yield break;
        }

        // Guardar para subirlo al final (un solo POST)
        if (Module1ActivityManager.Instance != null)
            Module1ActivityManager.Instance.SetActivity3Drawing(png);

        // Espera 5 segundos antes de cambiar de canvas
        yield return new WaitForSeconds(waitSeconds);

        // UI flow
        if (canva2 != null) canva2.SetActive(true);
        if (canva1 != null) canva1.SetActive(false);

        // (Opcional) Si quieres dejar el botón en "Guardando..." ya no restauras.
        // Si quieres restaurarlo por si vuelves a esta pantalla, descomenta:
        // RestoreUI();
    }

    private void CacheOriginalTextIfNeeded()
    {
        if (_originalText != "") return;

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