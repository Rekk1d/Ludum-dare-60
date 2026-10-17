public static class DamageCalculator
{
    public static int Calculate(Attack attack, DefenseType defense)
    {
        if (!attack.IsReal)
        {
            return 0;
        }

        switch (defense)
        {
            case DefenseType.AirDefense:
                return attack.Type == AttackType.Air ? 0 : attack.Power;
            case DefenseType.GroundDefense:
                return attack.Type == AttackType.Ground ? 0 : attack.Power;
            case DefenseType.Universal:
                return (attack.Power + 1) / 2;
            default:
                return attack.Power;
        }
    }
}
