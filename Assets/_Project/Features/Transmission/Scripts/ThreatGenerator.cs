using System;
using System.Collections.Generic;

public class ThreatGenerator
{
    private readonly Random _random;

    public ThreatGenerator(Random random)
    {
        _random = random;
    }

    public List<Attack> Generate(RoundConfig round)
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
            attacks.Add(new Attack(sectors[i], type, RollPower(round), i >= fakes));
        }

        attacks.Sort((a, b) => a.Sector.CompareTo(b.Sector));
        return attacks;
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

    private void Shuffle(List<Sector> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
