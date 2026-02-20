using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

#region DTOs (Members)
[Serializable]
public class MemberDto { public string id; public string name; public string email; }

[Serializable]
public class GroupInfoDto { public string id; public string name; public int memberCount; }

[Serializable]
public class GroupMembersResponse
{
    public bool ok;
    public GroupInfoDto group;
    public MemberDto[] members;
}
#endregion

public class YouthPanelController : MonoBehaviour
{
    [Header("Config")]
    public ApiConfig apiConfig;

    [Header("UI")]
    public GameObject panelRoot;
    public TMP_Text titleText;
    public Transform contentParent;          // ScrollView/Viewport/Content
    public StudentButtonItem studentButtonPrefab;
    public StudentModulesPanel modulesPanel;
    private AuthService _auth;

    // Para actualizar slider/label por estudiante
    private readonly Dictionary<string, StudentButtonItem> _itemsByStudentId =
        new Dictionary<string, StudentButtonItem>();

    private void Awake()
    {
        _auth = new AuthService(apiConfig);
        if (panelRoot == null) panelRoot = gameObject;
    }

    /// <summary>
    /// Carga miembros del grupo y luego carga progresos del grupo para actualizar cada item.
    /// </summary>
    public async Task LoadMembers(string groupId)
    {
        if (string.IsNullOrEmpty(_auth.Token))
        {
            Debug.LogWarning("No token. Inicia sesión.");
            return;
        }

        try
        {
            ClearList();

            // 1) GET miembros con nombre
            string membersUrl = $"{apiConfig.baseUrl}/groups/{groupId}/members";
            string membersRaw = await RestClient.SendJson(membersUrl, "GET", null, _auth.Token);

            var membersRes = JsonUtility.FromJson<GroupMembersResponse>(membersRaw);
            if (membersRes == null || !membersRes.ok) return;

            if (titleText != null && membersRes.group != null)
                titleText.text = $"{membersRes.group.name} ({membersRes.group.memberCount})";

            if (membersRes.members == null) return;

            // 2) Instanciar botones por miembro y guardarlos por ID
            foreach (var m in membersRes.members)
            {
                var item = Instantiate(studentButtonPrefab, contentParent);
                item.Setup(m.id, m.name, OnStudentClicked);

                _itemsByStudentId[m.id] = item;

                // estado inicial
                item.SetProgress01(0f);
            }

            // 3) Cargar progresos y aplicarlos al UI
            await LoadGroupProgressAndApply(groupId);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    /// <summary>
    /// Trae todos los progresos del grupo y actualiza slider/label de cada estudiante.
    /// Regla: si score es 25%, slider = 0.25 (score/100).
    /// </summary>
    private async Task LoadGroupProgressAndApply(string groupId)
    {
        // Usa los DTOs que YA tienes en ProgressModels.cs:
        // ProgressDto y GroupProgressResponse

        string url = $"{apiConfig.baseUrl}/progress/groups/{groupId}";
        string raw = await RestClient.SendJson(url, "GET", null, _auth.Token);

        var res = JsonUtility.FromJson<GroupProgressResponse>(raw);
        if (res == null || !res.ok || res.progress == null) return;

        // Agrupar por youthId
        var grouped = res.progress
            .Where(p => !string.IsNullOrEmpty(p.youthId))
            .GroupBy(p => p.youthId);

        foreach (var g in grouped)
        {
            string youthId = g.Key;

            // Promedio de score (0..100)
            float sum = 0f;
            int count = 0;

            foreach (var p in g)
            {
                sum += p.score;
                count++;
            }

            float avgScore = (count > 0) ? (sum / count) : 0f; // 0..100
            float value01 = Mathf.Clamp01(avgScore / 100f);    // 25 -> 0.25

            if (_itemsByStudentId.TryGetValue(youthId, out var item) && item != null)
                item.SetProgress01(value01);
        }
    }

    private void OnStudentClicked(string studentId)
    {
        PlayerPrefs.SetString("selected_student_id", studentId);
        PlayerPrefs.Save();

        if (modulesPanel != null)
            modulesPanel.ShowForStudent(studentId);
    }

    private void ClearList()
    {
        _itemsByStudentId.Clear();

        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }
}