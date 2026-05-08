using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds data about charts and notes
/// </summary>

[Serializable]
public class ChartData
{
    public string chartName;
    public string playerTag;
    public int bpm;
    public List<NoteEntry> notes;
    public int recordingStartBeat;

    public ChartData(string name, string tag, int beatsPerMinute)
    {
        chartName = name;
        playerTag = tag;
        bpm = beatsPerMinute;
        notes = new List<NoteEntry>();
    }
}

[Serializable]
public class NoteEntry
{
    public float beatIndex; // which beat this note appears on (0, 1, 2, 3...)
    public NoteType type; // tap or Hold
    public int lane; // 0-3
    public int holdDurationBeats; // for hold notes: how many beats it lasts
    
    // constructor for tap notes
    public NoteEntry(float beat, NoteType noteType, int noteLane)
    {
        beatIndex = beat;
        type = noteType;
        lane = noteLane;
        holdDurationBeats = 0;
    }
    
    // for hold notes
    public NoteEntry(float beat, NoteType noteType, int noteLane, int holdBeats)
    {
        beatIndex = beat;
        type = noteType;
        lane = noteLane;
        holdDurationBeats = holdBeats;
    }
}

public enum NoteType
{
    Tap,
    Hold
}