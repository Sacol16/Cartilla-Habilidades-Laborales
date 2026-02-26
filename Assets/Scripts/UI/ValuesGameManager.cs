using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValuesGameManager : MonoBehaviour
{
    [Header("Character UI")]
    public Image characterImage;

    [Tooltip("11 sprites: 0..4 malos, 5 mid, 6..10 buenos")]
    public Sprite[] states; // size 11
    public int midIndex = 5;

    [Header("Win")]
    public int requiredGoodToWin = 5;
    public GameObject winPanel;

    private int currentIndex;
    private readonly Dictionary<int, ValueUIData> placedBySlot = new Dictionary<int, ValueUIData>();
    private readonly HashSet<string> goodPlacedIds = new HashSet<string>();

    public bool IsLocked { get; private set; }

    private void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        SetState(midIndex);
    }

    private void SetState(int idx)
    {
        if (states == null || states.Length == 0 || characterImage == null) return;

        currentIndex = Mathf.Clamp(idx, 0, states.Length - 1);
        characterImage.sprite = states[currentIndex];
        characterImage.preserveAspect = true;
    }

    private void Step(int delta)
    {
        SetState(currentIndex + delta);
    }

    // Llamado cuando un slot coloca un item
    public void RegisterPlacement(int slotIndex, ValueUIData data)
    {
        if (IsLocked || data == null) return;

        placedBySlot[slotIndex] = data;

        if (data.isGood)
        {
            goodPlacedIds.Add(data.id);
            Step(+1);
        }
        else
        {
            Step(-1);
        }

        CheckWin();
    }

    // Llamado cuando un slot se vacía (por replace o undo)
    public void RegisterRemoval(int slotIndex, ValueUIData data)
    {
        if (data == null) return;

        placedBySlot.Remove(slotIndex);

        // Revertir efecto del que se está quitando
        if (data.isGood)
        {
            goodPlacedIds.Remove(data.id);
            Step(-1);
        }
        else
        {
            Step(+1);
        }
    }

    private void CheckWin()
    {
        bool byGoodSlots = goodPlacedIds.Count >= requiredGoodToWin;
        bool byMaxState = currentIndex >= states.Length - 1;

        if (byGoodSlots || byMaxState)
        {
            IsLocked = true;
            if (winPanel != null) winPanel.SetActive(true);
        }
    }
}