using TMPro;
using UnityEngine;

public class TimerView : MonoBehaviour
{
    private const string ScrambleChars = "0123456789-?";

    [SerializeField] private GameController controller;
    [SerializeField] private TMP_Text text;
    [SerializeField, Min(0.01f)] private float scrambleInterval = 0.08f;
    [SerializeField, Min(0f)] private float revealDuration = 1f;
    [SerializeField] private Vector2 firstRevealRange = new Vector2(0.1f, 0.33f);
    [SerializeField] private Vector2 secondRevealRange = new Vector2(0.67f, 0.9f);

    private readonly float[] _reveals = new float[2];
    private int _roundIndex = -1;
    private int _shown = int.MinValue;
    private float _nextScramble;

    private void Update()
    {
        var session = controller.Session;
        if (!session.IsStarted)
        {
            if (_shown != int.MaxValue)
            {
                _shown = int.MaxValue;
                text.text = string.Empty;
            }

            return;
        }

        if (session.RoundIndex != _roundIndex)
        {
            _roundIndex = session.RoundIndex;
            _reveals[0] = Random.Range(firstRevealRange.x, firstRevealRange.y) * session.TimerTotal;
            _reveals[1] = Random.Range(secondRevealRange.x, secondRevealRange.y) * session.TimerTotal;
        }

        if (session.TimerScrambled && !Revealed(session))
        {
            Scramble();
            return;
        }

        var seconds = session.HasTimer ? Mathf.CeilToInt(session.TimeLeft) : -1;
        if (seconds == _shown)
        {
            return;
        }

        _shown = seconds;
        text.text = seconds < 0 ? "-:--" : $"{seconds / 60}:{seconds % 60:00}";
    }

    private bool Revealed(GameSession session)
    {
        var elapsed = session.TimerTotal - session.TimeLeft;
        foreach (var start in _reveals)
        {
            if (elapsed >= start && elapsed < start + revealDuration)
            {
                return true;
            }
        }

        return false;
    }

    private void Scramble()
    {
        if (Time.time < _nextScramble)
        {
            return;
        }

        _nextScramble = Time.time + scrambleInterval;
        _shown = int.MinValue;
        text.text = $"{RandomChar()}:{RandomChar()}{RandomChar()}";
    }

    private static char RandomChar()
    {
        return ScrambleChars[Random.Range(0, ScrambleChars.Length)];
    }
}
