using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Handles input visuals under the chart (light up when input is pressed and fades out on release) (currently redundant)
/// </summary>

public class InputUIHandler : MonoBehaviour
{
    public static InputUIHandler Instance { get; private set; }
    
    [Header("Input Display Settings")]
    [SerializeField] private float defaultAlpha = 0.25f;
    [SerializeField] private float pressedAlpha = 1f;
    [SerializeField] private float fadeDuration = 0.1f;
    
    [Header("Player 1 Input Sprites")]
    [SerializeField] private SpriteRenderer p1LeftSprite;
    [SerializeField] private SpriteRenderer p1RightSprite;
    
    [Header("Player 2 Input Sprites")]
    [SerializeField] private SpriteRenderer p2LeftSprite;
    [SerializeField] private SpriteRenderer p2RightSprite;
    
    [Header("Input Icons")]
    [SerializeField] private InputIconLib iconLib;
    
    private bool _p1UsingController;
    private bool _p2UsingController;
    private float[] _targetAlphas = new float[4];
    private float[] _currentAlphas = new float[4];
    
    private SpriteRenderer[] _laneSprites = new SpriteRenderer[4]; // cached lane lookup
    
    #region Init

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        for (int i = 0; i < 4; i++)
        {
            _targetAlphas[i] = defaultAlpha;
            _currentAlphas[i] = defaultAlpha;
        }
        
        // cache lane sprites
        _laneSprites[0] = p1LeftSprite;
        _laneSprites[1] = p1RightSprite;
        _laneSprites[2] = p2LeftSprite;
        _laneSprites[3] = p2RightSprite;
    }

    private void Start()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLanePressed += OnLanePressed;
            InputManager.Instance.OnLaneReleased += OnLaneReleased;
            InputManager.Instance.OnDeviceChanged += OnDeviceChanged;
        }
        
        UpdateAllSprites();
        InitializeSprites();
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLanePressed -= OnLanePressed;
            InputManager.Instance.OnLaneReleased -= OnLaneReleased;
            InputManager.Instance.OnDeviceChanged -= OnDeviceChanged;
        }
    }

    #endregion Init

    #region Device Detection

    private void UpdateAllSprites()
    {
        if (InputManager.Instance == null) return;

        _p1UsingController = InputManager.Instance.IsPlayer1UsingGamepad();
        _p2UsingController = InputManager.Instance.IsPlayer2UsingGamepad();

        RefreshPlayerSprites(0, InputManager.Instance.GetControllerType(0));
        RefreshPlayerSprites(1, InputManager.Instance.GetControllerType(1));
    }

    private void OnDeviceChanged(int playerID, ControllerType type)
    {
        if (playerID == 0) _p1UsingController = type != ControllerType.Keyboard;
        else _p2UsingController = type != ControllerType.Keyboard;
        RefreshPlayerSprites(playerID, type);
    }

    private void RefreshPlayerSprites(int playerID, ControllerType type)
    {
        if (iconLib == null) return;

        InputManager im = InputManager.Instance;
        if (im == null) return;

        string leftPath  = playerID == 0 ? im._p1Left?.bindings[GetBindingIndex(type)].effectivePath
                                         : im._p2Left?.bindings[GetBindingIndex(type)].effectivePath;
        string rightPath = playerID == 0 ? im._p1Right?.bindings[GetBindingIndex(type)].effectivePath
                                         : im._p2Right?.bindings[GetBindingIndex(type)].effectivePath;

        SpriteRenderer leftSR = playerID == 0 ? p1LeftSprite : p2LeftSprite;
        SpriteRenderer rightSR = playerID == 0 ? p1RightSprite : p2RightSprite;

        if (leftSR != null && leftPath != null) leftSR.sprite = iconLib.GetIcon(type, leftPath);
        if (rightSR != null && rightPath != null) rightSR.sprite = iconLib.GetIcon(type, rightPath);
    }

    private int GetBindingIndex(ControllerType type) => type == ControllerType.Keyboard ? 0 : 1;

    #endregion Device Detection

    #region Sprites

    private void InitializeSprites()
    {
        SetAllAlphas(defaultAlpha);
    }

    private void SetAllAlphas(float alpha)
    {
        for (int i = 0; i < 4; i++)
        {
            SetSpriteAlpha(_laneSprites[i], alpha);
        }
    }

    private void SetSpriteAlpha(SpriteRenderer sprite, float alpha)
    {
        if (sprite == null) return;
        
        Color color = sprite.color;
        color.a = alpha;
        sprite.color = color;
    }

    #endregion Sprites

    #region Inputs

    private void OnLanePressed(int lane) // adjust transparency on lane input
    {
        if (lane < 0 || lane >= 4) return;
        _targetAlphas[lane] = pressedAlpha;
    }

    private void OnLaneReleased(int lane)
    {
        if (lane < 0 || lane >= 4) return;
        _targetAlphas[lane] = defaultAlpha;
    }

    #endregion Inputs

    #region Update

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Mathf.Abs(_currentAlphas[i] - _targetAlphas[i]) <= 0.01f) continue; // restrict transparencies
            
            _currentAlphas[i] = Mathf.Lerp(_currentAlphas[i], _targetAlphas[i], Time.deltaTime / fadeDuration); // update lerp
            SetSpriteAlpha(_laneSprites[i], _currentAlphas[i]);
        }
    }

    #endregion Update

    #region Publics

    public void SetDefaultAlpha(float alpha) => defaultAlpha = Mathf.Clamp01(alpha);
    public void SetPressedAlpha(float alpha) => pressedAlpha = Mathf.Clamp01(alpha);
    public bool IsP1UsingController() => _p1UsingController;
    public bool IsP2UsingController() => _p2UsingController;

    #endregion Publics
}