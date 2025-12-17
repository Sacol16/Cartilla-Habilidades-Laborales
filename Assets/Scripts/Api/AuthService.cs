using System.Threading.Tasks;
using UnityEngine;

public class AuthService
{
    private readonly ApiConfig _config;
    public string Token { get; private set; }
    public LoginUser CurrentUser { get; private set; }

    public AuthService(ApiConfig config)
    {
        _config = config;
        LoadToken();
    }

    public async Task RegisterFacilitator(string email, string name, string password, string code)
    {
        var req = new RegisterFacilitatorRequest { email = email, name = name, password = password, code = code };
        string json = JsonUtility.ToJson(req);

        string url = $"{_config.baseUrl}/auth/register-facilitator";
        string raw = await RestClient.SendJson(url, "POST", json);

        var res = JsonUtility.FromJson<RegisterFacilitatorResponse>(raw);
        if (res == null || !res.ok) throw new System.SystemException("Registro falló.");
    }

    public async Task Login(string email, string password)
    {
        var req = new LoginRequest { email = email, password = password };
        string json = JsonUtility.ToJson(req);

        string url = $"{_config.baseUrl}/auth/login";
        string raw = await RestClient.SendJson(url, "POST", json);

        var res = JsonUtility.FromJson<LoginResponse>(raw);
        if (res == null || !res.ok || string.IsNullOrEmpty(res.token))
            throw new System.SystemException("Login falló.");

        Token = res.token;
        CurrentUser = res.user;

        SaveToken(Token);
    }

    public void Logout()
    {
        Token = null;
        CurrentUser = null;
        PlayerPrefs.DeleteKey("jwt_token");
        PlayerPrefs.Save();
    }

    private void SaveToken(string token)
    {
        PlayerPrefs.SetString("jwt_token", token);
        PlayerPrefs.Save();
    }

    private void LoadToken()
    {
        if (PlayerPrefs.HasKey("jwt_token"))
            Token = PlayerPrefs.GetString("jwt_token");
    }
}
