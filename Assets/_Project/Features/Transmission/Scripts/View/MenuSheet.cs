using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuSheet : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform paper;
    [SerializeField] private TMP_Text[] typedTexts;
    [SerializeField] private GameObject resultStamp;
    [SerializeField] private GameObject[] shownWithOptions;
    [SerializeField] private MenuOption[] options;
    [SerializeField] private GameObject approvedStamp;
    [SerializeField, Min(0f)] private float startDelay = 0.4f;
    [SerializeField, Min(1f)] private float charsPerSecond = 40f;
    [SerializeField, Min(0f)] private float linePause = 0.15f;
    [SerializeField, Min(0f)] private float stampPause = 0.4f;
    [SerializeField, Min(0f)] private float checkPause = 0.3f;
    [SerializeField, Min(0f)] private float approvedPause = 0.7f;
    [SerializeField, Min(0.01f)] private float slideDuration = 0.4f;
    [SerializeField] private float offscreenDistance = 270f;
    [SerializeField, Min(0)] private int shakePixels = 2;
    [SerializeField, Min(0.01f)] private float shakeStep = 0.06f;

    private Vector2 _restPosition;
    private string[] _fullTexts;
    private bool _skip;

    public event Action<int> OptionClicked;

    private void Awake()
    {
        _restPosition = paper.anchoredPosition;
        _fullTexts = new string[typedTexts.Length];
        for (var i = 0; i < typedTexts.Length; i++)
        {
            _fullTexts[i] = typedTexts[i].text;
        }

        foreach (var option in options)
        {
            option.Clicked += OnOptionClicked;
        }
    }

    public void Open(bool slideIn)
    {
        gameObject.SetActive(true);
        StartCoroutine(Play(slideIn));
    }

    public void Approve(int index, Action done)
    {
        StartCoroutine(ApproveRoutine(index, done));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _skip = true;
    }

    private void OnOptionClicked(MenuOption option)
    {
        OptionClicked?.Invoke(Array.IndexOf(options, option));
    }

    private IEnumerator Play(bool slideIn)
    {
        _skip = false;
        foreach (var text in typedTexts)
        {
            text.maxVisibleCharacters = 0;
        }

        if (resultStamp != null)
        {
            resultStamp.SetActive(false);
        }

        approvedStamp.SetActive(false);
        foreach (var item in shownWithOptions)
        {
            item.SetActive(false);
        }

        foreach (var option in options)
        {
            option.ResetState();
            option.gameObject.SetActive(false);
        }

        if (slideIn)
        {
            yield return Slide(Offscreen(), _restPosition, false);
        }
        else
        {
            paper.anchoredPosition = _restPosition;
        }

        if (!_skip)
        {
            yield return new WaitForSeconds(startDelay);
        }

        var delay = new WaitForSeconds(1f / charsPerSecond);
        var pause = new WaitForSeconds(linePause);
        for (var i = 0; i < typedTexts.Length; i++)
        {
            var full = _fullTexts[i];
            for (var c = 0; c < full.Length && !_skip; c++)
            {
                typedTexts[i].maxVisibleCharacters = c + 1;
                yield return full[c] == '\n' ? pause : delay;
            }

            if (_skip)
            {
                typedTexts[i].maxVisibleCharacters = full.Length;
                continue;
            }

            yield return pause;
        }

        if (resultStamp != null)
        {
            if (!_skip)
            {
                yield return new WaitForSeconds(stampPause);
            }

            resultStamp.SetActive(true);
            yield return Shake();
        }

        foreach (var item in shownWithOptions)
        {
            item.SetActive(true);
        }

        foreach (var option in options)
        {
            option.gameObject.SetActive(true);
            option.SetInteractable(true);
        }
    }

    private IEnumerator ApproveRoutine(int index, Action done)
    {
        foreach (var option in options)
        {
            option.SetInteractable(false);
        }

        options[index].Choose();
        yield return new WaitForSeconds(checkPause);
        approvedStamp.SetActive(true);
        yield return Shake();
        yield return new WaitForSeconds(approvedPause);
        yield return Slide(_restPosition, Offscreen(), true);
        done?.Invoke();
        gameObject.SetActive(false);
    }

    private IEnumerator Slide(Vector2 from, Vector2 to, bool accelerate)
    {
        for (var time = 0f; time < slideDuration; time += Time.deltaTime)
        {
            var t = time / slideDuration;
            t = accelerate ? t * t : 1f - (1f - t) * (1f - t);
            var position = Vector2.Lerp(from, to, t);
            paper.anchoredPosition = new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
            yield return null;
        }

        paper.anchoredPosition = to;
    }

    private IEnumerator Shake()
    {
        var step = new WaitForSeconds(shakeStep);
        paper.anchoredPosition = _restPosition + new Vector2(shakePixels, 0f);
        yield return step;
        paper.anchoredPosition = _restPosition - new Vector2(shakePixels, 0f);
        yield return step;
        paper.anchoredPosition = _restPosition;
    }

    private Vector2 Offscreen()
    {
        return _restPosition + Vector2.down * offscreenDistance;
    }
}
