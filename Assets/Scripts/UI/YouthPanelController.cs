using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

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

public class YouthPanelController : MonoBehaviour
{
    [Header("Config")]
    public ApiConfig apiConfig;

    [Header("UI")]
    public GameObject panelRoot;         // opcional, si quieres ocultar/mostrar
    public TMP_Text titleText;           // opcional: mostrar nombre del grupo
    public Transform contentParent;      // Content del ScrollView
    public StudentButtonItem studentButtonPrefab;

    private AuthService _auth;

    private void Awake()
    {
        _auth = new AuthService(apiConfig);
        if (panelRoot == null) panelRoot = gameObject;
    }

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

            string url = $"{apiConfig.baseUrl}/groups/{groupId}/members";
            string raw = await RestClient.SendJson(url, "GET", null, _auth.Token);

            var res = JsonUtility.FromJson<GroupMembersResponse>(raw);
            if (res == null || !res.ok) return;

            if (titleText != null && res.group != null)
                titleText.text = $"{res.group.name} ({res.group.memberCount})";

            if (res.members == null) return;

            foreach (var m in res.members)
            {
                var item = Instantiate(studentButtonPrefab, contentParent);
                item.Setup(m.id, m.name, OnStudentClicked);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private void OnStudentClicked(string studentId)
    {
        Debug.Log("Joven seleccionado: " + studentId);
        PlayerPrefs.SetString("selected_student_id", studentId);
        PlayerPrefs.Save();

        // aquí puedes abrir panel de detalle/progreso si quieres
    }

    private void ClearList()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }
}
