/*using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections.Generic;
using System.IO;

/*
public enum NoteType
{
    Tap,
    Hold,
    Release,
    unresolved
}
#1#

// collecting data
public struct NoteData
{
    public NoteType type;
    public KeyCode key;
    public float startTime;
    public float endTime;
}

// spawning notes
public struct NoteSpawnData // eventually switch to this ver?
{
    public NoteType type;
    public bool isLeftKey;
}

public class NoteSpawnerScript : MonoBehaviour
{
    // potentially split functinoality into a player manager object

    // spawnables
    [SerializeField] GameObject tapNote;
    [SerializeField] GameObject holdNote;
    [SerializeField] GameObject beat;

    // controllables
    [SerializeField] GameObject leftKeyIcon;
    [SerializeField] GameObject rightKeyIcon;
    [SerializeField] GameObject staminaBar;

    // settings
    [SerializeField] Color leftKeyColor;
    [SerializeField] Color rightKeyColor;
    [SerializeField] float noteSpeed = 7;
    [SerializeField] bool isMovingUp = true;
    [SerializeField] float xOffset = 0.6f;
    [SerializeField] float bpm = 120f;
    [SerializeField] KeyCode leftKey;
    [SerializeField] KeyCode rightKey;
    [SerializeField] float tapCost = 20f;
    [SerializeField] float holdCost = 30f;
    [SerializeField] string playerNum; // for differentiating chart
    [SerializeField] string enemyNum; // for differentiating chart

    // states
    [SerializeField] bool isAttack;    // attack/counter
    [SerializeField] bool phaseChanged;  // inputs accepted, timing started


    // variables
    float beatLength;
    float beatTime;
    float executeBeatTime;
    [SerializeField] int beatNum; // watching
    Color transparentColor;
    StaminaBarScript stamina;
    bool beatExecuted;

    // writing
    string chartFile;
    StreamWriter chartWriter;

    // manage notes
    List<NoteData> noteBuffer = new List<NoteData>();
    Dictionary<KeyCode, float> holdStartLog = new Dictionary<KeyCode, float>();

    // track holds
    Dictionary<bool, GameObject> activeHolds = new Dictionary<bool, GameObject>();

    // counter
    Dictionary<int, List<NoteSpawnData>> chartNotes = new Dictionary<int, List<NoteSpawnData>>(); // beat, notes // now it's a list because i forgot we can 2 notes per beat

    void Start()
    {
        // prep writing
        //chartFile = Path.Combine(Application.persistentDataPath, "chart_" + playerNum + ".txt"); // can't access on uni computers
        var file = "C:\\Temp";
        chartFile = Path.Combine(file, "chart_" + playerNum + ".txt");
        chartWriter = new StreamWriter(chartFile, false);
        //Debug.Log(Application.persistentDataPath);

        // prep misc
        transparentColor = new Color(0f, 0f, 0f, 0.5f);
        stamina = staminaBar.GetComponent<StaminaBarScript>();

        Init();
        //enabled = false; // wait for start signal (disabled due to errors starting stamina)
    }

    public void Init()
    {
        // prep timing (refreshed on each phase change to prevent timing drift)
        beatLength = 60f / bpm;
        //Debug.Log("BPS calculated as " + beatLength);
        beatTime = Time.time;
        executeBeatTime = Time.time + 0.5f * beatLength;
        beatNum = 0;
    }

    void OnApplicationQuit() // just in case
    {
        chartWriter?.Flush();
        chartWriter?.Close();
    }

    void OnEnable() // arbitrarily sawp between OnEnable and SetAttack, was split due to some now fixed bugs
    {
        Init();
        if (isAttack)
        {
            // prep writing (closed during phase end)
            chartWriter = new StreamWriter(chartFile, false);
            stamina.enabled = true;
        }
        else
        {
            // counter phase, load chart data

            // get path
            //chartFile = Path.Combine(Application.persistentDataPath, "chart_" + enemyNum + ".txt"); // can't access at uni
            var file = "C:\\Temp";
            chartFile = Path.Combine(file, "chart_" + enemyNum + ".txt"); // read from other chart
            if (!File.Exists(chartFile))
            {
                Debug.LogError("Chart unavailable");
                return;
            }

            // extract data
            string[] lines = File.ReadAllLines(chartFile);
            Debug.Log(lines.Length);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');

                // convert note data
                NoteSpawnData note = new NoteSpawnData
                {
                    isLeftKey = parts[1] == "L",
                    type = (NoteType)int.Parse(parts[2])
                };

                // convert beat
                int beat = int.Parse(parts[0]);
                if (!chartNotes.ContainsKey(beat))
                {
                    chartNotes[beat] = new List<NoteSpawnData>(); // new list
                }
                chartNotes[beat].Add(note); // add note to list
            }
        }
    }

    void OnDisable()
    {
        // reset visuals
        leftKeyIcon.GetComponent<SpriteRenderer>().color = transparentColor;
        rightKeyIcon.GetComponent<SpriteRenderer>().color = transparentColor;
        stamina.enabled = false;

        if (isAttack)
        {
            // force release
            foreach (var hold in activeHolds)
            {
                // BAD! (curr version doesn't tie to beat)
                hold.Value.GetComponent<HoldScript>().ActivateRelease(hold.Value.transform.position);
                // log new holds to chart
                string key = hold.Key ? "L" : "R";
                chartWriter.WriteLine(string.Format("{0},{1},2", key, beatNum));
            }

            // save chart
            chartWriter?.Flush();
            chartWriter?.Close();
        }

        // clear buffers (both phases just in case)
        noteBuffer.Clear();
        holdStartLog.Clear();
        activeHolds.Clear();
        chartNotes.Clear();
    }

    public void SetPhase(bool attack)
    {
        isAttack = attack;
        isMovingUp = attack; // attack notes move up, counter notes move down
        if (isAttack)
        {
            transform.position = new Vector3(transform.position.x, -2.75f, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, 4.45f, transform.position.z); // hardcoded for now I LOVE MAGIC NUMBERS
        }
    }

    void Update()
    {
        // key visual feedback
        KeyVisualFeedback(leftKey, leftKeyIcon);
        KeyVisualFeedback(rightKey, rightKeyIcon);

        // execute beat logic
        if (Time.time >= beatTime + beatLength && beatExecuted == false) // on beat
        {
            SpawnBeatVisualiser();

            if (!isAttack)
            {
                // terrible logic
                beatTime += beatLength;
                beatNum++;
                beatExecuted = false;
                if (chartNotes.TryGetValue(beatNum, out var notes))
                {
                    Debug.Log("trying to spawn something");
                    foreach (var note in notes)
                        SpawnNote(note);
                }
            }
            else
            {
                beatExecuted = true;
            }
        }

        if (isAttack)
        {
            // check notes
            HandleKey(leftKey);
            HandleKey(rightKey);

            if (Time.time >= beatTime + 1.5 * beatLength) // half beat delay
            //  [----|----][----|----][----|
            //       ^          ^    ^
            //  "beatTime"  beatTime now
            {
                //Debug.Log("Beat executed at " + Time.time);
                beatTime += beatLength;
                beatNum++;
                ExecuteBeat();
                beatExecuted = false;
            }
        }
    }

    // what it says on the tin
    void KeyVisualFeedback(KeyCode key, GameObject icon)
    {
        if (Input.GetKeyDown(key))
        {
            icon.GetComponent<SpriteRenderer>().color = Color.white;
        }
        else if (Input.GetKeyUp(key))
        {
            icon.GetComponent<SpriteRenderer>().color = transparentColor;
        }
    }

    // process notes
    void HandleKey(KeyCode key)
    {
        // track any presses
        if (Input.GetKeyDown(key))
        {
            holdStartLog[key] = Time.time;
            //Debug.Log(string.Format("{0} has been pressed at {1}", key, Time.time));
            return;
        }

        // track any releases
        if (Input.GetKeyUp(key))
        {
            if (!holdStartLog.ContainsKey(key))
            {
                Debug.LogWarning(string.Format("unregistered key: {0} has been released", key));
                return;
            }

            float pressStart = holdStartLog[key];
            noteBuffer.Add(
            new NoteData
            {
                key = key,
                type = NoteType.unresolved,
                startTime = pressStart,
                endTime = Time.time
            });

            // no longer tracking this note
            holdStartLog.Remove(key);
        }
    }

    // determines note types, spawns notes, saves to chart
    void ExecuteBeat()
    {
        // flags for clarity
        float windowStart = beatTime - beatLength / 2;
        float windowEnd = beatTime + beatLength / 2;

        // process all completed notes for this beat
        for (int i = 0; i < noteBuffer.Count; i++) // foreach the long way because items are modified
        {
            var note = noteBuffer[i];

            // release
            if (note.startTime <= windowEnd - beatLength && // started during last beat or prior
                note.endTime >= windowStart && note.endTime <= windowEnd) // ended during this beat
            {
                note.type = NoteType.Release;
                noteBuffer[i] = note;
                continue;
            }
            // tap
            if ((note.startTime >= windowStart && note.startTime <= windowEnd || // started in window or
                note.endTime >= windowStart && note.endTime <= windowEnd) && // ended in window
                note.endTime - note.startTime < beatLength) // less than 1 beat long
            {
                note.type = NoteType.Tap;
                noteBuffer[i] = note;
            }
        }

        // process held notes
        foreach (var buttonStart in holdStartLog)
        {
            // add only if started during this beat
            if (buttonStart.Value >= windowStart && buttonStart.Value <= windowEnd)
            {
                noteBuffer.Add(new NoteData
                {
                    key = buttonStart.Key,
                    type = NoteType.Hold,
                    startTime = buttonStart.Value,
                    endTime = -1 // null
                });
            }
        }

        // eliminate multiple notes in a lane (prioritizing releases and later notes)
        noteBuffer = ResolveLaneConflicts(noteBuffer);

        // spawn notes
        foreach (var note in noteBuffer)
        {
            // stamina checked moved inside SpawnNote
            // (release notes spawn regardless)

            SpawnNote(note);
            SaveToChart(beatNum, note);
            //Debug.Log(string.Format("Beat {0}: Spawning {1} note for {2}", beatNum, note.type, note.key));
        }

        noteBuffer.Clear();
    }

    // deal with multiple same lane notes per beat
    List<NoteData> ResolveLaneConflicts(List<NoteData> notes)
    {
        List<NoteData> finalNotes = new List<NoteData>();
        List<KeyCode> uniqueLanes = new List<KeyCode>(); // only 0-2 but here for scalability
        NoteData? currNote = null;

        // get unique lanes
        foreach (var note in noteBuffer)
        {
            if (!uniqueLanes.Contains(note.key))
                uniqueLanes.Add(note.key);
        }

        // for each lane, log highest priority note
        for (int i = 0; i < uniqueLanes.Count; i++)
        {
            currNote = null;
            foreach (var note in noteBuffer)
            {
                // ignore if wrong key
                if (note.key != uniqueLanes[i])
                    continue;

                // add first note of this key
                if (currNote == null)
                {
                    currNote = note;
                    continue;
                }
                // compare priority
                else if (note.type == NoteType.Release ||
                        (note.startTime > currNote.Value.startTime &&
                        currNote.Value.type != NoteType.Release))
                {
                    currNote = note; // replace stored note
                }
            }
            // add stored note to finalNotes
            if (currNote != null)
            {
                finalNotes.Add((NoteData)currNote);
            }
        }

        return finalNotes;
    }

    void SpawnBeatVisualiser()
    {
        // spawn beat visualiser
        float beatDelay = Time.time - beatTime - beatLength;
        float distanceOffset = beatDelay * noteSpeed * (isMovingUp ? 1f : -1f);
        //distanceOffset = 0;
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + distanceOffset, transform.position.z);

        GameObject newBeat = Instantiate(this.beat, spawnPos, Quaternion.identity, transform); // transform parented

        NoteScript noteScript = newBeat.GetComponent<NoteScript>();
        newBeat.GetComponent<SpriteRenderer>().color = transparentColor;
        noteScript.moveSpeed = noteSpeed;
        noteScript.movesUp = isMovingUp;
    }

    // attack phase overload
    void SpawnNote(NoteData note)
    {
        // delay adjustment
        float beatDelay = Time.time - beatTime;
        float distanceOffset = beatDelay * noteSpeed * (isMovingUp ? 1 : -1);
        //distanceOffset = 0;
        Vector3 spawnPos;
        Color colour;
        bool isLeftKey = note.key == leftKey;

        if (isLeftKey)
        {
            spawnPos = new Vector3(transform.position.x - xOffset, transform.position.y + distanceOffset, transform.position.z);
            colour = leftKeyColor;
        }
        else
        {
            spawnPos = new Vector3(transform.position.x + xOffset, transform.position.y + distanceOffset, transform.position.z);
            colour = rightKeyColor;
        }

        // spawn logic
        if (note.type == NoteType.Tap && stamina.GetStamina() >= tapCost)
        {
            // spawn
            GameObject newTapNote = Instantiate(this.tapNote, spawnPos, Quaternion.identity, transform);
            //Debug.Log(string.Format("{0} has been spawned and is a {1} note", note.key, note.type));

            // set colour
            SpriteRenderer sr = newTapNote.GetComponent<SpriteRenderer>();
            sr.color = colour;

            // set movement
            NoteScript noteScript = newTapNote.GetComponent<NoteScript>();
            noteScript.moveSpeed = noteSpeed;
            noteScript.movesUp = isMovingUp;

            stamina.DecreaseStamina(tapCost);
        }
        else if (note.type == NoteType.Hold && stamina.GetStamina() >= holdCost)
        {
            if (activeHolds.ContainsKey(isLeftKey))
                return; // preventing respawning holds over several beats x2

            // spawn
            GameObject newHoldNote = Instantiate(this.holdNote, spawnPos, Quaternion.identity, transform);
            //Debug.Log(string.Format("{0} has been spawned and is a {1} note", note.key, note.type));
            HoldScript holdScript = newHoldNote.GetComponent<HoldScript>();

            // set colour
            SpriteRenderer sr;
            sr = holdScript.Head.GetComponent<SpriteRenderer>();
            sr.color = colour;
            sr = holdScript.Body.GetComponent<SpriteRenderer>();
            sr.color = colour;
            sr = holdScript.Tail.GetComponent<SpriteRenderer>();
            sr.color = colour;

            // set movement
            holdScript.speed = noteSpeed;
            holdScript.goesUp = isMovingUp;

            // tail should ignore distance offset
            Vector3 tailPos = spawnPos;
            tailPos.y = transform.position.y;
            holdScript.Tail.transform.position = tailPos;

            // save for release
            activeHolds.Add(isLeftKey, newHoldNote);

            stamina.DecreaseStamina(holdCost);
        }
        else if (note.type == NoteType.Release)
        {
            if (!activeHolds.TryGetValue(isLeftKey, out GameObject hold))
            {
                Debug.LogWarning("no active holds to release for " + note.key);
                return;
            }

            // activate release
            activeHolds[isLeftKey].GetComponent<HoldScript>().ActivateRelease(spawnPos);

            // remove from active holds
            activeHolds.Remove(isLeftKey);
        }
    }

    // counter phase overload
    void SpawnNote(NoteSpawnData note)
    {
        // delay adjustment (probably irrelevant for counter)
        float beatDelay = Time.time - beatTime;
        float distanceOffset = beatDelay * noteSpeed * (isMovingUp ? 1 : -1);
        //distanceOffset = 0;
        Vector3 spawnPos;
        Color colour;

        if (note.isLeftKey)
        {
            spawnPos = new Vector3(transform.position.x - xOffset, transform.position.y + distanceOffset, transform.position.z);
            colour = leftKeyColor;
        }
        else
        {
            spawnPos = new Vector3(transform.position.x + xOffset, transform.position.y + distanceOffset, transform.position.z);
            colour = rightKeyColor;
        }

        // spawn logic
        if (note.type == NoteType.Tap)
        {
            // spawn
            GameObject newTapNote = Instantiate(this.tapNote, spawnPos, Quaternion.identity, transform);
            //Debug.Log(string.Format("{0} has been spawned and is a {1} note", note.key, note.type));

            // set colour
            SpriteRenderer sr = newTapNote.GetComponent<SpriteRenderer>();
            sr.color = colour;

            // set movement
            NoteScript noteScript = newTapNote.GetComponent<NoteScript>();
            noteScript.moveSpeed = noteSpeed;
            noteScript.movesUp = isMovingUp;
        }
        else if (note.type == NoteType.Hold)
        {
            if (activeHolds.ContainsKey(note.isLeftKey))
                return; // preventing respawning holds over several beats x2

            // spawn
            GameObject newHoldNote = Instantiate(this.holdNote, spawnPos, Quaternion.identity, transform);
            //Debug.Log(string.Format("{0} has been spawned and is a {1} note", note.key, note.type));
            HoldScript holdScript = newHoldNote.GetComponent<HoldScript>();

            // set colour
            SpriteRenderer sr;
            sr = holdScript.Head.GetComponent<SpriteRenderer>();
            sr.color = colour;
            sr = holdScript.Body.GetComponent<SpriteRenderer>();
            sr.color = colour;
            sr = holdScript.Tail.GetComponent<SpriteRenderer>();
            sr.color = colour;

            // set movement
            holdScript.speed = noteSpeed;
            holdScript.goesUp = isMovingUp;

            // tail should ignore distance offset
            Vector3 tailPos = spawnPos;
            tailPos.y = transform.position.y;
            holdScript.Tail.transform.position = tailPos;

            // save for release
            activeHolds.Add(note.isLeftKey, newHoldNote);
        }
        else if (note.type == NoteType.Release)
        {
            if (!activeHolds.TryGetValue(note.isLeftKey, out GameObject hold))
            {
                Debug.LogWarning("no active holds to release for " + (note.isLeftKey ? "left" : "right"));
                return;
            }

            // activate release
            activeHolds[note.isLeftKey].GetComponent<HoldScript>().ActivateRelease(spawnPos);

            // remove from active holds
            activeHolds.Remove(note.isLeftKey);
        }
    }

    void SaveToChart(int beat, NoteData note)
    {
        string key = note.key == leftKey ? "L" : "R";
        chartWriter.WriteLine(string.Format("{0},{1},{2}", beat, key, (int)note.type));
    }
}*/