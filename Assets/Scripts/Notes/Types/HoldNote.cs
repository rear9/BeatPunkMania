using UnityEngine;

/// <summary>
/// Derives from NoteCore to shape hold note functionality
/// </summary>

public class HoldNote : NoteCore
{
    [SerializeField] private GameObject head;
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject tail_private;
    
    [Header("Hold State")]
    private bool _isHolding;
    private float _holdStartTime; // tracked to detect quick taps on hold notes
    private float _holdDuration;
    private float _currentHoldTime;
    private bool _isAttackPhase;
    private bool _holdReleased;
    private bool _isDefendPhaseHolding;
    private HitAccuracy _startAccuracy;
    private bool _hasBeenTapped;
    
    private Color _normalColor;
    private Color _approachGlowColor;
    private Color _bodyColor;
    private Color _holdingColor;
    
    private SpriteRenderer _headSR;
    private SpriteRenderer _bodySR;
    private SpriteRenderer _tailSR;
    
    public GameObject tail => tail_private;
    public GameObject Head => head;
    public GameObject Body => body;

    #region Lifecycle

    protected override void Awake()
    {
        base.Awake();
        if (head != null) _headSR = head.GetComponent<SpriteRenderer>();
        if (body != null) _bodySR = body.GetComponent<SpriteRenderer>();
        if (tail_private != null) _tailSR = tail_private.GetComponent<SpriteRenderer>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _isHolding = false;
        _currentHoldTime = 0f;
        _holdDuration = 0f;
        _isAttackPhase = false;
        _holdReleased = false;
        _isDefendPhaseHolding = false;
        _hasBeenTapped = false;
        _startAccuracy = HitAccuracy.Miss;
        
        if (head != null) head.SetActive(true);
        if (body != null) body.SetActive(true);
        if (tail_private != null) tail_private.SetActive(true);
        
        if (_headSR != null) { _headSR.enabled = true; _headSR.color = Color.white; }
        if (head != null) head.transform.localPosition = Vector3.zero;
        if (tail_private != null) tail_private.transform.localPosition = Vector3.zero;
        
        if (body != null)
        {
            body.transform.localScale = new Vector3(body.transform.localScale.x, 0f, 1f);
            body.transform.localPosition = Vector3.zero;
        }
    }

    public override void Init(string poolKey, int lane, float speed, bool goesDown)
    {
        base.Init(poolKey, lane, speed, goesDown);
        _isHolding = false;
        _currentHoldTime = 0f;
        _isAttackPhase = false;
        _holdReleased = false;
        _isDefendPhaseHolding = false;
        _hasBeenTapped = false;
        _startAccuracy = HitAccuracy.Miss;
        CacheLaneColors(lane);
        float xPos = GetLanePosition(lane);
        
        if (head != null)
        {
            Vector3 pos = head.transform.position;
            pos.x = xPos;
            head.transform.position = pos;
        }
        
        if (tail_private != null)
        {
            Vector3 pos = tail_private.transform.position;
            pos.x = xPos;
            tail_private.transform.position = pos;
        }
        
        if (_headSR != null) _headSR.color = _normalColor;
        if (_tailSR != null) _tailSR.color = _normalColor;
        if (_bodySR != null) _bodySR.color = _bodyColor;
    }

    #endregion Lifecycle

    #region Movement

    protected override void Update()
    {
        if (!_isActive) return;
        if (head == null || tail_private == null || body == null)
        {
            ReturnToPool();
            return;
        }
    
        UpdateMovement();
        UpdateBody();
        UpdateGlow();
    
        if (_isHolding) _currentHoldTime += Time.deltaTime;
        if (movesDown) UpdateHitZoneRegistration();
        CheckHoldBoundaries();
    }

