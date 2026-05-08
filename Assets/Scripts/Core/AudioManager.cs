using System.Collections;
using UnityEngine;

/// <summary>
///  Central audio handler - handles audio / sfx - optimized for wwise backend (but wwise got cut)
/// </summary>

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    /*[Header("Backend Selection")]
    [SerializeField] private AudioBackend backend = AudioBackend.Unity;*/
    
    [Header("Rhythm Game Settings")]
    [SerializeField] private float bpm = 140f;
    [SerializeField] private float beatsToTravel = 4f;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    
    [Header("Music Tracks")]
    [SerializeField] public AudioClip menuMusic;
    [SerializeField] public AudioClip cutsceneMusic;
    [SerializeField] public AudioClip roundMusic;
    
    [Header("Countdown SFX")]
    [SerializeField] public AudioClip countdownTick;
    [SerializeField] public AudioClip countdownGo;
    
    [Header("Player SFX")]
    [SerializeField] public AudioClip hitSFX;
    [SerializeField] public AudioClip noteMiss;
    
    [Header("Recording SFX")]
    [SerializeField] public AudioClip recordNote;
    [SerializeField] public AudioClip recordHold;
    
    [Header("UI/Menu SFX")]
    [SerializeField] public AudioClip menuClick;
    [SerializeField] public AudioClip menuQuitClick;
    [SerializeField] public AudioClip menuHover;
    
    [Header("Phase SFX")]
    [SerializeField] public AudioClip phaseTransition;
    [SerializeField] public AudioClip gameOver;
    [SerializeField] public AudioClip audienceCheer;
    [SerializeField] public AudioClip lowStamina;
    
    [Header("SFX Pool Settings")]
    [SerializeField] private int sfxPoolSize = 10;
    [SerializeField] private float defaultPitchMin = 0.95f;
    [SerializeField] private float defaultPitchMax = 1.05f;
    
    private AudioSource[] _sfxSources;
    public AudioSource _musicSource;

    private const string MASTER_VOL_KEY = "MasterVolume"; // for playerprefs
    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SFXVolume";

    public float BPM => bpm;
    public float NoteSpeed => CalculateNoteSpeed();

    public enum AudioBackend
    {
        Unity,
        Wwise
    }
    
    #region Init

    private void Awake() // singleton / initialize
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitUnityBackend();
        LoadVolumeSettings();
        ApplyVolume();
    }

    /*private void InitializeBackend()
    {
        switch (backend)
        {
            case AudioBackend.Unity:
                InitUnityBackend();
                break;
            
            case AudioBackend.Wwise:
                InitWwiseBackend();
                break;
        }
        
        Debug.Log($"AudioManager: {backend} | BPM={bpm} | Note Speed={NoteSpeed:F2}");
    }*/

    private void InitUnityBackend()
    {
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(transform);
        _musicSource = musicObj.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
        
        _sfxSources = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sfxObj = new GameObject($"SFX_Source_{i}");
            sfxObj.transform.SetParent(transform);
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            _sfxSources[i] = source;
        }
    }

    private void InitWwiseBackend()
    {
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOL_KEY, 0.5f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.7f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 0.8f);
    }

    #endregion Init
    
    #region Rhythm Calc

    private float CalculateNoteSpeed()
    {
        if (GameBoundaries.Instance == null) return 5f;
        
        float distance = GameBoundaries.Instance.TravelDistance; // distance calculated in gameboundaries
        float beatDuration = 60f / bpm; // beat in secs
        float travelTime = beatsToTravel * beatDuration;
        
        return distance / travelTime; // s = d/t
    }
    
    public void SetBPM(float newBPM)
    {
        bpm = Mathf.Clamp(newBPM, 60f, 300f); // minimum of 60 - max 300
        BeatManager.Instance.SetBPM(bpm);
    }
    
    #endregion Rhythm Calc

    #region Volume Control

    private float MapSliderToVolume(float sliderValue) // normalize slider value
    {
        return Mathf.Pow(sliderValue, 1.5f);
    }

    private void ApplyVolume()
    {
        // if (backend == AudioBackend.Unity)
        {
            float mappedMusic = MapSliderToVolume(musicVolume) * masterVolume;
            if (_musicSource != null)
            {
                _musicSource.volume = mappedMusic;
            }
            
            float mappedSFX = MapSliderToVolume(sfxVolume) * masterVolume;
            if (_sfxSources != null)
            {
                foreach (var source in _sfxSources)
                {
                    source.volume = mappedSFX;
                }
            }
        }
    }

    public void SetMasterVolume(float value) // called for slider updates
    {
        masterVolume = value;
        ApplyVolume();
        PlayerPrefs.SetFloat(MASTER_VOL_KEY, value);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        ApplyVolume();
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        ApplyVolume();
        PlayerPrefs.SetFloat(SFX_VOL_KEY, value);
        PlayerPrefs.Save();
    }

    #endregion Volume Control

    #region Music
    
    public void PlayMusic(AudioClip clip, bool loop = true, bool fadeIn = false, float fadeDuration = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Attempted to play null music clip");
            return;
        }

        // if (backend == AudioBackend.Unity)
        {
            if (fadeIn)
                StartCoroutine(FadeInMusic(clip, fadeDuration, loop));
            else
            {
                _musicSource.clip = clip;
                _musicSource.loop = loop;
                _musicSource.Play();
            }
        }
    }
    
    public void StopMusic(bool fadeOut = false, float fadeDuration = 1f)
    {
        // if (backend == AudioBackend.Unity)
        {
            if (fadeOut)
            {
                StartCoroutine(FadeOutMusic(fadeDuration));
            }
            else
            {
                _musicSource.Stop();
            }
        }
    }
    
    public void PauseMusic()
    {
        // if (backend == AudioBackend.Unity)
        {
            _musicSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        // if (backend == AudioBackend.Unity)
        {
            _musicSource.UnPause();
        }
    }
    
    public void CrossfadeMusic(AudioClip newClip, float duration = 1f, bool loop = true)
    {
        // if (backend == AudioBackend.Unity)
        {
            StartCoroutine(CrossfadeMusicCoroutine(newClip, duration, loop));
        }
    }
    
    private IEnumerator CrossfadeMusicCoroutine(AudioClip newClip, float duration, bool loop)
    {
        // if (backend == AudioBackend.Unity)
        {
            if (_musicSource.isPlaying)
            {
                yield return StartCoroutine(FadeOutMusic(duration / 2f));
            }
            
            if (newClip != null)
            {
                yield return StartCoroutine(FadeInMusic(newClip, duration, loop));
            }
        }
    }

    private IEnumerator FadeInMusic(AudioClip clip, float duration, bool loop)
    {
        float targetVolume = MapSliderToVolume(musicVolume) * masterVolume;
        
        _musicSource.clip = clip;
        _musicSource.loop = loop;
        _musicSource.volume = 0f;
        _musicSource.Play();
        
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // volume lerp
            _musicSource.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
            yield return null;
        }
        
        _musicSource.volume = targetVolume;
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = _musicSource.volume;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
        
        _musicSource.Stop();
        _musicSource.volume = startVolume;
    }
    
    #endregion Music

    #region SFX
    
    public void PlayCountdownTick() => PlaySFXClip(countdownTick, pitchMin: 1f, pitchMax: 1f); // helpers
    public void PlayCountdownGo() => PlaySFXClip(countdownGo, pitchMin: 1f, pitchMax: 1f);

    public void PlayHitPerfect() => PlaySFXClip(hitSFX);
    public void PlayHitGood() => PlaySFXClip(hitSFX);
    public void PlayHitOkay() => PlaySFXClip(hitSFX);
    public void PlayNoteMiss() => PlaySFXClip(noteMiss);

    public void PlayRecordNote() => PlaySFXClip(recordNote, pitchMin: 1f, pitchMax: 1f);
    public void PlayRecordHold() => PlaySFXClip(recordHold, pitchMin: 1f, pitchMax: 1f);

    public void PlayMenuClick() => PlaySFXClip(menuClick, pitchMin: 1f, pitchMax: 1f);
    public void PlayMenuQuitClick() => PlaySFXClip(menuQuitClick, pitchMin: 1f, pitchMax: 1f);
    public void PlayMenuHover() => PlaySFXClip(menuHover, pitchMin: 1f, pitchMax: 1f, volumeMin: 0.5f, volumeMax: 0.7f);

    public void PlayPhaseTransition() => PlaySFXClip(phaseTransition);
    public void PlayGameOver() => PlaySFXClip(gameOver, pitchMin: 1f, pitchMax: 1f);
    public void PlayAudienceCheer() => PlaySFXClip(audienceCheer, pitchMin: 1f, pitchMax: 1f);
    public void PlayLowStamina() => PlaySFXClip(lowStamina, pitchMin: 1f, pitchMax: 1f);
    
    public AudioSource PlaySFXClip(AudioClip clip, // randomize sfx
        float pitchMin = -1f, float pitchMax = -1f,
        float volumeMin = 0.85f, float volumeMax = 1.0f,
        bool looping = false,
        Vector3 position = default)
    {
        if (clip == null) return null;

        // if (backend == AudioBackend.Unity) // most of the stuff below can be done in wwise
        {
            AudioSource source = GetAvailableSFXSource();
            if (source == null) return null;
            
            if (pitchMin < 0) pitchMin = defaultPitchMin;
            if (pitchMax < 0) pitchMax = defaultPitchMax;
            
            float baseVolume = MapSliderToVolume(sfxVolume) * masterVolume;
            source.volume = baseVolume * Random.Range(volumeMin, volumeMax);
            source.pitch = Random.Range(pitchMin, pitchMax);
            source.loop = looping;
            source.clip = clip;
            source.transform.position = position;
            source.Play();
            
            return source;
        }
        // return null;
    }
    
    public void StopSFX(AudioSource source)
    {
        if (source != null)
        {
            source.Stop();
            source.loop = false;
        }
    }
    
    public void StopAllSFX()
    {
        // if (backend == AudioBackend.Unity)
        {
            foreach (var source in _sfxSources)
            {
                source.Stop();
                source.loop = false;
            }
        }
    }

    private AudioSource GetAvailableSFXSource() // for sfx overlap
    {
        foreach (var source in _sfxSources)
        {
            if (!source.isPlaying) return source;
        }
        
        return _sfxSources[0];
    }

    #endregion SFX

    #region Timing Queries

    public float GetMusicTime()
    {
        // if (backend == AudioBackend.Unity)
        {
            return _musicSource != null ? _musicSource.time : 0f;
        }
        
        // return 0f;
    }

    public bool IsMusicPlaying()
    {
        // if (backend == AudioBackend.Unity)
        {
            return _musicSource != null && _musicSource.isPlaying;
        }
        
        // return false;
    }

    public float GetBeatProgress()
    {
        if (BeatManager.Instance != null)
        {
            return BeatManager.Instance.GetBeatProgress();
        }
        return 0f;
    }

    public bool IsOnBeat(float tolerance = 0.1f)
    {
        if (BeatManager.Instance != null)
        {
            float timingOffset = BeatManager.Instance.CalculateTimingOffset(Time.time);
            return timingOffset <= tolerance;
        }
        return false;
    }

    public int GetCurrentBeat()
    {
        if (BeatManager.Instance != null)
        {
            return BeatManager.Instance.CurrentBeat;
        }
        return 0;
    }

    public int GetCurrentBar()
    {
        if (BeatManager.Instance != null)
        {
            return BeatManager.Instance.CurrentBar;
        }
        return 0;
    }

    public float GetTimeToNextBeat()
    {
        if (BeatManager.Instance != null)
        {
            float beatDuration = BeatManager.Instance.BeatDuration;
            float beatProgress = BeatManager.Instance.GetBeatProgress();
            return beatDuration * (1f - beatProgress);
        }
        return 0f;
    }


    #endregion Timing Queries
}