using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform area;
    [SerializeField] private RectTransform popup;
    [SerializeField] private Vector2 bottomPosition = new Vector2(219f, 129f);
    [SerializeField] private Vector2 topPosition = new Vector2(219f, 16f);
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text[] lines;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextText;
    [SerializeField] private string nextLabel = "NEXT >";
    [SerializeField] private string lastLabel = "ROGER >";
    [SerializeField] private RectTransform[] corners;
    [SerializeField, Min(0)] private int cornerGap = 1;
    [SerializeField, Min(0)] private int pulsePixels = 1;
    [SerializeField, Min(0.05f)] private float pulseInterval = 0.4f;
    [SerializeField, Min(1f)] private float charsPerSecond = 60f;

    private readonly Vector3[] _worldCorners = new Vector3[4];
    private Coroutine _typing;
    private bool _skip;
    private Vector2 _min;
    private Vector2 _max;
    private bool _highlighted;

    public event Action NextClicked;

    private void Awake()
    {
        nextButton.onClick.AddListener(OnNextClicked);
        Hide();
    }

    private void Update()
    {
        if (!_highlighted)
        {
            return;
        }

        var pulse = (int)(Time.unscaledTime / pulseInterval) % 2 * pulsePixels;
        PlaceCorners(cornerGap + pulse);
    }

    public void Show(string title, string body, bool top, bool last, RectTransform[] highlight)
    {
        var position = top ? topPosition : bottomPosition;
        popup.anchoredPosition = new Vector2(position.x, -position.y);
        popup.gameObject.SetActive(true);
        nextText.text = last ? lastLabel : nextLabel;
        titleText.text = title;
        var split = body.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i].text = i < split.Length ? split[i] : string.Empty;
        }

        SetHighlight(highlight);
        if (_typing != null)
        {
            StopCoroutine(_typing);
        }

        _typing = StartCoroutine(Type());
    }

    public void Hide()
    {
        if (_typing != null)
        {
            StopCoroutine(_typing);
            _typing = null;
        }

        popup.gameObject.SetActive(false);
        SetHighlight(null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _skip = true;
    }

    private void OnNextClicked()
    {
        NextClicked?.Invoke();
    }

    private IEnumerator Type()
    {
        _skip = false;
        titleText.maxVisibleCharacters = 0;
        foreach (var line in lines)
        {
            line.maxVisibleCharacters = 0;
        }

        var delay = new WaitForSeconds(1f / charsPerSecond);
        yield return TypeText(titleText, delay);
        foreach (var line in lines)
        {
            yield return TypeText(line, delay);
        }

        _typing = null;
    }

    private IEnumerator TypeText(TMP_Text text, WaitForSeconds delay)
    {
        var length = text.text.Length;
        for (var i = 1; i <= length && !_skip; i++)
        {
            text.maxVisibleCharacters = i;
            yield return delay;
        }

        text.maxVisibleCharacters = length;
    }

    private void SetHighlight(RectTransform[] rects)
    {
        var visible = rects != null && rects.Length > 0;
        _highlighted = visible;
        foreach (var corner in corners)
        {
            corner.gameObject.SetActive(visible);
        }

        if (!visible)
        {
            return;
        }

        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        foreach (var rect in rects)
        {
            rect.GetWorldCorners(_worldCorners);
            foreach (var point in _worldCorners)
            {
                Vector2 local = area.InverseTransformPoint(point);
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }
        }

        _min = new Vector2(Mathf.Round(min.x), Mathf.Round(min.y));
        _max = new Vector2(Mathf.Round(max.x), Mathf.Round(max.y));
        PlaceCorners(cornerGap);
    }

    private void PlaceCorners(int offset)
    {
        corners[0].anchoredPosition = new Vector2(_min.x - offset, _max.y + offset);
        corners[1].anchoredPosition = new Vector2(_max.x + offset, _max.y + offset);
        corners[2].anchoredPosition = new Vector2(_min.x - offset, _min.y - offset);
        corners[3].anchoredPosition = new Vector2(_max.x + offset, _min.y - offset);
    }
}
