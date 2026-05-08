using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Uses object pooling for beatbars (these help indicate the current phase and beats)
/// </summary>

public class BeatBarSpawner : MonoBehaviour
{
    public static BeatBarSpawner Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject beatBarPrefab;
    [SerializeField] private int poolSize = 16;
    [SerializeField] private float spawnOffset = 0f; // nudge spawn position to align bars visually with the beat
    
    private Queue<GameObject> _beatBarPool = new Queue<GameObject>();
    private Transform _poolParent;
    private bool _isSpawning;
    private bool _movesDown;

    private void OnBeat()
    {
        if (!_isSpawning) return;
        bool isDownbeat = BeatManager.Instance.CurrentBeat == 1;
        
        if (GameBoundaries.Instance != null)
        {
            SpawnBeatBar(GameBoundaries.Instance.P1CenterX, isDownbeat);
            SpawnBeatBar(GameBoundaries.Instance.P2CenterX, isDownbeat);
        }
    }
    
    #region Init

    private void Awake()
    {
        if (Instance != null && Instance != this) // singleton
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        InitPool();
    }

    private void InitPool() // object pooling setup
    {
        _poolParent = new GameObject("BeatBar_Pool").transform;
        _poolParent.SetParent(transform);
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bar = Instantiate(beatBarPrefab, _poolParent);
            bar.SetActive(false);
            _beatBarPool.Enqueue(bar);
        }
    }

    #endregion Init

    #region Spawning

    public void StartSpawning(bool spawnAtTop)
    {
        if (_isSpawning) return;
        _movesDown = spawnAtTop;
        _isSpawning = true;
        
        if (BeatManager.Instance != null)
        {
            BeatManager.Instance.OnBeat += OnBeat;
        }
    }

    public void StopSpawning()
    {
        _isSpawning = false;
        
        if (BeatManager.Instance != null)
        {
            BeatManager.Instance.OnBeat -= OnBeat;
        }
    }

    #endregion Spawning
    
    #region Logic

    private void SpawnBeatBar(float xPosition, bool isDownbeat)
    {
        if (GameBoundaries.Instance == null) return;
        
        float noteSpeed = AudioManager.Instance.NoteSpeed;
        float beatDuration = BeatManager.Instance.BeatDuration;
        float fadeInDuration = beatDuration;
        float oneBeatDistance = noteSpeed * beatDuration;
        float spawnY;
        
        if (_movesDown) // spawn outside of the main chart for fading
        {
            spawnY = GameBoundaries.Instance.TopY + oneBeatDistance - spawnOffset;
        }
        else
        {
            spawnY = GameBoundaries.Instance.HitZoneY - oneBeatDistance - spawnOffset;
        }
        
        GameObject bar = _beatBarPool.Count > 0 ? _beatBarPool.Dequeue() : Instantiate(beatBarPrefab, _poolParent);
        if (bar == null) return;
        
        bar.transform.position = new Vector3(xPosition, spawnY, 0);
        bar.SetActive(true);
        
        if (bar.TryGetComponent<BeatBar>(out var beatBar))
        {
            beatBar.Init(noteSpeed, _movesDown, isDownbeat, fadeInDuration);
        }
    }

    public void ReturnBeatBar(GameObject bar) // return to pool
    {
        if (bar == null) return;
        
        bar.SetActive(false);
        bar.transform.SetParent(_poolParent);
        _beatBarPool.Enqueue(bar);
    }

    #endregion Logic
}