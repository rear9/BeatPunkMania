using System;
using UnityEngine;

/// <summary>
/// beat tracking and timing windows - syncs to AudioManager's BPM.
/// </summary>

public enum HitAccuracy
{
    Perfect,
    Good,
    Okay,
    Miss
}

public class BeatManager : MonoBehaviour
{
    public static BeatManager Instance { get; private set; }

    [Header("Timing Windows (seconds)")]
    [SerializeField] private float perfectWindow = 0.07f;
    [SerializeField] private float goodWindow = 0.13f;
    [SerializeField] private float okayWindow = 0.20f;
    
    private float _beatDuration;
    private float _lastBeatTime;
    private int _currentBeat;
    private int _absoluteBeat = 0;
    private int _currentBar;
    private bool _isTracking;
    private int _beatsPerBar = 4;

    public event Action OnBeat; // called from beatmanager
    public event Action OnDownbeat;

    public float BPM => AudioManager.Instance != null ? AudioManager.Instance.BPM : 120f;
    public float SecondsPerBeat => 60f / BPM;
    public float BeatDuration => _beatDuration;
    public int CurrentBeat => _currentBeat;
    public int AbsoluteBeat => _absoluteBeat;
    public int CurrentBar => _currentBar;

    #region Init

    private void Awake()
    {
        if (Instance != null && Instance != this) // singleton
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _beatDuration = 60f / BPM;
    }


    public void SetBPM(float newBPM) // public to call private func
    {
        _beatDuration = 60f / BPM;
    }

    #endregion Init

    #region Tracking

    public void StartTracking()
    {
        _beatDuration = 60f / BPM;
        _isTracking = true;
        _lastBeatTime = Time.time;
        _currentBeat = 1;
        _currentBar = 0;
    }

    public void StopTracking() => _isTracking = false;
    
    private void Update()
    {
        if (!_isTracking) return;
        
        float timeSinceLastBeat = Time.time - _lastBeatTime;
        
        if (timeSinceLastBeat >= _beatDuration)
        {
            TriggerBeat(); // calls the onbeat event
            _lastBeatTime = Time.time; // marks time
        }
    }

    private void TriggerBeat()
    {
        _currentBeat++;
        _absoluteBeat++;
        
        if (_currentBeat > _beatsPerBar)
        {
            _currentBeat = 1;
            _currentBar++;
        }
        
        OnBeat?.Invoke();
        
        if (_currentBeat == 1)
        {
            OnDownbeat?.Invoke();
        }
    }

    #endregion Tracking

    #region Timing

    public float CalculateTimingOffset(float inputTime) // calculates accuracy & if something is on beat
    {
        if (!_isTracking) return float.MaxValue;
        
        float timeSinceLastBeat = inputTime - _lastBeatTime; 
        float timeToNextBeat = _beatDuration - timeSinceLastBeat;
        
        return Mathf.Min(timeSinceLastBeat, timeToNextBeat);
    }

    public HitAccuracy GetAccuracy(float timingOffset)
    {
        if (timingOffset <= perfectWindow) return HitAccuracy.Perfect;
        if (timingOffset <= goodWindow) return HitAccuracy.Good;
        if (timingOffset <= okayWindow) return HitAccuracy.Okay;
        return HitAccuracy.Miss;
    }
    
    public float CalculateAccuracyPercent(float timingOffset)
    {
        if (okayWindow <= 0f) return 0f;
        return Mathf.Clamp01(1f - (timingOffset / okayWindow)) * 100f;
    }

    public bool IsInHitWindow()
    {
        if (!_isTracking) return false;
        
        float timeSinceLastBeat = Time.time - _lastBeatTime;
        float timeToNextBeat = _beatDuration - timeSinceLastBeat;
        
        return Mathf.Min(timeSinceLastBeat, timeToNextBeat) <= okayWindow;
    }

    public float GetBeatProgress()
    {
        if (!_isTracking) return 0f;
        
        float timeSinceLastBeat = Time.time - _lastBeatTime;
        return Mathf.Clamp01(timeSinceLastBeat / _beatDuration);
    }

    #endregion Timing
    
    public void ResetBeatTimeline()
    {
        _lastBeatTime = Time.time;
        _currentBeat = 0;
        _currentBar = 0;
        _absoluteBeat = 0;
    }
}