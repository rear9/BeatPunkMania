using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Handles the menu including all settings functionality (audio/display/keybinding), plays cutscene videos and allows the user to skip them
/// </summary>

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject displayPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject keybindsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject cutscenePanel;

    [Header("Panel First Selected (for controller nav)")]
    [SerializeField] private Selectable mainFirstSelected;
    [SerializeField] private Selectable settingsFirstSelected;
    [SerializeField] private Selectable displayFirstSelected;
    [SerializeField] private Selectable audioFirstSelected;
    [SerializeField] private Selectable keybindsFirstSelected;
    [SerializeField] private Selectable creditsFirstSelected;

    [Header("Display Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Audio Settings")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider masterSlider;

    [Header("Double-Tap Back")]
    [SerializeField] private float doubleTapWindow = 0.35f;

    [Header("Cutscene Skip")]
    [SerializeField] private GameObject skipHoldUI;
    [SerializeField] private Image skipRadialFill;
    [SerializeField] private float skipHoldDuration = 3f;

    [Header("Main Buttons")]
    [SerializeField] private Button[] mainMenuButtons;
    [SerializeField] private Button[] settingsMenuButtons;
    [SerializeField] private Button quitButton;

    private List<Resolution> _filteredResolutions = new();
    private int _currentResolutionIndex;
    private float _lastBackTime = -999f;
    private float _skipHoldTime = 0f;
    private bool _skipHolding = false;
    private bool cutEnd = false;
    private GameObject _currentPanel;
    private bool _usingController = false; // temp
    private Vector2 _lastMousePos;

    private const string ResolutionKey = "ResolutionIndex";
    private const string FullscreenKey = "Fullscreen";

    public Action<int, string> OnRebindComplete;

    public VideoPlayer VP;
    
    #region Init

    private void Awake() // singleton pattern (should stay in menu scene)
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ShowPanel(mainPanel);
        InitDisplaySettings();
        InitAudioSettings();
        foreach (var btn in mainMenuButtons) HookButtonSFX(btn);
        foreach (var btn in settingsMenuButtons) HookButtonSFX(btn);
        HookButtonSFX(quitButton, useQuitClick: true);
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.menuMusic, loop: true);
        StartCoroutine(FadeInMenu());
        VP.SetTargetAudioSource(0, AudioManager.Instance?._musicSource); // sets audio of video player so cutscene volume matches everything else
        VP.Prepare();
        VP.loopPointReached += OnVideoEnd;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnBack += HandleBack;
            InputManager.Instance.OnSelect += HandleSkipPressed;
            InputManager.Instance.OnDeviceChanged += OnDeviceChanged;
        }

        if (skipHoldUI != null) skipHoldUI.SetActive(false);
    }

    private IEnumerator FadeInMenu()
    {
        yield return new WaitForSecondsRealtime(2f);
        yield return StartCoroutine(TransitionHandler.Instance.FadeIn(2f));
    }
    
    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnBack -= HandleBack;
            InputManager.Instance.OnSelect -= HandleSkipPressed;
            InputManager.Instance.OnDeviceChanged -= OnDeviceChanged;
        }
    }

    private void Update()
    {
        if (_currentPanel != cutscenePanel) return;

        bool escPressed = Keyboard.current?.escapeKey.isPressed ?? false; // hold to skip system (esc or confirm/back keys for both players)
        if (escPressed && !_skipHolding) BeginSkipHold();
        if (!escPressed && _skipHolding && !IsSkipInputHeld()) EndSkipHold();

        if (_skipHolding)
        {
            _skipHoldTime += Time.unscaledDeltaTime;
            if (skipRadialFill != null)
                skipRadialFill.fillAmount = Mathf.Clamp01(_skipHoldTime / skipHoldDuration);

            if (_skipHoldTime >= skipHoldDuration)
                SkipCutscene();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        StartCoroutine(LoadPlayArea()); // if the cutscene isn't skipped it should still load play area at the end
    }

    private IEnumerator LoadPlayArea()
    {
        if (TransitionHandler.Instance != null)
        {
            AudioManager.Instance?.StopMusic(fadeOut: true, fadeDuration: 2f);
            yield return StartCoroutine(TransitionHandler.Instance.FadeOut(2f));
        }
        VP?.Stop(); // turn videoplayer off to prevent mem leaks just in case
        SceneManager.LoadScene("PlayArea");
    }

    private void SkipCutscene()
    {
        EndSkipHold();
        cutEnd = true;
        StartCoroutine(LoadPlayArea());
    }

    #endregion Init

    #region Panel Nav

    public void ShowPanel(GameObject target) // flip-flop system for panels (only one is active at a time)
    {
        if (target == cutscenePanel) VP.Play();
        mainPanel.SetActive(false);
        settingsPanel.SetActive(false);
        displayPanel.SetActive(false);
        audioPanel.SetActive(false);
        keybindsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        if (target != null) target.SetActive(true);
        _currentPanel = target;

        RefreshSelection(target);
    }

    private void RefreshSelection(GameObject panel)
    {
        Selectable firstSelected = panel == mainPanel     ? mainFirstSelected
                                 : panel == settingsPanel ? settingsFirstSelected
                                 : panel == displayPanel  ? displayFirstSelected
                                 : panel == audioPanel    ? audioFirstSelected
                                 : panel == keybindsPanel ? keybindsFirstSelected
                                 : panel == creditsPanel  ? creditsFirstSelected
                                 : null;

        bool gamepadActive = (InputManager.Instance?.GetControllerType(0) != ControllerType.Keyboard) ||
                             (InputManager.Instance?.GetControllerType(1) != ControllerType.Keyboard);

        if (firstSelected != null && gamepadActive)
            EventSystem.current?.SetSelectedGameObject(firstSelected.gameObject);
        else
            EventSystem.current?.SetSelectedGameObject(null);
    }

    private void OnDeviceChanged(int playerID, ControllerType type) => RefreshSelection(_currentPanel);

    private void HandleBack(int playerID)
    {
        if (_currentPanel == cutscenePanel)
        {
            BeginSkipHold();
            return;
        }

        if (_currentPanel == mainPanel) return;

        float now = Time.unscaledTime;
        if (now - _lastBackTime <= doubleTapWindow)
        {
            _lastBackTime = -999f;
            NavigateBack();
        }
        else
        {
            _lastBackTime = now;
        }
    }

    private void HandleSkipPressed(int playerID)
    {
        if (_currentPanel != cutscenePanel) return;
        BeginSkipHold();
    }

    private void BeginSkipHold()
    {
        if (_skipHolding || cutEnd == true) return;
        _skipHolding = true;
        _skipHoldTime = 0f;
        if (skipRadialFill != null) skipRadialFill.fillAmount = 0f;
        if (skipHoldUI != null) skipHoldUI.SetActive(true);
    }

    private void EndSkipHold()
    {
        _skipHolding = false;
        _skipHoldTime = 0f;
        if (skipRadialFill != null) skipRadialFill.fillAmount = 0f;
        if (skipHoldUI != null) skipHoldUI.SetActive(false);
    }

    private bool IsSkipInputHeld()
    {
        if (Keyboard.current?.escapeKey.isPressed ?? false) return true;
        if (InputManager.Instance == null) return false;
        InputAction p1Select = InputManager.Instance.GetAction("Select", 0);
        InputAction p2Select = InputManager.Instance.GetAction("Select", 1);
        InputAction p1Back = InputManager.Instance.GetAction("Back", 0);
        InputAction p2Back = InputManager.Instance.GetAction("Back", 1);
        return (p1Select?.IsPressed() ?? false) || (p2Select?.IsPressed() ?? false) ||
               (p1Back?.IsPressed() ?? false) || (p2Back?.IsPressed() ?? false);
    }

    private void NavigateBack()
    {
        if (_currentPanel == settingsPanel || _currentPanel == displayPanel ||
            _currentPanel == audioPanel || _currentPanel == keybindsPanel ||
            _currentPanel == creditsPanel)
        {
            bool isSubSettings = _currentPanel == displayPanel ||
                                 _currentPanel == audioPanel ||
                                 _currentPanel == keybindsPanel;
            ShowPanel(isSubSettings ? settingsPanel : mainPanel);
        }
    }

    public void OnPlayPressed() => StartCoroutine(PlayPressedSequence());
    
    private IEnumerator PlayPressedSequence()
    {
        AudioManager.Instance?.StopMusic(fadeOut: true, fadeDuration: 2f);
        if (TransitionHandler.Instance != null) yield return StartCoroutine(TransitionHandler.Instance.FadeOut(1f));
        ShowPanel(cutscenePanel);
        if (TransitionHandler.Instance != null) yield return StartCoroutine(TransitionHandler.Instance.FadeIn(1f));
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.cutsceneMusic, loop: false);
    }
    public void OnSettingsPressed() => ShowPanel(settingsPanel);
    public void OnCreditsPressed() => ShowPanel(creditsPanel);
    public void OnQuitPressed() => StartCoroutine(QuitSequence());

    private IEnumerator QuitSequence()
    {
        AudioManager.Instance?.StopMusic(fadeOut: true, fadeDuration: 1f);
        if (TransitionHandler.Instance != null) yield return StartCoroutine(TransitionHandler.Instance.FadeOut(1f));
        Application.Quit();
    }

    private void HookButtonSFX(Button btn, bool useQuitClick = false)
    {
        if (btn == null) return;
        btn.onClick.AddListener(() => { if (useQuitClick) AudioManager.Instance?.PlayMenuQuitClick(); else AudioManager.Instance?.PlayMenuClick(); });
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>() ?? btn.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener(_ => AudioManager.Instance?.PlayMenuHover());
        trigger.triggers.Add(entry);
    }
    public void OnBackToMain() => ShowPanel(mainPanel);
    public void OnDisplayPressed() => ShowPanel(displayPanel);
    public void OnAudioPressed() => ShowPanel(audioPanel);
    public void OnKeybindsPressed() => ShowPanel(keybindsPanel);
    public void OnBackToSettings() => ShowPanel(settingsPanel);

    #endregion Panel Nav 

    #region Display

    private void InitDisplaySettings()
    {
        Resolution[] all = Screen.resolutions;
        var seen = new HashSet<(int, int, int)>();
        _filteredResolutions = new List<Resolution>();

        for (int i = all.Length - 1; i >= 0; i--)
        {
            Resolution r = all[i];
            int rr = Mathf.RoundToInt((float)r.refreshRateRatio.value);
            if (!seen.Add((r.width, r.height, rr))) continue;
            _filteredResolutions.Add(r);
        }

        resolutionDropdown.ClearOptions();
        List<string> options = new();
        _currentResolutionIndex = 0;

        for (int i = 0; i < _filteredResolutions.Count; i++)
        {
            Resolution r = _filteredResolutions[i];
            int rr = Mathf.RoundToInt((float)r.refreshRateRatio.value);
            options.Add($"{r.width} x {r.height} @ {rr}hz");
            if (r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height)
                _currentResolutionIndex = i;
        }

        int savedIdx = PlayerPrefs.GetInt(ResolutionKey, -1);
        if (savedIdx >= 0 && savedIdx < _filteredResolutions.Count)
            _currentResolutionIndex = savedIdx;

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = _currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    private void OnResolutionChanged(int index)
    {
        _currentResolutionIndex = index;
        Resolution r = _filteredResolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        PlayerPrefs.SetInt(ResolutionKey, index);
        PlayerPrefs.Save();
    }

    private void OnFullscreenChanged(bool isFullscreen) // flip-flop
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    #endregion Display

    #region Audio

    private void InitAudioSettings()
    {
        if (AudioManager.Instance == null) return;

        masterSlider.value = AudioManager.Instance.masterVolume;
        musicSlider.value  = AudioManager.Instance.musicVolume;
        sfxSlider.value    = AudioManager.Instance.sfxVolume;

        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }
    // AudioManager already handles saving / loading audio, sliders will call the functions
    private void OnMasterChanged(float value) => AudioManager.Instance?.SetMasterVolume(value);
    private void OnMusicChanged(float value)  => AudioManager.Instance?.SetMusicVolume(value);
    private void OnSFXChanged(float value)    => AudioManager.Instance?.SetSFXVolume(value);

    #endregion Audio

    #region Keybinds

    private static readonly HashSet<string> PlayActions = new() { "LeftNote", "RightNote", "Ability1", "Ability2" };
    private static readonly HashSet<string> MenuActions = new() { "Select", "Back" };

    public Action<int, string> OnRebindConflict;

    public void StartRebind(string actionName, int bindingIndex, int playerID)
    {
        InputAction action = InputManager.Instance?.GetAction(actionName, playerID);
        if (action == null) return;

        action.Disable();

        action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .WithCancelingThrough("<Gamepad>/select")
            .OnComplete(op =>
            {
                string newPath = action.bindings[bindingIndex].effectivePath;

                if (HasConflict(newPath, actionName, playerID, bindingIndex))
                {
                    action.RemoveBindingOverride(bindingIndex);
                    action.Enable();
                    OnRebindConflict?.Invoke(playerID, actionName);
                    op.Dispose();
                    return;
                }

                action.Enable();
                InputManager.Instance.SaveBindingOverrides();
                OnRebindComplete?.Invoke(playerID, actionName);
                op.Dispose();
            })
            .OnCancel(op =>
            {
                action.Enable();
                op.Dispose();
            })
            .Start();
    }

    private bool HasConflict(string newPath, string actionName, int playerID, int bindingIndex)
    {
        if (string.IsNullOrEmpty(newPath)) return false;

        var asset = InputManager.Instance?.InputActions;
        if (asset == null) return false;

        bool isPlayAction = PlayActions.Contains(actionName);
        bool isMenuAction = MenuActions.Contains(actionName);
        string ownMapName = GetMapName(playerID);

        foreach (var map in asset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                bool actionIsPlay = PlayActions.Contains(action.name);
                bool actionIsMenu = MenuActions.Contains(action.name);

                if (!actionIsPlay && !actionIsMenu) continue; // skip nav actions

                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (binding.isComposite || binding.isPartOfComposite) continue;
                    if (action.name == actionName && map.name == ownMapName && i == bindingIndex) continue;
                    if (binding.effectivePath != newPath) continue;

                    if (isPlayAction && actionIsPlay) return true; // play vs play — always conflict

                    if (isMenuAction && actionIsMenu && map.name == ownMapName) return true; // menu vs menu — same player only
                }
            }
        }
        return false;
    }

    private string GetMapName(int playerID) => playerID == 0 ? "Player1" : "Player2";

    #endregion Keybinds
}