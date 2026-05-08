using System.Collections;
using UnityEngine;

/// <summary>
/// Controls game state, handles getting scripts / assets ready for play
/// </summary>

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private float loadingDuration = 1f;
    
    [Header("Player Refs")]
    [SerializeField] private PlayerController player1;
    [SerializeField] private PlayerController player2;
    
    [Header("Manager Refs")]
    [SerializeField] private PhaseHandler phaseManager;
    
    [Header("Countdown UI")]
    [SerializeField] private SpriteRenderer countdownSprite;
    [SerializeField] private Sprite countdown3Sprite;
    [SerializeField] private Sprite countdown2Sprite;
    [SerializeField] private Sprite countdown1Sprite;
    [SerializeField] private Sprite countdownGoSprite;

    private bool _gameHasStarted = false;
    public bool GameHasStarted => _gameHasStarted;

    #region Init

    private void Awake() // singleton
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ChartSerializer.DeleteAllCharts(); // wipe charts just in case
        StartCoroutine(GameStartSequence());
    }

    #endregion Init

    #region Game Flow

    private IEnumerator GameStartSequence()
    {
        PreWarmAudio(); // play all the audio files to ensure they're always loaded at start of play
        yield return new WaitForSecondsRealtime(loadingDuration);

        if (TransitionHandler.Instance != null)
            yield return StartCoroutine(TransitionHandler.Instance.FadeIn());

        AudioManager.Instance.PlayMusic(AudioManager.Instance.roundMusic, loop: true, fadeIn: true, fadeDuration: 0.5f);
        BeatManager.Instance.StartTracking();
        yield return StartCoroutine(ToOneCountdown());
        StartCoroutine(ShowGo());
        StartGame();
    }
    private void PreWarmAudio()
    {
        float savedVolume = AudioManager.Instance.masterVolume;
        AudioManager.Instance.masterVolume = 0f;
        
        // force JIT compilation
        AudioManager.Instance.PlayCountdownTick();
        AudioManager.Instance.PlayCountdownGo();
        AudioManager.Instance.PlayHitPerfect();
        AudioManager.Instance.PlayHitGood();
        AudioManager.Instance.PlayHitOkay();
        AudioManager.Instance.PlayNoteMiss();
        AudioManager.Instance.PlayRecordNote();
        AudioManager.Instance.PlayRecordHold();
        AudioManager.Instance.PlayPhaseTransition();
        
        AudioManager.Instance.masterVolume = savedVolume;
        
        // immediately stop any sources that started playing
        AudioManager.Instance.StopAllSFX();
    }

    private IEnumerator ToOneCountdown()
    {
        // music + beat tracking started
        // exactly 8 beats (2 bars) from music start to gameplay
        countdownSprite.sprite = countdown3Sprite;
        countdownSprite.gameObject.SetActive(true);
        AudioManager.Instance.PlayCountdownTick();
        yield return WaitForNextBeat();
        yield return WaitForNextBeat();
        
        countdownSprite.sprite = countdown2Sprite;
        AudioManager.Instance.PlayCountdownTick();
        yield return WaitForNextBeat();
        yield return WaitForNextBeat();
        
        countdownSprite.sprite = countdown1Sprite;
        AudioManager.Instance.PlayCountdownTick();
        yield return WaitForNextBeat();
        yield return WaitForNextBeat();
    }

    private IEnumerator ShowGo()
    {
        countdownSprite.sprite = countdownGoSprite;
        AudioManager.Instance.PlayCountdownGo();
        yield return WaitForNextBeat();
        yield return WaitForNextBeat();
        countdownSprite.gameObject.SetActive(false);
    }

    private IEnumerator WaitForNextBeat()
    {
        int currentBeat = BeatManager.Instance.CurrentBeat;
        int currentBar  = BeatManager.Instance.CurrentBar;
        
        while (true)
        {
            int newBeat = BeatManager.Instance.CurrentBeat;
            int newBar  = BeatManager.Instance.CurrentBar;
            if (newBeat != currentBeat || newBar != currentBar) break;
            yield return null;
        }
    }

    private void StartGame()
    {
        _gameHasStarted = true;
        if (phaseManager != null) phaseManager.enabled = true;
        
        if (player1 != null) player1.SetActive(true);
        if (player2 != null) player2.SetActive(true);
        
        Debug.Log($"Game Start / BPM: {AudioManager.Instance.BPM} / Note Speed: {AudioManager.Instance.NoteSpeed:F2}");
    }

    #endregion Game Flow
    
    public PlayerController GetPlayer(int playerID)
    {
        return playerID == 1 ? player1 : player2;
    }
}