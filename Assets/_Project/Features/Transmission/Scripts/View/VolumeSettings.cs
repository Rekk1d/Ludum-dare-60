using TMPro;
using UnityEngine;

public class VolumeSettings : MonoBehaviour
{
    private const string PrefsKey = "volume";

    [SerializeField] private VolumeSlider slider;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private SoundPlayer sound;

    private void Start()
    {
        var value = Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey, slider.Steps), 0, slider.Steps);
        slider.SetValue(value);
        Apply(value);
        slider.Changed += OnChanged;
    }

    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.Changed -= OnChanged;
        }
    }

    private void OnChanged(int value)
    {
        Apply(value);
        PlayerPrefs.SetInt(PrefsKey, value);
        PlayerPrefs.Save();
        sound.PlaySmall();
    }

    private void Apply(int value)
    {
        AudioListener.volume = (float)value / slider.Steps;
        valueText.text = $"{value * 100 / slider.Steps}%";
    }
}
