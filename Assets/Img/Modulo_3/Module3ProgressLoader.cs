using System;
using System.Threading.Tasks;
using UnityEngine;

#region DTOs (API response)


#endregion

#region DTOs (dataJson -> typed)
#endregion

public class Module3ProgressLoader : MonoBehaviour
{
    [Header("Config")]
    public ApiConfig apiConfig;
    public string moduleId = "3";

    [Header("Presenters")]
    [Tooltip("Arrastra aquí el Module1Activity4Presenter de la escena.")]
    public Module1Activity4Presenter activity4Presenter;

    private AuthService _auth;

    private void Awake()
    {
        _auth = new AuthService(apiConfig);
    }

    private void Start()
    {
        LoadModule3ForSelectedStudent();
    }

    public async void LoadModule3ForSelectedStudent()
    {
        var studentId = PlayerPrefs.GetString("selected_student_id", "");
        if (string.IsNullOrEmpty(studentId))
        {
            Debug.LogWarning("[Module4ProgressLoader] No hay estudiante seleccionado (selected_student_id).");
            return;
        }

        await LoadModuleForStudent(studentId, moduleId);
    }

    public async Task LoadModuleForStudent(string studentId, string moduleId)
    {
        Debug.Log($"[Module4ProgressLoader] Token? {(string.IsNullOrEmpty(_auth.Token) ? "NO" : "SI")} | youthId={studentId} | moduleId={moduleId}");

        if (string.IsNullOrEmpty(_auth.Token))
        {
            Debug.LogWarning("[Module4ProgressLoader] No hay token.");
            return;
        }

        try
        {
            string url = $"{apiConfig.baseUrl}/progress/youth/{studentId}/module/{moduleId}";
            string raw = await RestClient.SendJson(url, "GET", null, _auth.Token);

            // No imprimas raw completo (puede ser gigante). Mejor longitud:
            Debug.Log($"[Module3ProgressLoader] RAW length: {(raw != null ? raw.Length : 0)}");

            var res = JsonUtility.FromJson<GetModuleResponse>(raw);
            if (res == null || !res.ok)
            {
                Debug.LogWarning("[Module3ProgressLoader] Respuesta inválida u ok=false.");
                return;
            }

            if (res.module == null)
            {
                Debug.Log("[Module3ProgressLoader] No hay progreso para este módulo (module=null).");
                return;
            }

            Debug.Log($"[Module3ProgressLoader] Module {res.module.moduleId} | Score: {res.module.score} | Done: {res.module.done}");
            Debug.Log($"[Module3ProgressLoader] dataJson len: {(string.IsNullOrEmpty(res.module.dataJson) ? 0 : res.module.dataJson.Length)}");

            if (string.IsNullOrEmpty(res.module.dataJson))
            {
                Debug.LogWarning("[Module3ProgressLoader] dataJson vacío. No hay nada que aplicar.");
                return;
            }

            // Parsear dataJson a DTO tipado
            var data = JsonUtility.FromJson<ModuleDataDto>(res.module.dataJson);
            var m1 = data?.module4;


            var selectedOptionId = m1?.activity4SelectedOptionId;
            var audioBase64 = m1?.activity4AudioBase64;

            Debug.Log($"[Module3ProgressLoader] activity4SelectedOptionId empty? {string.IsNullOrEmpty(selectedOptionId)}");
            Debug.Log($"[Module3ProgressLoader] activity4AudioBase64 len: {(string.IsNullOrEmpty(audioBase64) ? 0 : audioBase64.Length)}");

            // Espera 1 frame para evitar que LayoutGroups reacomoden después de reparent
            await Task.Yield();

            // Forzar update de layout (útil si es UI)
            Canvas.ForceUpdateCanvases();




            // Aplicar Activity 4 (selección + audio)
            if (activity4Presenter != null)
                activity4Presenter.Apply(selectedOptionId, audioBase64);
            else
                Debug.LogWarning("[Module3ProgressLoader] activity4Presenter NO asignado en el Inspector.");

            Canvas.ForceUpdateCanvases();
        }
        catch (Exception ex)
        {
            Debug.LogError("[Module3ProgressLoader] " + ex);
        }
    }
}