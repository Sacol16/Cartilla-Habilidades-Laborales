using UnityEngine;
using UnityEngine.UI;

public class CharacterTransformationController : MonoBehaviour
{
    public Image characterImage;

    [Tooltip("Deben ser 11 sprites: 0..4 malos, 5 mid, 6..10 buenos")]
    public Sprite[] forms; // size 11

    [Tooltip("Índice del estado MID dentro del array forms (normalmente 5).")]
    public int midIndex = 5;

    public int Index { get; private set; }

    private void Awake()
    {
        if (characterImage == null) characterImage = GetComponent<Image>();
        ResetToMid();
    }

    public void ResetToMid()
    {
        SetIndex(midIndex);
    }

    public void Step(int delta)
    {
        SetIndex(Index + delta);
    }

    public void SetIndex(int idx)
    {
        if (forms == null || forms.Length == 0) return;

        Index = Mathf.Clamp(idx, 0, forms.Length - 1);

        if (characterImage != null)
        {
            characterImage.sprite = forms[Index];
            characterImage.preserveAspect = true;
        }
    }

    public bool IsMaxGood()
    {
        return forms != null && forms.Length > 0 && Index == forms.Length - 1;
    }
}