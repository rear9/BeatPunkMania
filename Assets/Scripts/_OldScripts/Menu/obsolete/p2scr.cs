
//using System;
//using UnityEngine;

//public class p2scr : MonoBehaviour
//{
//    public CharData charDB;
//    public SpriteRenderer artworkSpr;

//    private int selOp = 0;
//    //private bool abilOp1 = false;
//    //private bool abilOp2 = false;
//    //private bool abilOp3 = false;
//    //private bool abilOp4 = false;

//    private void Start()
//    {
//        if (!PlayerPrefs.HasKey("selectedOption"))
//        {
//            selOp = 0;
//        }
//        else
//        {
//            Load();
//        }
//        bool temp = Convert.ToBoolean(selOp);
//        temp = !temp;
//        int last = Convert.ToInt32(temp);
//        UpdateCharacter(last);
//    }
//    private void Load()
//    {
//        selOp = PlayerPrefs.GetInt("selectedOption");
//    }

//    private void UpdateCharacter(int selOp)
//    {
//        Character character = charDB.GetCharacter(selOp);
//        artworkSpr.sprite = character.charSpr;
//    }
//}

