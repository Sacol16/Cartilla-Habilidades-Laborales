using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class StudentModulesPanel : MonoBehaviour
{
    [Header("Config")]
    public ApiConfig apiConfig;

    [Header("Panel Root")]
    public GameObject panelRoot;

    [Header("Module Buttons (4)")]
    public Button module1Button;
    public Button module2Button;
    public Button module3Button;
    public Button module4Button;

    [Header("Module IDs (deben coincidir con moduleId en DB)")]
    public string module1Id = "1";
    public string module2Id = "2";
    public string module3Id = "3";
    public string module4Id = "4";

    [Header("Behavior")]
    [Tooltip("Si está activo, el módulo 1 queda habilitado aunque no exista progreso aún.")]
    public bool alwaysEnableModule1 = true;

    [Tooltip("Si está activo, habilita el siguiente módulo solo cuando el anterior esté done=true.")]
    public bool unlockSequentiallyByDone = false;

    private AuthService _auth;

    private void Awake()
    {
        _auth = new AuthService(apiConfig);
        if (panelRoot == null) panelRoot = gameObject;

        SetAllInteractable(false);
    }

    public async void ShowForStudent(string studentId)
    {
        if (panelRoot != null) panelRoot.SetActive(true);

        SetAllInteractable(false);
        await LoadAndApply(studentId);
    }

    private async Task LoadAndApply(string studentId)
    {
        if (string.IsNullOrEmpty(studentId))
        {
            Debug.LogWarning("[StudentModulesPanel] studentId vacío.");
            SetAllInteractable(false);
            return;
        }

        // Debug útil
        Debug.Log($"[StudentModulesPanel] Token? {(string.IsNullOrEmpty(_auth.Token) ? "NO" : "SI")} | youthId={studentId}");

        if (string.IsNullOrEmpty(_auth.Token))
        {
            Debug.LogWarning("[StudentModulesPanel] No hay token (sesión).");
            // si quieres permitir ver botones sin token, aquí podrías habilitar algo,
            // pero lo normal es dejarlos apagados.
            SetAllInteractable(false);
            return;
        }

        try
        {
            string url = $"{apiConfig.baseUrl}/progress/youth/{studentId}";
            string raw = await RestClient.SendJson(url, "GET", null, _auth.Token);

            // Debug útil por si el backend cambia
            Debug.Log("[StudentModulesPanel] RAW: " + raw);

            // Tu endpoint devuelve: { ok: true, progress: [...] }
            var res = JsonUtility.FromJson<GroupProgressResponse>(raw);

            if (res == null || !res.ok)
            {
                Debug.LogWarning("[StudentModulesPanel] Respuesta inválida o ok=false.");
                SetAllInteractable(false);
                return;
            }

            // por defecto: habilita si existe progreso para ese módulo
            bool hasM1 = HasProgressFor(res.progress, module1Id);
            bool hasM2 = HasProgressFor(res.progress, module2Id);
            bool hasM3 = HasProgressFor(res.progress, module3Id);
            bool hasM4 = HasProgressFor(res.progress, module4Id);

            // opcional: módulo 1 siempre habilitado aunque no exista progreso
            if (alwaysEnableModule1) hasM1 = true;

            if (!unlockSequentiallyByDone)
            {
                ApplyInteractable(hasM1, hasM2, hasM3, hasM4);
                return;
            }

            // ===== desbloqueo secuencial por done =====
            bool m1Done = IsDone(res.progress, module1Id);
            bool m2Done = IsDone(res.progress, module2Id);
            bool m3Done = IsDone(res.progress, module3Id);

            // M1: siempre (si alwaysEnableModule1) o si existe progreso
            bool enableM1 = hasM1;
            bool enableM2 = m1Done;   // se habilita si M1 está terminado
            bool enableM3 = m2Done;   // se habilita si M2 está terminado
            bool enableM4 = m3Done;   // se habilita si M3 está terminado

            ApplyInteractable(enableM1, enableM2, enableM3, enableM4);
        }
        catch (Exception ex)
        {
            Debug.LogError("[StudentModulesPanel] " + ex.Message);
            SetAllInteractable(false);
        }
    }

    private bool HasProgressFor(ProgressDto[] progress, string moduleId)
    {
        if (progress == null || string.IsNullOrEmpty(moduleId)) return false;
        return progress.Any(p => p != null && p.moduleId == moduleId);
    }

    private bool IsDone(ProgressDto[] progress, string moduleId)
    {
        if (progress == null || string.IsNullOrEmpty(moduleId)) return false;
        var p = progress.FirstOrDefault(x => x != null && x.moduleId == moduleId);
        return p != null && p.done;
    }

    private void ApplyInteractable(bool m1, bool m2, bool m3, bool m4)
    {
        if (module1Button != null) module1Button.interactable = m1;
        if (module2Button != null) module2Button.interactable = m2;
        if (module3Button != null) module3Button.interactable = m3;
        if (module4Button != null) module4Button.interactable = m4;
    }

    private void SetAllInteractable(bool value)
    {
        ApplyInteractable(value, value, value, value);
    }

    public void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}