using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LampStrip : MonoBehaviour
{
    [SerializeField] private Image template;
    [SerializeField, Min(1)] private int capacity = 3;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite deadSprite;
    [SerializeField, Min(0)] private int blinkCount;
    [SerializeField, Min(0.01f)] private float blinkInterval = 0.1f;

    private readonly List<Image> _lamps = new List<Image>();
    private int _visible;
    private int _lit = -1;
    private int _blinkFrom;
    private int _blinkTo;
    private Coroutine _blink;

    public int Capacity => capacity;

    private void Awake()
    {
        Build();
    }

    public void SetState(int visible, int lit)
    {
        Build();
        if (_blink != null && lit > _lit)
        {
            StopCoroutine(_blink);
            _blink = null;
        }

        if (blinkCount > 0 && _lit > lit && isActiveAndEnabled)
        {
            if (_blink != null)
            {
                StopCoroutine(_blink);
            }

            _blink = StartCoroutine(Blink(lit, _lit));
        }

        _visible = visible;
        _lit = lit;
        Apply();
    }

    private void Apply()
    {
        for (var i = 0; i < _lamps.Count; i++)
        {
            if (_blink != null && i >= _blinkFrom && i < _blinkTo)
            {
                continue;
            }

            _lamps[i].sprite = i < _lit ? onSprite : i < _visible || deadSprite == null ? offSprite : deadSprite;
        }
    }

    private IEnumerator Blink(int from, int to)
    {
        _blinkFrom = from;
        _blinkTo = to;
        var wait = new WaitForSeconds(blinkInterval);
        for (var step = 0; step < blinkCount * 2; step++)
        {
            for (var i = from; i < to; i++)
            {
                _lamps[i].sprite = step % 2 == 1 ? onSprite : offSprite;
            }

            yield return wait;
        }

        _blink = null;
        Apply();
    }

    private void Build()
    {
        if (_lamps.Count > 0)
        {
            return;
        }

        _lamps.Add(template);
        var step = template.rectTransform.rect.width + spacing;
        for (var i = 1; i < capacity; i++)
        {
            var lamp = Instantiate(template, template.transform.parent);
            lamp.rectTransform.anchoredPosition = template.rectTransform.anchoredPosition + new Vector2(step * i, 0f);
            _lamps.Add(lamp);
        }
    }
}
