using UnityEngine;
using UnityEngine.UI;

public class ChecklistManager : MonoBehaviour
{
    public Toggle[] tasks;
    public Button continueButton;

    void Update()
    {
        bool allDone = true;

        foreach (Toggle t in tasks)
        {
            if (!t.isOn)
                allDone = false;
        }

        continueButton.interactable = allDone;
    }
}