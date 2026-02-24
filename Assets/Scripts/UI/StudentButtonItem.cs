using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StudentButtonItem : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text scoreText;    // NUEVO: "10%"
    public Slider progressSlider; // NUEVO: 0..1
    public Button button;

    private string _studentId;

    public void Setup(string studentId, string name, Action<string> onClick)
    {
        _studentId = studentId;

        if (nameText != null) nameText.text = name;

        // estado inicial
        SetProgress01(0f);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(_studentId));
        }
    }

    public void SetProgress01(float value01)
    {
        value01 = Mathf.Clamp01(value01);

        if (progressSlider != null) progressSlider.value = value01;
        if (scoreText != null) scoreText.text = Mathf.RoundToInt(value01 * 100f) + "%";
    }

    public string StudentId => _studentId;
}