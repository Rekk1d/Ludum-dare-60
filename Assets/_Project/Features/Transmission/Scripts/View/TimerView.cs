using TMPro;
using UnityEngine;

public class TimerView : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private TMP_Text text;

    private int _shown = int.MinValue;

    private void Update()
    {
        var session = controller.Session;
        var seconds = session.HasTimer ? Mathf.CeilToInt(session.TimeLeft) : -1;
        if (seconds == _shown)
        {
            return;
        }

        _shown = seconds;
        text.text = seconds < 0 ? "-:--" : $"{seconds / 60}:{seconds % 60:00}";
    }
}
