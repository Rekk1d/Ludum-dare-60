using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LockButtonView : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameController controller;
    [SerializeField] private Button button;
    [SerializeField] private Image label;
    [SerializeField] private Sprite labelSprite;
    [SerializeField] private Sprite labelDisabledSprite;

    private void Start()
    {
        controller.Session.Changed += Refresh;
    }

    private void OnDestroy()
    {
        if (controller != null && controller.Session != null)
        {
            controller.Session.Changed -= Refresh;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.interactable)
        {
            controller.Session.Lock();
        }
    }

    private void Refresh()
    {
        var canLock = controller.Session.CanLock;
        button.interactable = canLock;
        label.sprite = canLock ? labelSprite : labelDisabledSprite;
    }
}
