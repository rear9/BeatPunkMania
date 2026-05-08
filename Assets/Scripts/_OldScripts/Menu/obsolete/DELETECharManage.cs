//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEditor.Experimental.GraphView;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.InputSystem;
//using UnityEngine.Rendering.VirtualTexturing;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
////using static UnityEngine.Rendering.DebugUI;


//public class CharManage : MonoBehaviour
//{
//    [SerializeField] private EventSystem system;
//    [SerializeField] private Selectable elemSle;
//    public CharData charDB;
//    public SpriteRenderer artworkSpr;
//    public GameObject Lyd;
//    public GameObject All;
//    public GameObject theOther;
//    public GameObject theOtherSle;
//    public Text buttText;
//    private int selOp = 0;
//    public int globalvalue = 0;
//    bool side = false;
//    //public Button self;
//    //public buttonCode sun;
//    //private bool abilOp1 = false;
//    //private bool abilOp2 = false;
//    //private bool abilOp3 = false;
//    //private bool abilOp4 = false;
//    public int abilOp1 = 0;
//    public int abilOp2 = 0;
//    public int abilOp3 = 0;
//    public int abilOp4 = 0;
//    public int abilOp5 = 0;
//    public int abilOp6 = 0;
//    public int abilOp7 = 0;
//    public int abilOp8 = 0;
//    public List<int> Aabilities;
//    public List<int> Labilities;

//    void Start()
//    {
//        PlayerPrefs.DeleteAll();
//        //for (int i = 0; i < 7; i++)
//        //{
//        //    abilities.Add(0);
//        //}
//        if (!PlayerPrefs.HasKey("selectedOption"))
//        {
//            selOp = 0;
//        }
//        else
//        {
//            LoadChar();
//        }

//        //if (!PlayerPrefs.HasKey("Name1"))
//        //{
//        //    abilOp1 = false;
//        //}
//        //else
//        //{
//        //    LoadAbil(0);
//        //}

//        //if (!PlayerPrefs.HasKey("Name2"))
//        //{
//        //    abilOp2 = false;
//        //}
//        //else
//        //{
//        //    LoadAbil(1);
//        //}

//        //if (!PlayerPrefs.HasKey("Name3"))
//        //{
//        //    abilOp3 = false;
//        //}
//        //else
//        //{
//        //    LoadAbil(2);
//        //}

//        //if (!PlayerPrefs.HasKey("Name4"))
//        //{
//        //    abilOp4 = false;
//        //}
//        //else
//        //{
//        //    LoadAbil(3);
//        //}
//        if (!PlayerPrefs.HasKey("Name1"))
//        {
//            abilOp1 = 0;
//        }
//        else
//        {
//            LoadAbil(0);
//        }

//        if (!PlayerPrefs.HasKey("Name2"))
//        {
//            abilOp2 = 0;
//        }
//        else
//        {
//            LoadAbil(1);
//        }

//        if (!PlayerPrefs.HasKey("Name3"))
//        {
//            abilOp3 = 0;
//        }
//        else
//        {
//            LoadAbil(2);
//        }

//        if (!PlayerPrefs.HasKey("Name4"))
//        {
//            abilOp4 = 0;
//        }
//        else
//        {
//            LoadAbil(3);
//            //StartGame();
//        }

//        if (!PlayerPrefs.HasKey("Name5"))
//        {
//            abilOp5 = 0;
//        }
//        else
//        {
//            LoadAbil(4);
//            //StartGame();
//        }

//        if (!PlayerPrefs.HasKey("Name6"))
//        {
//            abilOp6 = 0;
//        }
//        else
//        {
//            LoadAbil(5);
//            //StartGame();
//        }

//        if (!PlayerPrefs.HasKey("Name7"))
//        {
//            abilOp7 = 0;
//        }
//        else
//        {
//            LoadAbil(6);
//            //StartGame();
//        }

//        if (!PlayerPrefs.HasKey("Name8"))
//        {
//            abilOp8 = 0;
//        }
//        else
//        {
//            LoadAbil(7);
//            //StartGame();
//        }

//        //if (Keyboard.current[Key.K].wasPressedThisFrame)
//        //{
//        //    PlayerPrefs.DeleteAll();
//        //}
//        //if (PlayerPrefs.HasKey("Name1") && PlayerPrefs.HasKey("Name2") && PlayerPrefs.HasKey("Name3") && PlayerPrefs.HasKey("Name4"))
//        //{
//        //    StartGame();
//        //}
//        //Debug.Log(abilOp1);
//    }

