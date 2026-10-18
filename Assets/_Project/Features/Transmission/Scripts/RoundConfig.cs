using System;
using UnityEngine;

[Serializable]
public class RoundConfig
{
    [SerializeField, Range(1, 4)] private int signalCount = 4;
    [SerializeField, Range(0, 4)] private int decryptCount = 4;
    [SerializeField, Range(1, 4)] private int orderCount = 2;
    [SerializeField, Range(0, 4)] private int minFakes = 1;
    [SerializeField, Range(0, 4)] private int maxFakes = 2;
    [SerializeField, Min(0f)] private float weightPower1 = 30f;
    [SerializeField, Min(0f)] private float weightPower2 = 30f;
    [SerializeField, Min(0f)] private float weightPower3 = 40f;
    [SerializeField] private bool revealTypes;
    [SerializeField] private bool realityEnabled = true;
    [SerializeField, Min(0f)] private float timerSeconds = 90f;
    [SerializeField, Min(0f)] private float sweepSpeed = 90f;
    [SerializeField, Min(0f)] private float targetSpeedMin = 0.01f;
    [SerializeField, Min(0f)] private float targetSpeedMax = 0.02f;
    [SerializeField, Range(0f, 1f)] private float spawnDistanceMin = 0.6f;
    [SerializeField, Range(0f, 1f)] private float spawnDistanceMax = 0.9f;
    [SerializeField, Min(0f)] private float spawnDelayMax = 10f;
    [SerializeField, Range(0, 8)] private int ghostCount;
    [SerializeField] private bool scrambledTimer;

    public int SignalCount => signalCount;
    public int DecryptCount => decryptCount;
    public int OrderCount => orderCount;
    public int MinFakes => minFakes;
    public int MaxFakes => maxFakes;
    public float WeightPower1 => weightPower1;
    public float WeightPower2 => weightPower2;
    public float WeightPower3 => weightPower3;
    public bool RevealTypes => revealTypes;
    public bool RealityEnabled => realityEnabled;
    public float TimerSeconds => timerSeconds;
    public float SweepSpeed => sweepSpeed;
    public float TargetSpeedMin => targetSpeedMin;
    public float TargetSpeedMax => targetSpeedMax;
    public float SpawnDistanceMin => spawnDistanceMin;
    public float SpawnDistanceMax => spawnDistanceMax;
    public float SpawnDelayMax => spawnDelayMax;
    public int GhostCount => ghostCount;
    public bool ScrambledTimer => scrambledTimer;

    public RoundConfig()
    {
    }

    public RoundConfig(int signalCount, int decryptCount, int orderCount, int minFakes, int maxFakes, float weightPower1, float weightPower2, float weightPower3, bool revealTypes = false, bool realityEnabled = true)
    {
        this.signalCount = signalCount;
        this.decryptCount = decryptCount;
        this.orderCount = orderCount;
        this.minFakes = minFakes;
        this.maxFakes = maxFakes;
        this.weightPower1 = weightPower1;
        this.weightPower2 = weightPower2;
        this.weightPower3 = weightPower3;
        this.revealTypes = revealTypes;
        this.realityEnabled = realityEnabled;
    }
}
