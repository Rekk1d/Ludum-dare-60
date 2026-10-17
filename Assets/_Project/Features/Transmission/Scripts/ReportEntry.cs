public class ReportEntry
{
    public Attack Attack { get; }
    public DefenseType Defense { get; }
    public int Damage { get; }

    public ReportEntry(Attack attack, DefenseType defense, int damage)
    {
        Attack = attack;
        Defense = defense;
        Damage = damage;
    }
}
