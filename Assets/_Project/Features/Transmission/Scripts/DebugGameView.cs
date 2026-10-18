using System;
using UnityEngine;

public class DebugGameView : MonoBehaviour
{
    private static readonly string[] DefenseLabels = { "None", "Air Def", "Ground Def", "Universal" };

    [SerializeField] private GameConfig config;
    [SerializeField, Min(0.5f)] private float uiScale = 2f;

    private GameSession _session;

    private void Start()
    {
        _session = new GameSession(config, new System.Random());
        _session.StartGame();
    }

    private void OnGUI()
    {
        if (_session == null)
        {
            return;
        }

        GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));
        GUILayout.BeginArea(new Rect(10f, 10f, Screen.width / uiScale - 20f, Screen.height / uiScale - 20f));

        GUILayout.Label($"Round {_session.RoundIndex + 1}/{_session.RoundCount}   HP {_session.Hp}   Decrypts {_session.DecryptsLeft}   Orders {_session.OrdersLeft}   Universal {_session.UniversalLeft}");
        GUILayout.Space(10f);

        foreach (Sector sector in Enum.GetValues(typeof(Sector)))
        {
            DrawSector(sector);
        }

        GUILayout.Space(10f);
        DrawPhase();

        GUILayout.EndArea();
    }

    private void DrawSector(Sector sector)
    {
        var attack = FindAttack(sector);

        GUILayout.BeginHorizontal();
        GUILayout.Label(sector.ToString(), GUILayout.Width(60f));

        if (attack == null)
        {
            GUILayout.Label("quiet");
            GUILayout.EndHorizontal();
            return;
        }

        var planning = _session.Phase == GamePhase.Planning;

        GUILayout.Label($"Power {attack.Power}", GUILayout.Width(70f));

        if (attack.TypeRevealed || !planning)
        {
            GUILayout.Label(attack.Type.ToString(), GUILayout.Width(80f));
        }
        else if (GUILayout.Button("Type ?", GUILayout.Width(80f)))
        {
            _session.TryRevealType(attack);
        }

        if (_session.RealityEnabled)
        {
            if (attack.RealityRevealed || !planning)
            {
                GUILayout.Label(attack.IsReal ? "REAL" : "FAKE", GUILayout.Width(80f));
            }
            else if (GUILayout.Button("Real ?", GUILayout.Width(80f)))
            {
                _session.TryRevealReality(attack);
            }
        }

        var current = (int)_session.GetDefense(sector);
        if (planning)
        {
            var selected = GUILayout.Toolbar(current, DefenseLabels, GUILayout.Width(320f));
            if (selected != current)
            {
                _session.TrySetDefense(sector, (DefenseType)selected);
            }
        }
        else
        {
            GUILayout.Label(DefenseLabels[current], GUILayout.Width(90f));
            GUILayout.Label($"-{FindDamage(attack)} HP");
        }

        GUILayout.EndHorizontal();
    }

    private void DrawPhase()
    {
        switch (_session.Phase)
        {
            case GamePhase.Planning:
                if (GUILayout.Button("Confirm orders", GUILayout.Width(160f)))
                {
                    _session.ConfirmOrders();
                }
                break;
            case GamePhase.Report:
                GUILayout.Label($"Damage taken: {_session.LastDamage}");
                if (GUILayout.Button("Next round", GUILayout.Width(160f)))
                {
                    _session.NextRound();
                }
                break;
            case GamePhase.Won:
            case GamePhase.Lost:
                GUILayout.Label($"Damage taken: {_session.LastDamage}");
                GUILayout.Label(_session.Phase == GamePhase.Won ? "STATION SURVIVED" : "STATION LOST");
                if (GUILayout.Button("Restart", GUILayout.Width(160f)))
                {
                    _session.StartGame();
                }
                break;
        }
    }

    private int FindDamage(Attack attack)
    {
        foreach (var entry in _session.LastReport)
        {
            if (entry.Attack == attack)
            {
                return entry.Damage;
            }
        }

        return 0;
    }

    private Attack FindAttack(Sector sector)
    {
        foreach (var attack in _session.Attacks)
        {
            if (attack.Sector == sector)
            {
                return attack;
            }
        }

        return null;
    }
}
