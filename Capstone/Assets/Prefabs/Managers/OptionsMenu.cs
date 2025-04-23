using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    // Rename these keys to match what you use below:
    private const string masterVolumePrefKey = "MasterVolume";
    private const string musicVolumePrefKey = "MusicVolume";
    private const string sfxVolumePrefKey = "SFXVolume";

    private void Start()
    {
        // Load saved volume, default to 1
        float masterValue = PlayerPrefs.GetFloat(masterVolumePrefKey, 1f);
        float musicValue = PlayerPrefs.GetFloat(musicVolumePrefKey, 1f);
        float sfxValue = PlayerPrefs.GetFloat(sfxVolumePrefKey, 1f);

        masterVolumeSlider.value = masterValue;
        musicVolumeSlider.value = musicValue;
        sfxVolumeSlider.value = sfxValue;

        // Apply initial volumes
        UpdateMasterVolume(masterValue);
        UpdateMusicVolume(musicValue);
        UpdateSFXVolume(sfxValue);

        // Hook up slider callbacks
        masterVolumeSlider.onValueChanged.AddListener(UpdateMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(UpdateSFXVolume);
    }

    public void UpdateMasterVolume(float newVolume)
    {
        PlayerPrefs.SetFloat(masterVolumePrefKey, newVolume);
        AudioManager.instance.SetMasterVolume(newVolume);
    }

    public void UpdateMusicVolume(float newVolume)
    {
        PlayerPrefs.SetFloat(musicVolumePrefKey, newVolume);
        AudioManager.instance.SetMusicVolume(newVolume);
    }

    public void UpdateSFXVolume(float newVolume)
    {
        PlayerPrefs.SetFloat(sfxVolumePrefKey, newVolume);
        AudioManager.instance.SetSFXVolume(newVolume);
    }

    private void OnDestroy()
    {
        // Unsubscribe the correct listeners!
        masterVolumeSlider.onValueChanged.RemoveListener(UpdateMasterVolume);
        musicVolumeSlider.onValueChanged.RemoveListener(UpdateMusicVolume);
        sfxVolumeSlider.onValueChanged.RemoveListener(UpdateSFXVolume);
    }
}
