using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages player states, stamina, confidence, and input routing
/// 1 per player
/// </summary>

public class PlayerController : MonoBehaviour
{
    [Header("Player Identity")]
    [SerializeField] private int playerID;
    [SerializeField] private string playerTag;
    
    [Header("Player Stats")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private int confidence = 500;

    [Header("Lane Config")]
    [SerializeField] private int leftLane;
    [SerializeField] private int rightLane;
    
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer characterSprite;
    [SerializeField] private SpriteRenderer staminaBar;
    [SerializeField] private SpriteRenderer confidenceBar;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioManager beatAnimator; // for syncing animations to beat

    [Header("Hit Detection")]
    [SerializeField] private NoteHitDetection hitDetector;

    public UnityEvent<float> OnStaminaChanged;
    public UnityEvent<HitAccuracy> OnNoteHit;
    public UnityEvent OnNoteMiss;

    public float _currentStamina;
    private float _lastUIUpdateStamina;
    private int _currentConfidence;
    private bool _isActive = true;
    private bool _isRecording = false;
    
    public int PlayerID => playerID;
    public string PlayerTag => playerTag;
    public int Confidence => confidence;
    public int LeftLane => leftLane;
    public int RightLane => rightLane;
    public bool IsActive => _isActive;

    #region Init

    private void Start()
    {
        _currentStamina = maxStamina; // 2 stam variables for current & last update
        _lastUIUpdateStamina = maxStamina;

        if (hitDetector != null)
        {
            hitDetector.Init(this); // start note hit detection
        }

        UIManager.Instance?.UpdateStamina(playerID, _currentStamina, maxStamina); // stamina init
        UIManager.Instance?.UpdateConfidence(playerID, confidence); // confidence init
        if (characterSprite != null) characterSprite.color = Color.white; // character sprite init

        if (animator != null)
        {
            if (beatAnimator != null)
                animator.speed = beatAnimator.BPM / 60f;
            animator.SetInteger("confidence", confidence);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnConfidenceChanged += OnConfidenceChanged;
            ScoreManager.Instance.AddConfidence(playerID, confidence);
        }
    }
    private void Update()
    {
        if (!_isActive) return;

        if (_currentStamina < maxStamina) // constant stamina regen
        {
            _currentStamina += staminaRegenRate * Time.deltaTime; // frame independent
            if (_currentStamina > maxStamina)
            {
                _currentStamina = maxStamina;
            }

            if (Mathf.Abs(_currentStamina - _lastUIUpdateStamina) > 0.5f) // ui throttling to only run this when necessary
            {
                OnStaminaChanged?.Invoke(_currentStamina);
                UIManager.Instance?.UpdateStamina(playerID, _currentStamina, maxStamina);
                _lastUIUpdateStamina = _currentStamina;
            }

            // update confidence
        }
    }

    #endregion Init

    #region Stamina

    public bool UseStamina(float amount) // public to remove stamina, used by note recorder when placing notes
    {
        if (_currentStamina < amount) return false;
        
        _currentStamina -= amount;
        if (_currentStamina < 0f)
        {
            _currentStamina = 0f;
        }
        
        OnStaminaChanged?.Invoke(_currentStamina);
        UIManager.Instance?.UpdateStamina(playerID, _currentStamina, maxStamina);
        _lastUIUpdateStamina = _currentStamina;
        
        return true;
    }

    public void RestoreStamina(float amount) // helper if we want to have an ability for stamina restoration
    {
        _currentStamina += amount;
        if (_currentStamina > maxStamina)
        {
            _currentStamina = maxStamina;
        }
        
        OnStaminaChanged?.Invoke(_currentStamina);
        UIManager.Instance?.UpdateStamina(playerID, _currentStamina, maxStamina);
        _lastUIUpdateStamina = _currentStamina;
    }

    public float GetCurrentStamina() => _currentStamina;
    public float GetMaxStamina() => maxStamina;
    public float GetStaminaPercentage() => _currentStamina / maxStamina;

    #endregion Stamina

    #region confidence

    private void OnConfidenceChanged(int plrID, int confidenceVal)
    {
        if (plrID != playerID) return;
        UIManager.Instance?.UpdateConfidence(plrID, confidenceVal);
        if (animator != null) animator.SetInteger("confidence", confidenceVal);
    }

    #endregion confidence

    public void SetRecording(bool recording) // helpers
    {
        _isRecording = recording;
    }

    public bool IsRecording() => _isRecording;
    public bool IsMyLane(int lane) => lane == leftLane || lane == rightLane;
    public void OnNoteHitSuccessful(HitAccuracy accuracy) => OnNoteHit?.Invoke(accuracy);
    public void OnNoteMissed() => OnNoteMiss?.Invoke();
    
    public void SetActive(bool active)
    {
        _isActive = active;
        
        if (characterSprite != null)
        {
            characterSprite.color = active ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
    }
    
    public void Reset()
    {
        _currentStamina = maxStamina;
        _lastUIUpdateStamina = maxStamina;
        _isActive = true;
        _isRecording = false;
        
        UIManager.Instance?.UpdateStamina(playerID, _currentStamina, maxStamina);
        if (characterSprite != null) characterSprite.color = Color.white;
    }
}