using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Helper script re-used from old project - holds functions to lerp the alpha of a black UI panel
/// </summary>

public class TransitionHandler : MonoBehaviour
{
    public static TransitionHandler Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float defaultDuration = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 1);
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        yield return Fade(0f, 1f, duration < 0 ? defaultDuration : duration);
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        yield return Fade(1f, 0f, duration < 0 ? defaultDuration : duration);
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        fadeImage.gameObject.SetActive(true);
        float t = 0f;
        Color c = fadeImage.color;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fadeImage.color = new Color(c.r, c.g, c.b, Mathf.Lerp(start, end, t / duration));
            yield return null;
        }
        fadeImage.color = new Color(c.r, c.g, c.b, end);
        if (end == 0f) fadeImage.gameObject.SetActive(false);
    }
}