//using TMPro;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.InputSystem;
//using UnityEngine.UI;
////using UnityEngine.UIElements;
//using UnityEngine.Events;


//public class buttonCode : MonoBehaviour
//{
//    BaseEventData m_BaseEvent;
//    public CharManage weep;
//    //public Text buttText;
//    public TextMeshProUGUI TextField;
//    //public GameObject f;
//    public int value;
//    public Button self;
//    public int count = 0;
//    public MmManager cou;
//    bool selected = false;
//    //public int signal;
//    public void add()
//    {
//        count++;
//        TextField.text = "x" + count;
//    }

//    void Update()
//    {
//        //Debug.Log(weep.globalvalue + " was selected");
//        if (EventSystem.current.currentSelectedGameObject == self.gameObject)
//        {
//            if (Input.GetKeyDown(KeyCode.Q))
//            {
//                if (count != 0) {
//                    remove();
//                }
                
//                //if (value == weep.abilOp1)
//                //{
//                //    weep.abilOp1 = weep.abilOp2;
//                //    weep.abilOp2 = weep.abilOp3;
//                //}
//                //Debug.Log(weep.globalvalue + " was selected");
//                //weep.globalvalue
//            }
//            //Debug.Log(this.gameObject.name + " is currently selected!");
//        }
//    }
//    //void Update()
//    //{
//    //    //Check if the GameObject is being highlighted
//    //    if (IsHighlighted() == true)
//    //    {
//    //        //Output that the GameObject was highlighted, or do something else
//    //        Debug.Log("Selectable is Highlighted");
//    //        if (Input.GetKeyDown(KeyCode.Q))
//    //        {
//    //            remove();
//    //            Debug.Log(" was selected");
//    //        }
//    //    }
//    //}
//    //public void OnSelect(BaseEventData eventData)
//    //{
//    //    selected = true;
//    //    //Debug.Log("true");
//    //}

//    //public void OnDeselect(BaseEventData eventData)
//    //{
//    //    selected = false;
//    //    //Debug.Log("false");
//    //}

//    //public void Update()
//    //{
//    //    //Debug.Log(selected);
//    //    if (selected == true && Input.GetKeyDown(KeyCode.Z))
//    //    {
//    //        remove();
//    //        Debug.Log(" was selected");
//    //    }
//    //}
//    //public GameObject ButtonGameObject;

//    //public void Update()
//    //{
//    //    // Compare selected gameObject with referenced Button gameObject
//    //    if (EventSystem.current.currentSelectedGameObject == ButtonGameObject)
//    //    {
//    //        //Debug.Log(this.ButtonGameObject.name + " was selected");
//    //        if (Input.GetKeyDown(KeyCode.Q))
//    //        {
//    //            remove();
//    //            Debug.Log(" was selected");
//    //        }
//    //    }
//    //}

//    public void remove()
//    {
//        count--; 
//        if (count != 0)
//        {
//            TextField.text = "x" + count;
//        }
//        else
//        {
//            TextField.text = "";
//        }
//        weep.globalvalue = value;
//    }
//}
