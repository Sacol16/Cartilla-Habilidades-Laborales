using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance;

    public GameObject[] screens;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowScreen(int index)
    {
        for (int i = 0; i < screens.Length; i++)
        {
            screens[i].SetActive(false);
        }

        screens[index].SetActive(true);
    }
}