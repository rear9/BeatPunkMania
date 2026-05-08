using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

/// <summary>
/// Calculates the spawn location and proper travel time of notes based on the BPM of the audio track to ensure they reach hitzone on beat
/// </summary>

public class NoteSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int playerID;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite downSprite;

    private Coroutine _playbackCoroutine;
    private bool _isPlaying;
    private float _spawnOffsetBeats;
    private int _notesSpawned;

    private ChartData _currentChart;

    #region Init

    private void Awake()
    {
        CalcSpawnTiming();
    }

    private void CalcSpawnTiming()
    {
        if (GameBoundaries.Instance == null) return;
        float noteSpeed = AudioManager.Instance.NoteSpeed;
        float travelDistance = GameBoundaries.Instance.TravelDistance;
        float travelTime = travelDistance / noteSpeed;
        float beatDuration = 60f / (BeatManager.Instance?.BPM ?? 120f);
        _spawnOffsetBeats = Mathf.CeilToInt(travelTime / beatDuration);
    }

    #endregion Init

    #region Playback

    public void PlayChart(ChartData chart, float speed)
    {
        if (_isPlaying) return;
        if (chart == null) return;
        if (chart.notes == null || chart.notes.Count == 0) return;

        _currentChart = chart;
        _notesSpawned = 0;
        CalcSpawnTiming();
        
        _playbackCoroutine = StartCoroutine(StartPlayback(chart));
    }

    private IEnumerator StartPlayback(ChartData chart)
    {
        _isPlaying = true;
        List<NoteEntry> sortedNotes = new List<NoteEntry>(chart.notes);
        sortedNotes.Sort((a, b) => a.beatIndex.CompareTo(b.beatIndex));
        
        int noteIndex = 0;
        
        while (BeatManager.Instance == null) yield return null;
        
        int startBar = BeatManager.Instance.CurrentBar;
        int startBeat = BeatManager.Instance.CurrentBeat;
        float startBeatPos = (startBar * 4 + startBeat) + BeatManager.Instance.GetBeatProgress();
        
        while (_isPlaying && noteIndex < sortedNotes.Count)
        {
            int currentBeat = BeatManager.Instance.CurrentBeat;
            int currentBar = BeatManager.Instance.CurrentBar;
            float beatProgress = BeatManager.Instance.GetBeatProgress();
            float currentBeatPos = (currentBar * 4 + currentBeat) + beatProgress;
            float relativeBeatPos = currentBeatPos - startBeatPos;
            
            while (noteIndex < sortedNotes.Count)
            {
                NoteEntry noteData = sortedNotes[noteIndex];
                float targetSpawnBeat = noteData.beatIndex - _spawnOffsetBeats;
                if (relativeBeatPos < targetSpawnBeat) break;
                
                if (relativeBeatPos > targetSpawnBeat + 2f)
                {
                    noteIndex++;
                    continue;
                }
                SpawnNote(noteData);
                _notesSpawned++;
                noteIndex++;
            }
            yield return null;
        }
        _isPlaying = false;
        Debug.Log($"p{playerID} spawns - spawned {_notesSpawned}/{sortedNotes.Count}");
    }
    
    public void StopPlayback()
    {
        _isPlaying = false;
        if (_playbackCoroutine != null)
        {
            StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
        }
    }

    #endregion Playback

    #region DoubleTime
    
    // probably run a scan through the chart for the specific lane and build a list of extra tap notes inserted inbetween beatIndexes
    // should be a coroutine similar to StartPlayBack @ line 51, call SpawnNote for each extra
    public void EnableDoubleTime(int lane) // this got cut
    {
    }
    
    // stop and null the coroutine
    public void DisableDoubleTime(int lane)
    {
    }

    #endregion DoubleTime

    #region Spawning

    private void SpawnNote(NoteEntry noteData)
    {
        if (GameBoundaries.Instance == null) return;

        int localLane = playerID == 1 
            ? Mathf.Clamp(noteData.lane - 2, 0, 1)
            : Mathf.Clamp(noteData.lane + 2, 2, 3);

        if (localLane < 0 || localLane >= 4) return;
        
        float xPos = GameBoundaries.Instance.GetLaneX(localLane);
        float topY = GameBoundaries.Instance.TopY;
        Vector3 spawnPos = new Vector3(xPos, topY, 0);
        string poolKey = noteData.type == NoteType.Tap ? "TapNote" : "HoldNote";

        GameObject noteObj = NotePoolManager.Instance.SpawnNote(poolKey, spawnPos);
        
        if (noteObj == null) return;
        if (noteObj.TryGetComponent<NoteCore>(out var noteCore))
        {
            float noteSpeed = AudioManager.Instance.NoteSpeed;

            noteCore.Init(poolKey, localLane, noteSpeed, true);

            if (noteCore is HoldNote holdNote)
            {
                float beatDuration = 60f / BeatManager.Instance.BPM;
                float timeDuration = noteData.holdDurationBeats * beatDuration;
                holdNote.SetHoldDuration(timeDuration);

                if (localLane % 2 == 1) //down note
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
            else
            {
                if (localLane % 2 == 1) //down note
                {
                    noteCore.GetComponent<SpriteRenderer>().sprite = downSprite;
                }
                else
                {
                    noteCore.GetComponent<SpriteRenderer>().sprite = leftSprite;
                }
            }
        }
        else NotePoolManager.Instance.ReturnNote(poolKey, noteObj);
    }

    #endregion Spawning
}