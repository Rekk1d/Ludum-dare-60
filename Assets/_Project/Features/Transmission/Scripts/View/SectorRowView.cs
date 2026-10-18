using TMPro;
using UnityEngine;

public class SectorRowView : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private Sector sector;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private LampStrip threatLamps;
    [SerializeField] private DecodeSlot typeSlot;
    [SerializeField] private DecodeSlot realitySlot;
    [SerializeField] private DefenseButton[] defenseButtons;

    private void Start()
    {
        nameText.text = sector.ToString().ToUpperInvariant();
        controller.Session.Changed += Refresh;
        typeSlot.Button.onClick.AddListener(OnTypeClicked);
        realitySlot.Button.onClick.AddListener(OnRealityClicked);
        foreach (var defenseButton in defenseButtons)
        {
            var type = defenseButton.Type;
            defenseButton.Button.onClick.AddListener(() => OnDefenseClicked(type));
        }
    }

    private void OnDestroy()
    {
        if (controller != null && controller.Session != null)
        {
            controller.Session.Changed -= Refresh;
        }
    }

    private void Refresh()
    {
        var session = controller.Session;
        var attack = FindAttack();
        var planning = session.Phase == GamePhase.Planning;

        if (attack == null)
        {
            threatLamps.SetState(3, 0);
            typeSlot.Hide();
            realitySlot.Hide();
            foreach (var defenseButton in defenseButtons)
            {
                defenseButton.SetState(false, false);
            }

            return;
        }

        var lost = planning && attack.Lost;
        var canDecrypt = session.IsActive && session.DecryptsLeft > 0 && attack.Locked;
        threatLamps.SetState(3, attack.Power);
        typeSlot.SetState(attack.TypeRevealed || !planning || lost, canDecrypt, lost ? "NO DATA" : attack.Type == AttackType.Air ? "AIR" : "GROUND");

        if (session.RealityEnabled)
        {
            realitySlot.SetState(attack.RealityRevealed || !planning || lost, canDecrypt, lost ? "NO DATA" : attack.IsReal ? "REAL" : "FAKE");
        }
        else
        {
            realitySlot.Hide();
        }

        var current = session.GetDefense(sector);
        var canOrder = session.IsActive && (current != DefenseType.None || session.OrdersLeft > 0);
        foreach (var defenseButton in defenseButtons)
        {
            var selected = current == defenseButton.Type;
            var enabled = canOrder;
            if (defenseButton.Type == DefenseType.Universal && !selected && session.UniversalLeft <= 0)
            {
                enabled = false;
            }

            defenseButton.SetState(selected, enabled);
        }
    }

    private void OnTypeClicked()
    {
        var attack = FindAttack();
        if (attack != null)
        {
            controller.Session.TryRevealType(attack);
        }
    }

    private void OnRealityClicked()
    {
        var attack = FindAttack();
        if (attack != null)
        {
            controller.Session.TryRevealReality(attack);
        }
    }

    private void OnDefenseClicked(DefenseType type)
    {
        var session = controller.Session;
        session.TrySetDefense(sector, session.GetDefense(sector) == type ? DefenseType.None : type);
    }

    private Attack FindAttack()
    {
        foreach (var attack in controller.Session.Attacks)
        {
            if (attack.Sector == sector)
            {
                return attack;
            }
        }

        return null;
    }
}
