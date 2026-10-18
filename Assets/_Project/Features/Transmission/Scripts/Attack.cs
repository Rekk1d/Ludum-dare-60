using System;

public class Attack
{
    public Sector Sector { get; }
    public AttackType Type { get; }
    public int Power { get; }
    public bool IsReal { get; }
    public float Angle { get; }
    public float Speed { get; }
    public float SpawnDelay { get; }
    public float Distance { get; private set; }
    public bool TypeRevealed { get; private set; }
    public bool RealityRevealed { get; private set; }
    public bool Spawned { get; private set; }
    public bool Locked { get; private set; }
    public bool Lost { get; private set; }

    public Attack(Sector sector, AttackType type, int power, bool isReal, float angle, float distance, float speed, float spawnDelay)
    {
        Sector = sector;
        Type = type;
        Power = power;
        IsReal = isReal;
        Angle = angle;
        Distance = distance;
        Speed = speed;
        SpawnDelay = spawnDelay;
    }

    public void RevealType()
    {
        TypeRevealed = true;
    }

    public void RevealReality()
    {
        RealityRevealed = true;
    }

    public void Spawn()
    {
        Spawned = true;
    }

    public void Move(float deltaTime)
    {
        Distance = Math.Max(0f, Distance - Speed * deltaTime);
    }

    public void Lock()
    {
        Locked = true;
    }

    public void MarkLost()
    {
        Lost = true;
    }
}
