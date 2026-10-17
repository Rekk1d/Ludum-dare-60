using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecodeSlot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject display;
    [SerializeField] private TMP_Text displayText;

    public Button Button => button;

    public void SetState(bool revealed, bool interactable, string value)
    {
        gameObject.SetActive(true);
        button.gameObject.SetActive(!revealed);
        button.interactable = interactable;
        display.SetActive(revealed);
        displayText.text = revealed ? value : string.Empty;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
