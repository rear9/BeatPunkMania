using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ControllerType { Keyboard, Xbox, PlayStation, Generic }

/// <summary>
/// Handles new Unity input detection for Keyboard/Xbox/PS, holds events for all keys, input actions & device detection
/// </summary>

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap _player1Map;
    private InputActionMap _player2Map;

    public InputAction _p1Left, _p1Right, _p2Left, _p2Right;
    private InputAction _p1NavLeft, _p1NavRight, _p1NavUp, _p1NavDown, _p1Select, _p1Back;
    private InputAction _p2NavLeft, _p2NavRight, _p2NavUp, _p2NavDown, _p2Select, _p2Back;
    public InputAction _p1Ability1, _p1Ability2;
    public InputAction _p2Ability1, _p2Ability2;
    private bool[] _lanesSwapped = new bool[2];

    private bool _player1UsingGamepad;
    private bool _player2UsingGamepad;
    private int _lastGamepadCount = -1;

    private bool[] _laneHeldStates = new bool[4];

    public event Action<int> OnLanePressed;
    public event Action<int> OnLaneReleased;
    public event Action<int> OnLaneHeld;

    public event Action<int> OnNavLeft;
    public event Action<int> OnNavRight;
    public event Action<int> OnNavUp;
    public event Action<int> OnNavDown;
    public event Action<int> OnSelect;
    public event Action<int> OnBack;

    /*public event Action<int> OnAbility1;
    public event Action<int> OnAbility2;*/

    public event Action<int, ControllerType> OnDeviceChanged; // playerID, new type
    
    #region Init

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeInputActions();
        LoadBindingOverrides();
    }

    private void InitializeInputActions() // makes all inputs subscribe to an event
    {
        if (inputActions == null) return;

        _player1Map = inputActions.FindActionMap("Player1");
        _player2Map = inputActions.FindActionMap("Player2");

        _p1Left = _player1Map?.FindAction("LeftNote");
        _p1Right = _player1Map?.FindAction("RightNote");
        _p2Left = _player2Map?.FindAction("LeftNote");
        _p2Right = _player2Map?.FindAction("RightNote");

        _p1NavLeft = _player1Map?.FindAction("NavLeft");
        _p1NavRight = _player1Map?.FindAction("NavRight");
        _p1NavUp = _player1Map?.FindAction("NavUp");
        _p1NavDown = _player1Map?.FindAction("NavDown");
        _p1Select = _player1Map?.FindAction("Select");
        _p1Back = _player1Map?.FindAction("Back");

        _p2NavLeft = _player2Map?.FindAction("NavLeft");
        _p2NavRight = _player2Map?.FindAction("NavRight");
        _p2NavUp = _player2Map?.FindAction("NavUp");
        _p2NavDown = _player2Map?.FindAction("NavDown");
        _p2Select = _player2Map?.FindAction("Select");
        _p2Back = _player2Map?.FindAction("Back");

        /*_p1Ability1 = _player1Map?.FindAction("Ability1");
        _p1Ability2 = _player1Map?.FindAction("Ability2");
        _p2Ability1 = _player2Map?.FindAction("Ability1");
        _p2Ability2 = _player2Map?.FindAction("Ability2");*/

        _p1Left.performed += _ => OnNoteInput(0, true);
        _p1Left.canceled += _ => OnNoteInput(0, false);
        _p1Right.performed += _ => OnNoteInput(1, true);
        _p1Right.canceled += _ => OnNoteInput(1, false);
        _p2Left.performed += _ => OnNoteInput(2, true);
        _p2Left.canceled += _ => OnNoteInput(2, false);
        _p2Right.performed += _ => OnNoteInput(3, true);
        _p2Right.canceled += _ => OnNoteInput(3, false);

        _p1NavLeft.performed += _ => OnNavLeft?.Invoke(0);
        _p1NavRight.performed += _ => OnNavRight?.Invoke(0);
        _p1NavUp.performed += _ => OnNavUp?.Invoke(0);
        _p1NavDown.performed += _ => OnNavDown?.Invoke(0);
        _p1Select.performed += _ => OnSelect?.Invoke(0);
        _p1Back.performed += _ => OnBack?.Invoke(0);

        _p2NavLeft.performed += _ => OnNavLeft?.Invoke(1);
        _p2NavRight.performed += _ => OnNavRight?.Invoke(1);
        _p2NavUp.performed += _ => OnNavUp?.Invoke(1);
        _p2NavDown.performed += _ => OnNavDown?.Invoke(1);
        _p2Select.performed += _ => OnSelect?.Invoke(1);
        _p2Back.performed += _ => OnBack?.Invoke(1);

        /*if (_p1Ability1 != null) _p1Ability1.performed += _ => OnAbility1?.Invoke(0);
        if (_p1Ability2 != null) _p1Ability2.performed += _ => OnAbility2?.Invoke(0);
        if (_p2Ability1 != null) _p2Ability1.performed += _ => OnAbility1?.Invoke(1);
        if (_p2Ability2 != null) _p2Ability2.performed += _ => OnAbility2?.Invoke(1);*/

        UpdateDeviceAssignments();
    }

    private void OnEnable()
    {
        _player1Map?.Enable();
        _player2Map?.Enable();
    }

    private void OnDisable()
    {
        _player1Map?.Disable();
        _player2Map?.Disable();
    }

    #endregion Init

    #region Device Assignment

    private void UpdateDeviceAssignments()
    {
        int gamepadCount = Gamepad.all.Count;

        bool p1WasGamepad = _player1UsingGamepad;
        bool p2WasGamepad = _player2UsingGamepad;

        if (gamepadCount >= 2) // assigning first gamepad to player1 and second to p2
        {
            _player1Map.devices = new[] { Gamepad.all[0] };
            _player2Map.devices = new[] { Gamepad.all[1] };
            _player1UsingGamepad = true;
            _player2UsingGamepad = true;
        }
        else if (gamepadCount == 1) // in case of 1 controller, player1 is assigned to controller and player2 is assigned to keyboard
        {
            _player1Map.devices = new[] { Gamepad.all[0] };
            if (Keyboard.current != null)
                _player2Map.devices = new[] { Keyboard.current };
            _player1UsingGamepad = true;
            _player2UsingGamepad = false;
        }
        else if (Keyboard.current != null)
        {
            _player1Map.devices = new[] { Keyboard.current };
            _player2Map.devices = new[] { Keyboard.current };
            _player1UsingGamepad = false;
            _player2UsingGamepad = false;
        }

        _lastGamepadCount = gamepadCount;

        if (p1WasGamepad != _player1UsingGamepad)
            OnDeviceChanged?.Invoke(0, GetControllerType(0));
        if (p2WasGamepad != _player2UsingGamepad)
            OnDeviceChanged?.Invoke(1, GetControllerType(1));
    }

    public ControllerType GetControllerType(int playerID) // tries to detect the type of controller (playstation / xbox) based on device name
    {
        bool usingGamepad = playerID == 0 ? _player1UsingGamepad : _player2UsingGamepad;
        if (!usingGamepad) return ControllerType.Keyboard;

        int gamepadIndex = playerID == 0 ? 0 : (Gamepad.all.Count >= 2 ? 1 : 0);
        if (gamepadIndex >= Gamepad.all.Count) return ControllerType.Keyboard;

        Gamepad pad = Gamepad.all[gamepadIndex];
        string padName = pad.name.ToLower();

        if (padName.Contains("dualsense") || padName.Contains("dualshock") || padName.Contains("playstation") || padName.Contains("ps4") || padName.Contains("ps5"))
            return ControllerType.PlayStation;
        if (padName.Contains("xinput") || padName.Contains("xbox"))
            return ControllerType.Xbox;

        return ControllerType.Generic;
    }

    #endregion Device Assignment

    #region Handling

    private void OnNoteInput(int lane, bool isPressed)
    {
        int playerID = lane < 2 ? 0 : 1; // using 0/1 system here because why not
        if (_lanesSwapped[playerID]) lane = lane % 2 == 0 ? lane + 1 : lane - 1; // lane swap for ability using modulo = odd lanes turn into even and vice versa
        _laneHeldStates[lane] = isPressed;
        if (isPressed) OnLanePressed?.Invoke(lane);
        else OnLaneReleased?.Invoke(lane);
    }

    private void Update() // constantly check for new gamepads / unplugging of controllers
    {
        if (Gamepad.all.Count != _lastGamepadCount)
            UpdateDeviceAssignments();

        for (int i = 0; i < _laneHeldStates.Length; i++)
        {
            if (_laneHeldStates[i])
                OnLaneHeld?.Invoke(i);
        }
    }

    #endregion Handling

    #region Keybinds
    // keybind stuff
    public InputActionAsset InputActions => inputActions;
    
    public InputAction GetAction(string key, int playerID) // duplicate checks are done manually in menumanager due to low amount of keybinds
    {
        return playerID switch
        {
            0 => _player1Map?.FindAction(key),
            1 => _player2Map?.FindAction(key),
            _ => null
        };
    }
    
    public void SaveBindingOverrides() => PlayerPrefs.SetString("Keybinds", inputActions.SaveBindingOverridesAsJson()); // playerprefs saving / loading for strings
    
    public void LoadBindingOverrides()
    {
        string saved = PlayerPrefs.GetString("Keybinds", null);
        if (!string.IsNullOrEmpty(saved))
            inputActions.LoadBindingOverridesFromJson(saved);
    }
    
    #endregion Keybinds
    
    #region Helpers

    public void SwapLanes(int playerID)      => _lanesSwapped[playerID] = true;
    public void ClearLaneSwap(int playerID)  => _lanesSwapped[playerID] = false;
    public bool IsLaneHeld(int lane) => lane >= 0 && lane < _laneHeldStates.Length && _laneHeldStates[lane];
    public bool IsPlayer1UsingGamepad() => _player1UsingGamepad;
    public bool IsPlayer2UsingGamepad() => _player2UsingGamepad;

    public void SetInputEnabled(bool active)
    {
        if (active) { _player1Map?.Enable(); _player2Map?.Enable(); }
        else        { _player1Map?.Disable(); _player2Map?.Disable(); }
    }

    #endregion Helpers
}