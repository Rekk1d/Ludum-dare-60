using System;
using System.Collections.Generic;

public class GameSession
{
    private readonly GameConfig _config;
    private readonly ThreatGenerator _generator;
    private readonly Dictionary<Sector, DefenseType> _orders = new Dictionary<Sector, DefenseType>();
    private readonly List<ReportEntry> _lastReport = new List<ReportEntry>();
    private List<Attack> _attacks = new List<Attack>();
    private RoundConfig _currentRound;

    public event Action Changed;
    public event Action RoundStarted;
    public event Action<Attack> TypeRevealed;
    public event Action<Attack> RealityRevealed;
    public event Action RoundResolved;

    public int Hp { get; private set; }
    public int MaxHp => _config.StartHp;
    public int RoundIndex { get; private set; }
    public int RoundCount => _config.Rounds.Count;
    public int DecryptsLeft { get; private set; }
    public int DecryptTotal => _currentRound.DecryptCount;
    public int OrdersLeft => _currentRound.OrderCount - _orders.Count;
    public int OrdersTotal => _currentRound.OrderCount;
    public bool RealityEnabled => _currentRound.RealityEnabled;
    public int UniversalLeft => _config.MaxUniversalPerRound - CountOrders(DefenseType.Universal);
    public bool CanConfirm => Phase != GamePhase.Planning || OrdersLeft <= 0 || _orders.Count >= _attacks.Count;
    public int LastDamage { get; private set; }
    public GamePhase Phase { get; private set; }
    public IReadOnlyList<Attack> Attacks => _attacks;
    public IReadOnlyList<ReportEntry> LastReport => _lastReport;

    public GameSession(GameConfig config, Random random)
    {
        _config = config;
        _generator = new ThreatGenerator(random);
    }

    public void StartGame()
    {
        Hp = _config.StartHp;
        RoundIndex = 0;
        BeginRound();
    }

    public DefenseType GetDefense(Sector sector)
    {
        return _orders.TryGetValue(sector, out var defense) ? defense : DefenseType.None;
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
            if (_orders.Remove(sector))
            {
                Changed?.Invoke();
            }

            return true;
        }

        if (!_orders.ContainsKey(sector) && OrdersLeft <= 0)
        {
            return false;
        }

        if (defense == DefenseType.Universal && GetDefense(sector) != DefenseType.Universal && UniversalLeft <= 0)
        {
            return false;
        }

        _orders[sector] = defense;
        Changed?.Invoke();
        return true;
    }

    public void ConfirmOrders()
    {
        if (Phase != GamePhase.Planning || !CanConfirm)
        {
            return;
        }

        _lastReport.Clear();
        LastDamage = 0;
        foreach (var attack in _attacks)
        {
            var defense = GetDefense(attack.Sector);
            var damage = DamageCalculator.Calculate(attack, defense);
            _lastReport.Add(new ReportEntry(attack, defense, damage));
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
        foreach (var value in _orders.Values)
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
        _currentRound = _config.Rounds[RoundIndex];
        _attacks = _generator.Generate(_currentRound);
        foreach (var attack in _attacks)
        {
            if (_currentRound.RevealTypes)
            {
                attack.RevealType();
            }
        }

        DecryptsLeft = _currentRound.DecryptCount;
        _orders.Clear();
        Phase = GamePhase.Planning;
        RoundStarted?.Invoke();
        Changed?.Invoke();
    }
}
