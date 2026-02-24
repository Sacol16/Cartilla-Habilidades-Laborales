using UnityEngine;
using TMPro;

public class Module1Activity2Presenter : MonoBehaviour
{
    [Header("Inputs (orden 0..4)")]
    [Tooltip("Arrastra los TMP_InputField en el mismo orden en que se guardan en activity2Answers")]
    public TMP_InputField[] inputs = new TMP_InputField[5];

    /// <summary>
    /// Aplica las respuestas obtenidas del backend:
    /// activity2Answers: ["resp1","resp2","resp3","resp4","resp5"]
    /// </summary>
    public void Apply(string[] answers)
    {
        if (inputs == null || inputs.Length == 0)
        {
            Debug.LogWarning("[Activity2Presenter] No hay inputs asignados.");
            return;
        }

        if (answers == null || answers.Length == 0)
        {
            Debug.Log("[Activity2Presenter] answers vacío.");
            return;
        }

        int count = Mathf.Min(inputs.Length, answers.Length);

        for (int i = 0; i < count; i++)
        {
            if (inputs[i] == null)
            {
                Debug.LogWarning($"[Activity2Presenter] Input {i} es null.");
                continue;
            }

            inputs[i].text = answers[i] ?? "";
        }

        Debug.Log($"[Activity2Presenter] Respuestas aplicadas: {count}");
    }

    /// <summary>
    /// Extrae el contenido actual de los inputs (útil para guardar).
    /// </summary>
    public string[] Extract()
    {
        if (inputs == null || inputs.Length == 0)
            return new string[0];

        string[] result = new string[inputs.Length];

        for (int i = 0; i < inputs.Length; i++)
        {
            result[i] = inputs[i] != null ? inputs[i].text : "";
        }

        return result;
    }

    /// <summary>
    /// Limpia todos los campos.
    /// </summary>
    public void Clear()
    {
        if (inputs == null) return;

        foreach (var input in inputs)
        {
            if (input != null)
                input.text = "";
        }
    }
}