//    public void AlLyst(int num)
//    {
//        //if (EventSystem.current.currentSelectedGameObject == self.gameObject)
//        //{
//        //    if (Input.GetKeyDown(KeyCode.Q))
//        //    {
//        //        remove();
//        //        if (value == weep.abilOp1)
//        //        {
//        //            weep.abilOp1 = weep.abilOp2;
//        //            weep.abilOp2 = weep.abilOp3;
//        //        }
//        //        //Debug.Log(weep.globalvalue + " was selected");
//        //        //weep.globalvalue
//        //    }
//        //    //Debug.Log(this.gameObject.name + " is currently selected!");
//        //}

//        //globalvalue = num;
//        side = false;
//        Aabilities.Add(num);
//        if (Aabilities.Count == 4)
//        {
//            if (Labilities.Count == 4)
//            {
//                abilOp1 = Aabilities[0];
//                SaveAbil(0);
//                abilOp2 = Aabilities[1];
//                SaveAbil(1);
//                abilOp3 = Aabilities[2];
//                SaveAbil(2);
//                abilOp4 = Aabilities[3];
//                SaveAbil(3);
//                abilOp5 = Labilities[0];
//                SaveAbil(4);
//                abilOp6 = Labilities[1];
//                SaveAbil(5);
//                abilOp7 = Labilities[2];
//                SaveAbil(6);
//                abilOp8 = Labilities[3];
//                SaveAbil(7);
//                StartGame();
//            }
//            else {
//                Lyd.GetComponent<select>().JumpSelect();
//                //side = true;
//            }
//            //Lyd.GetComponent<select>().JumpSelect();
//            //side = true;
//        }
//    }

//    public void LydList(int num)
//    {
//        side = true;
//        Labilities.Add(num);
//        if (Labilities.Count == 4)
//        {
//            if (Aabilities.Count == 4)
//            {
//                abilOp1 = Aabilities[0];
//                SaveAbil(0);
//                abilOp2 = Aabilities[1];
//                SaveAbil(1);
//                abilOp3 = Aabilities[2];
//                SaveAbil(2);
//                abilOp4 = Aabilities[3];
//                SaveAbil(3);
//                abilOp5 = Labilities[0];
//                SaveAbil(4);
//                abilOp6 = Labilities[1];
//                SaveAbil(5);
//                abilOp7 = Labilities[2];
//                SaveAbil(6);
//                abilOp8 = Labilities[3];
//                SaveAbil(7);
//                StartGame();
//            }
//            else
//            {
//                All.GetComponent<select>().JumpSelect();
//                //side = false;
//            }
//        }
//    }

//    void Update()
//    {
//        if (globalvalue != 0)
//        {
//            if (side == false)
//            {
//                Aabilities.Remove(globalvalue);
//                globalvalue = 0;
//            }
//            else
//            {
//                Labilities.Remove(globalvalue);
//                globalvalue = 0;
//            }
//        }
//        ////Debug.Log(weep.globalvalue + " was selected");
//        //if (EventSystem.current.currentSelectedGameObject == self.gameObject)
//        //{
//        //    if (Input.GetKeyDown(KeyCode.Q))
//        //    {
//        //        //remove();
//        //        //if (value == weep.abilOp1)
//        //        //{
//        //        //    weep.abilOp1 = weep.abilOp2;
//        //        //    weep.abilOp2 = weep.abilOp3;
//        //        //}
//        //        Debug.Log(" was selected");
//        //        //weep.globalvalue
//        //        abilities.Remove(sun.value);
//        //    }
//        //    //Debug.Log(this.gameObject.name + " is currently selected!");
//        //}
//    }

//    public void AlexSele()
//    {
//        selOp = 0;
//        SaveChar();
//        side = false;
//        //StartGame();
//    }

//    public void LydSele()
//    {
//        selOp = 1;
//        SaveChar();
//        side = true;
//        //StartGame();
//    }

//    //public void Ability1()
//    //{
//    //    abilOp1 = true;
//    //    SaveAbil(0);
//    //}

//    //public void Ability2()
//    //{
//    //    abilOp2 = true;
//    //    SaveAbil(1);
//    //}

//    //public void Ability3()
//    //{
//    //    abilOp3 = true;
//    //    SaveAbil(2);
//    //}

