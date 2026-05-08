using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Major/Gambit")]
public class Gambit : Ability
{
    public int requiredHits = 4;
    public float bonusMultiplier = 2f;
    private PlayerController _playerController;

    public override AbilityCategory Category => AbilityCategory.Major;
    
    public override void Activate(int casterID, AbilityContext ctx)
    {
            ScoreManager scoreManager = ScoreManager.Instance;
            int opponentID = casterID == 0 ? 1 : 0;
            //if (scoreManager._playerConfidence[opponentID] < 250) 
            //{
            ctx.scoreManager.BeginGambitWindow(casterID, requiredHits, bonusMultiplier);
            _playerController = GameManager.Instance?.GetPlayer(opponentID);
            _playerController.UseStamina(25);
            //}
    }

    public override void OnExpire(int casterID, AbilityContext ctx)
    {
        ctx.scoreManager.EndGambitWindow(casterID);
    }
}
