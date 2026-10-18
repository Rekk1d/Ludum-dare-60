using System;
using System.Collections.Generic;

public class ThreatGenerator
{
    private readonly Random _random;

    public ThreatGenerator(Random random)
    {
        _random = random;
    }

    public List<Attack> Generate(RoundConfig round, float angleSpread)
    {
        var sectors = new List<Sector>((Sector[])Enum.GetValues(typeof(Sector)));
        Shuffle(sectors);

        var count = Math.Min(round.SignalCount, sectors.Count);
        var maxFakes = Math.Max(round.MinFakes, round.MaxFakes);
        var fakes = round.RealityEnabled ? Math.Min(count, _random.Next(round.MinFakes, maxFakes + 1)) : 0;

        var attacks = new List<Attack>(count);
        for (var i = 0; i < count; i++)
        {
            var type = _random.Next(2) == 0 ? AttackType.Air : AttackType.Ground;
            var angle = (SectorAngle(sectors[i]) + Range(-angleSpread, angleSpread) + 360f) % 360f;
            var distance = Range(round.SpawnDistanceMin, round.SpawnDistanceMax);
            var speed = Range(round.TargetSpeedMin, round.TargetSpeedMax);
            var delay = Range(0f, round.SpawnDelayMax);
            attacks.Add(new Attack(sectors[i], type, RollPower(round), i >= fakes, angle, distance, speed, delay));
        }

        attacks.Sort((a, b) => a.Sector.CompareTo(b.Sector));
        return attacks;
    }

    public List<Ghost> GenerateGhosts(RoundConfig round, IReadOnlyList<Attack> attacks, float minSeparation)
    {
        var taken = new List<float>(attacks.Count + round.GhostCount);
        foreach (var attack in attacks)
        {
            taken.Add(attack.Angle);
        }

        var ghosts = new List<Ghost>(round.GhostCount);
        for (var i = 0; i < round.GhostCount; i++)
        {
            var angle = PickAngle(taken, minSeparation);
            taken.Add(angle);
            ghosts.Add(new Ghost(
                angle,
                Range(round.SpawnDistanceMin, round.SpawnDistanceMax),
                Range(round.TargetSpeedMin, round.TargetSpeedMax),
                Range(0f, round.SpawnDelayMax)));
        }

        return ghosts;
    }

    private float PickAngle(List<float> taken, float minSeparation)
    {
        var best = 0f;
        var bestGap = -1f;
        for (var attempt = 0; attempt < 64; attempt++)
        {
            var angle = Range(0f, 360f);
            var gap = MinGap(angle, taken);
            if (gap >= minSeparation)
            {
                return angle;
            }

            if (gap > bestGap)
            {
                bestGap = gap;
                best = angle;
            }
        }

        return best;
    }

    private static float MinGap(float angle, List<float> taken)
    {
        var min = 360f;
        foreach (var other in taken)
        {
            var diff = Math.Abs(angle - other) % 360f;
            min = Math.Min(min, Math.Min(diff, 360f - diff));
        }

        return min;
    }

    private int RollPower(RoundConfig round)
    {
        var total = round.WeightPower1 + round.WeightPower2 + round.WeightPower3;
        if (total <= 0f)
        {
            return 1;
        }

        var roll = _random.NextDouble() * total;
        if (roll < round.WeightPower1)
        {
            return 1;
        }

        if (roll < round.WeightPower1 + round.WeightPower2)
        {
            return 2;
        }

        return 3;
    }

    private float Range(float a, float b)
    {
        var min = Math.Min(a, b);
        var max = Math.Max(a, b);
        return min + (float)_random.NextDouble() * (max - min);
    }

    private void Shuffle(List<Sector> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static float SectorAngle(Sector sector)
    {
        switch (sector)
        {
            case Sector.North:
                return 0f;
            case Sector.East:
                return 90f;
            case Sector.South:
                return 180f;
            default:
                return 270f;
        }
    }
}
