using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Module1ProgressSubmitter : MonoBehaviour
{
    [Header("Config")]
    public ApiConfig apiConfig;

    [Header("Scoring")]
    [Range(0, 100)]
    public int module1Score = 100;

    [Tooltip("El moduleId que espera el backend. Si tu endpoint es /progress/modules/1, pon '1'.")]
    public string moduleId = "1";

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

    // Llama esto desde un botón "Finalizar" o cuando completes el módulo 1
    public async void SubmitModule1()
    {
        try
        {
            if (string.IsNullOrEmpty(_auth.Token))
            {
                Debug.LogWarning("No hay token. Debes iniciar sesión.");
                return;
            }

            var mgr = Module1ActivityManager.Instance;
            if (mgr == null)
            {
                Debug.LogError("No existe Module1ActivityManager.Instance");
                return;
            }

            var req = BuildRequestFromManager(mgr);

            string url = $"{apiConfig.baseUrl}/progress/modules/{moduleId}";
            string body = JsonUtility.ToJson(req);

            string raw = await RestClient.SendJson(url, "PUT", body, _auth.Token);

            // Si tu backend devuelve { ok: true, progress: {...} }
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
            Debug.LogError("SubmitModule1 error: " + ex.Message);
        }
        if (canva2 != null) canva2.SetActive(true);
        //if (canva1 != null) canva1.SetActive(false);

    }

    private UpsertModuleProgressRequest BuildRequestFromManager(Module1ActivityManager mgr)
    {
        // Activity1: dict -> array
        Dictionary<int, string> placements = mgr.GetActivity1PlacementsCopy();
        var list = new List<SlotPlacementDto>();

        foreach (var kv in placements)
        {
            list.Add(new SlotPlacementDto
            {
                slotIndex = kv.Key,
                itemObjectName = kv.Value ?? ""
            });
        }

        // Activity2: 5 inputs
        string[] a2 = mgr.GetActivity2AnswersArray();

        // Activity3: bytes -> base64
        string pngB64 = (mgr.Activity3PngBytes != null && mgr.Activity3PngBytes.Length > 0)
            ? Convert.ToBase64String(mgr.Activity3PngBytes)
            : "";

        // Activity4: selección + audio base64 (ya está como string)
        string selected = mgr.Activity4SelectedOptionId ?? "";
        string audioB64 = mgr.Activity4AudioBase64 ?? "";

        return new UpsertModuleProgressRequest
        {
            score = module1Score,
            done = true,
            data = new ModuleDataDto
            {
                module1 = new Module1DataDto
                {
                    activity1 = list.ToArray(),
                    activity2Answers = a2,
                    activity3PngBase64 = pngB64,
                    activity4SelectedOptionId = selected,
                    activity4AudioBase64 = audioB64
                }
            }
        };
    }
}