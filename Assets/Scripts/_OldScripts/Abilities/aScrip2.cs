//using UnityEngine;

//public class aScrip2 : MonoBehaviour
//{
//    public AbilityManager theMangager;
//    public SpriteRenderer artworkSpr;
//    public int oof;
//    public int slot;
//    private bool timerActive = false;
//    private float timer = 0f;

//    private void Start()
//    {
//        if (!timerActive)
//        {
//            timerActive = true;
//            timer = 0.0000001f;
//        }

//    }

//    void Update()
//    {
//        if (timerActive)
//        {
//            timer -= Time.deltaTime;

//            if (timer <= 0f)
//            {
//                timerActive = false;
//                UpdateAbility(oof, slot);
//            }
//        }
//    }

//    private void UpdateAbility(int pl, int selOp)
//    {
//        Ability ability = theMangager.GetPlayerAbility(pl, selOp);
//        artworkSpr.sprite = ability.abilSprite;

//    }
//}



