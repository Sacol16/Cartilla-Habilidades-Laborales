using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StudentButtonItem : MonoBehaviour
{
    public TMP_Text nameText;
    public Button button;

    private string _studentId;

    public void Setup(string studentId, string name, Action<string> onClick)
    {
        _studentId = studentId;
        if (nameText != null) nameText.text = name;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(_studentId));
        }
    }
}
