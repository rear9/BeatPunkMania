using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Uses standard object pooling for 2 pools of Tap/Hold notes
/// </summary>

public class NotePoolManager : MonoBehaviour
{
    public static NotePoolManager Instance { get; private set; }

    [System.Serializable]
    public class NotePool
    {
        public string poolKey; // "TapNote" or "HoldNote"
        public GameObject notePrefab;
        public int initialSize = 20;
    }

    [SerializeField] private List<NotePool> notePools = new();
    
    [Header("Note Colors")]
    [Tooltip("Assign the NoteColorConfig asset here to drive all lane note colors")]
    [SerializeField] private NoteColorConfig colorConfig;
    
    public NoteColorConfig ColorConfig => colorConfig;
    
    private readonly Dictionary<string, Queue<GameObject>> _poolDict = new();
    private readonly Dictionary<string, Transform> _poolParentDict = new();
    private readonly List<GameObject> _activeNotes = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        InitPools();
    }

    private void InitPools()
    {
        foreach (var pool in notePools)
        {
            var noteQueue = new Queue<GameObject>();
            
            // parent folder for organization
            GameObject folder = new GameObject($"{pool.poolKey}_Pool");
            folder.transform.SetParent(transform);
            _poolParentDict[pool.poolKey] = folder.transform;
            
            for (int i = 0; i < pool.initialSize; i++) // fill pool
            {
                GameObject note = Instantiate(pool.notePrefab, folder.transform);
                note.SetActive(false);
                noteQueue.Enqueue(note);
            }
            
            _poolDict[pool.poolKey] = noteQueue;
            Debug.Log($"Initialized {pool.poolKey} pool with {pool.initialSize} notes");
        }
    }
    
    public GameObject SpawnNote(string poolKey, Vector3 position) // pushes note by key
    {
        PhaseHandler phaseHandler = PhaseHandler.Instance;
        UIManager uiManager = UIManager.Instance;
        if (phaseHandler.tutorial == true && phaseHandler.almightyTutorial == true)
        {
            foreach (GameObject obj in uiManager.tutButts)
            {
                obj.SetActive(false);
            }
            phaseHandler.tutorial = false;
        }
        if (!_poolDict.TryGetValue(poolKey, out var queue))
        {
            Debug.LogError($"pool '{poolKey}' doesn't exist");
            return null;
        }
        
        GameObject note; // temp
        
        if (queue.Count == 0) // can expand dynamically
        {
            Debug.LogWarning($"{poolKey} pool empty, creating new note");
            var poolConfig = notePools.Find(p => p.poolKey == poolKey);
            note = Instantiate(poolConfig.notePrefab, _poolParentDict[poolKey]);
        }
        else
        {
            note = queue.Dequeue();
        }

        note.transform.SetParent(null); // detach from pool parent
        note.transform.SetPositionAndRotation(position, Quaternion.identity);

        note.SetActive(true);
        _activeNotes.Add(note); // adds to list

        return note;
    }
    
    public void ReturnNote(string poolKey, GameObject note) // reverse above
    {
        if (!_poolDict.TryGetValue(poolKey, out var queue))
        {
            Debug.LogWarning($"pool '{poolKey}' not found, destroying note");
            Destroy(note);
            return;
        }
        
        note.SetActive(false);
        _activeNotes.Remove(note);
        note.transform.SetParent(_poolParentDict[poolKey]);
        queue.Enqueue(note);
    }

    public void ReturnAllNotes() // for quick return on phase transitions
    {
        var copy = new List<GameObject>(_activeNotes);
        
        foreach (var note in copy)
        {
            if (note.TryGetComponent<NoteCore>(out var noteCore))
            {
                noteCore.ReturnToPool();
            }
            else
            {
                note.SetActive(false);
            }
        }
        _activeNotes.Clear();
    }

    public int GetActiveNoteCount() => _activeNotes.Count; // get count helper (debugging)
}