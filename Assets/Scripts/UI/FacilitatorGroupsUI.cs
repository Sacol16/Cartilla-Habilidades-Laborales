using TMPro;
using UnityEngine;

public class FacilitatorGroupsUI : MonoBehaviour
{
    public ApiConfig apiConfig;
    public TMP_Text totalStudentsText;

    private AuthService _auth;

    private async void Start()
    {
        _auth = new AuthService(apiConfig);

        // Si no hay token guardado, manda a login
        if (string.IsNullOrEmpty(_auth.Token))
        {
            totalStudentsText.text = "Sin sesión.";
            return;
        }

        try
        {
            string url = $"{apiConfig.baseUrl}/groups/my";
            string raw = await RestClient.SendJson(url, "GET", null, _auth.Token);
            var res = JsonUtility.FromJson<GetMyGroupsResponse>(raw);

            int total = 0;
            if (res != null && res.ok && res.groups != null)
            {
                foreach (var g in res.groups) total += g.memberCount;
            }

            totalStudentsText.text = $"Estudiantes: {total}";
        }
        catch (System.Exception ex)
        {
            totalStudentsText.text = $"Error: {ex.Message}";
        }
    }
}
