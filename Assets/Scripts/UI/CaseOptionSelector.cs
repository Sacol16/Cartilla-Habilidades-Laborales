using UnityEngine;
using UnityEngine.UI;

public class CaseOptionSelector : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private Button[] optionButtons;

    [SerializeField] private string[] answers;

    [Header("Colors")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color unselectedColor = Color.red;

    [Header("UI to enable")]
    [SerializeField] private GameObject enableWhenSelected;

    [Header("Activity Manager (Assign in Inspector)")]
    [Tooltip("Arrastra aquí Module1ActivityManager o Module2ActivityManager")]
    [SerializeField] private MonoBehaviour activityManager;

    private IActivity4Receiver mgr;

    [Header("State (read-only)")]
    [SerializeField] private int selectedIndex = -1;

    private void Awake()
    {
        // Resolver manager
        mgr = activityManager as IActivity4Receiver;
        if (mgr == null && activityManager != null)
            mgr = activityManager.GetComponent<IActivity4Receiver>();

        if (mgr == null)
            Debug.LogError("[CaseOptionSelector] ActivityManager no asignado o no implementa IActivity4Receiver.");

        if (optionButtons == null || optionButtons.Length == 0)
        {
            Debug.LogWarning("[CaseOptionSelector] No hay optionButtons asignados.");
            return;
        }

        if (answers == null || answers.Length != optionButtons.Length)
        {
            Debug.LogWarning("[CaseOptionSelector] 'answers' debe tener el mismo tamaño que 'optionButtons'.");
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            if (optionButtons[i] != null)
                optionButtons[i].onClick.AddListener(() => SelectOption(index));
        }

        RefreshVisuals();
    }

    public void SelectOption(int index)
    {
        if (index < 0 || index >= optionButtons.Length) return;

        if (selectedIndex == index) return;

        selectedIndex = index;
        RefreshVisuals();

        if (mgr != null)
            mgr.SetActivity4SelectedOption(GetSelectedAnswer());
    }

    public void ClearSelection()
    {
        selectedIndex = -1;
        RefreshVisuals();

        if (mgr != null)
            mgr.ClearActivity4Selection();
    }

    public int GetSelectedIndex() => selectedIndex;

    public bool HasSelection() => selectedIndex >= 0;

    public string GetSelectedAnswer()
    {
        if (selectedIndex < 0) return "";
        if (answers == null) return "";
        if (selectedIndex >= answers.Length) return "";
        return answers[selectedIndex] ?? "";
    }

    private void RefreshVisuals()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            var btn = optionButtons[i];
            if (btn == null) continue;

            var img = btn.image;
            if (img == null) continue;

            img.color = (i == selectedIndex) ? selectedColor : unselectedColor;
        }

        if (enableWhenSelected != null)
            enableWhenSelected.SetActive(HasSelection());
    }
}