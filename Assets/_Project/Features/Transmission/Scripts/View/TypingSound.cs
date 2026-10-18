using UnityEngine;

public class TypingSound : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] keyClips;
    [SerializeField] private AudioClip[] spaceClips;
    [SerializeField] private Vector2 keyGap = new Vector2(0.08f, 0.1f);
    [SerializeField] private Vector2 volume = new Vector2(0.7f, 1f);
    [SerializeField] private bool playWhilePaused;

    private float _ready;

    private void Awake()
    {
        source.ignoreListenerPause = playWhilePaused;
    }

    public void Play(char character)
    {
        if (character == ' ')
        {
            _ready = 0f;
            PlayRandom(spaceClips);
            return;
        }

        if (char.IsWhiteSpace(character) || Time.unscaledTime < _ready)
        {
            return;
        }

        _ready = Time.unscaledTime + Random.Range(keyGap.x, keyGap.y);
        PlayRandom(keyClips);
    }

    private void PlayRandom(AudioClip[] clips)
    {
        if (clips.Length == 0)
        {
            return;
        }

        source.PlayOneShot(clips[Random.Range(0, clips.Length)], Random.Range(volume.x, volume.y));
    }
}
