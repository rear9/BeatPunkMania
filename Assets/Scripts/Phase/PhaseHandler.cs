using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages attack/defend/transition phases, handles UI changes & tutorialization.
/// </summary>

public class PhaseHandler : MonoBehaviour
{
    public static PhaseHandler Instance { get; private set; }
    
    public enum Phase { Attack, Defend, Transition }
    
    [Header("Phase Settings")]
    private int _phaseDurationBeats; // calculated at runtime: nearest multiple of 4 to 15s at current BPM
    [SerializeField] private int transitionDurationBeats = 8;
    
    
    [Header("Refs")]
    [SerializeField] private NoteRecorder player1Recorder;
    [SerializeField] private NoteRecorder player2Recorder;
    [SerializeField] private NoteSpawner player1Spawner;
    [SerializeField] private NoteSpawner player2Spawner;
    [SerializeField] private BeatBarSpawner beatBarSpawner;
    [SerializeField] private GameObject p1Stamina;
    [SerializeField] private GameObject p2Stamina;
    [SerializeField] private GameObject p1Confidence;
    [SerializeField] private GameObject p2Confidence;

    [Header("Phase Visuals")]
    [SerializeField] private SpriteRenderer phaseIndicator;
    [SerializeField] private Sprite attackPhaseSprite;
    [SerializeField] private Sprite defendPhaseSprite;
    [SerializeField] private Sprite transitionSprite;
    
    [Header("Phase Countdown")]
    [SerializeField] private SpriteRenderer countdownIndicator; // separate renderer from phase indicator
    [SerializeField] private Sprite countdown3Sprite;
    [SerializeField] private Sprite countdown2Sprite;
    [SerializeField] private Sprite countdown1Sprite;
    
    public UnityEvent<Phase> OnPhaseChanged;
    public event Action OnPhaseExpired;
    
    private Phase _currentPhase = Phase.Transition;
    private int _currentRound = 0;
    
    private ChartData _p1Chart;
    private ChartData _p2Chart;

