using UnityEngine;

/// <summary>
/// Helper script used to dictate where the top/bottom of a lane is and where notes can be hit
/// </summary>

public class GameBoundaries : MonoBehaviour
{
    public static GameBoundaries Instance { get; private set; }
    
    [Header("Boundary Transforms")]
    [SerializeField] private Transform topBoundary;
    [SerializeField] private Transform bottomBoundary;
    [SerializeField] private Transform hitBoundary;
    
    [Header("Lane Transforms")]
    [SerializeField] private Transform p1LeftLane;
    [SerializeField] private Transform p1RightLane;
    [SerializeField] private Transform p1Center;
    [SerializeField] private Transform p2LeftLane;
    [SerializeField] private Transform p2RightLane;
    [SerializeField] private Transform p2Center;

    private float _topY;
    private float _bottomY;
    private float _hitY;
    private float _p1CenterX;
    private float _p2CenterX;
    private float[] _laneX;
    // private bool _cached; // use later for loading checks
    
    public float TopY => _topY;
    public float BottomY => _bottomY;
    public float HitZoneY => _hitY;
    public float GetLaneX(int lane) => _laneX != null && lane >= 0 && lane < 4 ? _laneX[lane] : 0f;
    public float P1CenterX => _p1CenterX;
    public float P2CenterX => _p2CenterX;
    
    public float TravelDistance => _topY - _hitY;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) // singleton
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _topY = topBoundary != null ? topBoundary.position.y : 6f; // hardcoded fallbacks
        _bottomY = bottomBoundary != null ? bottomBoundary.position.y : -4f;
        _hitY = hitBoundary != null ? hitBoundary.position.y : -1.4f;
        _p1CenterX = p1Center.position.x; 
        _p2CenterX = p2Center.position.x;

        _laneX = new float[4];
        _laneX[0] = p1LeftLane != null ? p1LeftLane.position.x  : -2.76f;
        _laneX[1] = p1RightLane != null ? p1RightLane.position.x : -1.44f;

        _laneX[2] = p2LeftLane != null ? p2LeftLane.position.x  :  1.44f;
        _laneX[3] = p2RightLane != null ? p2RightLane.position.x :  2.76f;
        
        // _cached = true;
    }
}