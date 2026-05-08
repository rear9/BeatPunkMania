using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles scoring & confidence and holds data to be used in results screen
/// </summary>

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    //[Header("Combo Settings")] //TODO;
    //[SerializeField] private int comboBreakThreshold = 3; // misses before combo breaks (maybe for leniency)
    //[SerializeField] private float comboMultiplierStep = 0.1f; // mult increase per combo level
    //[SerializeField] private int comboLevelInterval = 10; // notes per combo level

    [Header("Confidence Settings")] // some differences between combo and confidence so i'm separating them
    [SerializeField] private int confidenceGainPerHit = 100;
    [SerializeField] private int confidenceGainPerMiss = -100;
    [SerializeField] private int highConfidenceThreshold = 750;
    [SerializeField] private int lowConfidenceThreshold = 250;

    [Header("Point Values")]
    [SerializeField] private int perfectPoints = 3;
    [SerializeField] private int goodPoints = 2;
    [SerializeField] private int okayPoints = 1;

    // Score tracking (per player)
    [SerializeField] public int[] _playerScores = new int[2]; // exposed for debug
    [SerializeField] public int[] _playerConfidence = new int[2];

    //[SerializeField] private int[] _playerCombos = new int[2];
    //private int[] _playerMaxCombos = new int[2];
    //private int[] _consecutiveMisses = new int[2]; 

    // accuracy tracking (per player)
    private int[] _perfectHits = new int[2];
    private int[] _goodHits = new int[2];
    private int[] _okayHits = new int[2];
    private int[] _missedHits = new int[2];

    public struct GambitState // for gambit ability
    {
        public int hitsRequired;
        public float bonusMulti;
        public int hitCount;
        public int perfectCount;
        public int scoreGained;
        public bool active;
    }
    public GambitState[] _gambit = new GambitState[2];
    public NoteHitDetection[] noteHitTransplant;
    // events per player (playerID, val)
    public event Action<int, int> OnScoreChanged;
    public event Action<int, HitAccuracy> OnHitRegistered;
    public event Action<int, int> OnConfidenceChanged;
    //public event Action<int, int> OnComboChanged;
    //public event Action<int> OnComboBreak;
    //public event Action<int, int> OnMaxComboUpdated;
    
    private void Awake() // manager oop
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #region Hit Reg
    
    public void RegisterHit(int playerID, HitAccuracy accuracy) // accepts accuracy directly instead of recalculating from timing offset
    {
        if (playerID < 0 || playerID >= 2) return;
        
        if (_gambit[playerID].active)
        {
            // if perfect then add to gambit count until it reaches what it wants, then end gambit once needed count reached w/ function
        }
        
        switch (accuracy) // update variables and values based on type of hit
        {
            case HitAccuracy.Perfect:
                _perfectHits[playerID]++;
                AddScore(playerID, perfectPoints);
                AddConfidence(playerID, confidenceGainPerHit);
                //IncrementCombo(playerID);
                //_consecutiveMisses[playerID] = 0;
                break;

            case HitAccuracy.Good:
                _goodHits[playerID]++;
                AddScore(playerID, goodPoints);
                AddConfidence(playerID, confidenceGainPerHit);
                //IncrementCombo(playerID);
                //_consecutiveMisses[playerID] = 0;
                break;

            case HitAccuracy.Okay:
                _okayHits[playerID]++;
                AddScore(playerID, okayPoints);
                AddConfidence(playerID, confidenceGainPerHit);
                //IncrementCombo(playerID);
                //_consecutiveMisses[playerID] = 0;
                break;

            case HitAccuracy.Miss:
                RegisterMiss(playerID);
                return;
        }

        // fire hit event (UI listens)
        OnHitRegistered?.Invoke(playerID, accuracy);
    }
    
    public void RegisterMiss(int playerID)
    {
        if (playerID < 0 || playerID >= 2) return;

        _missedHits[playerID]++;
        AddScore(playerID, -perfectPoints);
        AddConfidence(playerID, confidenceGainPerMiss);
        OnHitRegistered?.Invoke(playerID, HitAccuracy.Miss);
    }

    public void AddConfidence(int playerID, int amount)
    {
        if (playerID < 0 || playerID >= 2) return;

        _playerConfidence[playerID] += amount;
        _playerConfidence[playerID] = Mathf.Clamp(_playerConfidence[playerID], 0, 1000);

        OnConfidenceChanged?.Invoke(playerID, _playerConfidence[playerID]);
    }
    #endregion Hit Reg

    #region Score Management

    private void AddScore(int playerID, int points)
    {

        //_playerScores[playerID] += Mathf.RoundToInt(points * GetComboMultiplier(playerID));

        if (points > 0 && _playerConfidence[playerID] >= highConfidenceThreshold)
        {
            points = Mathf.RoundToInt(points * 1.5f); // point multiplier for high confidence
        }
        else if (points < 0 && _playerConfidence[playerID] <= lowConfidenceThreshold)
        {
            points = Mathf.RoundToInt(points / 1.5f); // reduce penalty for low confidence
        }
        _playerScores[playerID] += Mathf.RoundToInt(points);
        _playerScores[playerID] = Mathf.Max(0, _playerScores[playerID]);
        OnScoreChanged?.Invoke(playerID, _playerScores[playerID]);
    }

    public int GetScore(int playerID) // helper
    {
        return (playerID >= 0 && playerID < 2) ? _playerScores[playerID] : 0;
    }
    
    public void SetScore(int playerID, int value)
    {
        if (playerID < 0 || playerID >= 2) return;
        _playerScores[playerID] = value;
        OnScoreChanged?.Invoke(playerID, value);
    }
    
    
 
    public void BeginGambitWindow(int playerID, int hitCount, float multiplier)
    {
        if (playerID < 0 || playerID >= 2) return;
        _gambit[playerID] = new GambitState
        {
            hitsRequired = hitCount,
            bonusMulti   = multiplier,
            active       = true
        };
    }
 
    public void EndGambitWindow(int playerID) //if (phaseS.GetCurrentPhase() == PhaseManager.Phase.Transition && players[pl].accuratey.Count != 0)
    {
        if (playerID < 0 || playerID >= 2) return;
        // calculate score to add here
        int amount = noteHitTransplant[playerID].accuracies.Count(x => x == "Perfect");
        int total = noteHitTransplant[playerID].accuracies.Count;
        if (((amount / total) * 100) < 75)
        {
            _playerScores[playerID] -= amount * 3;
        }
        else
        {
            _playerScores[playerID] += amount * 3;
        }
        noteHitTransplant[playerID].accuracies.Clear();
        _gambit[playerID] = new GambitState
        {
            hitsRequired = 0,
            bonusMulti = 0,
            active = false
        };
    }

    #endregion Score Management

    #region Combo Management [to discuss]

    //private void IncrementCombo(int playerID)
    //{
    //    _playercombos[playerID]++;

    //    if (_playercombos[playerID] > _playermaxcombos[playerID])
    //    {
    //        _playermaxcombos[playerID] = _playercombos[playerID];
    //        onmaxcomboupdated?.invoke(playerid, _playermaxcombos[playerID]);
    //    }

    //    oncombochanged?.invoke(playerid, _playercombos[playerID]);
    //}

    //private void BreakCombo(int playerID)
    //{
    //    if (_playerCombos[playerID] > 0)
    //    {
    //        _playerCombos[playerID] = 0;
    //        _consecutiveMisses[playerID] = 0;

    //        // Fire combo break event
    //        OnComboBreak?.Invoke(playerID);
    //        OnComboChanged?.Invoke(playerID, 0);
    //    }
    //}

    //public int GetCombo(int playerID)
    //{
    //    return (playerID >= 0 && playerID < 2) ? _playerCombos[playerID] : 0;
    //}

    //public int GetMaxCombo(int playerID)
    //{
    //    return (playerID >= 0 && playerID < 2) ? _playerMaxCombos[playerID] : 0;
    //}

    //private float GetComboMultiplier(int playerID)
    //{
    //    int combo = GetCombo(playerID);
    //    int comboLevel = combo / comboLevelInterval;
    //    return 1f + (comboLevel * comboMultiplierStep);
    //}

    #endregion Combo Management [to discuss]

    #region Accuracy Getters

    // for score display screen at end and showing all types of hits (osu inspiration here)

    public int GetPerfectHits(int playerID)
    {
        return (playerID >= 0 && playerID < 2) ? _perfectHits[playerID] : 0;
    }

    public int GetGoodHits(int playerID)
    {
        return (playerID >= 0 && playerID < 2) ? _goodHits[playerID] : 0;
    }

    public int GetOkayHits(int playerID)
    {
        return (playerID >= 0 && playerID < 2) ? _okayHits[playerID] : 0;
    }

    public int GetMissedHits(int playerID)
    {
        return (playerID >= 0 && playerID < 2) ? _missedHits[playerID] : 0;
    }

    public int GetTotalHits(int playerID)
    {
        return GetPerfectHits(playerID) + GetGoodHits(playerID) + 
               GetOkayHits(playerID) + GetMissedHits(playerID);
    }

    public float GetAccuracyPercentage(int playerID)
    {
        int total = GetTotalHits(playerID);
        if (total == 0) return 0f;
        
        float weighted = (GetPerfectHits(playerID) * 300f) +
                         (GetGoodHits(playerID) * 100f) +
                         (GetOkayHits(playerID) *  50f);

        return weighted / (total * 300f) * 100f;
    }

    #endregion Accuracy Getters
}