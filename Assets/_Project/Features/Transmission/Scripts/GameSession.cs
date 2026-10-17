using System;
using System.Collections.Generic;

public class GameSession
{
    private readonly GameConfig config;
    private readonly ThreatGenerator generator;
    private readonly Dictionary<Sector, DefenseType> orders = new Dictionary<Sector, DefenseType>();
    private readonly List<ReportEntry> lastReport = new List<ReportEntry>();
    private List<Attack> attacks = new List<Attack>();
    private RoundConfig currentRound;

    public event Action Changed;
    public event Action RoundStarted;
    public event Action<Attack> TypeRevealed;
    public event Action<Attack> RealityRevealed;
    public event Action RoundResolved;

    public int Hp { get; private set; }
    public int MaxHp => config.StartHp;
    public int RoundIndex { get; private set; }
    public int RoundCount => config.Rounds.Count;
    public int DecryptsLeft { get; private set; }
    public int DecryptTotal => currentRound.DecryptCount;
    public int OrdersLeft => currentRound.OrderCount - orders.Count;
    public int OrdersTotal => currentRound.OrderCount;
    public bool RealityEnabled => currentRound.RealityEnabled;
    public int UniversalLeft => config.MaxUniversalPerRound - CountOrders(DefenseType.Universal);
    public int LastDamage { get; private set; }
    public GamePhase Phase { get; private set; }
    public IReadOnlyList<Attack> Attacks => attacks;
    public IReadOnlyList<ReportEntry> LastReport => lastReport;

    public GameSession(GameConfig config, Random random)
    {
        this.config = config;
        generator = new ThreatGenerator(random);
    }

    public void StartGame()
    {
        Hp = config.StartHp;
        RoundIndex = 0;
        BeginRound();
    }

    public DefenseType GetDefense(Sector sector)
    {
        return orders.TryGetValue(sector, out var defense) ? defense : DefenseType.None;
    }

    public bool TryRevealType(Attack attack)
    {
        if (!CanDecrypt() || attack.TypeRevealed)
        {
            return false;
        }

        attack.RevealType();
        DecryptsLeft--;
        TypeRevealed?.Invoke(attack);
        Changed?.Invoke();
        return true;
    }

    public bool TryRevealReality(Attack attack)
    {
        if (!RealityEnabled || !CanDecrypt() || attack.RealityRevealed)
        {
            return false;
        }

        attack.RevealReality();
        DecryptsLeft--;
        RealityRevealed?.Invoke(attack);
        Changed?.Invoke();
        return true;
    }

    public bool TrySetDefense(Sector sector, DefenseType defense)
    {
        if (Phase != GamePhase.Planning)
        {
            return false;
        }

        if (defense == DefenseType.None)
        {
            if (orders.Remove(sector))
            {
                Changed?.Invoke();
            }

            return true;
        }

        if (!orders.ContainsKey(sector) && OrdersLeft <= 0)
        {
            return false;
        }

        if (defense == DefenseType.Universal && GetDefense(sector) != DefenseType.Universal && UniversalLeft <= 0)
        {
            return false;
        }

        orders[sector] = defense;
        Changed?.Invoke();
        return true;
    }

    public void ConfirmOrders()
    {
        if (Phase != GamePhase.Planning)
        {
            return;
        }

        lastReport.Clear();
        LastDamage = 0;
        foreach (var attack in attacks)
        {
            var defense = GetDefense(attack.Sector);
            var damage = DamageCalculator.Calculate(attack, defense);
            lastReport.Add(new ReportEntry(attack, defense, damage));
            LastDamage += damage;
        }

        Hp = Math.Max(0, Hp - LastDamage);

        if (Hp <= 0)
        {
            Phase = GamePhase.Lost;
        }
        else if (RoundIndex >= RoundCount - 1)
        {
            Phase = GamePhase.Won;
        }
        else
        {
            Phase = GamePhase.Report;
        }

        RoundResolved?.Invoke();
        Changed?.Invoke();
    }

    public void NextRound()
    {
        if (Phase != GamePhase.Report)
        {
            return;
        }

        RoundIndex++;
        BeginRound();
    }

    private bool CanDecrypt()
    {
        return Phase == GamePhase.Planning && DecryptsLeft > 0;
    }

    private int CountOrders(DefenseType defense)
    {
        var count = 0;
        foreach (var value in orders.Values)
        {
            if (value == defense)
            {
                count++;
            }
        }

        return count;
    }

    private void BeginRound()
    {
        currentRound = config.Rounds[RoundIndex];
        attacks = generator.Generate(currentRound);
        foreach (var attack in attacks)
        {
            if (currentRound.RevealTypes)
            {
                attack.RevealType();
            }
        }

        DecryptsLeft = currentRound.DecryptCount;
        orders.Clear();
        Phase = GamePhase.Planning;
        RoundStarted?.Invoke();
        Changed?.Invoke();
    }
}
