using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TerminalView : MonoBehaviour, IPointerClickHandler
{
    private const string GarbleChars = "#%?*/0";

    [SerializeField] private GameController controller;
    [SerializeField] private TMP_Text text;
    [SerializeField, Min(1)] private int maxLines = 5;
    [SerializeField, Min(1f)] private float charsPerSecond = 60f;
    [SerializeField, Range(0f, 1f)] private float garbleChance = 0.25f;

    private readonly Queue<string> _pending = new Queue<string>();
    private readonly List<string> _lines = new List<string>();
    private Coroutine _typing;
    private bool _ordersAnnounced;
    private bool _introPending;
    private bool _endPending;
    private bool _skip;
    private bool _skipped;

    public bool LastSkipped { get; private set; }

    public event Action IntroPrinted;
    public event Action EndPrinted;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_typing != null)
        {
            _skip = true;
        }
    }

    private void Start()
    {
        text.text = string.Empty;
        var session = controller.Session;
        session.Changed += OnChanged;
        session.RoundStarted += OnRoundStarted;
        session.TypeRevealed += OnTypeRevealed;
        session.RealityRevealed += OnRealityRevealed;
        session.TargetsLocked += OnTargetsLocked;
        session.GhostsRemoved += OnGhostsRemoved;
        session.TargetLost += OnTargetLost;
        session.RoundResolved += OnRoundResolved;
    }

    private void OnDestroy()
    {
        if (controller == null || controller.Session == null)
        {
            return;
        }

        var session = controller.Session;
        session.Changed -= OnChanged;
        session.RoundStarted -= OnRoundStarted;
        session.TypeRevealed -= OnTypeRevealed;
        session.RealityRevealed -= OnRealityRevealed;
        session.TargetsLocked -= OnTargetsLocked;
        session.GhostsRemoved -= OnGhostsRemoved;
        session.TargetLost -= OnTargetLost;
        session.RoundResolved -= OnRoundResolved;
    }

    private void OnRoundStarted()
    {
        var session = controller.Session;
        Clear();
        _ordersAnnounced = false;
        _introPending = true;
        _endPending = false;
        Print($"WAVE {session.RoundIndex + 1:00}/{session.RoundCount:00}");
        Print("INCOMING TRANSMISSION...");
        Print($"{session.Attacks.Count} SIGNALS RECEIVED");
        if (session.TimerScrambled)
        {
            Print("EMP DETECTED. CLOCK FAILURE");
        }
        else if (session.Ghosts.Count > 0)
        {
            Print("RADAR INTERFERENCE DETECTED");
        }

        Print("AWAITING ORDERS");
    }

    private void OnChanged()
    {
        var session = controller.Session;
        if (_ordersAnnounced || session.Phase != GamePhase.Planning || !session.ClockRunning || !session.CanConfirm)
        {
            return;
        }

        _ordersAnnounced = true;
        Print("ORDERS SET. AWAITING CONFIRM");
    }

    private void OnTypeRevealed(Attack attack)
    {
        Print($"DECODING {Name(attack)}... {TypeName(attack)}");
    }

    private void OnRealityRevealed(Attack attack)
    {
        Print($"DECODING {Name(attack)}... {(attack.IsReal ? "BANDIT" : "FALSE ALARM")}");
    }

    private void OnTargetsLocked(IReadOnlyList<Attack> attacks)
    {
        foreach (var attack in attacks)
        {
            Print($"{Name(attack)} LOCKED");
        }
    }

    private void OnGhostsRemoved(int count)
    {
        for (var i = 0; i < count; i++)
        {
            Print("FALSE ECHO - NO TARGET");
        }
    }

    private void OnTargetLost(Attack attack)
    {
        Print($"{Name(attack)} LOST - NO DATA");
    }

    private void OnRoundResolved()
    {
        var session = controller.Session;
        Clear();
        foreach (var entry in session.LastReport)
        {
            Print(ReportLine(entry));
        }

        var prefix = session.TimedOut ? "TIME OUT. " : string.Empty;
        var damage = session.LastDamage > 0 ? $"DAMAGE -{session.LastDamage}" : "NO DAMAGE";
        switch (session.Phase)
        {
            case GamePhase.Won:
                Print(session.TimedOut
                    ? $"{prefix}{damage}. SURVIVED"
                    : $"{damage}. SURVIVED. BASE HP {session.Hp}/{session.MaxHp}");
                break;
            case GamePhase.Lost:
                Print($"{prefix}{damage}. BASE LOST");
                break;
            default:
                Print($"{prefix}{damage}. BASE HP {session.Hp}/{session.MaxHp}");
                break;
        }

        _endPending = session.Phase == GamePhase.Won || session.Phase == GamePhase.Lost;
    }

    private static string ReportLine(ReportEntry entry)
    {
        var attack = entry.Attack;
        if (!attack.IsReal)
        {
            return entry.Defense == DefenseType.None
                ? $"{Name(attack)} FALSE ALARM"
                : $"{Name(attack)} FALSE ALARM. ORDER WASTED";
        }

        var prefix = $"{Name(attack)} {TypeName(attack)}";
        if (entry.Damage == 0)
        {
            return $"{prefix} BLOCKED";
        }

        switch (entry.Defense)
        {
            case DefenseType.None:
                return $"{prefix} HIT -{entry.Damage} NO DEFENSE";
            case DefenseType.Universal:
                return $"{prefix} HIT -{entry.Damage} HALVED";
            default:
                return $"{prefix} HIT -{entry.Damage} WRONG DEFENSE";
        }
    }

    private void Print(string line)
    {
        var session = controller.Session;
        var garbled = session.TimerScrambled && session.ClockRunning;
        _pending.Enqueue("> " + (garbled ? Garble(line) : line));
        if (_typing == null)
        {
            _typing = StartCoroutine(Type());
        }
    }

    private string Garble(string line)
    {
        var chars = line.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] != ' ' && UnityEngine.Random.value < garbleChance)
            {
                chars[i] = GarbleChars[UnityEngine.Random.Range(0, GarbleChars.Length)];
            }
        }

        return new string(chars);
    }

    private void Clear()
    {
        if (_typing != null)
        {
            StopCoroutine(_typing);
            _typing = null;
        }

        _skip = false;
        _pending.Clear();
        _lines.Clear();
        text.text = string.Empty;
    }

    private IEnumerator Type()
    {
        var delay = new WaitForSeconds(1f / charsPerSecond);
        _skipped = false;
        while (_pending.Count > 0)
        {
            var line = _pending.Dequeue();
            _lines.Add(string.Empty);
            if (_lines.Count > maxLines)
            {
                _lines.RemoveAt(0);
            }

            for (var i = 1; i <= line.Length; i++)
            {
                if (_skip)
                {
                    _skipped = true;
                    i = line.Length;
                }

                _lines[_lines.Count - 1] = line.Substring(0, i);
                text.text = string.Join("\n", _lines);
                if (!_skip)
                {
                    yield return delay;
                }
            }
        }

        LastSkipped = _skipped;
        _skip = false;
        _typing = null;
        if (_introPending)
        {
            _introPending = false;
            IntroPrinted?.Invoke();
        }

        if (_endPending)
        {
            _endPending = false;
            EndPrinted?.Invoke();
        }
    }

    private static string Name(Attack attack)
    {
        return attack.Sector.ToString().ToUpperInvariant();
    }

    private static string TypeName(Attack attack)
    {
        return attack.Type == AttackType.Air ? "AIR" : "GROUND";
    }
}
