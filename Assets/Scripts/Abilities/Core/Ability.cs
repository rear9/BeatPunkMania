using UnityEngine;

/// <summary>
/// Core ability script; handles information for all ScriptableObjs
/// </summary>

public enum AbilityCategory { Major, Minor }

public class AbilityContext
{
    public NoteHitDetection[] noteDetection;  // [0] = P1, [1] = P2
    public PlayerController[]  players;        // [0] = P1, [1] = P2
    public ScoreManager scoreManager;
    public PhaseHandler phaseManager;
    public InputManager inputManager;
    public UIManager  uiManager;
    public NoteSpawner[] noteSpawners;   // [0] = P1, [1] = P2 – needed by DoubleTime
}

public abstract class Ability : ScriptableObject
{
    [Header("Ability Info")]
    public string abilName;
    public Sprite abilSprite;
    
    public abstract AbilityCategory Category { get; }
    public abstract void Activate(int casterID, AbilityContext ctx);
    public virtual void Tick(float deltaTime, int casterID, AbilityContext ctx) { }
    public virtual void OnExpire(int casterID, AbilityContext ctx) { }
}