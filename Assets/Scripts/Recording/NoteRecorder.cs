using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack phase handler, visualizes notes and writes them to a json through returning ChartData
/// </summary>

public class NoteRecorder : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private int playerID;
    
    [Header("Note Recording Settings")]
    [SerializeField] private float tapThreshold = 0.15f;
    [SerializeField] private float maxHoldBeats = 2f;
    [SerializeField] private float staminaCostPerNote = 5f;

    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite downSprite;

    private ChartData _currentChart;
    private bool _isRecording;
    private string _playerTag;
    private PlayerController _playerController;
    private int _recordingStartBeat;
    private int _beatOffset;
    
    private Dictionary<int, int> _activeHoldStartBeats = new Dictionary<int, int>();
    private Dictionary<int, GameObject> _activeVisualHolds = new Dictionary<int, GameObject>();
    private Dictionary<int, float> _holdStartTimes = new Dictionary<int, float>();
    private Dictionary<int, int> _lastPlacedBeat = new Dictionary<int, int>();

    #region Init

    private void Awake()
    {
        CalculateBeatOffset();
    }

    private void CalculateBeatOffset() // do the maths
    {
        if (GameBoundaries.Instance == null) return;
        if (AudioManager.Instance == null) return;
        if (BeatManager.Instance == null) return;
        float noteSpeed = AudioManager.Instance.NoteSpeed;
        float travelDistance = GameBoundaries.Instance.TravelDistance;
        float travelTime = travelDistance / noteSpeed;
        float beatDuration = 60f / BeatManager.Instance.BPM;
        float travelBeats = travelTime / beatDuration;
        _beatOffset = Mathf.CeilToInt(travelBeats);
    }

    #endregion Init

    #region Recording

    public void StartRecording(string playerTag) // called by phasemanager to start a chart designated for a player with a bpm
    {
        CalculateBeatOffset();
        _playerTag = playerTag;
        int bpm = Mathf.RoundToInt(BeatManager.Instance.BPM);
        _currentChart = new ChartData($"Chart_{playerTag}_Round{Time.time}", playerTag, bpm);
        
        int startBeat = (BeatManager.Instance.CurrentBar * 4) + BeatManager.Instance.CurrentBeat;
        _currentChart.recordingStartBeat = startBeat;
        _recordingStartBeat = startBeat;
        _isRecording = true;
        _activeHoldStartBeats.Clear();
        _activeVisualHolds.Clear();
        _holdStartTimes.Clear();
        _lastPlacedBeat.Clear();
        _playerController = GameManager.Instance?.GetPlayer(playerID);
        
        if (_playerController != null)
        {
            _playerController.SetRecording(true);
        }
        
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLanePressed += OnLanePressed;
            InputManager.Instance.OnLaneReleased += OnLaneReleased;
        }
    }

    public ChartData StopRecording() // saved to phasemanager
    {
        if (_playerController != null)
        {
            _playerController.SetRecording(false);
        }
        
        var activeLanes = new List<int>(_activeHoldStartBeats.Keys);
        foreach (int lane in activeLanes)
        {
            OnLaneReleased(lane);
        }
        
        _isRecording = false;
        _activeVisualHolds.Clear();
        _holdStartTimes.Clear();
        
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLanePressed -= OnLanePressed;
            InputManager.Instance.OnLaneReleased -= OnLaneReleased;
        }
        
        return _currentChart;
    }

    #endregion Recording

    #region Inputs

    private void OnLanePressed(int lane) // to spawn notes for visual feedback
    {
        if (!_isRecording) return;
        if (!IsMyLane(lane)) return;
        if (_activeHoldStartBeats.ContainsKey(lane)) return;
        if (_activeVisualHolds.ContainsKey(lane)) return;

        int currentAbsoluteBeat = (BeatManager.Instance.CurrentBar * 4) + BeatManager.Instance.CurrentBeat;
        if (currentAbsoluteBeat <= _recordingStartBeat) return;

        int absoluteBeat = GetNearestBeat();
        if (_lastPlacedBeat.TryGetValue(lane, out int lastBeat) && lastBeat == absoluteBeat) return;

        int relativeBeat = absoluteBeat - _recordingStartBeat;
        int offsetBeat = relativeBeat + _beatOffset;

        if (_playerController != null && !_playerController.UseStamina(staminaCostPerNote)) return;

        _lastPlacedBeat[lane] = absoluteBeat;
        _activeHoldStartBeats[lane] = offsetBeat;
        _holdStartTimes[lane] = Time.time;

        SpawnVisualHoldNote(lane, absoluteBeat);
    }

    private void OnLaneReleased(int lane) // note snapping on release
    {
        if (!_isRecording) return;
        if (!IsMyLane(lane)) return;
        if (!_activeHoldStartBeats.TryGetValue(lane, out var startBeatOffset)) return;
        
        int absoluteBeat = GetNearestBeat();
        int relativeBeat = absoluteBeat - _recordingStartBeat;
        int releaseBeatOffset = relativeBeat + _beatOffset;
        float holdDuration = Time.time - _holdStartTimes[lane];
        int beatDuration = releaseBeatOffset - startBeatOffset;
        
        GameObject visualHold = _activeVisualHolds.GetValueOrDefault(lane);
        
        if (holdDuration < tapThreshold || beatDuration <= 0)
        {
            if (visualHold != null && visualHold.TryGetComponent<HoldNote>(out var holdNote))
            {
                Vector3 headPos = holdNote.Head != null ? holdNote.Head.transform.position : visualHold.transform.position;
                
                NotePoolManager.Instance.ReturnNote("HoldNote", visualHold);
                _activeVisualHolds.Remove(lane);
                
                SpawnVisualTapNote(lane, absoluteBeat, headPos);
            }
            _currentChart.notes.Add(new NoteEntry(startBeatOffset, NoteType.Tap, lane));
            AudioManager.Instance?.PlayRecordNote();
        }
        else
        {
            if (visualHold != null && visualHold.TryGetComponent<HoldNote>(out var holdNote))
            {
                SnapTailToBeat(holdNote, absoluteBeat);
                holdNote.ReleaseHold();
                _activeVisualHolds.Remove(lane);
            }
            
            _currentChart.notes.Add(new NoteEntry(startBeatOffset, NoteType.Hold, lane, beatDuration));
            AudioManager.Instance?.PlayRecordHold();
        }
        
        _activeHoldStartBeats.Remove(lane);
        _holdStartTimes.Remove(lane);
    }

    #endregion Inputs

    #region Beat Snapping

    private int GetNearestBeat() // math helpers
    {
        int currentBeat = BeatManager.Instance.CurrentBeat;
        int currentBar = BeatManager.Instance.CurrentBar;
        float beatProgress = BeatManager.Instance.GetBeatProgress();
        if (currentBeat == 0) currentBeat = 1;
        int absoluteBeat = (currentBar * 4) + currentBeat;
        if (beatProgress > 0.5f) absoluteBeat++;
        return absoluteBeat;
    }

    private float CalculateBeatBarPosition(int targetBeat) // finds beat bar position to snap to
    {
        if (GameBoundaries.Instance == null) return 0f;
        
        int currentBeat = BeatManager.Instance.CurrentBeat;
        int currentBar = BeatManager.Instance.CurrentBar;
        float beatProgress = BeatManager.Instance.GetBeatProgress();
        
        float currentState = (currentBar * 4 + currentBeat) + beatProgress;
        float beatDuration = 60f / BeatManager.Instance.BPM;
        float timeSinceSpawn = (currentState - targetBeat) * beatDuration;
        float noteSpeed = AudioManager.Instance.NoteSpeed;
        float distanceTraveled = timeSinceSpawn * noteSpeed;
        float beatBarY = GameBoundaries.Instance.HitZoneY + distanceTraveled;
        
        return beatBarY;
    }

    private void SnapTailToBeat(HoldNote holdNote, int absoluteBeat) // for hold notes
    {
        float tailY = CalculateBeatBarPosition(absoluteBeat);
        
        if (holdNote.tail != null)
        {
            Vector3 tailPos = holdNote.tail.transform.position;
            tailPos.y = tailY;
            holdNote.tail.transform.position = tailPos;
        }
    }

    #endregion Beat Snapping
    
    #region Visuals

    private void SpawnVisualHoldNote(int lane, int absoluteBeatIndex) // using note pool for hold visuals in attack phase
    {
        if (GameBoundaries.Instance == null) return;
        
        float xPos = GameBoundaries.Instance.GetLaneX(lane);
        float hitY = GameBoundaries.Instance.HitZoneY;
        Vector3 spawnPos = new Vector3(xPos, hitY, 0);
        GameObject noteObj = NotePoolManager.Instance.SpawnNote("HoldNote", spawnPos);
        if (noteObj == null) return;
        
        if (noteObj.TryGetComponent<HoldNote>(out var holdNote))
        {
            float noteSpeed = AudioManager.Instance.NoteSpeed;
            holdNote.Init("HoldNote", lane, noteSpeed, false);
            holdNote.SetRecordingMode(true);
            
            float headY = CalculateBeatBarPosition(absoluteBeatIndex);
      
            if (holdNote.Head != null)
            {
                Vector3 headPos = holdNote.Head.transform.position;
                headPos.y = headY;
                holdNote.Head.transform.position = headPos;
            }
            
            float distance = headY - hitY;
            float duration = Mathf.Max(0.1f, distance / noteSpeed);
            holdNote.SetHoldDuration(duration);

            if (lane % 2 == 1) //down note
            {
                Debug.Log("down sprite");
                holdNote.Head.GetComponent<SpriteRenderer>().sprite = downSprite;
                holdNote.tail.GetComponent<SpriteRenderer>().sprite = downSprite;
            }
            else //left note
            {
                holdNote.Head.GetComponent<SpriteRenderer>().sprite = leftSprite;
                holdNote.tail.GetComponent<SpriteRenderer>().sprite = leftSprite;
            }
        }
        
        _activeVisualHolds[lane] = noteObj;
    }

    private void SpawnVisualTapNote(int lane, int absoluteBeatIndex, Vector3 position)
    {
        GameObject noteObj = NotePoolManager.Instance.SpawnNote("TapNote", position);
        if (noteObj == null) return;
        
        if (noteObj.TryGetComponent<TapNote>(out var tapNote))
        {
            float noteSpeed = AudioManager.Instance.NoteSpeed;
            tapNote.Init("TapNote", lane, noteSpeed, false);

            if (lane % 2 == 1) //down note
            {
                tapNote.GetComponent<SpriteRenderer>().sprite = downSprite;
            }
            else //left note
            {
                tapNote.GetComponent<SpriteRenderer>().sprite = leftSprite;
            }
        }
    }
    
    private void Update() // makes sure lanes auto release on phase transition
    {
        if (!_isRecording) return;
        
        float beatDuration = 60f / (BeatManager.Instance?.BPM ?? 120f);
        float maxHoldDuration = maxHoldBeats * beatDuration;
        
        List<int> lanesToAutoRelease = null;
        
        foreach (var pair in _activeVisualHolds)
        {
            int lane = pair.Key;
            GameObject noteObj = pair.Value;
            
            if (noteObj == null || !noteObj.activeInHierarchy || !_holdStartTimes.ContainsKey(lane)) continue;
            
            float duration = Time.time - _holdStartTimes[lane];
            
            if (duration >= maxHoldDuration)
            {
                lanesToAutoRelease ??= new List<int>();
                lanesToAutoRelease.Add(lane);
            }
            else if (noteObj.TryGetComponent<HoldNote>(out var holdNote))
            {
                holdNote.SetHoldDuration(duration);
            }
        }
        
        if (lanesToAutoRelease == null) return;
        
        foreach (int lane in lanesToAutoRelease)
        {
            OnLaneReleased(lane);
        }
    }

    #endregion Visuals
    
    private bool IsMyLane(int lane) => playerID == 1 ? lane <= 1 : lane >= 2;
    public int GetRecordedNoteCount() => _currentChart?.notes.Count ?? 0;
}