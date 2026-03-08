using UnityEngine;

public class ExplorationManager : MonoBehaviour
{
    public bool finance;
    public bool production;
    public bool communication;

    public GameObject continueButton;

    public void ClickFinance()
    {
        finance = true;
        CheckProgress();
    }

    public void ClickProduction()
    {
        production = true;
        CheckProgress();
    }

    public void ClickCommunication()
    {
        communication = true;
        CheckProgress();
    }

    void CheckProgress()
    {
        if (finance && production && communication)
        {
            continueButton.SetActive(true);
        }
    }
}