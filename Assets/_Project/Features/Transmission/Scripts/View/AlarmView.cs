using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AlarmView : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private TerminalView terminal;
    [SerializeField] private AudioSource siren;
    [SerializeField] private Image lamp;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;
    [SerializeField, Min(0.05f)] private float blinkInterval = 0.25f;
    [SerializeField, Min(0f)] private float fadeOut = 0.2f;

    private Coroutine _blink;
    private Coroutine _fade;
    private float _volume;

    private void Start()
    {
        _volume = siren.volume;
        lamp.sprite = offSprite;
        terminal.IntroPrinted += OnIntroPrinted;
        controller.Session.RoundStarted += OnRoundStarted;
    }

    private void OnDestroy()
    {
        if (terminal != null)
        {
            terminal.IntroPrinted -= OnIntroPrinted;
        }

        if (controller != null && controller.Session != null)
        {
            controller.Session.RoundStarted -= OnRoundStarted;
        }
    }

    public void StartAlarm(bool loop)
    {
        Halt();
        siren.loop = loop;
        siren.Play();
        _blink = StartCoroutine(Blink(loop ? float.MaxValue : siren.clip.length));
    }

    public void StopAlarm()
    {
        StopBlink();
        if (siren.isPlaying && _fade == null)
        {
            _fade = StartCoroutine(FadeOut());
        }
    }

    private void OnIntroPrinted()
    {
        var session = controller.Session;
        if (session.IsReplay && session.TimerScrambled)
        {
            StartAlarm(false);
        }
    }

    private void OnRoundStarted()
    {
        Halt();
    }

    private void Halt()
    {
        StopBlink();
        if (_fade != null)
        {
            StopCoroutine(_fade);
            _fade = null;
        }

        siren.Stop();
        siren.volume = _volume;
    }

    private void StopBlink()
    {
        if (_blink != null)
        {
            StopCoroutine(_blink);
            _blink = null;
        }

        lamp.sprite = offSprite;
    }

    private IEnumerator Blink(float duration)
    {
        var wait = new WaitForSeconds(blinkInterval);
        for (var time = 0f; time < duration; time += blinkInterval)
        {
            lamp.sprite = lamp.sprite == onSprite ? offSprite : onSprite;
            yield return wait;
        }

        _blink = null;
        StopAlarm();
    }

    private IEnumerator FadeOut()
    {
        for (var time = 0f; time < fadeOut; time += Time.deltaTime)
        {
            siren.volume = _volume * (1f - time / fadeOut);
            yield return null;
        }

        siren.Stop();
        siren.volume = _volume;
        _fade = null;
    }
}
