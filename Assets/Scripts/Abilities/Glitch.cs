using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Major/Glitch")]
public class Glitch : Ability
{
    public override AbilityCategory Category => AbilityCategory.Major;
    private PlayerController _playerController;

    public override void Activate(int casterID, AbilityContext ctx) // ability start & end funcs
    {
            int opponentID = casterID == 0 ? 1 : 0;
            _playerController = GameManager.Instance?.GetPlayer(opponentID);
            _playerController.UseStamina(25);
            ctx.uiManager?.SetGlitchOverlay(opponentID, true); // overlay on/off switch

            // other visual logic can go here
    }
 
    public override void OnExpire(int casterID, AbilityContext ctx)
    {
        int opponentID = casterID == 0 ? 1 : 0;
        ctx.uiManager?.SetGlitchOverlay(opponentID, false);
        
        // reverse above
    }
}
