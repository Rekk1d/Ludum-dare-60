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

    private readonly List<Image> lamps = new List<Image>();

    private void Awake()
    {
        Build();
    }

    public void SetState(int visible, int lit)
    {
        Build();
        for (var i = 0; i < lamps.Count; i++)
        {
            lamps[i].gameObject.SetActive(i < visible);
            lamps[i].sprite = i < lit ? onSprite : offSprite;
        }
    }

    private void Build()
    {
        if (lamps.Count > 0)
        {
            return;
        }

        lamps.Add(template);
        var step = template.rectTransform.rect.width + spacing;
        for (var i = 1; i < capacity; i++)
        {
            var lamp = Instantiate(template, template.transform.parent);
            lamp.rectTransform.anchoredPosition = template.rectTransform.anchoredPosition + new Vector2(step * i, 0f);
            lamps.Add(lamp);
        }
    }
}
