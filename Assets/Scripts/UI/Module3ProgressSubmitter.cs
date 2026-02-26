using System;
using System.Collections.Generic;
using UnityEngine;

public class Module3ProgressSubmitter : MonoBehaviour
{
    [Header("Config")]
    public ApiConfig apiConfig;

    [Header("Scoring")]
    [Range(0, 100)]
    public int module3Score = 100;

    public string moduleId = "3";

    [Header("UI")]
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

    public async void SubmitModule3()
    {
        try
        {
            if (string.IsNullOrEmpty(_auth.Token))
            {
                Debug.LogWarning("No hay token. Debes iniciar sesión.");
                return;
            }

            var mgr = Module3ActivityManager.Instance;
            if (mgr == null)
            {
                Debug.LogError("No existe Module3ActivityManager.Instance");
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

            Debug.Log($"✅ Progreso módulo {moduleId} guardado. Score total server: {res.progress?.score}");
        }
        catch (Exception ex)
        {
            Debug.LogError("SubmitModule3 error: " + ex.Message);
        }

        if (canva2 != null) canva2.SetActive(true);
        // if (canva1 != null) canva1.SetActive(false);
    }

    private UpsertModuleProgressRequest BuildRequestFromManager(Module3ActivityManager mgr)
    {
        Dictionary<int, string> placementsA1 = mgr.GetActivity1PlacementsCopy();
        Dictionary<int, string> placementsA2 = mgr.GetActivity2PlacementsCopy();

        var listA1 = new List<SlotPlacementDto>();
        foreach (var kv in placementsA1)
        {
            listA1.Add(new SlotPlacementDto
            {
                slotIndex = kv.Key,
                itemObjectName = kv.Value ?? ""
            });
        }

        var listA2 = new List<SlotPlacementDto>();
        foreach (var kv in placementsA2)
        {
            listA2.Add(new SlotPlacementDto
            {
                slotIndex = kv.Key,
                itemObjectName = kv.Value ?? ""
            });
        }

        return new UpsertModuleProgressRequest
        {
            score = module3Score,
            done = true,
            data = new ModuleDataDto
            {
                module3 = new Module3DataDto
                {
                    activity1 = listA1.ToArray(),
                    activity2 = listA2.ToArray() // ✅ NUEVO
                }
            }
        };
    }
}