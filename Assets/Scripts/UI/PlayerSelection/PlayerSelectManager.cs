/*using System.Collections.Generic; *** CUT ***
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSelectManager : MonoBehaviour
{
    public static PlayerSelectManager Instance { get; private set; }

    [Header("Char Select UI")]
    [SerializeField] private UnityEngine.UI.Image background;
    [SerializeField] private UnityEngine.UI.Image alex;
    [SerializeField] private UnityEngine.UI.Image lydia;
    [SerializeField] private GameObject charSelectPanel;
    [SerializeField] private GameObject abilSelectPanel;
    [SerializeField] private UnityEngine.UI.Image p1ReadyImage;
    [SerializeField] private UnityEngine.UI.Image p2ReadyImage;

    [Header("Input Systems")]
    [SerializeField] private MultiplayerEventSystem system;
    [SerializeField] private MultiplayerEventSystem system2;
    [SerializeField] private GameObject p2EventSystem;
    [SerializeField] public Selectable algo;
    [SerializeField] public Selectable lydgo;

    private int _selOp = 0;
    private bool _abilsel = false;
    private bool _p1AbilsDone = false;
    private bool _p2AbilsDone = false;
    private bool _p1Ready = false;
    private bool _p2Ready = false;

    public List<int> Aabilities = new List<int>();
    public List<int> Labilities = new List<int>();

    private const int MaxAbilities = 2;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SessionData.Reset();

        system.SetSelectedGameObject(algo.gameObject);
        system.playerRoot = charSelectPanel;

        SetImageAlpha(p1ReadyImage, 0f);
        SetImageAlpha(p2ReadyImage, 0f);

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnBack += HandleBack;
            InputManager.Instance.OnSelect += HandleSelect;
            InputManager.Instance.OnNavLeft += HandleNavLeft;
            InputManager.Instance.OnNavRight += HandleNavRight;
            InputManager.Instance.OnNavUp += HandleNavUp;
            InputManager.Instance.OnNavDown += HandleNavDown;
        }
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnBack -= HandleBack;
            InputManager.Instance.OnSelect -= HandleSelect;
            InputManager.Instance.OnNavLeft -= HandleNavLeft;
            InputManager.Instance.OnNavRight -= HandleNavRight;
            InputManager.Instance.OnNavUp -= HandleNavUp;
            InputManager.Instance.OnNavDown -= HandleNavDown;
        }
    }

    private void HandleNav(int playerID, Vector2 dir)
    {
        if (!_abilsel) return;
        MultiplayerEventSystem es = playerID == 0 ? system : system2;
        GameObject current = es.currentSelectedGameObject;
        if (current == null) return;

        Selectable sel = current.GetComponent<Selectable>();
        if (sel == null) return;

        Selectable next = dir.x > 0 ? sel.FindSelectableOnRight()
                        : dir.x < 0 ? sel.FindSelectableOnLeft()
                        : dir.y > 0 ? sel.FindSelectableOnUp()
                                     : sel.FindSelectableOnDown();

        if (next != null)
            es.SetSelectedGameObject(next.gameObject);
    }

    private void HandleNavLeft(int playerID)  => HandleNav(playerID, Vector2.left);
    private void HandleNavRight(int playerID) => HandleNav(playerID, Vector2.right);
    private void HandleNavUp(int playerID)    => HandleNav(playerID, Vector2.up);
    private void HandleNavDown(int playerID)  => HandleNav(playerID, Vector2.down);

    private void HandleBack(int playerID)
    {
        if (!_abilsel) return;

        bool playerDone = playerID == 0 ? _p1AbilsDone : _p2AbilsDone;
        bool playerReady = playerID == 0 ? _p1Ready : _p2Ready;

        if (playerReady)
        {
            if (playerID == 0) { _p1Ready = false; SetImageAlpha(p1ReadyImage, 0f); }
            else { _p2Ready = false; SetImageAlpha(p2ReadyImage, 0f); }
            return;
        }

        removeList(playerID == 0 ? system : system2);
    }

    private void HandleSelect(int playerID)
    {
        MultiplayerEventSystem es = playerID == 0 ? system : system2;
        GameObject current = es.currentSelectedGameObject;
        if (current == null) return;

        bool playerDone = playerID == 0 ? _p1AbilsDone : _p2AbilsDone;
        if (playerDone)
        {
            if (playerID == 0) { _p1Ready = true; SetImageAlpha(p1ReadyImage, 1f); }
            else { _p2Ready = true; SetImageAlpha(p2ReadyImage, 1f); }

            if (_p1Ready && _p2Ready)
                StartGame();
            return;
        }

        if (!_abilsel)
        {
            if (current == algo.gameObject) AlexSele();
            else if (current == lydgo.gameObject) LydSele();
            return;
        }

        addList(current, playerID);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("PlayArea");
    }

    public void AlexSele()
    {
        _selOp = 0;
        //SaveChar();
        _abilsel = true;
        p2EventSystem?.SetActive(true);
        charSelectPanel.SetActive(false);
        abilSelectPanel.SetActive(true);
        system.SetSelectedGameObject(algo.gameObject);
        system.playerRoot = abilSelectPanel;
        system2.SetSelectedGameObject(lydgo.gameObject);
        system2.playerRoot = abilSelectPanel;
    }

    public void LydSele()
    {
        _selOp = 1;
        //SaveChar();
        _abilsel = true;
        p2EventSystem?.SetActive(true);
        charSelectPanel.SetActive(false);
        abilSelectPanel.SetActive(true);
        system.SetSelectedGameObject(lydgo.gameObject);
        system.playerRoot = abilSelectPanel;
        system2.SetSelectedGameObject(algo.gameObject);
        system2.playerRoot = abilSelectPanel;
    }

    public void addList(GameObject self, int playerID)
    {
        int buttnum = int.Parse(self.name);
        List<int> targetList = buttnum < 6 ? Aabilities : Labilities;

        if (targetList.Count >= MaxAbilities) return;

        targetList.Add(buttnum);
        int amount = targetList.Count(x => x == buttnum);
        self.GetComponentInChildren<TextMeshProUGUI>().text = "x" + amount;

        List<int> myList = playerID == 0 ? Aabilities : Labilities;
        if (myList.Count == MaxAbilities)
        {
            if (playerID == 0) _p1AbilsDone = true;
            else _p2AbilsDone = true;
        }

        if (_p1AbilsDone && _p2AbilsDone)
            TryStartGame();
    }

    private void TryStartGame()
    {
        if (_selOp == 0)
        {
            SessionData.Abilities[0] = Aabilities[0] - 1;
            SessionData.Abilities[1] = Aabilities[1] - 1;
            SessionData.Abilities[2] = Labilities[0] - 6;
            SessionData.Abilities[3] = Labilities[1] - 6;
        }
        else
        {
            SessionData.Abilities[0] = Labilities[0] - 6;
            SessionData.Abilities[1] = Labilities[1] - 6;
            SessionData.Abilities[2] = Aabilities[0] - 1;
            SessionData.Abilities[3] = Aabilities[1] - 1;
        }
    }

    public void removeList(EventSystem hello)
    {
        if (hello.currentSelectedGameObject == null) return;

        int buttnum = int.Parse(hello.currentSelectedGameObject.name);
        List<int> targetList = buttnum > 5 ? Labilities : Aabilities;
        int playerID = buttnum > 5 ? 1 : 0;

        int amount = targetList.Count(x => x == buttnum);
        if (amount == 0) return;

        targetList.Remove(buttnum);
        amount--;

        if (playerID == 0 && _p1AbilsDone) { _p1AbilsDone = false; _p1Ready = false; SetImageAlpha(p1ReadyImage, 0f); }
        else if (playerID == 1 && _p2AbilsDone) { _p2AbilsDone = false; _p2Ready = false; SetImageAlpha(p2ReadyImage, 0f); }

        TextMeshProUGUI itext = hello.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>();
        itext.text = amount > 0 ? "x" + amount : "";
    }

    private void SetImageAlpha(UnityEngine.UI.Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    //private void SaveChar() => SessionData.SelectedCharacter = _selOp;
}*/