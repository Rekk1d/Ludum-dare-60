using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class VolumeSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform track;
    [SerializeField] private RectTransform handle;
    [SerializeField, Min(1)] private int steps = 10;
    [SerializeField, Min(1f)] private float trackLength = 163f;
    [SerializeField] private float handleOffset = -3f;

    private int _value = -1;

    public event Action<int> Changed;

    public int Steps => steps;

    public void SetValue(int value)
    {
        _value = Mathf.Clamp(value, 0, steps);
        Place();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Pick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Pick(eventData);
    }

    private void Pick(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(track, eventData.position, eventData.pressEventCamera, out var local);
        var value = Mathf.Clamp(Mathf.RoundToInt((local.x - track.rect.xMin) / trackLength * steps), 0, steps);
        if (value == _value)
        {
            return;
        }

        _value = value;
        Place();
        Changed?.Invoke(value);
    }

    private void Place()
    {
        var x = track.anchoredPosition.x + Mathf.RoundToInt(_value * trackLength / steps) + handleOffset;
        handle.anchoredPosition = new Vector2(x, handle.anchoredPosition.y);
    }
}
