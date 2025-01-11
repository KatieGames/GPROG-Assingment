using System.Collections;
using UnityEngine.Audio;
using UnityEngine;
using System;


public class AudioManager : MonoBehaviour
{

    // Sound Categories
    [Header("Sound Categories")]
    [Tooltip("Array of one-shot sound effects.")]
    public Sound[] oneShotSounds;

    [Tooltip("Array of music tracks.")]
    public Sound[] music;

    // Audio Mixers
    [Header("Audio Mixers")]
    [Tooltip("Audio Mixer Group for music.")]
    public AudioMixerGroup musicMixer;

    [Tooltip("Audio Mixer Group for sound effects.")]
    public AudioMixerGroup soundMixer;

    // Fade Settings
    [Header("Fade Settings")]
    [Tooltip("Duration for fading out in seconds.")]
    public float fadeDuration = 1.0f;
    
    public static AudioManager instance;

    void Awake()
    {
        // makes it a singleton so it works when carried over accross scenes and referenced by its instance
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        


        // add sounds to audio sources with their varying info
        foreach (Sound s in oneShotSounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.dopplerLevel = s.dopplerLevel;
            s.source.spatialBlend = s.spatialBlend;
            s.source.outputAudioMixerGroup = s.outputAudioMixerGroup;
        }

        foreach (Sound m in music)
        {
            m.source = gameObject.AddComponent<AudioSource>();
            m.source.clip = m.clip;

            m.source.volume = m.volume;
            m.source.pitch = m.pitch;
            m.source.loop = m.loop;
            m.source.dopplerLevel = m.dopplerLevel;
            m.source.spatialBlend = m.spatialBlend;
            m.source.outputAudioMixerGroup = m.outputAudioMixerGroup;
        }

        // this could be used to play music when a scene loads. Ideally this should be done in a gameplay manager of some sorts
        // PlayMusic("titleMusic"); //this will play the music sound with the name titleMusic
    }
        
    // plays oneshot sound. use for overlapping audio such as gunshots or footsteps
    public void PlayOneShotSound(string name)
    {
        Sound s = Array.Find(oneShotSounds, sound => sound.name == name);

        // custom error message when cant find clip name
        if (s == null)
        {
            Debug.LogWarning($"Sound: {name} not found!");
            return;
        }
        s.source.PlayOneShot(s.clip, s.volume);
    }

    // plays sound
    public void PlaySound(string name)
    {
        Sound s = Array.Find(oneShotSounds, sound => sound.name == name);

        // custom error message when cant find clip name
        if (s == null)
        {
            Debug.LogWarning($"Sound: {name} not found!");
            return;
        }
        Debug.Log("Playing sound");
        s.source.Play();
    }

    // stop all sounds and play a specific one
    public void ForcePlaySound(string name)
    {
        // iterate all the oneshot osunds and stop them
        foreach (Sound so in oneShotSounds)
        {
            if(so != null)
            {
                so.source.Stop();
            }
        }

        Sound s = Array.Find(oneShotSounds, sound => sound.name == name);

        // error message when cant find clip name
        if (s == null)
        {
            Debug.LogWarning($"Sound: {name} not found!");
            return;
        }
        Debug.Log("Playing sound");
        s.source.Play();
    }

    // play music
    public void PlayMusic(string name)
    {
        Sound m = Array.Find(music, sound => sound.name == name);
        
        // custom error message when cant find clip name
        if (m == null)
        {
            Debug.LogWarning($"Music: {name} not found!");
            return;
        }
        Debug.Log("Playing music");
        m.source.Play();
    }

    // stop all sounds and music (NOT ONESHOT SOUNDS)
    public void StopAll()
    {
        Component[] sources;
        sources = GetComponentsInChildren<AudioSource>();

        // iterate all the audio sources to stop them
        foreach(AudioSource tSource in sources)
        {
            tSource.Stop();
        }
    }

    // stop a specific sound or piece of music
    public void StopSpecific(string name)
    {
        // if we can find the specific sound
        Sound m = Array.Find(music, sound => sound.name == name);
        if (m == null)
        {
            // if its not in the music look in the oneshot sounds
            Sound s = Array.Find(oneShotSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning($"Sound: {name} not found!");
                return;
            }
            else
            {
                Debug.Log("Stopping sound");
                s.source.Stop();
            }
        }
        else
        {
            Debug.Log("Stopping music");
            m.source.Stop();
        }
    }


    // gradually fades out and stops all music audio sources. Can parse in a new piece of audio to play if wanted
    public void FadeOutAllMusic(string musicName = null)
    {
        // iterate all the music sources
        foreach (Sound m in music)
        {
            if (m != null && m.source != null)
            {
                StartCoroutine(FadeOutAndStop(m.source, musicName));
            }
        }
    }

    // fades out the volume of a music audio source and then stops it and plays a new piece of music
    private IEnumerator FadeOutAndStop(AudioSource audioSource, string musicName = null)
    {
        float startVolume = audioSource.volume;

        // gradually decrease volume over fadeDuration
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        // ensure volume is 0 before stopping
        audioSource.volume = 0;

        // stop the audio
        audioSource.Stop();

        // reset volume to original
        audioSource.volume = startVolume;

        // if a new music name is provided, play it and fade it in
        if (!string.IsNullOrEmpty(musicName))
        {
            Sound m = Array.Find(music, sound => sound.name == musicName);

            if (m != null)
            {
                float newMusicStartVolume = m.source.volume;
                m.source.volume = 0;

                PlayMusic(musicName);

                // gradually increase volume over fadeDuration
                for (float t = 0; t < fadeDuration; t += Time.deltaTime)
                {
                    m.source.volume = Mathf.Lerp(0, newMusicStartVolume, t / fadeDuration);
                    yield return null;
                }
            }
        }
    }


    //to play from anywhere do
    //AudioManager.instance.PlaySound("SoundName")

    //to edit volume from another script do
    //AudioManager manager = FindObjectOfType<AudioManager>();
    //audioMixer.SetFloat("BgmVolume", volume);
}