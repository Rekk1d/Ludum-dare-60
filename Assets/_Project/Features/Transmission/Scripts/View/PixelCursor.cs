using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PixelCursor : MonoBehaviour
{
    [SerializeField] private RectTransform area;
    [SerializeField] private Image image;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Vector2 hotspot = new Vector2(5f, 0f);

    private void OnEnable()
    {
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void LateUpdate()
    {
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(area, mouse.position.ReadValue(), null, out var local);
        image.rectTransform.anchoredPosition = new Vector2(Mathf.Round(local.x - hotspot.x), Mathf.Round(local.y + hotspot.y));
        image.sprite = mouse.leftButton.isPressed ? pressedSprite : normalSprite;
    }
}
