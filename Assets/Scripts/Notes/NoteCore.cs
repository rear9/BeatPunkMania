using UnityEngine;

/// <summary>
/// Core note class, holds standard data for hit handling + hit windows, note movement directions and updating positions
/// </summary>

public abstract class NoteCore : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected bool movesDown = true;
    
    [Header("Detection")]
    [SerializeField] protected float hitWindowRadius = 0.5f;
    
    protected string _poolKey;
    protected int _lane;
    protected bool _isActive;
    protected bool _wasHit;
    protected NoteHitDetection _currentDetector;
    
    #region Movement
    
    protected virtual void Update()
    {
        if (!_isActive) return;
        
        float direction = movesDown ? -1f : 1f; // core note movement
        transform.Translate(Vector3.up * (direction * moveSpeed * Time.deltaTime));
        
        if (movesDown)
        {
            UpdateHitZoneRegistration(); // check if in hitzone
        }
        CheckBoundaries(); // check if it's still in the boundaries
    }
    
    protected virtual void CheckBoundaries()
    {
        if (GameBoundaries.Instance == null) return;
        
        float y = GetHitPositionY();
        if (y >= GameBoundaries.Instance.TopY || y <= GameBoundaries.Instance.BottomY)
        {
            if (movesDown)
            {
                OnMiss();
            }
            else
            {
                ReturnToPool();
            }
        }
    }
    
    #endregion Movement
    
    #region Init

    public virtual void Init(string poolKey, int lane, float speed, bool goesDown) // setting variables for initializing each note
    {
        _poolKey = poolKey;
        _lane = lane;
        moveSpeed = speed;
        movesDown = goesDown;
        _isActive = true;
        _wasHit = false;
        _currentDetector = null;
        
        Vector3 pos = transform.position;
        pos.x = GetLanePosition(lane);
        transform.position = pos;
    }

    protected float GetLanePosition(int lane)
    {
        if (GameBoundaries.Instance != null)
        {
            return GameBoundaries.Instance.GetLaneX(lane);
        }
        return 0f;
    }

    #endregion Init
    
    #region Registration
    
    protected void UpdateHitZoneRegistration()
    {
        if (GameBoundaries.Instance == null) return;
        
        float distanceFromHitZone = Mathf.Abs(GetHitPositionY() - GameBoundaries.Instance.HitZoneY);
        bool inHitZone = distanceFromHitZone <= hitWindowRadius;
        
        if (inHitZone && _currentDetector == null)
        {
            int playerID = _lane <= 1 ? 1 : 2;
            NoteHitDetection detector = GameManager.Instance?.GetPlayer(playerID)?.GetComponent<NoteHitDetection>();
            if (detector != null)
            {
                detector.RegisterNote(this);
                _currentDetector = detector;
            }
        }
        else if (!inHitZone && _currentDetector != null)
        {
            _currentDetector.UnregisterNote(this);
            _currentDetector = null;
        }
    }


    public bool IsInHitZone()
    {
        if (GameBoundaries.Instance == null) return false;
        
        float distanceFromHitZone = Mathf.Abs(GetHitPositionY() - GameBoundaries.Instance.HitZoneY);
        return distanceFromHitZone <= hitWindowRadius;
    }

    #endregion Registration
    
    #region Cache

    protected virtual void Awake() // to inc; for loading
    {
    }

    protected virtual void OnEnable()
    {
        _isActive = true;
        _wasHit = false;
        _currentDetector = null;
    }

    #endregion Cache

    #region Hit Handling

    public virtual void OnHit() // defaults to unregistering and returning to pool
    {
        if (_wasHit) return;
        _wasHit = true;
        _isActive = false;
        
        if (_currentDetector != null)
        {
            _currentDetector.UnregisterNote(this);
            _currentDetector = null;
        }
        
        ReturnToPool();
    }

    protected virtual void OnMiss()
    {
        if (_wasHit) return;
        _wasHit = true;
        _isActive = false;

        if (_currentDetector != null)
        {
            _currentDetector.UnregisterNote(this);
            _currentDetector = null;
        }

        if (movesDown)
        {
            int playerID = _lane <= 1 ? 0 : 1;
            int gameManagerID = _lane <= 1 ? 1 : 2;
            bool isDefendPhase = PhaseHandler.Instance?.GetCurrentPhase() == PhaseHandler.Phase.Defend;
            bool isWithinRange = Mathf.Abs(GetHitPositionY() - GetHitZoneY()) <= MissDetectionRange;

            if (isDefendPhase && isWithinRange)
            {
                GameManager.Instance?.GetPlayer(gameManagerID)?.OnNoteMissed();
                ScoreManager.Instance?.RegisterMiss(playerID);
                HitEffectManager.Instance?.SpawnMissEffect(_lane);
                AudioManager.Instance?.PlaySFXClip(AudioManager.Instance?.noteMiss);
            }
        }
        ReturnToPool();
    }

    #endregion Hit Handling

    #region Pool

    public void ReturnToPool()
    {
        _isActive = false;
        
        if (_currentDetector != null)
        {
            _currentDetector.UnregisterNote(this);
            _currentDetector = null;
        }
        
        if (!string.IsNullOrEmpty(_poolKey) && NotePoolManager.Instance != null)
        {
            NotePoolManager.Instance.ReturnNote(_poolKey, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion Pool

    #region Getters

    public int GetLane() => _lane;
    public bool IsActive() => _isActive;
    
    protected float GetHitZoneY() => GameBoundaries.Instance != null ? GameBoundaries.Instance.HitZoneY : -1.4f;
    protected float GetTopBoundaryY() => GameBoundaries.Instance != null ? GameBoundaries.Instance.TopY : 6f;
    public virtual float GetHitPositionY() => transform.position.y;
    protected float GetBottomBoundaryY() => GameBoundaries.Instance != null ? GameBoundaries.Instance.BottomY : -4f;

    protected virtual float MissDetectionRange => 2f; // override in subclasses to match their approach glow distance

    #endregion Getters
}