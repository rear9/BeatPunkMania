/*
using UnityEngine;

public class aScrip : MonoBehaviour
{
    public AbilData abilDB;
    public SpriteRenderer artworkSpr;
    public int oof;
    private int abilOp1 = 0;
    private int abilOp2 = 0;
    private int abilOp3 = 0;
    private int abilOp4 = 0;
    private int abilOp5 = 0;
    private int abilOp6 = 0;
    private int abilOp7 = 0;
    private int abilOp8 = 0;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("Abil1"))
        {
            abilOp1 = 0;
        }
        else
        {
            LoadAbil(0);
        }

        if (!PlayerPrefs.HasKey("Abil2"))
        {
            abilOp2 = 0;
        }
        else
        {
            LoadAbil(1);
        }

        if (!PlayerPrefs.HasKey("Abil3"))
        {
            abilOp3 = 0;
        }
        else
        {
            LoadAbil(2);
        }

        if (!PlayerPrefs.HasKey("Abil4"))
        {
            abilOp4 = 0;
        }
        else
        {
            LoadAbil(3);
        }

        if (!PlayerPrefs.HasKey("Abil5"))
        {
            abilOp5 = 0;
        }
        else
        {
            LoadAbil(4);
        }

        if (!PlayerPrefs.HasKey("Abil6"))
        {
            abilOp6 = 0;
        }
        else
        {
            LoadAbil(5);
        }

        if (!PlayerPrefs.HasKey("Abil7"))
        {
            abilOp7 = 0;
        }
        else
        {
            LoadAbil(6);
        }

        if (!PlayerPrefs.HasKey("Abil8"))
        {
            abilOp8 = 0;
        }
        else
        {
            LoadAbil(7);
        }
        finalAbil(oof);
    }

    //void Update()
    //{
    //    Debug.Log("abil1: " + abilOp1);
    //    Debug.Log("abil2: " + abilOp2);
    //    Debug.Log("abil3: " + abilOp3);
    //    Debug.Log("abil4: " + abilOp4);
    //    Debug.Log("abil5: " + abilOp5);
    //    Debug.Log("abil6: " + abilOp6);
    //    Debug.Log("abil7: " + abilOp7);
    //    Debug.Log("abil8: " + abilOp8);
    //}
   
    private void LoadAbil(int which)
    {
        if (which == 0)
        {
            abilOp1 = (PlayerPrefs.GetInt("Abil1"));

        }
        else if (which == 1)
        {
            abilOp2 = (PlayerPrefs.GetInt("Abil2"));
        }
        else if (which == 2)
        {
            abilOp3 = (PlayerPrefs.GetInt("Abil3"));
        }
        else if (which == 3)
        {
            abilOp4 = (PlayerPrefs.GetInt("Abil4"));
        }
        else if (which == 4)
        {
            abilOp5 = (PlayerPrefs.GetInt("Abil5"));
        }
        else if (which == 5)
        {
            abilOp6 = (PlayerPrefs.GetInt("Abil6"));
        }
        else if (which == 6)
        {
            abilOp7 = (PlayerPrefs.GetInt("Abil7"));
        }
        else
        {
            abilOp8 = (PlayerPrefs.GetInt("Abil8"));
        }
    }

    private void UpdateAbility(int selOp)
    {
        Ability ability = abilDB.GetAbility(selOp);
        artworkSpr.sprite = ability.abilSprite;
    }

    private void finalAbil(int which)
    {
        if (which == 0)
        {
            UpdateAbility(abilOp1);
        }
        else if (which == 1)
        {
            UpdateAbility(abilOp2);
        }
        else if (which == 2)
        {
            UpdateAbility(abilOp3);
        }
        else if (which == 3)
        {
            UpdateAbility(abilOp4);
        }
        else if (which == 4)
        {
            UpdateAbility(abilOp5);
        }
        else if (which == 5)
        {
            UpdateAbility(abilOp6);
        }
        else if (which == 6)
        {
            UpdateAbility(abilOp7);
        }
        else
        {
            UpdateAbility(abilOp8);
        }
    }
}
*/


