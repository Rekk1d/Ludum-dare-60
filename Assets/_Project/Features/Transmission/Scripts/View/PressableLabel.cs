using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PressableLabel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private RectTransform label;
    [SerializeField] private float pressedOffset = 3f;

    private Vector2 restPosition;
    private bool down;
    private bool inside;

    private void Awake()
    {
        restPosition = label.anchoredPosition;
    }

    private void OnDisable()
    {
        down = false;
        inside = false;
        Apply();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        down = true;
        Apply();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        down = false;
        Apply();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        inside = true;
        Apply();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inside = false;
        Apply();
    }

    private void Apply()
    {
        var pressed = down && inside && button.interactable;
        label.anchoredPosition = restPosition + (pressed ? new Vector2(0f, -pressedOffset) : Vector2.zero);
    }
}
