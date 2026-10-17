public class Attack
{
    public Sector Sector { get; }
    public AttackType Type { get; }
    public int Power { get; }
    public bool IsReal { get; }
    public bool TypeRevealed { get; private set; }
    public bool RealityRevealed { get; private set; }

    public Attack(Sector sector, AttackType type, int power, bool isReal)
    {
        Sector = sector;
        Type = type;
        Power = power;
        IsReal = isReal;
    }

    public void RevealType()
    {
        TypeRevealed = true;
    }

    public void RevealReality()
    {
        RealityRevealed = true;
    }
}
