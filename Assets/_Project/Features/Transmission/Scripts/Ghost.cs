using System;

public class Ghost
{
    public float Angle { get; }
    public float Speed { get; }
    public float SpawnDelay { get; }
    public float Distance { get; private set; }
    public bool Spawned { get; private set; }
    public bool Gone { get; private set; }

    public Ghost(float angle, float distance, float speed, float spawnDelay)
    {
        Angle = angle;
        Distance = distance;
        Speed = speed;
        SpawnDelay = spawnDelay;
    }

    public void Spawn()
    {
        Spawned = true;
    }

    public void Move(float deltaTime)
    {
        Distance = Math.Max(0f, Distance - Speed * deltaTime);
    }

    public void Remove()
    {
        Gone = true;
    }
}
