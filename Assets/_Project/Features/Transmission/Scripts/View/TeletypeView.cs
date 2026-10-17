using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeletypeView : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private TMP_Text text;
    [SerializeField, Min(1)] private int maxLines = 5;
    [SerializeField, Min(1f)] private float charsPerSecond = 60f;

    private readonly Queue<string> pending = new Queue<string>();
    private readonly List<string> lines = new List<string>();
    private Coroutine typing;

    private void Start()
    {
        text.text = string.Empty;
        var session = controller.Session;
        session.RoundStarted += OnRoundStarted;
        session.TypeRevealed += OnTypeRevealed;
        session.RealityRevealed += OnRealityRevealed;
        session.RoundResolved += OnRoundResolved;
    }

    private void OnDestroy()
    {
        if (controller == null || controller.Session == null)
        {
            return;
        }

        var session = controller.Session;
        session.RoundStarted -= OnRoundStarted;
        session.TypeRevealed -= OnTypeRevealed;
        session.RealityRevealed -= OnRealityRevealed;
        session.RoundResolved -= OnRoundResolved;
    }

    private void OnRoundStarted()
    {
        var session = controller.Session;
        Clear();
        Print($"ROUND {session.RoundIndex + 1}/{session.RoundCount}");
        foreach (var attack in session.Attacks)
        {
            Print($"{Name(attack)} SIGNAL PWR {attack.Power}");
        }
    }

    private void OnTypeRevealed(Attack attack)
    {
        Print($"{Name(attack)} TYPE: {(attack.Type == AttackType.Air ? "AIR" : "GROUND")}");
    }

    private void OnRealityRevealed(Attack attack)
    {
        Print($"{Name(attack)}: {(attack.IsReal ? "REAL THREAT" : "FALSE ALARM")}");
    }

    private void OnRoundResolved()
    {
        var session = controller.Session;
        Clear();
        foreach (var entry in session.LastReport)
        {
            if (!entry.Attack.IsReal)
            {
                Print($"{Name(entry.Attack)} FALSE ALARM");
            }
            else if (entry.Damage == 0)
            {
                Print($"{Name(entry.Attack)} BLOCKED");
            }
            else
            {
                Print($"{Name(entry.Attack)} HIT -{entry.Damage}");
            }
        }

        switch (session.Phase)
        {
            case GamePhase.Won:
                Print($"HP {session.Hp}/{session.MaxHp}. SURVIVED. PRESS RESTART");
                break;
            case GamePhase.Lost:
                Print($"HP {session.Hp}/{session.MaxHp}. STATION LOST. PRESS RESTART");
                break;
            default:
                Print($"HP {session.Hp}/{session.MaxHp}. PRESS NEXT");
                break;
        }
    }

    private void Print(string line)
    {
        pending.Enqueue("> " + line);
        if (typing == null)
        {
            typing = StartCoroutine(Type());
        }
    }

    private void Clear()
    {
        if (typing != null)
        {
            StopCoroutine(typing);
            typing = null;
        }

        pending.Clear();
        lines.Clear();
        text.text = string.Empty;
    }

    private IEnumerator Type()
    {
        var delay = new WaitForSeconds(1f / charsPerSecond);
        while (pending.Count > 0)
        {
            var line = pending.Dequeue();
            lines.Add(string.Empty);
            if (lines.Count > maxLines)
            {
                lines.RemoveAt(0);
            }

            for (var i = 1; i <= line.Length; i++)
            {
                lines[lines.Count - 1] = line.Substring(0, i);
                text.text = string.Join("\n", lines);
                yield return delay;
            }
        }

        typing = null;
    }

    private static string Name(Attack attack)
    {
        return attack.Sector.ToString().ToUpperInvariant();
    }
}
