using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CreateGroupModal : MonoBehaviour
{
    [Header("Config")]
    public ApiConfig apiConfig;

    [Header("UI")]
    public TMP_InputField groupNameInput;
    public TMP_Text feedbackText;
    public GameObject modalRoot; // el objeto del modal (panel)

    [Header("Dependencies")]
    public FacilitatorGroupsList groupsList; // referencia al script que lista/instancia botones

    private AuthService _auth;

    [Serializable]
    private class CreateGroupRequest { public string name; }

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

    public async void OnClickCreateGroup()
    {
        SetFeedback("");

        var name = groupNameInput != null ? groupNameInput.text.Trim() : "";
        if (name.Length < 2)
        {
            SetFeedback("El nombre del grupo debe tener al menos 2 caracteres.");
            return;
        }

        if (string.IsNullOrEmpty(_auth.Token))
        {
            SetFeedback("No hay sesión activa. Inicia sesión de nuevo.");
            return;
        }

        try
        {
            SetFeedback("Creando grupo...");

            string url = $"{apiConfig.baseUrl}/groups";
            string body = JsonUtility.ToJson(new CreateGroupRequest { name = name });

            // POST /groups (token facilitador)
            await RestClient.SendJson(url, "POST", body, _auth.Token);

            SetFeedback("? Grupo creado. Actualizando lista...");

            // Recargar la lista de grupos
            if (groupsList != null)
                await groupsList.LoadGroups();

            // esperar 3 segundos y cerrar modal
            await Task.Delay(3000);

            if (modalRoot != null)
                modalRoot.SetActive(false);

            // opcional: limpiar input
            if (groupNameInput != null)
                groupNameInput.text = "";
        }
        catch (Exception ex)
        {
            SetFeedback($"Error: {ex.Message}");
        }
    }
}
