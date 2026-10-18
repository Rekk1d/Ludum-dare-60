using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PressableLabel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private RectTransform label;
    [SerializeField] private float pressedOffset = 3f;

    private Vector2 _restPosition;
    private bool _down;
    private bool _inside;

    private void Awake()
    {
        _restPosition = label.anchoredPosition;
    }

    private void OnDisable()
    {
        _down = false;
        _inside = false;
        Apply();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _down = true;
        Apply();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _down = false;
        Apply();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _inside = true;
        Apply();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _inside = false;
        Apply();
    }

    private void Apply()
    {
        var pressed = _down && _inside && button.interactable;
        label.anchoredPosition = _restPosition + (pressed ? new Vector2(0f, -pressedOffset) : Vector2.zero);
    }
}