    private void UpdateMovement()
    {
        float direction = movesDown ? -1f : 1f;
        float movement = direction * moveSpeed * Time.deltaTime;
        
        if (_isAttackPhase && !_holdReleased)
        {
            head.transform.Translate(Vector3.up * movement);
        }
        else if (_isAttackPhase && _holdReleased)
        {
            head.transform.Translate(Vector3.up * movement);
            tail_private.transform.Translate(Vector3.up * movement);
        }
        else if (movesDown && _isDefendPhaseHolding)
        {
            // head is locked at hit zone
            Vector3 headPos = head.transform.position;
            headPos.y = GetHitZoneY();
            head.transform.position = headPos;
            
            // tail continues moving DOWN toward the head so the body shrinks
            tail_private.transform.Translate(Vector3.up * movement);
        }
        else
        {
            head.transform.Translate(Vector3.up * movement);
            tail_private.transform.Translate(Vector3.up * movement);
        }
    }

    private void UpdateBody()
    {
        float headY = head.transform.position.y;
        float tailY = tail_private.transform.position.y;
        
        float distance = Mathf.Abs(headY - tailY);
        float length = distance / 15f;
        
        body.transform.localScale = new Vector3(body.transform.localScale.x, length, 1f);
        
        float midY = (headY + tailY) / 2f;
        Vector3 bodyPos = body.transform.position;
        bodyPos.y = midY;
        body.transform.position = bodyPos;
    }

    #endregion Movement

    #region Bounds

    protected override void CheckBoundaries() // reset this so it doesn't double-check
    {
    }

    private void CheckHoldBoundaries()
    {
        float tailY = tail_private.transform.position.y;
        float hitY = GetHitZoneY();
        
        if (_isAttackPhase && !_holdReleased)
        {
            if (head != null && head.transform.position.y >= GetTopBoundaryY()) ReturnToPool();
        }
        else if (movesDown)
        {
            if (_isDefendPhaseHolding)
            {
                if (tailY <= hitY + 0.1f) CompleteHold();
            }
            else
            {
                float headY = head.transform.position.y;
                if (headY <= hitY - hitWindowRadius) OnMiss();
            }
        }
        else
        {
            if (tailY >= GetTopBoundaryY()) ReturnToPool();
        }
    }

    #endregion Bounds

    #region Hold Mechanics

    public void SetRecordingMode(bool active)
    {
        _isAttackPhase = active;
        _holdReleased = false;
    }

    public void ReleaseHold()
    {
        _holdReleased = true;
    }

    public void SetHoldDuration(float duration)
    {
        _holdDuration = duration;
        if (_isAttackPhase && !_holdReleased) return;
        
        if (movesDown && head != null && tail_private != null)
        {
            float distance = duration * moveSpeed;
            Vector3 tailPos = head.transform.position;
            tailPos.y += distance; // tail starts ABOVE head
            tail_private.transform.position = tailPos;
        }
    }

    public void BeginHold(HitAccuracy startAccuracy)
    {
        if (_hasBeenTapped)
        {
            OnMiss();
            return;
        }
        
        _isHolding = true;
        _isDefendPhaseHolding = true;
        _holdStartTime = Time.time;
        _startAccuracy = startAccuracy;
        
        // head and tail both switch to holding color
        if (_headSR != null) _headSR.color = _holdingColor;
        if (_tailSR != null) _tailSR.color = _holdingColor;
    }

    public void EndHold()
    {
        if (!_isDefendPhaseHolding) return;
        
        // if released too quickly it's a tap, not a hold - counts as a miss
        if (Time.time - _holdStartTime < 0.1f)
        {
            _isDefendPhaseHolding = false;
            OnMiss();
            return;
        }
        
        _isDefendPhaseHolding = false;
        
        float timingOffset = BeatManager.Instance.CalculateTimingOffset(Time.time);
        HitAccuracy endAccuracy = BeatManager.Instance.GetAccuracy(timingOffset);
        HitAccuracy finalAccuracy = endAccuracy < _startAccuracy ? endAccuracy : _startAccuracy; // returns lowest accuracy from bunch for now
        
        ScoreManager.Instance?.RegisterHit(GetPlayerID(), finalAccuracy);
        CompleteHoldWithAccuracy(finalAccuracy);
    }
    
    private void CompleteHold()
    {
        if (!_isDefendPhaseHolding) return;
        _isDefendPhaseHolding = false;
        ScoreManager.Instance?.RegisterHit(GetPlayerID(), _startAccuracy);
        CompleteHoldWithAccuracy(_startAccuracy);
    }
    
