using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class GetModuleResponse
{
    public bool ok;
    public ModuleProgressWithDataDto module;
}

[Serializable]
public class ModuleProgressWithDataDto
{
    public string moduleId;
    public float score;
    public bool done;

    // OJO: JsonUtility NO parsea bien object/dictionary.
    // Si necesitas leer data, lo mejor es que el backend mande "dataJson" string.
    public string dataJson;

    public string updatedAt;
}

public class Module1ProgressLoader : MonoBehaviour
{
    public ApiConfig apiConfig;
    public string moduleId = "1";

    private AuthService _auth;

    private void Awake()
    {
        _auth = new AuthService(apiConfig);
    }

    private void Start()
    {
        LoadModule1ForSelectedStudent();
    }

    public async void LoadModule1ForSelectedStudent()
    {
        var studentId = PlayerPrefs.GetString("selected_student_id", "");
        if (string.IsNullOrEmpty(studentId))
        {
            Debug.LogWarning("[Module1ProgressLoader] No hay estudiante seleccionado (selected_student_id).");
            return;
        }

        await LoadModuleForStudent(studentId, moduleId);
    }

    public async Task LoadModuleForStudent(string studentId, string moduleId)
    {
        Debug.Log($"[Module1ProgressLoader] Token? {(string.IsNullOrEmpty(_auth.Token) ? "NO" : "SI")} | youthId={studentId} | moduleId={moduleId}");

        if (string.IsNullOrEmpty(_auth.Token))
        {
            Debug.LogWarning("[Module1ProgressLoader] No hay token.");
            return;
        }

        try
        {
            string url = $"{apiConfig.baseUrl}/progress/youth/{studentId}/module/{moduleId}";
            string raw = await RestClient.SendJson(url, "GET", null, _auth.Token);

            Debug.Log("[Module1ProgressLoader] RAW => " + raw);

            var res = JsonUtility.FromJson<GetModuleResponse>(raw);
            if (res == null || !res.ok)
            {
                Debug.LogWarning("[Module1ProgressLoader] Respuesta inválida u ok=false.");
                return;
            }

            if (res.module == null)
            {
                Debug.Log("[Module1ProgressLoader] No hay progreso para este módulo (module=null).");
                return;
            }

            Debug.Log($"[Module1ProgressLoader] Module {res.module.moduleId} | Score: {res.module.score} | Done: {res.module.done}");
            Debug.Log($"[Module1ProgressLoader] dataJson len: {(string.IsNullOrEmpty(res.module.dataJson) ? 0 : res.module.dataJson.Length)}");

            // Aquí ya puedes llamar a tu manager que rehidrata actividades con dataJson
            // Example:
            // Module1ActivityRestorer.Instance.RestoreFromJson(res.module.dataJson);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Module1ProgressLoader] " + ex.Message);
        }
    }
}