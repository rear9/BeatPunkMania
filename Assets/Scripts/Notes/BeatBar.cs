using UnityEngine;

/// <summary>
/// Handles movement & visuals of beat bars in both charts
/// </summary>

public class BeatBar : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool movesDown = true;
    
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color alexColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color lydColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color downbeatColor = new Color(1f, 1f, 0f, 0.5f);
    
    [Header("Effects")]
    [SerializeField] private float flashIntensity = 0.3f;
    [SerializeField] private float flashDuration = 0.15f;
    private float _fadeDuration;
    
    private bool _isActive;
    private bool _isDownbeat;
    private float _fadeTimer;
    private bool _isFadingIn;
    private Color _targetColor;
    private float _flashTimer;
    private bool _isFlashing;
    
    private void OnEnable()
    {
        _isActive = true;
        if (BeatManager.Instance != null) BeatManager.Instance.OnBeat += OnBeat;
    }
    
    private void OnDisable()
    {
        if (BeatManager.Instance != null) BeatManager.Instance.OnBeat -= OnBeat;
    }

    public void Init(float speed, bool goesDown, bool isDownbeat, float fadeInDuration)
    {
        moveSpeed = speed;
        movesDown = goesDown;
        _isDownbeat = isDownbeat;
        _isActive = true;

        if (Mathf.Approximately(transform.position.x, GameBoundaries.Instance.P1CenterX))
        {
            _targetColor = isDownbeat ? alexColor : normalColor;
        }
        else
        {
            _targetColor = isDownbeat ? lydColor : normalColor;
        }
        
        _isFadingIn = true;
        _fadeTimer = 0f;
        _fadeDuration = fadeInDuration;
        
        if (spriteRenderer != null)
        {
            Color startColor = _targetColor;
            startColor.a = 0f;
            spriteRenderer.color = startColor;
        }
    }

    private void Update()
    {
        if (!_isActive) return;
        
        if (_isFadingIn)
        {
            _fadeTimer += Time.deltaTime; // alpha lerp
            float t = Mathf.Clamp01(_fadeTimer / _fadeDuration);
            
            if (spriteRenderer != null)
            {
                Color color = _targetColor;
                color.a = Mathf.Lerp(0f, _targetColor.a, t);
                spriteRenderer.color = color;
            }
            
            if (_fadeTimer >= _fadeDuration)
            {
                _isFadingIn = false;
            }
        }
        
        if (_isFlashing)
        {
            _flashTimer += Time.deltaTime;
            float flashProgress = _flashTimer / flashDuration;
            
            if (spriteRenderer != null && !_isFadingIn)
            {
                float brightness = Mathf.Lerp(1f + flashIntensity, 1f, flashProgress);
                Color flashColor = _targetColor * brightness;
                flashColor.a = _targetColor.a;
                spriteRenderer.color = flashColor;
            }
            
            if (_flashTimer >= flashDuration)
            {
                _isFlashing = false;
                if (spriteRenderer != null && !_isFadingIn)
                {
                    spriteRenderer.color = _targetColor;
                }
            }
        }
        
        float direction = movesDown ? -1f : 1f;
        transform.Translate(Vector3.up * (direction * moveSpeed * Time.deltaTime)); // move beatbar
        float y = transform.position.y;
        
        if (GameBoundaries.Instance != null)
        {
            if ((movesDown && y <= GameBoundaries.Instance.BottomY) || (!movesDown && y >= GameBoundaries.Instance.TopY))
            {
                _isActive = false;
                if (BeatBarSpawner.Instance != null) BeatBarSpawner.Instance.ReturnBeatBar(gameObject);
            }
        }
    }
    
    private void OnBeat()
    {
        if (_isActive && !_isFadingIn)
        {
            _isFlashing = true;
            _flashTimer = 0f;
        }
    }
}