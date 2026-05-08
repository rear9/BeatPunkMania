using UnityEngine;

public class BeatSyncVisualizer : MonoBehaviour // redundant
{
    [SerializeField] private SpriteRenderer pulseSprite;
    [SerializeField] private float pulseScale = 1.2f;
    
    private void Update()
    {
        if (AudioManager.Instance.IsMusicPlaying())
        {
            float beatProgress = AudioManager.Instance.GetBeatProgress();
            
            // do things during a beat (scaling example)
            float scale = Mathf.Lerp(1f, pulseScale, 1f - beatProgress);
            pulseSprite.transform.localScale = Vector3.one * scale;
            
            // or check to do things if on beat
            if (AudioManager.Instance.IsOnBeat(tolerance: 0.05f))
            {
                // flash effect, spawn vfx and things etc.
            }
        }
    }
}
