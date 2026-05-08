using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Major/Sabotage")]
public class Sabotage : Ability
{
    public override AbilityCategory Category => AbilityCategory.Major;
    private PlayerController _playerController;

    public override void Activate(int casterID, AbilityContext ctx)
    {
        
            _playerController.UseStamina(25);
            ctx.scoreManager.SetScore(casterID, 0);
            ctx.inputManager.SwapLanes(Opponent(casterID)); // input switching done from InputManager w/ bool
            int opponentID = casterID == 0 ? 1 : 0;
            _playerController = GameManager.Instance?.GetPlayer(opponentID);
            // visual calls will need to be done for UI / UX
  
    }
 
    public override void OnExpire(int casterID, AbilityContext ctx)
    {
        ctx.inputManager.ClearLaneSwap(Opponent(casterID));
    }
    
    private static int Opponent(int casterID) => casterID == 0 ? 1 : 0;
}
