using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public EventInstance musicInstance; // Store the music instance

    public static AudioManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ✅ Keep this AudioManager alive
        }
        else
        {
            Destroy(gameObject); // ✅ Ensure only one instance exists
        }
    }

    public void SetMasterVolume(float volume)
    {
        Debug.Log(volume);
        // Set the global parameter "MasterVolume" to the new value (assumed range 0–1).
        // Make sure you have this parameter defined in FMOD.
        RuntimeManager.StudioSystem.setParameterByName("MasterVolume", volume);
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public void PlayOneShotUI(EventReference sound)
    {
        RuntimeManager.PlayOneShot(sound);
    }


    public void PlayMusic(EventReference music)
    {
        if (musicInstance.isValid()) // Stop any existing music before playing a new one
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
        musicInstance = RuntimeManager.CreateInstance(music);
        musicInstance.start();
    }

    public void StopMusic()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }
}
