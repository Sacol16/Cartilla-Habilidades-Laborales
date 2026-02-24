using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GroupButtonItem : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text countText;
    public Button button;

    private string _groupId;

    public void Setup(string groupId, string groupName, int memberCount, Action<string> onClick)
    {
        _groupId = groupId;

        if (nameText != null) nameText.text = groupName;
        if (countText != null) countText.text = $"Estudiantes: {memberCount}";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(_groupId));
        }
    }
}
