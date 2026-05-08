using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles ability / stamina / confidence visual bars and note feedback
/// </summary>

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Player 1 UI")]
    [SerializeField] private Image p1StaminaBar;
    [SerializeField] private Image p1ConfidenceBar;
    [SerializeField] private Image p1ConfidenceDial;
    [SerializeField] private float p1DialOffset = 16;
    //[SerializeField] private SpriteRenderer[] p1AbilitySlots;
    [SerializeField] private Transform p1FeedbackSpawn;
    private float p1CurrStamina, p1CurrConfidence, p1CurrAngle,
        p1TargetStamina, p1TargetConfidence, p1TargetAngle = 0;
    
    [Header("Player 2 UI")]
    [SerializeField] private Image p2StaminaBar;
    [SerializeField] private Image p2ConfidenceBar;
    [SerializeField] private Image p2ConfidenceDial;
    [SerializeField] private float p2DialOffset = 16;
    //[SerializeField] private SpriteRenderer[] p2AbilitySlots;
    [SerializeField] private Transform p2FeedbackSpawn;
    private float p2CurrStamina, p2CurrConfidence, p2CurrAngle,
        p2TargetStamina, p2TargetConfidence, p2TargetAngle = 0;
    
    [Header("Hit Feedback Sprites")]
    [SerializeField] private Sprite perfectSprite;
    [SerializeField] private Sprite goodSprite;
    [SerializeField] private Sprite okaySprite;
    [SerializeField] private Sprite missSprite;
    [SerializeField] private float feedbackDisplayDuration = 0.5f;

    //[Header("Ability Overlay")]
    //[SerializeField] private GameObject p1GlitchOverlay;
    //[SerializeField] private GameObject p2GlitchOverlay;

    [Header("Score Display")]
    [SerializeField] private Image ScoreBar;
    [SerializeField] private SpriteRenderer ScoreDivider;
    [SerializeField] private int maxScore = 50000;
    [SerializeField] private float power = 27f; // controls score bar curve
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float barWidth = 14f;
    private float currScore, currDividerPos,
        targetScore, targetDividerPos = 0;

    [Header("Crowd")]
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 2.0f;
    [SerializeField] private float lerpSpeed = 5f;
    private float[] targetSpeeds;
    

    [Header("Pooling")]
    [SerializeField] private GameObject spritePopupPrefab;
    [SerializeField] private int popupPoolSize = 20;
    
    private Queue<GameObject> _popupPool = new Queue<GameObject>();
    private List<GameObject> _activePopups = new List<GameObject>();

    private bool[] _lowStamActive = new bool[2];
    public bool[] timerActive;
    public float[] timer;

    public Animator[] animators;
    public GameObject[] lowStam;
    public GameObject[] tutButts;
    public GameObject phaseChange;
    public RuntimeAnimatorController aButt;
    public RuntimeAnimatorController xButt;


    #region Init

    private void Start()
    {
        OnScoreChanged(1, 0); // initialize score display
        foreach (var anim in animators)
        {
            anim.speed = minSpeed;
            float randomOffset = Random.value;
            anim.Play(0, 0, randomOffset);
        }
        
        timerActive = new bool[] { false, false };
        timer = new float[] { 0f, 0f };
        if (InputManager.Instance.IsPlayer1UsingGamepad())
        {
            tutButts[0].GetComponent<Animator>().runtimeAnimatorController = aButt;
            tutButts[1].GetComponent<Animator>().runtimeAnimatorController = xButt;
        }
        else if (InputManager.Instance.IsPlayer1UsingGamepad()) 
        {
            tutButts[2].GetComponent<Animator>().runtimeAnimatorController = aButt;
            tutButts[3].GetComponent<Animator>().runtimeAnimatorController = xButt;
        }
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // singleton & pooling stuff
            return;
        }
        Instance = this;
        InitPopupPool();
        targetSpeeds = new float[animators.Length];
    }

    private void Update()
    {
        // p1
        p1CurrConfidence = Mathf.Lerp(p1CurrConfidence, p1TargetConfidence, Time.deltaTime * smoothSpeed);
        p1CurrAngle = Mathf.Lerp(p1CurrAngle, p1TargetAngle, Time.deltaTime * smoothSpeed);
        p1CurrStamina = Mathf.Lerp(p1CurrStamina, p1TargetStamina, Time.deltaTime * smoothSpeed);
        p1ConfidenceBar.fillAmount = p1CurrConfidence;
        p1ConfidenceDial.transform.localRotation = Quaternion.Euler(0, 0, p1CurrAngle);
        p1StaminaBar.fillAmount = p1CurrStamina;

        // p2
        p2CurrConfidence = Mathf.Lerp(p2CurrConfidence, p2TargetConfidence, Time.deltaTime * smoothSpeed);
        p2CurrAngle = Mathf.Lerp(p2CurrAngle, p2TargetAngle, Time.deltaTime * smoothSpeed);
        p2CurrStamina = Mathf.Lerp(p2CurrStamina, p2TargetStamina, Time.deltaTime * smoothSpeed);
        p2ConfidenceBar.fillAmount = p2CurrConfidence;
        p2ConfidenceDial.transform.localRotation = Quaternion.Euler(0, 0, p2CurrAngle);
        p2StaminaBar.fillAmount = p2CurrStamina;

        // score
        currScore = Mathf.Lerp(currScore, targetScore, Time.deltaTime * smoothSpeed);
        currDividerPos = Mathf.Lerp(currDividerPos, targetDividerPos, Time.deltaTime * smoothSpeed);
        ScoreBar.fillAmount = currScore;
        ScoreDivider.transform.localPosition = new Vector3(currDividerPos, ScoreDivider.transform.localPosition.y, ScoreDivider.transform.localPosition.z);

        // crowd
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].speed = Mathf.Lerp(
                animators[i].speed,
                targetSpeeds[i],
                Time.deltaTime * lerpSpeed
            );
        }
        
        /*if (_Inputmanager._p1Ability1.WasPerformedThisFrame())
        {
            UpdateAbilitySlot(0, 0, _Abilitymanager._playerAbilities[0, 0].abilSprite, false);
        }
        if (_Inputmanager._p2Ability1.WasPerformedThisFrame())
        {
            UpdateAbilitySlot(1, 0, _Abilitymanager._playerAbilities[1, 0].abilSprite, false);
        }
        if (_Inputmanager._p1Ability2.WasPerformedThisFrame())
        {
            UpdateAbilitySlot(0, 1, _Abilitymanager._playerAbilities[0, 1].abilSprite, false);
        }
        if (_Inputmanager._p2Ability2.WasPerformedThisFrame())
        {
            UpdateAbilitySlot(1, 1, _Abilitymanager._playerAbilities[1, 1].abilSprite, false);
        }
        foreach (var abilID in _Abilitymanager._active)
        {
            int playerID = abilID.Key / 10;
            //Debug.Log(playerID);
            if (timerActive[playerID])
            {
                timer[playerID] -= Time.deltaTime;
                Debug.Log(timer[playerID]);
                if (timer[playerID] <= 0f)
                {
                    if (abilID.Value.abilName == "Glitch")
                    {
                        timerActive[playerID] = false;
                        abilID.Value.OnExpire(playerID, _Abilitymanager._ctx);
                    }
                }
            }
        }*/
    }

    private void OnEnable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnHitRegistered += OnHitRegistered;
            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
        }
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnHitRegistered -= OnHitRegistered;
            ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
        }
    }

    private void InitPopupPool()
    {
        for (int i = 0; i < popupPoolSize; i++)
        {
            GameObject popup = Instantiate(spritePopupPrefab, transform);
            popup.SetActive(false);
            _popupPool.Enqueue(popup);
        }
    }

    #endregion Init

    #region Events

    private void OnHitRegistered(int playerID, HitAccuracy accuracy) // accuracy popups
    {
        UIManager uI = Instance;
        PhaseHandler phaseHandler = PhaseHandler.Instance;
        if (phaseHandler.tutorial)
        {
            foreach (GameObject obj in uI.tutButts)
            {
                obj.SetActive(false);
            }
            phaseHandler.tutorial = false;
        }
        Transform spawnPoint = playerID == 0 ? p1FeedbackSpawn : p2FeedbackSpawn;
        Sprite feedbackSprite = accuracy switch
        {
            HitAccuracy.Perfect => perfectSprite,
            HitAccuracy.Good => goodSprite,
            HitAccuracy.Okay => okaySprite,
            _ => missSprite
        };
        ShowSpritePopup(feedbackSprite, spawnPoint.position, feedbackDisplayDuration);
    }

    private void OnScoreChanged(int playerID, int newScore)
    {
        int difference = ScoreManager.Instance.GetScore(0) - ScoreManager.Instance.GetScore(1);

        float normalised = ((difference + maxScore) / (maxScore * 2f)) * 2f - 1f;
        float curve = Mathf.Sign(normalised) * (1 - Mathf.Pow(1 - Mathf.Abs(normalised), power));

        targetScore = Mathf.Clamp01((1f + curve) / 2f);
        targetDividerPos = (targetScore * barWidth) - (barWidth / 2f);

        CrowdLove(targetScore);
    }

    /*private void CrowdLove(float amm) clunk
    {
        if (amm < 0.5)
        {
            animators[0].speed += Time.deltaTime * 1;
            animators[1].speed += Time.deltaTime * 1;
            animators[2].speed += Time.deltaTime * 1;
            animators[3].speed += Time.deltaTime * 1;
            animators[4].speed += Time.deltaTime * 10;
            animators[5].speed += Time.deltaTime * 10;
            animators[6].speed += Time.deltaTime * 10;
            animators[7].speed += Time.deltaTime * 10;
        }
        else 
        {
            animators[0].speed += Time.deltaTime * 10;
            animators[1].speed += Time.deltaTime * 10;
            animators[2].speed += Time.deltaTime * 10;
            animators[3].speed += Time.deltaTime * 10;
            animators[4].speed += Time.deltaTime * 1;
            animators[5].speed += Time.deltaTime * 1;
            animators[6].speed += Time.deltaTime * 1;
            animators[7].speed += Time.deltaTime * 1;
        }
    }*/
    
    private void CrowdLove(float amm)
    {
        bool leftWinning = amm > 0.5f;
        float intensity = Mathf.Abs(amm - 0.5f) * 2f;

        float boostedSpeed = Mathf.Lerp(minSpeed, maxSpeed, intensity);

        for (int i = 0; i < animators.Length; i++)
        {
            bool isLeftSide = i < animators.Length / 2;
            targetSpeeds[i] = (isLeftSide == leftWinning) ? boostedSpeed : minSpeed;
        }
    }
    
    #endregion Events

    #region Popup System

    private void ShowSpritePopup(Sprite sprite, Vector3 position, float duration) // spawn & fade-out popup
    {
        if (sprite == null) return;
        GameObject popup;
        if (_popupPool.Count > 0) { popup = _popupPool.Dequeue(); _activePopups.Add(popup); }
        else if (_activePopups.Count > 0) { popup = _activePopups[0]; _activePopups.RemoveAt(0); _activePopups.Add(popup); }
        else return;
        popup.transform.position = position;
        popup.SetActive(true);
        
        SpriteRenderer sr = popup.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = sprite;
        }
        
        StartCoroutine(FadeOutPopup(popup, sr, duration));
    }

    private IEnumerator FadeOutPopup(GameObject popup, SpriteRenderer sr, float duration)
    {
        float time = 0f;
        Color startColor = sr.color;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, time / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            
            float scale = Mathf.Lerp(1f, 1.5f, time / duration);
            popup.transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        popup.SetActive(false);
        popup.transform.localScale = Vector3.one;
        sr.color = startColor;
        
        _activePopups.Remove(popup);
        _popupPool.Enqueue(popup);
    }

    #endregion Popup System

    #region Stamina

    public void UpdateStamina(int playerID, float currentStamina, float maxStamina) // bar update & colour
    {
        float fill = Mathf.Clamp01(currentStamina / maxStamina);
        
        if (playerID == 0)
        {
            p1TargetStamina = fill;
        }
        else if (playerID == 1)
        {
            p2TargetStamina = fill;
        }
        if (currentStamina < 20f)
        {
            if (!_lowStamActive[playerID])
            {
                _lowStamActive[playerID] = true;
                lowStam[playerID].SetActive(true);
                AudioManager.Instance?.PlayLowStamina();
            }
        }
        else
        {
            _lowStamActive[playerID] = false;
            lowStam[playerID].SetActive(false);
        }
    }

    #endregion Stamina

    #region Confidence

    public void UpdateConfidence(int playerID, float currentConfidence, float maxConfidence = 1000f) // bar update & colour
    {
        float fill = Mathf.Clamp01(currentConfidence / maxConfidence);

        if (playerID == 0)
        {
            p1TargetConfidence = fill;
            p1TargetAngle = Mathf.Lerp(90, -90, fill) + p1DialOffset;
        }
        else if (playerID == 1)
        {
            p2TargetConfidence = fill;
            p2TargetAngle = Mathf.Lerp(90, -90, fill) + p2DialOffset;
        }
    }
    #endregion Confidence

    #region Abilities

    public void SetGlitchOverlay(int playerID, bool active)
    {
        //GameObject overlay = playerID == 0 ? p1GlitchOverlay : p2GlitchOverlay;
        //if (overlay != null) overlay.SetActive(active);
        //int opponentID = playerID == 0 ? 1 : 0;
        //if (!timerActive[opponentID] && active)
        //{
        //    timerActive[opponentID] = true;
        //    timer[opponentID] = 5f;
        //}
        //// additional logic (playing mp4's / sfx etc.) can go here
    }
    
    public void UpdateAbilitySlot(int playerID, int slotIndex, Sprite abilitySprite, bool isAvailable) // to imp
    {
        //SpriteRenderer[] abilitySlots = playerID == 0 ? p1AbilitySlots : p2AbilitySlots;
        
        //if (slotIndex < 0 || slotIndex >= abilitySlots.Length) return;
        
        //SpriteRenderer slot = abilitySlots[slotIndex];
        //if (slot == null) return;
        
        //slot.sprite = abilitySprite;
        //slot.color = isAvailable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    #endregion Abilities
}