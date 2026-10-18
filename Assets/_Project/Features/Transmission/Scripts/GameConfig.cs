using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Transmission/Game Config")]
public class GameConfig : ScriptableObject
{
    [SerializeField, Min(1)] private int startHp = 10;
    [SerializeField, Range(0, 4)] private int maxUniversalPerRound = 1;
    [SerializeField, Range(0f, 1f)] private float lostRadius = 0.336f;
    [SerializeField, Range(0f, 90f)] private float lockWindow = 15f;
    [SerializeField, Range(0f, 45f)] private float angleSpread = 25f;
    [SerializeField, Min(0f)] private float ghostLockPenalty = 1.5f;
    [SerializeField, Range(0f, 90f)] private float ghostSeparation = 20f;
    [SerializeField] private List<RoundConfig> rounds = new List<RoundConfig>
    {
        new RoundConfig(3, 3, 3, 0, 0, 30f, 30f, 40f, realityEnabled: false),
        new RoundConfig(4, 4, 3, 1, 1, 30f, 30f, 40f, revealTypes: true),
        new RoundConfig(4, 4, 2, 1, 2, 30f, 30f, 40f),
        new RoundConfig(4, 4, 2, 1, 2, 30f, 30f, 40f),
        new RoundConfig(4, 4, 2, 1, 2, 30f, 30f, 40f),
        new RoundConfig(4, 4, 2, 1, 2, 30f, 30f, 40f),
        new RoundConfig(4, 4, 2, 1, 2, 30f, 30f, 40f)
    };

    public int StartHp => startHp;
    public int MaxUniversalPerRound => maxUniversalPerRound;
    public float LostRadius => lostRadius;
    public float LockWindow => lockWindow;
    public float AngleSpread => angleSpread;
    public float GhostLockPenalty => ghostLockPenalty;
    public float GhostSeparation => ghostSeparation;
    public IReadOnlyList<RoundConfig> Rounds => rounds;
}
