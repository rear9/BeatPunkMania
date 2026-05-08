using UnityEngine;

/// <summary>
/// Scriptable object to edit note colours and pick them by putting one of these into NotePoolManager
/// </summary>

[CreateAssetMenu(fileName = "NoteColorConfig", menuName = "BeatPunkMania/Note Color Config")]
public class NoteColorConfig : ScriptableObject
{
    [System.Serializable]
    public class LaneColors
    {
        [Header("Traveling")]
        [Tooltip("Base color while the note is traveling toward the hit zone")]
        public Color normalColor = Color.white;

        [Tooltip("Color to glow toward as the note approaches the hit zone (lerp target)")]
        public Color approachGlowColor = new(1f, 1f, 0.4f);

        [Header("Tap Note — Hit Zone")]
        [Tooltip("Tap note color once it is very close to the hit zone (visual only — independent of gameplay window size)")]
        public Color hitZoneColor = Color.yellow;

        [Tooltip("Brief flash color when a tap note is successfully hit")]
        public Color hitFlashColor = Color.green;

        [Header("Hold Note")]
        [Tooltip("Hold note body (middle segment) color — kept distinct from head/tail")]
        public Color bodyColor = new(1f, 1f, 1f, 0.4f);

        [Tooltip("Hold note head and tail color while the player is holding the input")]
        public Color holdingColor = Color.cyan;
    }

    [SerializeField] private LaneColors[] lanes = new LaneColors[4];
    
    public LaneColors GetLane(int lane) => lanes[Mathf.Clamp(lane, 0, 3)];

    private void OnValidate()
    {
        if (lanes is not { Length: 4 })
        {
            System.Array.Resize(ref lanes, 4);
            for (int i = 0; i < 4; i++)
            {
                lanes[i] ??= new LaneColors();
            }
        }
    }
}