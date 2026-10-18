using UnityEngine;
using UnityEngine.UI;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button[] smallButtons;
    [SerializeField] private AudioClip confirmClip;
    [SerializeField] private AudioClip lockClip;
    [SerializeField] private AudioClip smallClip;
    [SerializeField] private AudioClip paperClip;
    [SerializeField] private AudioClip radarClip;
    [SerializeField] private AudioClip selectClip;
    [SerializeField] private AudioClip[] stampClips;
    [SerializeField, Range(0f, 1f)] private float stampVolume = 0.4f;
    [SerializeField, Range(0f, 1f)] private float radarVolume = 0.4f;

    private void Start()
    {
        confirmButton.onClick.AddListener(() => Play(confirmClip, 1f));
        foreach (var button in smallButtons)
        {
            button.onClick.AddListener(() => Play(smallClip, 1f));
        }
    }

    public void PlayLock()
    {
        Play(lockClip, 1f);
    }

    public void PlayPaper()
    {
        Play(paperClip, 1f);
    }

    public void PlayRadar()
    {
        Play(radarClip, radarVolume);
    }

    public void PlaySelect()
    {
        Play(selectClip, 1f);
    }

    public void PlayStamp()
    {
        if (stampClips.Length > 0)
        {
            Play(stampClips[Random.Range(0, stampClips.Length)], stampVolume);
        }
    }

    private void Play(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            source.PlayOneShot(clip, volume);
        }
    }
}
