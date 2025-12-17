using TMPro;
using UnityEngine;

public class AuthController : MonoBehaviour
{
    [Header("Config")]
    public ApiConfig apiConfig;

    [Header("Login UI")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;

    [Header("Register Facilitator UI")]
    public TMP_InputField regEmail;
    public TMP_InputField regName;
    public TMP_InputField regPassword;
    public TMP_InputField regCode;

    [Header("Output")]
    public TMP_Text statusText;

    private AuthService _auth;

    private void Awake()
    {
        _auth = new AuthService(apiConfig);
    }

    public async void OnClickLogin()
    {
        try
        {
            statusText.text = "Iniciando sesión...";
            await _auth.Login(loginEmail.text.Trim(), loginPassword.text);
            statusText.text = $"OK ✅ Rol: {_auth.CurrentUser.role}";
            // Aquí cargas escena según rol
        }
        catch (System.Exception ex)
        {
            statusText.text = $"Error: {ex.Message}";
        }
    }

    public async void OnClickRegisterFacilitator()
    {
        try
        {
            statusText.text = "Registrando facilitador...";
            await _auth.RegisterFacilitator(
                regEmail.text.Trim(),
                regName.text.Trim(),
                regPassword.text,
                regCode.text.Trim()
            );

            statusText.text = "Registro OK ✅ Ahora inicia sesión.";
        }
        catch (System.Exception ex)
        {
            statusText.text = $"Error: {ex.Message}";
        }
    }
}
