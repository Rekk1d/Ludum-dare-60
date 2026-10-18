using System;
using System.Collections.Generic;

public class GameSession
{
    private readonly GameConfig _config;
    private readonly ThreatGenerator _generator;
    private readonly Dictionary<Sector, DefenseType> _orders = new Dictionary<Sector, DefenseType>();
    private readonly List<ReportEntry> _lastReport = new List<ReportEntry>();
    private readonly List<Attack> _justLocked = new List<Attack>();
    private List<Attack> _attacks = new List<Attack>();
    private RoundConfig _currentRound;
    private float _elapsed;

    public event Action Changed;
    public event Action RoundStarted;
    public event Action<Attack> TypeRevealed;
    public event Action<Attack> RealityRevealed;
    public event Action<IReadOnlyList<Attack>> TargetsLocked;
    public event Action<Attack> TargetLost;
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
    public bool ClockRunning { get; private set; }
    public bool IsActive => Phase == GamePhase.Planning && ClockRunning;
    public bool CanConfirm => Phase != GamePhase.Planning || (ClockRunning && (OrdersLeft <= 0 || _orders.Count >= _attacks.Count));
    public bool CanLock => IsActive && _attacks.Exists(IsLockable);
    public bool HasTimer => _currentRound != null && _currentRound.TimerSeconds > 0f;
    public float TimeLeft { get; private set; }
    public bool TimedOut { get; private set; }
    public float SweepAngle { get; private set; }
    public float LostRadius => _config.LostRadius;
    public float LockWindow => _config.LockWindow;
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

    public void StartClock()
    {
        if (Phase != GamePhase.Planning || ClockRunning)
        {
            return;
        }

        ClockRunning = true;
        Changed?.Invoke();
    }

    public void Tick(float deltaTime)
    {
        if (_currentRound == null)
        {
            return;
        }

        SweepAngle = (SweepAngle + _currentRound.SweepSpeed * deltaTime) % 360f;
        if (!IsActive)
        {
            return;
        }

        _elapsed += deltaTime;
        var changed = false;
        foreach (var attack in _attacks)
        {
            if (attack.Locked || attack.Lost)
            {
                continue;
            }

            if (!attack.Spawned)
            {
                if (_elapsed < attack.SpawnDelay)
                {
                    continue;
                }

                attack.Spawn();
                changed = true;
            }

            attack.Move(deltaTime);
            if (attack.Distance <= _config.LostRadius)
            {
                attack.MarkLost();
                TargetLost?.Invoke(attack);
                changed = true;
            }
        }

        if (HasTimer)
        {
            TimeLeft = Math.Max(0f, TimeLeft - deltaTime);
            if (TimeLeft <= 0f)
            {
                Resolve(true);
                return;
            }
        }

        if (changed)
        {
            Changed?.Invoke();
        }
    }

    public int Lock()
    {
        if (!IsActive)
        {
            return 0;
        }

        _justLocked.Clear();
        foreach (var attack in _attacks)
        {
            if (IsLockable(attack) && UnderBeam(attack))
            {
                attack.Lock();
                _justLocked.Add(attack);
            }
        }

        if (_justLocked.Count == 0)
        {
            return 0;
        }

        TargetsLocked?.Invoke(_justLocked);
        Changed?.Invoke();
        return _justLocked.Count;
    }

    public bool InLockZone(Attack attack)
    {
        return IsActive && IsLockable(attack) && UnderBeam(attack);
    }

    public DefenseType GetDefense(Sector sector)
    {
        return _orders.TryGetValue(sector, out var defense) ? defense : DefenseType.None;
    }

    public bool TryRevealType(Attack attack)
    {
        if (!CanDecrypt() || !attack.Locked || attack.TypeRevealed)
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
        if (!RealityEnabled || !CanDecrypt() || !attack.Locked || attack.RealityRevealed)
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
        if (!IsActive)
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
        if (!IsActive || !CanConfirm)
        {
            return;
        }

        Resolve(false);
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

    private void Resolve(bool timedOut)
    {
        TimedOut = timedOut;
        ClockRunning = false;
        _lastReport.Clear();
        LastDamage = 0;
        foreach (var attack in _attacks)
        {
            var defense = timedOut ? DefenseType.None : GetDefense(attack.Sector);
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

    private bool CanDecrypt()
    {
        return IsActive && DecryptsLeft > 0;
    }

    private static bool IsLockable(Attack attack)
    {
        return attack.Spawned && !attack.Locked && !attack.Lost;
    }

    private bool UnderBeam(Attack attack)
    {
        return (SweepAngle - attack.Angle + 360f) % 360f <= _config.LockWindow;
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
        _attacks = _generator.Generate(_currentRound, _config.AngleSpread);
        _elapsed = 0f;
        ClockRunning = false;
        TimedOut = false;
        TimeLeft = _currentRound.TimerSeconds;
        foreach (var attack in _attacks)
        {
            if (_currentRound.RevealTypes)
            {
                attack.RevealType();
            }

            if (attack.SpawnDelay <= 0f)
            {
                attack.Spawn();
            }
        }

        DecryptsLeft = _currentRound.DecryptCount;
        _orders.Clear();
        Phase = GamePhase.Planning;
        RoundStarted?.Invoke();
        Changed?.Invoke();
    }
}