//    //public void Ability4()
//    //{
//    //    abilOp4 = true;
//    //    SaveAbil(3);
//    //}
//    //public void Update()
//    //{
//    //    if (abilOp1 == 0)
//    //    {
//    //        abilOp1 = globalvalue;
//    //        SaveAbil(0);
//    //        //StartGame();
//    //    }
//    //    else
//    //    {
//    //        if (abilOp2 == 0)
//    //        {
//    //            abilOp2 = globalvalue;
//    //            SaveAbil(1);
//    //        }
//    //        else
//    //        {
//    //            if (abilOp3 == 0)
//    //            {
//    //                abilOp3 = globalvalue;
//    //                SaveAbil(2);
//    //            }
//    //            else
//    //            {
//    //                if (abilOp4 == 0)
//    //                {
//    //                    abilOp4 = globalvalue;
//    //                    SaveAbil(3);
//    //                    Lyd.GetComponent<select>().JumpSelect();
//    //                }
//    //                else
//    //                {
//    //                    if (abilOp5 == 0)
//    //                    {
//    //                        abilOp5 = globalvalue;
//    //                        SaveAbil(4);
//    //                    }
//    //                    else
//    //                    {
//    //                        if (abilOp6 == 0)
//    //                        {
//    //                            abilOp6 = globalvalue;
//    //                            SaveAbil(5);
//    //                        }
//    //                        else
//    //                        {
//    //                            if (abilOp7 == 0)
//    //                            {
//    //                                abilOp7 = globalvalue;
//    //                                SaveAbil(6);
//    //                            }
//    //                            else
//    //                            {
//    //                                if (abilOp8 == 0)
//    //                                {
//    //                                    abilOp8 = globalvalue;
//    //                                    SaveAbil(7);
//    //                                    StartGame();
//    //                                }
//    //                            }
//    //                        }
//    //                    }
//    //                }
//    //            }
//    //        }
//    //    }
//    //}

//    //public void Ability(int which)
//    //{
//    //    globalvalue = which;
//    //    //if (which == 0)
//    //    //{
//    //    //    abilOp1 = which;
//    //    //    SaveAbil(0);
//    //    //}
//    //    //else if (which == 1)
//    //    //{
//    //    //    abilOp1 = which;
//    //    //    SaveAbil(1);
//    //    //}
//    //    //abilOp1 = which;
//    //    //SaveAbil(0);
//    //    if (abilOp1 == 0)
//    //    {
//    //        abilOp1 = which;
//    //        SaveAbil(0);
//    //        //StartGame();
//    //    }
//    //    else
//    //    {
//    //        if (abilOp2 == 0)
//    //        {
//    //            abilOp2 = which;
//    //            SaveAbil(1);
//    //        }
//    //        else
//    //        {
//    //            if (abilOp3 == 0)
//    //            {
//    //                abilOp3 = which;
//    //                SaveAbil(2);
//    //            }
//    //            else
//    //            {
//    //                if (abilOp4 == 0)
//    //                {
//    //                    abilOp4 = which;
//    //                    SaveAbil(3);
//    //                    Lyd.GetComponent<select>().JumpSelect();
//    //                }
//    //                else
//    //                {
//    //                    if (abilOp5 == 0)
//    //                    {
//    //                        abilOp5 = which;
//    //                        SaveAbil(4);
//    //                    }
//    //                    else
//    //                    {
//    //                        if (abilOp6 == 0)
//    //                        {
//    //                            abilOp6 = which;
//    //                            SaveAbil(5);
//    //                        }
//    //                        else
//    //                        {
//    //                            if (abilOp7 == 0)
//    //                            {
//    //                                abilOp7 = which;
//    //                                SaveAbil(6);
//    //                            }
//    //                            else
//    //                            {
//    //                                if (abilOp8 == 0)
//    //                                {
//    //                                    abilOp8 = which;
//    //                                    SaveAbil(7);
//    //                                    StartGame();
//    //                                }
//    //                            }
//    //                        }
//    //                    }
//    //                }
//    //            }
//    //        }
//    //    }
//    //}

//    public void add()
//    {
//        buttText.text = "hehe";
//    }
//    //public void Ability2(int which)
//    //{
//    //    abilOp2 = which;
//    //    SaveAbil(1);
//    //}

//    //public void Ability3(int which)
//    //{
//    //    abilOp3 = which;
//    //    SaveAbil(2);
//    //}

//    //public void Ability4(int which)
//    //{
//    //    abilOp4 = which;
//    //    SaveAbil(3);
//    //}


//    //if (theOther.GetComponent<CharManage>().abilOp4 != 0)
//    //{
//    //    StartGame();
//    //}

//    //if (Lyd.GetComponent<CharData>()  != null)
//    //{

