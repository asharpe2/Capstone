using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider volumeSlider;             // Assign in Inspector
    public TMP_Text volumeLabel;            // Optional: to display the current volume as text

    private const string VolumePrefKey = "MasterVolume";

    private void Start()
    {
        // Load saved volume, defaulting to 1 (full volume)
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        volumeSlider.value = savedVolume;
        UpdateVolume(savedVolume);

        // Register to slider's value changed event
        volumeSlider.onValueChanged.AddListener(UpdateVolume);
    }

    public void UpdateVolume(float newVolume)
    {
        // Update the label text, if there is one.
        if (volumeLabel != null)
        {
            volumeLabel.text = $"Volume: {(int)(newVolume * 100)}%";
        }

        // Save the volume so it persists across sessions
        PlayerPrefs.SetFloat(VolumePrefKey, newVolume);

        // Update your AudioManager's master volume here.
        // For example, if you have a method SetMasterVolume(float value)
        AudioManager.instance.SetMasterVolume(newVolume);

        // If you're using FMOD or another system, call the relevant method.
    }

    private void OnDestroy()
    {
        // Unsubscribe when this object is destroyed
        volumeSlider.onValueChanged.RemoveListener(UpdateVolume);
    }
}
