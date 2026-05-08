/* ** TEMP **
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class ESManager : MonoBehaviour
{
    public GameObject win;
    [SerializeField] private ScoreManager scoreManager;
    public TextMeshProUGUI alScr;
    public TextMeshProUGUI lydScr;
    public TextMeshProUGUI resu;
    public EventSystem eventSystem;
    public InputActionAsset act;
    public void EndGame()
    {
        win.SetActive(true);
        //EventSystem currentSystem = EventSystem.current;
        var uiModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        uiModule.actionsAsset = act;
        //InputAction clickAction = act.FindAction("UI/Click");
        uiModule.point = InputActionReference.Create(act.FindAction("UI/Point"));
        uiModule.leftClick = InputActionReference.Create(act.FindAction("UI/Click"));
        //act.
        //eventSystem.
        //Debug.Log(scoreManager._playerScores[0] + " " + scoreManager._playerScores[1]);
        alScr.text = scoreManager._playerScores[0].ToString();
        lydScr.text = scoreManager._playerScores[1].ToString();
        if (scoreManager._playerScores[0] >  scoreManager._playerScores[1])
        {
            resu.text = "Alex Wins";
        }
        else if (scoreManager._playerScores[1] > scoreManager._playerScores[0])
        {
            resu.text = "Lydia Wins";
        }
        else
        {
            resu.text = "Tie";
        }

    }

    public void ChangeScene(int which)
    {
        if (which == 0)
        {
            //Debug.Log("eheyhe");
            SceneManager.LoadScene("Menu");
        }
        else 
        {
            SceneManager.LoadScene("PlayArea");
        }
    }
}
*/
