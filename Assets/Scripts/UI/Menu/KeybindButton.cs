using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Handles visual state & button binding functionality for each button
/// </summary>

public class KeybindButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private string actionName;
    [SerializeField] private int playerID;
    [SerializeField] private TextMeshProUGUI bindingLabel;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private InputIconLib iconLib;

    [Header("Rebind Colours")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color listeningColor = Color.yellow;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color conflictColor = Color.red;
    
    private enum RebindVisualState
    {
        Normal,
        Listening,
        Success,
        Conflict
    }
    private Coroutine visualRoutine;
    private Coroutine _colorRoutine;
    
    private const int KeyboardBindingIndex = 0; // keyboard - 0, controller - 1
    private const int GamepadBindingIndex  = 1;

    #region Init
    
    private void Start()
    {
        RefreshIcon();
        button.onClick.AddListener(OnClick);
        if (InputManager.Instance != null) InputManager.Instance.OnDeviceChanged += OnDeviceChanged;
    }

    private void OnEnable() // subscribe to rebinding events
    {
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.OnRebindComplete += OnRebindComplete;
            MenuManager.Instance.OnRebindConflict += OnRebindConflict;
        }
    }

    private void OnDisable()
    {
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.OnRebindComplete -= OnRebindComplete;
            MenuManager.Instance.OnRebindConflict -= OnRebindConflict;
        }
    }
    
    private void OnDestroy()
    {
        if (InputManager.Instance != null) InputManager.Instance.OnDeviceChanged -= OnDeviceChanged;
    }

    #endregion Init
    
    #region Bindings
    
    private void OnDeviceChanged(int changedPlayerID, ControllerType type) // calls to refresh visuals when a device & controller gets plugged in
    {
        if (changedPlayerID != playerID) return;
        RefreshIcon();
    }

    private void OnClick()
    {
        EventSystem.current?.SetSelectedGameObject(null);
        SetVisualState(RebindVisualState.Listening);
        MenuManager.Instance.StartRebind(actionName, GetCurrentBindingIndex(), playerID);
    }

    private void OnRebindComplete(int changedPlayer, string changedAction)
    {
        if (changedPlayer != playerID) return;
        if (changedAction != actionName) return;
        RefreshIcon();
        SetVisualState(RebindVisualState.Success);
        RestartVisualRoutine(ResetVisualAfterDelay(1.2f));
    }

    private void OnRebindConflict(int changedPlayer, string changedAction)
    {
        if (changedPlayer != playerID) return;
        if (changedAction != actionName) return;
        SetVisualState(RebindVisualState.Conflict);
        RestartVisualRoutine(ResetVisualAfterDelay(1.5f));
    }

    private void RestartVisualRoutine(IEnumerator routine)
    {
        if (visualRoutine != null) StopCoroutine(visualRoutine);
        visualRoutine = StartCoroutine(routine);
    }

    private IEnumerator ResetVisualAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        SetVisualState(RebindVisualState.Normal);
        visualRoutine = null;
    }
    
    private void RefreshIcon() // refreshes the icon for current button
    {
        if (iconImage == null || iconLib == null) return;

        InputAction action = InputManager.Instance?.GetAction(actionName, playerID);
        int bindingIndex = GetCurrentBindingIndex();
        if (action == null || bindingIndex >= action.bindings.Count) return;

        string path = action.bindings[bindingIndex].effectivePath;
        ControllerType type = InputManager.Instance?.GetControllerType(playerID) ?? ControllerType.Keyboard;

        iconImage.sprite = iconLib.GetIcon(type, path);
    }

    private int GetCurrentBindingIndex() // needed to know which icon to use
    {
        ControllerType type = InputManager.Instance?.GetControllerType(playerID) ?? ControllerType.Keyboard;
        return type == ControllerType.Keyboard ? KeyboardBindingIndex : GamepadBindingIndex;
    }
    
    #endregion Bindings
    
    private void SetVisualState(RebindVisualState state)
    {
        if (iconImage == null) return;
        Color target = state switch
        {
            RebindVisualState.Listening => listeningColor,
            RebindVisualState.Success   => successColor,
            RebindVisualState.Conflict  => conflictColor,
            _                           => normalColor
        };
        if (_colorRoutine != null) StopCoroutine(_colorRoutine);
        _colorRoutine = StartCoroutine(LerpToColor(target, 0.24f));
    }

    private IEnumerator LerpToColor(Color target, float duration)
    {
        Color start = iconImage.color;
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.unscaledDeltaTime / duration, 1f);
            float eased = 1f - (1f - t) * (1f - t); // quadratic ease out
            iconImage.color = Color.Lerp(start, target, eased);
            yield return null;
        }
        _colorRoutine = null;
    }
    
}