    public bool almightyTutorial = true;
    public bool tutorial = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (countdownIndicator != null) countdownIndicator.gameObject.SetActive(false);
        StartCoroutine(ThreePhaseLoop());
    }
    
    private IEnumerator ThreePhaseLoop() // changed to a 3-loop for play party
    {
        while (GameManager.Instance == null || !GameManager.Instance.GameHasStarted)
        {
            yield return null;
        }
        
        // beat tracking and music already started by GameManager
        float rawBeats = 15f * (AudioManager.Instance.BPM / 60f);
        _phaseDurationBeats = Mathf.Max(4, Mathf.RoundToInt(rawBeats / 4f) * 4);
        float beatDuration = BeatManager.Instance.BeatDuration;
        beatBarSpawner.StartSpawning(spawnAtTop: false);
        
        for (int i = 0; i < 3; i++)
        {
            _currentRound++;
            yield return AttackPhase();
            yield return TransitionPhase(spawnAtTop: true);
            yield return DefendPhase();
            yield return TransitionPhase(spawnAtTop: false);
            OnPhaseExpired?.Invoke();
        }
        yield return new WaitForSecondsRealtime(2f);
        //SceneManager.LoadScene("SelectScene");

    }

    #region Phase Implementation

    private IEnumerator AttackPhase()
    {
        p1Stamina.SetActive(true);
        p2Stamina.SetActive(true);
        p1Confidence.SetActive(false);
        p2Confidence.SetActive(false);

        _currentPhase = Phase.Attack;
        UpdateVisuals();
        OnPhaseChanged?.Invoke(Phase.Attack);
        
        beatBarSpawner.StartSpawning(spawnAtTop: false);
        
        player1Recorder.StartRecording("P1");
        player2Recorder.StartRecording("P2");
        
        // wait the main phase duration, showing a 3-2-1 on the final beats
        yield return WaitForBeatsWithCountdown(_phaseDurationBeats);
        
        _p1Chart = player1Recorder.StopRecording();
        _p2Chart = player2Recorder.StopRecording();
        
        beatBarSpawner.StopSpawning();
        
        ChartSerializer.SaveChart(_p1Chart);
        ChartSerializer.SaveChart(_p2Chart);
    }

    private IEnumerator DefendPhase()
    {
        p1Stamina.SetActive(false);
        p2Stamina.SetActive(false);
        p1Confidence.SetActive(true);
        p2Confidence.SetActive(true);

        UIManager uIManager = UIManager.Instance;
        if (almightyTutorial == true)
        {
            uIManager.phaseChange.SetActive(false);
            foreach (GameObject obj in uIManager.tutButts)
            {
                obj.SetActive(true);
            }
            tutorial = true;
            almightyTutorial = false;
        }
        _currentPhase = Phase.Defend;
        UpdateVisuals();
        OnPhaseChanged?.Invoke(Phase.Defend);
        
        float noteSpeed = GetNoteSpeed();
        
        beatBarSpawner.StartSpawning(spawnAtTop: true);
        
        player1Spawner.PlayChart(_p2Chart, noteSpeed);
        player2Spawner.PlayChart(_p1Chart, noteSpeed);

        yield return WaitForBeatsWithCountdown(_phaseDurationBeats);
        
        beatBarSpawner.StopSpawning();
        player1Spawner.StopPlayback();
        player2Spawner.StopPlayback();
    }

    private IEnumerator TransitionPhase(bool spawnAtTop)
    {
        UIManager uiManager = UIManager.Instance;
        if (almightyTutorial == true)
        {
            uiManager.phaseChange.SetActive(true);
        }
        _currentPhase = Phase.Transition;
        UpdateVisuals();
        OnPhaseChanged?.Invoke(Phase.Transition);
        beatBarSpawner.StopSpawning();
        yield return WaitForBeats(transitionDurationBeats);
        float beatDuration = BeatManager.Instance.BeatDuration;
        beatBarSpawner.StartSpawning(spawnAtTop);
        NotePoolManager.Instance.ReturnAllNotes();
    }

    #endregion Phase Implementation

    #region Helpers
    
    private IEnumerator WaitForBeatsWithCountdown(int totalBeats) // cd inbetween phases
    {
        int startAbsoluteBeat = GetAbsoluteBeat();
        int targetBeat = startAbsoluteBeat + totalBeats;
        int lastCountdownShown = -1;
        
        while (true)
        {
            int currentAbsoluteBeat = GetAbsoluteBeat();
            if (currentAbsoluteBeat >= targetBeat) break;
            int beatsRemaining = targetBeat - currentAbsoluteBeat;
            
            if (beatsRemaining <= 3 && beatsRemaining != lastCountdownShown)
            {
                ShowCountdown(beatsRemaining);
                lastCountdownShown = beatsRemaining;
            }
            
            yield return null;
        }
        HideCountdown();
    }

    private IEnumerator WaitForBeats(int beats)
    {
        int targetBeat = GetAbsoluteBeat() + beats;
        
        while (GetAbsoluteBeat() < targetBeat)
        {
            yield return null;
        }
    }
    
    private int GetAbsoluteBeat()
    {
        int bar  = BeatManager.Instance.CurrentBar;
        int beat = BeatManager.Instance.CurrentBeat;
        return (bar * 4) + beat;
    }
    
    private void ShowCountdown(int number) // sprite switching
    {
        if (countdownIndicator == null) return;
        
        Sprite sprite = number switch
        {
            3 => countdown3Sprite,
            2 => countdown2Sprite,
            1 => countdown1Sprite,
            _ => null
        };
        
        if (sprite == null) return;
        
        countdownIndicator.sprite = sprite;
        countdownIndicator.gameObject.SetActive(true);
        AudioManager.Instance.PlayCountdownTick();
    }
    
    private void HideCountdown()
    {
        if (countdownIndicator != null)
        {
            countdownIndicator.gameObject.SetActive(false);
        }
    }
    
    private void UpdateVisuals()
    {
        if (phaseIndicator == null) return;
        phaseIndicator.sprite = _currentPhase switch
        {
            Phase.Attack     => attackPhaseSprite,
            Phase.Defend     => defendPhaseSprite,
            Phase.Transition => transitionSprite,
            _                => transitionSprite
        };
        // AudioManager.Instance?.PlayPhaseTransition();
    }
    
    private float GetNoteSpeed()
    {
        return AudioManager.Instance != null ? AudioManager.Instance.NoteSpeed : 5f;
    }

    public Phase GetCurrentPhase() => _currentPhase;
    public int GetCurrentRound() => _currentRound;
    
    #endregion Helpers
}