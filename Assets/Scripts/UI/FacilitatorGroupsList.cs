using System.Threading.Tasks;
using UnityEngine;

public class FacilitatorGroupsList : MonoBehaviour
{
    [Header("Config")]
    public ApiConfig apiConfig;

    [Header("UI")]
    public Transform contentParent;          // donde se instancian (ej: Content de ScrollView)
    public GroupButtonItem groupButtonPrefab;

    private AuthService _auth;

    public YouthPanelController youthPanel;

    private async void Start()
    {
        _auth = new AuthService(apiConfig);

        if (string.IsNullOrEmpty(_auth.Token))
        {
            Debug.LogWarning("No hay token. Debes iniciar sesión.");
            return;
        }

        await LoadGroups();
    }

    public async Task LoadGroups()
    {
        ClearContent();

        string url = $"{apiConfig.baseUrl}/groups/my";
        string raw = await RestClient.SendJson(url, "GET", null, _auth.Token);

        var res = JsonUtility.FromJson<GetMyGroupsResponse>(raw);
        if (res == null || !res.ok || res.groups == null) return;

        foreach (var g in res.groups)
        {
            var item = Instantiate(groupButtonPrefab, contentParent);
            item.Setup(g._id, g.name, g.memberCount, OnGroupClicked);
        }
    }

    private async void OnGroupClicked(string groupId)
    {
        PlayerPrefs.SetString("selected_group_id", groupId);
        PlayerPrefs.Save();

        if (youthPanel != null)
        {
            youthPanel.gameObject.SetActive(true);
            await youthPanel.LoadMembers(groupId);
        }
    }

    private void ClearContent()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }
}
