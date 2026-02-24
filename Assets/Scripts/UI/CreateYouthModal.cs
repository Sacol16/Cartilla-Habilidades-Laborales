using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CreateYouthModal : MonoBehaviour
{
    [Header("Config")]
    public ApiConfig apiConfig;

    [Header("UI Inputs")]
    public TMP_InputField emailInput;
    public TMP_InputField nameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;

    [Header("UI Feedback")]
    public TMP_Text feedbackText;
    public GameObject modalRoot; // panel/modal a cerrar

    [Header("Dependencies")]
    public YouthPanelController youthPanel; // el panel/lista que muestra miembros del grupo

    private AuthService _auth;

    [Serializable]
    private class CreateYouthRequest
    {
        public string name;
        public string email;
        public string tempPassword;
    }

    private void Awake()
    {
        _auth = new AuthService(apiConfig);
        if (modalRoot == null) modalRoot = gameObject;
        SetFeedback("");
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null) feedbackText.text = msg;
    }

    private bool Validate()
    {
        var email = emailInput != null ? emailInput.text.Trim() : "";
        var name = nameInput != null ? nameInput.text.Trim() : "";
        var pass = passwordInput != null ? passwordInput.text : "";
        var confirm = confirmPasswordInput != null ? confirmPasswordInput.text : "";

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            SetFeedback("Ingresa un correo válido.");
            return false;
        }

        if (name.Length < 2)
        {
            SetFeedback("El nombre debe tener al menos 2 caracteres.");
            return false;
        }

        if (pass.Length < 6)
        {
            SetFeedback("La contraseña debe tener al menos 6 caracteres.");
            return false;
        }

        if (pass != confirm)
        {
            SetFeedback("Las contraseñas no coinciden.");
            return false;
        }

        return true;
    }

    public async void OnClickAddYouth()
    {
        SetFeedback("");

        if (!Validate()) return;

        var groupId = PlayerPrefs.GetString("selected_group_id", "");
        if (string.IsNullOrEmpty(groupId))
        {
            SetFeedback("No hay grupo seleccionado.");
            return;
        }

        if (string.IsNullOrEmpty(_auth.Token))
        {
            SetFeedback("No hay sesión activa. Inicia sesión de nuevo.");
            return;
        }

        try
        {
            SetFeedback("Creando joven...");

            var req = new CreateYouthRequest
            {
                email = emailInput.text.Trim(),
                name = nameInput.text.Trim(),
                tempPassword = passwordInput.text
            };

            string url = $"{apiConfig.baseUrl}/groups/{groupId}/youths";
            string body = JsonUtility.ToJson(req);

            await RestClient.SendJson(url, "POST", body, _auth.Token);

            SetFeedback("? Joven creado. Actualizando...");

            // ? refresca y ESPERA a que termine
            if (youthPanel != null)
            {
                await youthPanel.LoadMembers(groupId);
            }

            // espera 3s y cierra modal
            await Task.Delay(3000);
            modalRoot.SetActive(false);

            // opcional: limpiar campos
            if (emailInput != null) emailInput.text = "";
            if (nameInput != null) nameInput.text = "";
            if (passwordInput != null) passwordInput.text = "";
            if (confirmPasswordInput != null) confirmPasswordInput.text = "";
        }
        catch (Exception ex)
        {
            SetFeedback($"Error: {ex.Message}");
        }
    }

}
