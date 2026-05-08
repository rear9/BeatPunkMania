
/*
using System;
using UnityEngine;

public class pScript : MonoBehaviour
{
    public CharData charDB;
    public SpriteRenderer artworkSpr;

    private int selOp;
    public bool iden;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("selectedOption"))
        {
            selOp = 0;
        }
        else
        {
            Load();
        }
        UpdateCharacter(selOp);
    }

    private void Load()
    {
        selOp = PlayerPrefs.GetInt("selectedOption");
    }

    private void UpdateCharacter(int selOp)
    {
        if (iden == false)
        {
            Character character = charDB.GetCharacter(selOp);
            artworkSpr.sprite = character.charSprite;
        }
        else
        {
            bool temp = Convert.ToBoolean(selOp);
            temp = !temp;
            int last = Convert.ToInt32(temp);
            Character character = charDB.GetCharacter(last);
            artworkSpr.sprite = character.charSprite;
        }
    }
}
*/

