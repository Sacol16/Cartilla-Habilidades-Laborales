using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Module4ProgressSubmitter : MonoBehaviour
{
    [Header("Config")]
    public ApiConfig apiConfig;

    [Header("Scoring")]
    [Range(0, 100)]
    public int module4Score = 100;

    [Tooltip("El moduleId que espera el backend. Si tu endpoint es /progress/modules/2, pon '2'.")]
    public string moduleId = "2";

    [Header("UI (Opcional)")]
    public GameObject canva1;
    public GameObject canva2;

    private AuthService _auth;

    private void Start()
    {
        _auth = new AuthService(apiConfig);

        if (string.IsNullOrEmpty(_auth.Token))
        {
            Debug.LogWarning("No hay token. Debes iniciar sesión.");
            return;
        }
    }

    // Llama esto desde un botón "Finalizar" o cuando completes el módulo 2
    public async void SubmitModule4()
    {
        try
        {
            if (string.IsNullOrEmpty(_auth.Token))
            {
                Debug.LogWarning("No hay token. Debes iniciar sesión.");
                return;
            }

            var mgr = Module4ActivityManager.Instance;
            if (mgr == null)
            {
                Debug.LogError("No existe Module4ActivityManager.Instance");
                return;
            }

            var req = BuildRequestFromManager(mgr);

            string url = $"{apiConfig.baseUrl}/progress/modules/{moduleId}";
            string body = JsonUtility.ToJson(req);

            string raw = await RestClient.SendJson(url, "PUT", body, _auth.Token);

            var res = JsonUtility.FromJson<UpsertModuleProgressResponse>(raw);

            if (res == null || !res.ok)
            {
                Debug.LogError("Error guardando progreso: " + raw);
                return;
            }

            Debug.Log($"? Progreso módulo {moduleId} guardado. Score total server: {res.progress?.score}");
        }
        catch (Exception ex)
        {
            Debug.LogError("SubmitModule4 error: " + ex.Message);
        }

        if (canva2 != null) canva2.SetActive(true);
        // if (canva1 != null) canva1.SetActive(false);
    }

    private UpsertModuleProgressRequest BuildRequestFromManager(Module4ActivityManager mgr)
    {
        

        // Activity4: selección + audio base64 (o bytes si estás usando ese flujo)
        string selected = mgr.Activity4SelectedOptionId ?? "";

        // Prioridad: base64 si existe, si no bytes
        string audioB64 = "";
        if (!string.IsNullOrEmpty(mgr.Activity4AudioBase64))
        {
            audioB64 = mgr.Activity4AudioBase64;
        }
        else if (mgr.Activity4AudioBytes != null && mgr.Activity4AudioBytes.Length > 0)
        {
            audioB64 = Convert.ToBase64String(mgr.Activity4AudioBytes);
        }

        return new UpsertModuleProgressRequest
        {
            score = module4Score,
            done = true,
            data = new ModuleDataDto
            {
                module4 = new Module4DataDto
                {
                    activity4SelectedOptionId = selected,
                    activity4AudioBase64 = audioB64
                }
            }
        };
    }
}
[Serializable]
public class Module4DataDto
{
    public string activity4SelectedOptionId;
    public string activity4AudioBase64;
}