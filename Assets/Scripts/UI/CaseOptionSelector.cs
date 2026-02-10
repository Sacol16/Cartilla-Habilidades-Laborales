using UnityEngine;
using UnityEngine.UI;

public class CaseOptionSelector : MonoBehaviour
{
    [Header("Options")]
    [Tooltip("Arrastra aquí los botones de respuesta (en el orden que quieras).")]
    [SerializeField] private Button[] optionButtons;

    [Tooltip("Texto/ID de cada opción. Debe tener el MISMO tamaño y orden que optionButtons.")]
    [SerializeField] private string[] answers;

    [Header("Colors")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color unselectedColor = Color.red;

    [Header("State (read-only)")]
    [SerializeField] private int selectedIndex = -1;

    private void Awake()
    {
        // Validación rápida
        if (optionButtons == null || optionButtons.Length == 0)
        {
            Debug.LogWarning("[CaseOptionSelector] No hay optionButtons asignados.");
            return;
        }

        if (answers == null || answers.Length != optionButtons.Length)
        {
            Debug.LogWarning("[CaseOptionSelector] 'answers' debe tener el mismo tamaño que 'optionButtons'.");
        }

        // Vincular listeners
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i; // captura segura
            if (optionButtons[i] != null)
                optionButtons[i].onClick.AddListener(() => SelectOption(index));
        }

        RefreshVisuals();
    }

    /// <summary>
    /// Selecciona una opción (solo puede haber 1 activa).
    /// Si seleccionas otra, deselecciona la anterior.
    /// </summary>
    public void SelectOption(int index)
    {
        if (optionButtons == null || optionButtons.Length == 0) return;
        if (index < 0 || index >= optionButtons.Length) return;

        // Si ya estaba seleccionada esa misma, no hacemos nada (sigue seleccionada)
        if (selectedIndex == index) return;

        selectedIndex = index;
        RefreshVisuals();
    }

    /// <summary>
    /// Limpia la selección (opcional).
    /// </summary>
    public void ClearSelection()
    {
        selectedIndex = -1;
        RefreshVisuals();
    }

    public int GetSelectedIndex() => selectedIndex;

    public bool HasSelection() => selectedIndex >= 0;

    /// <summary>
    /// Devuelve el texto/ID de la opción seleccionada. Retorna "" si no hay selección o si answers no está bien configurado.
    /// </summary>
    public string GetSelectedAnswer()
    {
        if (selectedIndex < 0) return "";
        if (answers == null) return "";
        if (selectedIndex >= answers.Length) return "";
        return answers[selectedIndex] ?? "";
    }

    private void RefreshVisuals()
    {
        if (optionButtons == null) return;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            var btn = optionButtons[i];
            if (btn == null) continue;

            var img = btn.image;
            if (img == null) continue;

            img.color = (i == selectedIndex) ? selectedColor : unselectedColor;
        }
    }
}