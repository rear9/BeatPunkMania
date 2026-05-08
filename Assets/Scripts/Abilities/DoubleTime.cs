using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Minor/DoubleTime")]
public class DoubleTime : Ability
{
    [Tooltip("0 = caster left lane, 1 = caster right lane")]
    public int targetLaneOffset = 0;

    public override AbilityCategory Category => AbilityCategory.Minor;
    
    public override void Activate(int casterID, AbilityContext ctx)
    {
        NoteSpawner spawner = GetSpawner(casterID, ctx);
        if (spawner == null) return;
        spawner.EnableDoubleTime(GetLane(casterID, ctx)); // double-time functionality goes in NoteSpawner as that handles spawning
    }

    public override void OnExpire(int casterID, AbilityContext ctx)
    {
        NoteSpawner spawner = GetSpawner(casterID, ctx); // reverse
        if (spawner == null) return;
        spawner.DisableDoubleTime(GetLane(casterID, ctx));
    }
    
    private NoteSpawner GetSpawner(int casterID, AbilityContext ctx) // helpers
    {
        if (ctx.noteSpawners == null || casterID >= ctx.noteSpawners.Length) return null;
        return ctx.noteSpawners[casterID];
    }

    private int GetLane(int casterID, AbilityContext ctx)
    {
        PlayerController player = ctx.players[casterID];
        return targetLaneOffset == 0 ? player.LeftLane : player.RightLane;
    }
}
