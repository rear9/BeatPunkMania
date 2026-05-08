using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Handles result screen button functions & illustration switching dependent on which player wins along with displaying stats from ScoreManager
/// </summary>

public class ScorePanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Selectable firstSelected;

    [Header("Player 1 Stats")]
    [SerializeField] private TextMeshProUGUI p1PerfectText;
    [SerializeField] private TextMeshProUGUI p1GoodText;
    [SerializeField] private TextMeshProUGUI p1OkayText;
    [SerializeField] private TextMeshProUGUI p1MissText;
    [SerializeField] private TextMeshProUGUI p1AccuracyText;

    [Header("Player 2 Stats")]
    [SerializeField] private TextMeshProUGUI p2PerfectText;
    [SerializeField] private TextMeshProUGUI p2GoodText;
    [SerializeField] private TextMeshProUGUI p2OkayText;
    [SerializeField] private TextMeshProUGUI p2MissText;
    [SerializeField] private TextMeshProUGUI p2AccuracyText;
    
    [Header("Winner Illustration")]
    [SerializeField] private GameObject winnerIllustration;
    [SerializeField] private Sprite alexWin;
    [SerializeField] private Sprite lydiaWin;

    [Header("Timing")]
    [SerializeField] private float countDuration = 0.8f;
    [SerializeField] private float delayBetweenStats = 0.15f;

    #region Lifecycle

    private void Start()
    {
        panel.SetActive(false);
        if (PhaseHandler.Instance != null) PhaseHandler.Instance.OnPhaseExpired += OnPhaseExpired; // use phase expiration to know when to start scoring
    }

    private void OnDestroy()
    {
        if (PhaseHandler.Instance != null) PhaseHandler.Instance.OnPhaseExpired -= OnPhaseExpired;
    }

    #endregion Lifecycle

    #region Display

    private void OnPhaseExpired()
    { 
        if (PhaseHandler.Instance.GetCurrentRound() < 3) return; // start scoring after 3 rounds (will probably change)
        StartScoring();
    }

    public void StartScoring()
    {
        if (ScoreManager.Instance == null) return;

        panel.SetActive(true);
        InputManager.Instance?.SetInputEnabled(false);
        AudioManager.Instance?.PlayGameOver();
        AudioManager.Instance?.PlayAudienceCheer();
        StartCoroutine(SetFirstSelectedNextFrame());

        ShowWinner(); // set the winning illustration and start the sequence
        StartCoroutine(ScoringSequence());
    }

    private IEnumerator SetFirstSelectedNextFrame()
    {
        yield return null;
        bool gamepadActive = (InputManager.Instance?.GetControllerType(0) != ControllerType.Keyboard) ||
                             (InputManager.Instance?.GetControllerType(1) != ControllerType.Keyboard);
        if (firstSelected != null && gamepadActive)
            EventSystem.current?.SetSelectedGameObject(firstSelected.gameObject);
        else
            EventSystem.current?.SetSelectedGameObject(null);
    }

    private void ShowWinner()
    {
        int p1Score = ScoreManager.Instance.GetScore(0);
        int p2Score = ScoreManager.Instance.GetScore(1);
        winnerIllustration.gameObject.SetActive(true);
        if (p1Score > p2Score)
        {
            winnerIllustration.GetComponent<Image>().sprite = alexWin;
        }
        else
        {
            winnerIllustration.GetComponent<Image>().sprite = lydiaWin;
        }
    }

    private IEnumerator ScoringSequence()
    {
        ScoreManager sm = ScoreManager.Instance;

        yield return LerpUp(p1PerfectText, p2PerfectText, sm.GetPerfectHits(0), sm.GetPerfectHits(1), false); // run lerps on all the accuracy data in sequence
        yield return new WaitForSeconds(delayBetweenStats);
        yield return LerpUp(p1GoodText, p2GoodText, sm.GetGoodHits(0), sm.GetGoodHits(1), false);
        yield return new WaitForSeconds(delayBetweenStats);
        yield return LerpUp(p1OkayText, p2OkayText, sm.GetOkayHits(0), sm.GetOkayHits(1), false);
        yield return new WaitForSeconds(delayBetweenStats);
        yield return LerpUp(p1MissText, p2MissText, sm.GetMissedHits(0), sm.GetMissedHits(1), false);
        yield return new WaitForSeconds(delayBetweenStats);
        yield return LerpUp(p1AccuracyText, p2AccuracyText, sm.GetAccuracyPercentage(0), sm.GetAccuracyPercentage(1), true);
    }

    private IEnumerator LerpUp(TextMeshProUGUI left, TextMeshProUGUI right, float targetA, float targetB, bool isFloat) // lerp func using ease out cubic
    {
        float elapsed = 0f;
        while (elapsed < countDuration)
        {
            elapsed += Time.deltaTime;
            float t     = Mathf.Clamp01(elapsed / countDuration);
            float eased = OutCubic(t);
            left.text  = FormatValue(eased * targetA, isFloat);
            right.text = FormatValue(eased * targetB, isFloat);
            yield return null;
        }

        left.text  = FormatValue(targetA, isFloat);
        right.text = FormatValue(targetB, isFloat);
    }

    private string FormatValue(float value, bool isFloat) => isFloat ? $"{value:F1}%" : Mathf.FloorToInt(value) + "x"; // only formats accuracy with %
    private float OutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f); // 1 - (1 - x)^3 where x is time

    #endregion Display

    #region Buttons

    public void OnReturn() => StartCoroutine(ReturnSequence());
    public void OnReplay() => StartCoroutine(ReplaySequence());
    public void OnExit()   => StartCoroutine(ExitSequence());

    private IEnumerator ReturnSequence()
    {
        AudioManager.Instance?.StopMusic(fadeOut: true, fadeDuration: 1f);
        if (TransitionHandler.Instance != null) yield return StartCoroutine(TransitionHandler.Instance.FadeOut(1f));
        InputManager.Instance?.SetInputEnabled(true);
        SceneManager.LoadScene("Menu");
    }

    private IEnumerator ReplaySequence()
    {
        AudioManager.Instance?.StopMusic(fadeOut: true, fadeDuration: 1f);
        if (TransitionHandler.Instance != null) yield return StartCoroutine(TransitionHandler.Instance.FadeOut(1f));
        InputManager.Instance?.SetInputEnabled(true);
        SceneManager.LoadScene("PlayArea");
    }

    private IEnumerator ExitSequence()
    {
        AudioManager.Instance?.StopMusic(fadeOut: true, fadeDuration: 1f);
        if (TransitionHandler.Instance != null) yield return StartCoroutine(TransitionHandler.Instance.FadeOut(1f));
        Application.Quit();
    }

    #endregion Buttons
}