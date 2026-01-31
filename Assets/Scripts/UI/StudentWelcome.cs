using System;
using TMPro;
using UnityEngine;

[Serializable]
public class MeUserDto
{
    public string id;
    public string name;
    public string email;
    public string role;
    public string groupId;
}

[Serializable]
public class MeResponse
{
    public bool ok;
    public MeUserDto user;
}

public class StudentWelcome : MonoBehaviour
{
    public ApiConfig apiConfig;
    public TMP_Text welcomeText;

    private AuthService _auth;

    private async void Start()
    {
        _auth = new AuthService(apiConfig);

        if (welcomeText == null) return;

        if (string.IsNullOrEmpty(_auth.Token))
        {
            welcomeText.text = "¡Bienvenido!";
            return;
        }

        try
        {
            string url = $"{apiConfig.baseUrl}/auth/me";
            string raw = await RestClient.SendJson(url, "GET", null, _auth.Token);

            var res = JsonUtility.FromJson<MeResponse>(raw);

            if (res != null && res.ok && res.user != null && !string.IsNullOrEmpty(res.user.name))
                welcomeText.text = $"¡Bienvenido {res.user.name}!";
            else
                welcomeText.text = "¡Bienvenido!";
        }
        catch
        {
            welcomeText.text = "¡Bienvenido!";
        }
    }
}
