using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Minor/ConfidenceSap")]
public class ConfidenceSap : Ability
{
    [Range(0f, 1f)]
    public float sapPercent = 0.25f;
    private PlayerController _playerController;

    public override AbilityCategory Category => AbilityCategory.Minor;
    
    public override void Activate(int casterID, AbilityContext ctx)
    {
            int opponentID = casterID == 0 ? 1 : 0;
            _playerController = GameManager.Instance?.GetPlayer(opponentID);
            _playerController.UseStamina(25);
            ScoreManager scoreManager = ScoreManager.Instance;
            scoreManager.AddConfidence(opponentID, -250);
            scoreManager.AddConfidence(casterID, 250);
            // ScoreManager has confidence adjustment functions, these can be called here
    }
}
