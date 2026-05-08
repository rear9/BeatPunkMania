using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Detects when notes should be able to be hit in each lane (1 per player)
/// </summary>

public class NoteHitDetection : MonoBehaviour
{
    [Header("Hit Detection Settings")]
    [SerializeField] private float hitWindowTolerance = 2.0f; // how big the window is
    [SerializeField] private Transform hitZoneCenter; // uses the y pos of the hit zone

    private PlayerController _player;
    public ScoreManager scoring;
    private Dictionary<int, List<NoteCore>> _notesInLanes = new Dictionary<int, List<NoteCore>>();
    private float _hitZoneY;
    private bool _isSubscribed = false;
    public List<string> accuracies = new();


    #region Init

    public void Init(PlayerController player)
    {
        _player = player;
        _notesInLanes[_player.LeftLane] = new List<NoteCore>(); // makes a list of notes in the lane for each player (this gets used ~line 229)
        _notesInLanes[_player.RightLane] = new List<NoteCore>();

        _hitZoneY = hitZoneCenter != null
            ? hitZoneCenter.position.y
            : GameBoundaries.Instance.HitZoneY;

        SubscribeToInput();
    }

    private void SubscribeToInput()
    {
        if (_isSubscribed) return;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLanePressed += HandleLanePressed;
            InputManager.Instance.OnLaneReleased += HandleLaneReleased;
            _isSubscribed = true;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromInput();
    }

