using System;
using System.Collections;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private TutorialView view;
    [SerializeField] private TerminalView terminal;
    [SerializeField] private SectorRowView[] rows;
    [SerializeField] private TutorialTip[] tips;
    [SerializeField, Min(0f)] private float tipDelay = 2f;
    [SerializeField] private AlarmView alarm;

    private Coroutine _running;
    private bool _next;

    private GameSession Session => controller.Session;

    public bool TimerHidden { get; private set; }

    private void Start()
    {
        view.NextClicked += OnNext;
        Session.RoundStarted += OnRoundStarted;
    }

    private void OnDestroy()
    {
        if (view != null)
        {
            view.NextClicked -= OnNext;
        }

        if (controller != null && controller.Session != null)
        {
            controller.Session.RoundStarted -= OnRoundStarted;
        }
    }

    public void BeforeClock(Action startClock)
    {
        Stop();
        if (Session.IsReplay)
        {
            startClock();
            return;
        }

        _running = StartCoroutine(RunRound(Session.RoundIndex, startClock));
    }

    private void OnRoundStarted()
    {
        Stop();
    }

    private void OnNext()
    {
        _next = true;
    }

    private void Stop()
    {
        if (_running != null)
        {
            StopCoroutine(_running);
            _running = null;
        }

        TimerHidden = false;
        view.Hide();
    }

    private IEnumerator RunRound(int round, Action startClock)
    {
        var delayed = false;
        TimerHidden = false;
        foreach (var tip in tips)
        {
            if (tip.Round == round && tip.BeforeClock && tip.RevealsTimer)
            {
                TimerHidden = true;
            }
        }

        for (var i = 0; i < tips.Length; i++)
        {
            if (tips[i].Round == round && tips[i].BeforeClock)
            {
                if (!delayed)
                {
                    delayed = true;
                    yield return new WaitForSeconds(terminal.LastSkipped ? 0f : tipDelay);
                }

                if (tips[i].RevealsTimer)
                {
                    TimerHidden = false;
                }

                yield return Show(i);
            }
        }

        TimerHidden = false;
        startClock();
        for (var i = 0; i < tips.Length; i++)
        {
            var tip = tips[i];
            if (tip.Round != round || tip.BeforeClock || Done(tip.Goal))
            {
                continue;
            }

            if (!delayed)
            {
                delayed = true;
                yield return new WaitForSeconds(terminal.LastSkipped ? 0f : tipDelay);
                if (Done(tip.Goal))
                {
                    continue;
                }
            }

            if (Session.Phase != GamePhase.Planning)
            {
                break;
            }

            yield return Show(i);
        }

        _running = null;
    }

    private IEnumerator Show(int index)
    {
        var tip = tips[index];
        _next = false;
        view.Show(tip.Title, tip.Body, tip.Top, IsLast(index), Highlight(tip));
        if (tip.Alarm)
        {
            alarm.StartAlarm(true);
        }

        while (!_next && !Done(tip.Goal) && (tip.BeforeClock || Session.Phase == GamePhase.Planning))
        {
            yield return null;
        }

        if (tip.Alarm)
        {
            alarm.StopAlarm();
        }

        view.Hide();
    }

    private bool IsLast(int index)
    {
        var tip = tips[index];
        for (var i = 0; i < tips.Length; i++)
        {
            var other = tips[i];
            if (i == index || other.Round != tip.Round || Done(other.Goal))
            {
                continue;
            }

            var later = other.BeforeClock == tip.BeforeClock ? i > index : tip.BeforeClock;
            if (later)
            {
                return false;
            }
        }

        return true;
    }

    private bool Done(TutorialGoal goal)
    {
        var session = Session;
        switch (goal)
        {
            case TutorialGoal.Lock:
                return Exists(attack => attack.Locked);
            case TutorialGoal.DecodeType:
                return Exists(attack => attack.TypeRevealed);
            case TutorialGoal.Defense:
                return session.OrdersLeft < session.OrdersTotal;
            case TutorialGoal.Confirm:
                return session.Phase != GamePhase.Planning;
            default:
                return false;
        }
    }

    private RectTransform[] Highlight(TutorialTip tip)
    {
        switch (tip.Highlight)
        {
            case TutorialHighlight.Targets:
                return tip.Targets;
            case TutorialHighlight.LockedTypeSlot:
            {
                var row = FindRow(attack => attack.Locked && !attack.TypeRevealed) ?? FindRow(attack => attack.Locked) ?? FindRow(attack => true);
                return row != null ? new[] { row.TypeSlotRect } : null;
            }
            case TutorialHighlight.DecodedDefense:
            {
                var row = FindRow(attack => attack.TypeRevealed && Session.GetDefense(attack.Sector) == DefenseType.None) ?? FindRow(attack => attack.TypeRevealed) ?? FindRow(attack => attack.Locked) ?? FindRow(attack => true);
                return row != null ? row.DefenseRects() : null;
            }
            default:
                return null;
        }
    }

    private bool Exists(Predicate<Attack> match)
    {
        foreach (var attack in Session.Attacks)
        {
            if (match(attack))
            {
                return true;
            }
        }

        return false;
    }

    private SectorRowView FindRow(Predicate<Attack> match)
    {
        foreach (var attack in Session.Attacks)
        {
            if (!match(attack))
            {
                continue;
            }

            foreach (var row in rows)
            {
                if (row.Sector == attack.Sector)
                {
                    return row;
                }
            }
        }

        return null;
    }
}
