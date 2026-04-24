using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [System.Serializable]
    public class SFXEntry
    {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] private List<SFXEntry> soundEffects = new List<SFXEntry>();
    private Dictionary<string, AudioClip> sfxDictionary;
    private AudioSource audioSource;
    private AudioSource musicSource;


    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize AudioSources here
        audioSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        InitializeSFXDictionary();
    }

    private void InitializeSFXDictionary()
    {
        sfxDictionary = new Dictionary<string, AudioClip>();
        foreach (var entry in soundEffects)
        {
            if (!sfxDictionary.ContainsKey(entry.name))
            {
                sfxDictionary.Add(entry.name, entry.clip);
            }
        }
    }

    public void PlaySFX(string name)
    {
        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {

            audioSource.PlayOneShot(clip);

        }
        else
        {
            Debug.LogWarning($"Sound effect '{name}' not found!");
        }
    }


    public void PlayLoopingMusic(string name, float startTime = 0f, float volume = 1f)
    {
        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.time = startTime;
            musicSource.volume = volume;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"Music clip '{name}' not found!");
        }
    }

    public void StopLoopingMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}


