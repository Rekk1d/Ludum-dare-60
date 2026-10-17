using UnityEngine;
using UnityEngine.UI;

public class DefenseButton : MonoBehaviour
{
    [SerializeField] private DefenseType type;
    [SerializeField] private Button button;
    [SerializeField] private Image image;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite disabledSprite;

    public DefenseType Type => type;
    public Button Button => button;

    public void SetState(bool selected, bool enabled)
    {
        button.interactable = enabled;
        image.sprite = selected ? onSprite : enabled ? offSprite : disabledSprite;
    }
}