    private void CompleteHoldWithAccuracy(HitAccuracy accuracy)
    {
        if (_wasHit) return;
        _wasHit = true;
        _isActive = false;
        _isHolding = false;
        
        if (_currentDetector != null)
        {
            _currentDetector.UnregisterNote(this);
            _currentDetector = null;
        }
        
        if (HitEffectManager.Instance != null)
        {
            float noteY = head != null ? head.transform.position.y : transform.position.y;
            HitEffectManager.Instance.SpawnHitEffect(accuracy, new Vector3(transform.position.x, noteY, 0f), _lane);
        }
        
        if (accuracy == HitAccuracy.Miss) AudioManager.Instance?.PlayNoteMiss();
        else AudioManager.Instance?.PlayHitPerfect();
        
        // fire after hold actually completes, not at press time
        GameManager.Instance?.GetPlayer(GetPlayerID())?.OnNoteHitSuccessful(accuracy);
        
        ResetNoteColors();
        ReturnToPool();
    }
    
    public void OnTapped()
    {
        _hasBeenTapped = true;
        OnMiss();
    }

    public bool IsHolding() => _isHolding;

    #endregion Hold Mechanics

    #region Overrides

    public override void OnHit()
    {
        if (_hasBeenTapped) return;
        if (_wasHit) return;
        _wasHit = true;
        _isActive = false;
        _isHolding = false;
        _isDefendPhaseHolding = false;
        
        if (_currentDetector != null)
        {
            _currentDetector.UnregisterNote(this);
            _currentDetector = null;
        }
        
        ResetNoteColors();
        ReturnToPool();
    }

    protected override void OnMiss()
    {
        if (_wasHit) return;
        _isHolding = false;
        _isDefendPhaseHolding = false;
        ResetNoteColors();
        base.OnMiss();
    }

    #endregion Overrides

    #region Helpers

    public override float GetHitPositionY() => head != null ? head.transform.position.y : transform.position.y;
    
    protected override float MissDetectionRange => 2f;
    
    public new bool IsInHitZone()
    {
        if (head == null) return false;
        float distanceFromHitZone = Mathf.Abs(head.transform.position.y - GetHitZoneY());
        return distanceFromHitZone <= hitWindowRadius;
    }
    
    private void UpdateGlow()
    {
        // don't overwrite the holding color, and skip when not moving toward hit zone
        if (!movesDown || _isHolding || _wasHit || _headSR == null) return;
    
        float distanceToHitZone = Mathf.Abs(head.transform.position.y - GetHitZoneY());
        float glowIntensity = 1f - Mathf.Clamp01(distanceToHitZone / 2f);
        Color glowColor = Color.Lerp(_normalColor, _approachGlowColor, glowIntensity);
    
        // head and tail share the same glow
        _headSR.color = glowColor;
        if (_tailSR != null) _tailSR.color = glowColor;
    }
    
    private void ResetNoteColors()
    {
        // reset to white so the pooled object starts clean
        if (_headSR != null) { _headSR.enabled = true; _headSR.color = Color.white; }
        if (_tailSR != null) { _tailSR.enabled = true; _tailSR.color = Color.white; }
        if (_bodySR != null) { _bodySR.enabled = true; _bodySR.color = Color.white; }
    }
    
    private void CacheLaneColors(int lane)
    {
        NoteColorConfig config = NotePoolManager.Instance?.ColorConfig;
        
        if (config != null)
        {
            NoteColorConfig.LaneColors laneColors = config.GetLane(lane);
            _normalColor = laneColors.normalColor;
            _approachGlowColor = laneColors.approachGlowColor;
            _bodyColor = laneColors.bodyColor;
            _holdingColor = laneColors.holdingColor;
        }
        else
        {
            _normalColor = Color.white;
            _approachGlowColor = Color.white * 1.5f;
            _bodyColor = new Color(1f, 1f, 1f, 0.4f);
            _holdingColor = Color.cyan;
        }
    }
    
    private int GetPlayerID() => _lane <= 1 ? 0 : 1;

    #endregion Helpers
}