    private void UnsubscribeFromInput()
    {
        if (!_isSubscribed) return;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLanePressed -= HandleLanePressed; // input functions
            InputManager.Instance.OnLaneReleased -= HandleLaneReleased;
            _isSubscribed = false;
        }
    }

    #endregion Init

    #region Inputs

    private void HandleLanePressed(int lane)
    {
        if (!_player.IsMyLane(lane)) return;
        if (!_player.IsActive) return;
        if (_player.IsRecording()) return;
        bool isPreGame = PhaseHandler.Instance?.GetCurrentPhase() == PhaseHandler.Phase.Transition
                         && PhaseHandler.Instance?.GetCurrentRound() == 0;
        if (isPreGame) return;

        NoteCore closestNote = GetClosestNoteInLane(lane);

        if (closestNote != null)
        {
            float timingOffset = BeatManager.Instance.CalculateTimingOffset(Time.time); // finds accuracy here
            HitAccuracy accuracy = BeatManager.Instance.GetAccuracy(timingOffset);

            if (closestNote is HoldNote holdNote)
            {
                if (holdNote.IsHolding())
                {
                    holdNote.OnTapped(); // counts as a miss if you tap a hold note
                }
                else if (accuracy != HitAccuracy.Miss)
                {
                    HandleHoldStart(holdNote, accuracy, timingOffset); // calculates start of hold accuracy
                }
                else
                {
                    HandleNoteMiss(lane);
                }
            }
            else
            {
                if (accuracy != HitAccuracy.Miss)
                {
                    HandleNoteHit(closestNote, accuracy, timingOffset); // accuracies for standard notes
                }
                else
                {
                    HandleNoteMiss(lane);
                }
            }
        }
        else
        {
            bool isDefendPhase = PhaseHandler.Instance?.GetCurrentPhase() == PhaseHandler.Phase.Defend;
            if (isDefendPhase)
                HandleNoteMiss(lane);
        }
    }

    private void HandleLaneReleased(int lane) // for holds
    {
        if (!_player.IsMyLane(lane)) return;
        if (!_player.IsActive) return;
        bool isPreGame = PhaseHandler.Instance?.GetCurrentPhase() == PhaseHandler.Phase.Transition
                         && PhaseHandler.Instance?.GetCurrentRound() == 0;
        if (isPreGame) return;

        NoteCore activeHold = GetActiveHoldInLane(lane);

        if (activeHold is HoldNote holdNote)
        {
            holdNote.EndHold();
        }
    }

    #endregion Inputs

    #region Hits

    private void HandleNoteHit(NoteCore note, HitAccuracy accuracy, float timingOffset) // accuracy calculations and debugs
    {
        float accuracyPercent = BeatManager.Instance.CalculateAccuracyPercent(timingOffset);
        float spatialDistance = Mathf.Abs(note.GetHitPositionY() - _hitZoneY);
        Debug.Log($"[hit] P{_player.PlayerID} / Lane {note.GetLane()} / {accuracy} / {accuracyPercent:F1}% / timing: {timingOffset * 1000f:F1}ms / dist: {spatialDistance:F3}u");

        if (scoring._gambit[_player.PlayerID].active && this.accuracies.Count < 4)
        {
            string acc = accuracy.ToString();
            this.accuracies.Add(acc);
        }

        ScoreManager.Instance?.RegisterHit(_player.PlayerID, accuracy);
        HitEffectManager.Instance?.SpawnHitEffectForNote(accuracy, note);
        _player.OnNoteHitSuccessful(accuracy);

        if (accuracy == HitAccuracy.Miss) AudioManager.Instance?.PlayNoteMiss();
        else AudioManager.Instance?.PlayHitPerfect();
        note.OnHit();
    }

    private void HandleHoldStart(HoldNote holdNote, HitAccuracy startAccuracy, float timingOffset) // basically same as above for holds
    {
        float accuracyPercent = BeatManager.Instance.CalculateAccuracyPercent(timingOffset);
        float spatialDistance = Mathf.Abs(holdNote.GetHitPositionY() - _hitZoneY);
        Debug.Log($"[hold] P{_player.PlayerID} / Lane {holdNote.GetLane()} / {startAccuracy} /" +
                  $" {accuracyPercent:F1}% / timing: {timingOffset * 1000f:F1}ms / dist: {spatialDistance:F3}u"); // debug line
        // OnNoteHitSuccessful fires in CompleteHoldWithAccuracy once the hold is actually finished

        if (scoring._gambit[_player.PlayerID].active && accuracies.Count < 4)
        {
            string acc = startAccuracy.ToString();
            accuracies.Add(acc);
        }

        holdNote.BeginHold(startAccuracy);
        if (startAccuracy == HitAccuracy.Miss) AudioManager.Instance?.PlayNoteMiss();
        else AudioManager.Instance?.PlayHitPerfect();
    }

    private void HandleNoteMiss(int lane)
    {
        ScoreManager.Instance?.RegisterMiss(_player.PlayerID); // register a miss & spawn miss effect
        HitEffectManager.Instance?.SpawnMissEffect(lane);
        _player.OnNoteMissed();
        AudioManager.Instance.PlaySFXClip(AudioManager.Instance.noteMiss);
        if (scoring._gambit[_player.PlayerID].active && accuracies.Count < 4)
        {
            accuracies.Add("Miss");
        }
    }

    #endregion Hits

    #region Note Finding

    private NoteCore GetClosestNoteInLane(int lane) // finds & returns closest note through y position distance calculation
    {
        if (!_notesInLanes.ContainsKey(lane)) return null;
        NoteCore closest = null;
        float closestDistance = float.MaxValue;

        foreach (var note in _notesInLanes[lane])
        {
            if (note == null || !note.gameObject.activeInHierarchy || !note.IsActive())
            {
                continue;
            }

            float distance = Mathf.Abs(note.GetHitPositionY() - _hitZoneY);

            if (distance <= hitWindowTolerance && distance < closestDistance)
            {
                closestDistance = distance;
                closest = note;
            }
        }
        return closest;
    }

    private NoteCore GetActiveHoldInLane(int lane) // used to cancel holds in phase transitions
    {
        if (!_notesInLanes.ContainsKey(lane)) return null;

        foreach (var note in _notesInLanes[lane])
        {
            if (note == null || !note.gameObject.activeInHierarchy) continue;

            if (note is HoldNote holdNote && holdNote.IsHolding())
            {
                return holdNote;
            }
        }
        return null;
    }

    #endregion Note Finding

    #region Note Reg

    public void RegisterNote(NoteCore note) // searching then adding and removing notes in each lane
    {
        if (note == null) return;

        int lane = note.GetLane();

        if (_notesInLanes.ContainsKey(lane))
        {
            if (!_notesInLanes[lane].Contains(note))
            {
                _notesInLanes[lane].Add(note);
            }
        }
    }

    public void UnregisterNote(NoteCore note)
    {
        if (note == null) return;

        int lane = note.GetLane();

        if (_notesInLanes.ContainsKey(lane))
        {
            _notesInLanes[lane].Remove(note);
        }
    }

    public void ClearAllNotes() // optional for debugging
    {
        foreach (var list in _notesInLanes.Values)
        {
            list.Clear();
        }
    }

    #endregion Note Reg
}