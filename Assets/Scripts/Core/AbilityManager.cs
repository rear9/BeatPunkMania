/*using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all ability contexts, activation & inputs through a library system (abilities got cut)
/// </summary>

public class AbilityManager : MonoBehaviour
{
    [Header("Ability Library")]
    [SerializeField] private Ability[] abilityLib;

    [Header("Scene References")]
    [SerializeField] private NoteHitDetection p1NoteDetection;
    [SerializeField] private NoteHitDetection p2NoteDetection;
    [SerializeField] private PlayerController p1Controller;
    [SerializeField] private PlayerController p2Controller;
    [SerializeField] private NoteSpawner p1NoteSpawner;
    [SerializeField] private NoteSpawner p2NoteSpawner;
    public UIManager _UImanager;
    public NoteHitDetection[] noteHitTransplant;
    public Ability[,] _playerAbilities = new Ability[2, 2];
    public Dictionary<int, Ability> _active = new();
    public AbilityContext _ctx;

    #region Lifecycle
    private void Start()
    {
        _ctx = BuildContext();
        // LoadAbilitiesFromSession();
        SubscribeInputEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeInputEvents();
    }

    private void Update()
    {
        foreach (var abilID in _active)
        {
            int playerID = abilID.Key / 10;
            if (noteHitTransplant[playerID].accuracies.Count == 4)
            {
                if (abilID.Value.abilName == "Gambit")
                {
                    abilID.Value.OnExpire(playerID, _ctx);
                }
            }
            abilID.Value.Tick(Time.deltaTime, playerID, _ctx);
        }
    }
    #endregion

    #region Context
    
    private AbilityContext BuildContext() => new()
    {
        noteDetection = new[] {p1NoteDetection, p2NoteDetection},
        players = new[] {p1Controller, p2Controller},
        noteSpawners = new[] {p1NoteSpawner, p2NoteSpawner},
        scoreManager = ScoreManager.Instance,
        phaseManager = FindFirstObjectByType<PhaseHandler>(),
        inputManager = InputManager.Instance,
        uiManager = UIManager.Instance,
    };
    
    #endregion Context

    /*#region Loading **CUT**
    
    private void LoadAbilitiesFromSession() // loads abilities from static class
    {
        _playerAbilities[0, 0] = GetAbility(SessionData.Abilities[0]);
        _UImanager.UpdateAbilitySlot(0, 0, GetAbility(SessionData.Abilities[0]).abilSprite, true);
        _playerAbilities[0, 1] = GetAbility(SessionData.Abilities[1]);
        _UImanager.UpdateAbilitySlot(0, 1, GetAbility(SessionData.Abilities[1]).abilSprite, true);
        _playerAbilities[1, 0] = GetAbility(SessionData.Abilities[2]);
        _UImanager.UpdateAbilitySlot(1, 0, GetAbility(SessionData.Abilities[2]).abilSprite, true);
        _playerAbilities[1, 1] = GetAbility(SessionData.Abilities[3]);
        _UImanager.UpdateAbilitySlot(1, 1, GetAbility(SessionData.Abilities[3]).abilSprite, true);
    }

    private Ability GetAbility(int index) // finds ability in library by index
    {
        if (abilityLib == null || index < 0 || index >= abilityLib.Length) return null;
        return abilityLib[index];
    }
    
    #endregion Loading#1#

    #region Activation
    
    private void TryActivate(int playerID, int slotIndex)
    {
        Ability ability = _playerAbilities[playerID, slotIndex];
        if (ability == null) return;

        int key = playerID * 10 + slotIndex;
        if (!_active.TryAdd(key, ability)) return;

        ability.Activate(playerID, _ctx);
    }

    public void OnPhaseCycleComplete() // temp
    {
        foreach (var abil in _active)
        {
            int playerID = abil.Key / 10;
            _UImanager.UpdateAbilitySlot(playerID, 0, _playerAbilities[playerID, 0].abilSprite, true);
            _UImanager.UpdateAbilitySlot(playerID, 1, _playerAbilities[playerID, 1].abilSprite, true);
            abil.Value.OnExpire(playerID, _ctx);
        }
        _active.Clear();
    }
    
    #endregion Activation

    #region Input
    
    private void SubscribeInputEvents() // inputs
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnAbility1 += OnAbility1;
        InputManager.Instance.OnAbility2 += OnAbility2;
    }

    private void UnsubscribeInputEvents()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnAbility1 -= OnAbility1;
        InputManager.Instance.OnAbility2 -= OnAbility2;
    }

    private void OnAbility1(int playerID) => TryActivate(playerID, 0);
    private void OnAbility2(int playerID) => TryActivate(playerID, 1);
    
    #endregion Input

    #region Helpers
    
    public Ability GetPlayerAbility(int playerID, int slot) => _playerAbilities[playerID, slot];
    public bool IsActive(int playerID, int slot) => _active.ContainsKey(playerID * 10 + slot);
    
    #endregion Helpers
}*/