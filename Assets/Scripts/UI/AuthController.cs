using System.Text.RegularExpressions;
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
    public TMP_InputField regConfirmPassword;
    public TMP_InputField regCode;

    [Header("Output")]
    public TMP_Text statusText; // mensajes de estado (API, cargando, etc.)

    [Header("Validation Messages (por campo)")]
    public TMP_Text emailValidationText;
    public TMP_Text passwordValidationText;
    public TMP_Text confirmPasswordValidationText;

    private AuthService _auth;

    // Email simple (suficiente para UI)
    private static readonly Regex EmailRegex =
        new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    private void Awake()
    {
        _auth = new AuthService(apiConfig);
        ClearMessages();
    }

    private void ClearMessages()
    {
        if (statusText != null) statusText.text = "";

        if (emailValidationText != null) emailValidationText.text = "";
        if (passwordValidationText != null) passwordValidationText.text = "";
        if (confirmPasswordValidationText != null) confirmPasswordValidationText.text = "";
    }

    private void ShowStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    private void ShowEmailValidation(string message)
    {
        if (emailValidationText != null) emailValidationText.text = message;
    }

    private void ShowPasswordValidation(string message)
    {
        if (passwordValidationText != null) passwordValidationText.text = message;
    }

    private void ShowConfirmPasswordValidation(string message)
    {
        if (confirmPasswordValidationText != null) confirmPasswordValidationText.text = message;
    }

    // -------- VALIDACIONES --------
    private bool ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowEmailValidation("El correo es obligatorio.");
            return false;
        }

        if (!EmailRegex.IsMatch(email))
        {
            ShowEmailValidation("Ingresa un correo válido (ej: nombre@dominio.com).");
            return false;
        }

        return true;
    }

    private bool ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            ShowPasswordValidation("La contraseña es obligatoria.");
            return false;
        }

        if (password.Length < 6)
        {
            ShowPasswordValidation("La contraseña debe tener al menos 6 caracteres.");
            return false;
        }

        return true;
    }

    private bool ValidateConfirmPassword(string password, string confirm)
    {
        if (string.IsNullOrEmpty(confirm))
        {
            ShowConfirmPasswordValidation("Debes repetir la contraseña.");
            return false;
        }

        if (password != confirm)
        {
            ShowConfirmPasswordValidation("Las contraseñas no coinciden.");
            return false;
        }

        return true;
    }

    // -------- BOTONES --------
    public async void OnClickLogin()
    {
        ClearMessages();

        string email = loginEmail != null ? loginEmail.text.Trim() : "";
        string password = loginPassword != null ? loginPassword.text : "";

        bool okEmail = ValidateEmail(email);
        bool okPass = ValidatePassword(password);

        if (!okEmail || !okPass) return;

        try
        {
            ShowStatus("Iniciando sesión...");
            await _auth.Login(email, password);
            ShowStatus($"OK ✅ Rol: {_auth.CurrentUser.role}");
        }
        catch (System.Exception ex)
        {
            ShowStatus($"Error: {ex.Message}");
        }
    }

    public async void OnClickRegisterFacilitator()
    {
        ClearMessages();

        string email = regEmail != null ? regEmail.text.Trim() : "";
        string password = regPassword != null ? regPassword.text : "";
        string confirm = regConfirmPassword != null ? regConfirmPassword.text : "";

        bool okEmail = ValidateEmail(email);
        bool okPass = ValidatePassword(password);
        bool okConfirm = okPass && ValidateConfirmPassword(password, confirm);

        if (!okEmail || !okPass || !okConfirm) return;

        try
        {
            ShowStatus("Registrando facilitador...");
            await _auth.RegisterFacilitator(
                email,
                regName != null ? regName.text.Trim() : "",
                password,
                regCode != null ? regCode.text.Trim() : ""
            );

            ShowStatus("Registro OK ✅ Ahora inicia sesión.");
        }
        catch (System.Exception ex)
        {
            ShowStatus($"Error: {ex.Message}");
        }
    }
}
