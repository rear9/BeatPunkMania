using UnityEngine;

/// <summary>
/// Derives from NoteCore to shape tap note functionality
/// </summary>

public class TapNote : NoteCore
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Visual Thresholds")]
    [Tooltip("Distance from hit zone at which the glow ramp begins")]
    [SerializeField] private float approachDistance = 2f;
    [Tooltip("Distance from hit zone at which the note snaps to hitZoneColor — intentionally small, independent of the gameplay hit window")]
    [SerializeField] private float visualSnapDistance = 0.3f;
    
    private Color _normalColor;
    private Color _approachGlowColor;
    private Color _hitZoneColor;
    private Color _hitFlashColor;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    public override void Init(string poolKey, int lane, float speed, bool goesDown) // overrides
    {
        base.Init(poolKey, lane, speed, goesDown);
        
        CacheLaneColors(lane);
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = _normalColor;
        }
    }

    protected override void Update()
    {
        base.Update();
    
        if (!_isActive || spriteRenderer == null) return;
        
        if (!movesDown)
        {
            spriteRenderer.color = _normalColor;
            return;
        }
        
        float distanceToHitZone = Mathf.Abs(GetHitPositionY() - GetHitZoneY());
        
        if (distanceToHitZone <= visualSnapDistance) // so the colour can glow before snapping into hitzone
        {
            spriteRenderer.color = _hitZoneColor;
        }
        else
        {
            float glowIntensity = 1f - Mathf.Clamp01(distanceToHitZone / approachDistance);
            spriteRenderer.color = Color.Lerp(_normalColor, _approachGlowColor, glowIntensity);
        }
    }

    public override void OnHit()
    {
        if (_wasHit) return;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = _hitFlashColor;
        }
        
        base.OnHit();
    }

    public override float GetHitPositionY() => transform.position.y;

    protected override float MissDetectionRange => approachDistance;

    private void CacheLaneColors(int lane)
    {
        NoteColorConfig config = NotePoolManager.Instance?.ColorConfig;
        
        if (config != null) // note colour config
        {
            NoteColorConfig.LaneColors laneColors = config.GetLane(lane);
            _normalColor = laneColors.normalColor;
            _approachGlowColor = laneColors.approachGlowColor;
            _hitZoneColor = laneColors.hitZoneColor;
            _hitFlashColor = laneColors.hitFlashColor;
        }
        else
        {
            _normalColor = Color.white;
            _approachGlowColor = Color.white * 1.5f;
            _hitZoneColor = Color.yellow;
            _hitFlashColor = Color.green;
        }
    }
}