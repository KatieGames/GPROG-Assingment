using UnityEngine.Audio;
using UnityEngine;


[System.Serializable]
public class Sound
{
    [Header("Sound Settings")]
    public string name;

    public AudioClip clip;

    [Tooltip("The AudioMixerGroup this sound will output to.")]
    public AudioMixerGroup outputAudioMixerGroup;

    [Header("Audio Properties")]
    [Range(0f, 1f)]
    [Tooltip("The volume of the sound, defaults to one")]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    [Tooltip("The pitch of the sound, should be left as 1 usually.")]
    public float pitch = 1f;

    [Range(0f, 1f)]
    [Tooltip("For 3D audio (sound emitted directionally in the scene), set spatialBlend to 1. For non-directional sound (e.g., background music or sound emitted from the player), set to 0.5.")]
    public float spatialBlend = 1f;

    [Range(0f, 1f)]
    [Tooltip("Pitch distortion based on distance, e.g., the pitch changing as a bullet flies past you. In many cases, it is best to keep this at 0.")]
    public float dopplerLevel = 0f;

    [Space]
    [Tooltip("The maximum distance at which the sound can be heard.")]
    public float maxDistance = 50f;

    [Tooltip("Controls how the volume decreases with distance.")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

    [Header("Playback Options")]
    [Tooltip("Loop the sound perpetually.")]
    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
