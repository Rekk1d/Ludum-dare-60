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
    private List<Ghost> _ghosts = new List<Ghost>();
    private RoundConfig _currentRound;
    private float _elapsed;
    private float _lockBlockedUntil;

    public event Action Changed;
    public event Action RoundStarted;
    public event Action<Attack> TypeRevealed;
    public event Action<Attack> RealityRevealed;
    public event Action<IReadOnlyList<Attack>> TargetsLocked;
    public event Action<int> GhostsRemoved;
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
    public bool LockBlocked => _elapsed < _lockBlockedUntil;
    public bool CanLock => IsActive && !LockBlocked && (_attacks.Exists(attack => !attack.Locked && !attack.Lost) || _ghosts.Exists(ghost => !ghost.Gone));
    public bool HasTimer => _currentRound != null && _currentRound.TimerSeconds > 0f;
    public bool TimerScrambled => _currentRound != null && _currentRound.ScrambledTimer;
    public float TimerTotal => _currentRound != null ? _currentRound.TimerSeconds : 0f;
    public float TimeLeft { get; private set; }
    public bool TimedOut { get; private set; }
    public float SweepAngle { get; private set; }
    public float LostRadius => _config.LostRadius;
    public float LockWindow => _config.LockWindow;
    public int LastDamage { get; private set; }
    public GamePhase Phase { get; private set; }
    public IReadOnlyList<Attack> Attacks => _attacks;
    public IReadOnlyList<Ghost> Ghosts => _ghosts;
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

        var wasBlocked = LockBlocked;
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

        foreach (var ghost in _ghosts)
        {
            if (ghost.Gone)
            {
                continue;
            }

            if (!ghost.Spawned)
            {
                if (_elapsed < ghost.SpawnDelay)
                {
                    continue;
                }

                ghost.Spawn();
                changed = true;
            }

            ghost.Move(deltaTime);
            if (ghost.Distance <= _config.LostRadius)
            {
                ghost.Remove();
                changed = true;
            }
        }

        if (wasBlocked && !LockBlocked)
        {
            changed = true;
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

    public void Lock()
    {
        if (!IsActive || LockBlocked)
        {
            return;
        }

        _justLocked.Clear();
        foreach (var attack in _attacks)
        {
            if (IsLockable(attack) && UnderBeam(attack.Angle))
            {
                attack.Lock();
                _justLocked.Add(attack);
            }
        }

        var removed = 0;
        foreach (var ghost in _ghosts)
        {
            if (IsLockable(ghost) && UnderBeam(ghost.Angle))
            {
                ghost.Remove();
                removed++;
            }
        }

        if (_justLocked.Count == 0 && removed == 0)
        {
            return;
        }

        if (removed > 0)
        {
            _lockBlockedUntil = _elapsed + _config.GhostLockPenalty;
        }

        if (_justLocked.Count > 0)
        {
            TargetsLocked?.Invoke(_justLocked);
        }

        if (removed > 0)
        {
            GhostsRemoved?.Invoke(removed);
        }

        Changed?.Invoke();
    }

    public bool InLockZone(Attack attack)
    {
        return IsActive && IsLockable(attack) && UnderBeam(attack.Angle);
    }

    public bool InLockZone(Ghost ghost)
    {
        return IsActive && IsLockable(ghost) && UnderBeam(ghost.Angle);
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

    private static bool IsLockable(Ghost ghost)
    {
        return ghost.Spawned && !ghost.Gone;
    }

    private bool UnderBeam(float angle)
    {
        return (SweepAngle - angle + 360f) % 360f <= _config.LockWindow;
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
        _ghosts = _generator.GenerateGhosts(_currentRound, _attacks, _config.GhostSeparation);
        _elapsed = 0f;
        _lockBlockedUntil = 0f;
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

        foreach (var ghost in _ghosts)
        {
            if (ghost.SpawnDelay <= 0f)
            {
                ghost.Spawn();
            }
        }

        DecryptsLeft = _currentRound.DecryptCount;
        _orders.Clear();
        Phase = GamePhase.Planning;
        RoundStarted?.Invoke();
        Changed?.Invoke();
    }
}
