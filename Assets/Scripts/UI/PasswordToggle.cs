using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordToggleTMP : MonoBehaviour
{
    [Serializable]
    public class ToggleBinding
    {
        public Toggle toggle;
        public TMP_InputField[] fields;
        public bool defaultShow = false;
    }

    [Header("Bindings (cada toggle controla sus campos)")]
    public ToggleBinding[] bindings;

    private void Awake()
    {
        if (bindings == null) return;

        foreach (var b in bindings)
        {
            if (b.toggle == null) continue;

            // Para evitar capturas raras en foreach:
            var localBinding = b;

            // Estado inicial
            if (localBinding.toggle != null)
                localBinding.toggle.isOn = localBinding.defaultShow;

            localBinding.toggle.onValueChanged.AddListener(show => Apply(localBinding, show));

            Apply(localBinding, localBinding.toggle.isOn);
        }
    }

    private void OnDestroy()
    {
        if (bindings == null) return;

        foreach (var b in bindings)
        {
            if (b.toggle == null) continue;
            b.toggle.onValueChanged.RemoveAllListeners(); // simple y seguro
        }
    }

    private void Apply(ToggleBinding binding, bool show)
    {
        if (binding.fields == null) return;

        foreach (var f in binding.fields)
        {
            if (f == null) continue;

            f.contentType = show ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            f.inputType   = show ? TMP_InputField.InputType.Standard : TMP_InputField.InputType.Password;

            bool wasFocused = f.isFocused;
            f.DeactivateInputField();

            // Refresca render
            f.SetTextWithoutNotify(f.text);
            f.ForceLabelUpdate();

            if (wasFocused) f.ActivateInputField();
        }
    }
}