//    //}
//    //}
//    //StartGame();
//    //myObject.GetComponent<MyScript>().MyFunction();
//    //system.SetSelectedGameObject(elemSle.gameObject);
//    private void LoadChar()
//    {
//        selOp = PlayerPrefs.GetInt("selectedOption");
//    }

//    private void SaveChar()
//    {
//        PlayerPrefs.SetInt("selectedOption", selOp);
//    }

//    //private void SaveAbil(int which)
//    //{
//    //    if (which == 0)
//    //    {
//    //        PlayerPrefs.SetInt("Name1", (abilOp1 ? 1 : 0));

//    //    }
//    //    else if (which == 1)
//    //    {
//    //        PlayerPrefs.SetInt("Name2", (abilOp2 ? 1 : 0));
//    //    }
//    //    else if (which == 2)
//    //    {
//    //        PlayerPrefs.SetInt("Name3", (abilOp3 ? 1 : 0));
//    //    }
//    //    else
//    //    {
//    //        PlayerPrefs.SetInt("Name4", (abilOp4 ? 1 : 0));
//    //    }

//    //}
//    private void SaveAbil(int which)
//    {
//        if (which == 0)
//        {
//            PlayerPrefs.SetInt("Name1", abilOp1);
//        }
//        else if (which == 1)
//        {
//            PlayerPrefs.SetInt("Name2", abilOp2);
//        }
//        else if (which == 2)
//        {
//            PlayerPrefs.SetInt("Name3", abilOp3);
//        }
//        else if (which == 3)
//        {
//            PlayerPrefs.SetInt("Name4", abilOp4);
//        }
//        else if (which == 4)
//        {
//            PlayerPrefs.SetInt("Name5", abilOp5);
//        }
//        else if (which == 5)
//        {
//            PlayerPrefs.SetInt("Name6", abilOp6);
//        }
//        else if (which == 6)
//        {
//            PlayerPrefs.SetInt("Name7", abilOp7);
//        }
//        else
//        {
//            PlayerPrefs.SetInt("Name8", abilOp8);
//        }

//    }

//    //private void LoadAbil(int which)
//    //{
//    //    if (which == 0)
//    //    {
//    //        abilOp1 = (PlayerPrefs.GetInt("Name1") != 0);

//    //    }
//    //    else if (which == 1)
//    //    {
//    //        abilOp2 = (PlayerPrefs.GetInt("Name2") != 0);
//    //    }
//    //    else if (which == 2)
//    //    {
//    //        abilOp3 = (PlayerPrefs.GetInt("Name3") != 0);
//    //    }
//    //    else
//    //    {
//    //        abilOp4 = (PlayerPrefs.GetInt("Name4") != 0);
//    //    }
//    //}

//    private void LoadAbil(int which)
//    {
//        if (which == 0)
//        {
//            abilOp1 = (PlayerPrefs.GetInt("Name1"));

//        }
//        else if (which == 1)
//        {
//            abilOp2 = (PlayerPrefs.GetInt("Name2"));
//        }
//        else if (which == 2)
//        {
//            abilOp3 = (PlayerPrefs.GetInt("Name3"));
//        }
//        else if (which == 3)
//        {
//            abilOp4 = (PlayerPrefs.GetInt("Name4"));
//        }
//        else if (which == 4)
//        {
//            abilOp5 = (PlayerPrefs.GetInt("Name5"));
//        }
//        else if (which == 5)
//        {
//            abilOp6 = (PlayerPrefs.GetInt("Name6"));
//        }
//        else if (which == 6)
//        {
//            abilOp7 = (PlayerPrefs.GetInt("Name7"));
//        }
//        else
//        {
//            abilOp8 = (PlayerPrefs.GetInt("Name8"));
//        }
//    }

//    public void StartGame()
//    {
//        SceneManager.LoadScene("PlayArea");
//    }

//    //private void UpdateCharacter(int selOp)
//    //{
//    //    Character character = charDB.GetCharacter(selOp);
//    //}

//    //void Update()
//    //{
//    //    Debug.Log("abil 1: " + abilOp1);
//    //    Debug.Log("abil 2: " + abilOp2);
//    //    Debug.Log("abil 3: " + abilOp3);
//    //    Debug.Log("abil 4: " + abilOp4);
//    //    Debug.Log("abil 5: " + abilOp5);
//    //    Debug.Log("abil 6: " + abilOp6);
//    //    Debug.Log("abil 7: " + abilOp7);
//    //    Debug.Log("abil 8: " + abilOp8);
//    //}
//}
