using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Faux note hit VFX done by scaling, colouring and fading sprites
/// </summary>

public class HitEffectManager : MonoBehaviour
{
    public static HitEffectManager Instance { get; private set; }
    
    [Header("Effect Settings")]
    [SerializeField] private GameObject hitEffectLeftPrefab;
    [SerializeField] private GameObject hitEffectDownPrefab;
    [SerializeField] private int poolSize = 20;
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private float scaleMultiplier = 2f;
    [SerializeField] private float baseScale = 1f;
    
    [Header("Colors")]
    [SerializeField] private Color perfectColor = new Color(0f, 0.6f, 0f, 1f);
    [SerializeField] private Color goodColor = new Color(0.5f, 1f, 0.5f, 1f);
    [SerializeField] private Color okayColor = Color.yellow;
    [SerializeField] private Color missColor = Color.red;
    
    private Queue<GameObject> _effectPoolL = new Queue<GameObject>();
    private Queue<GameObject> _effectPoolD = new Queue<GameObject>();
    private List<GameObject> _activeEffectsL = new List<GameObject>();
    private List<GameObject> _activeEffectsD = new List<GameObject>();
    private Transform _poolLParent, _poolDParent;

    #region Init

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitPools();
    }

    private void InitPools() // more obj pooling
    {
        _poolLParent = new GameObject("HitEffectLeft_Pool").transform;
        _poolLParent.SetParent(transform);
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject effect = Instantiate(hitEffectLeftPrefab, _poolLParent);
            effect.SetActive(false);
            _effectPoolL.Enqueue(effect);
        }
        
        _poolDParent = new GameObject("HitEffectDown_Pool").transform;
        _poolDParent.SetParent(transform);
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject effect = Instantiate(hitEffectDownPrefab, _poolDParent);
            effect.SetActive(false);
            _effectPoolD.Enqueue(effect);
        }
    }

    #endregion Init

    #region Spawning

    public void SpawnHitEffect(HitAccuracy accuracy, Vector3 position, int lane)
    {
        Debug.Log(lane);
        GameObject effect = null;
        if (lane % 2 == 0)
        {
            effect = _effectPoolL.Count > 0 ? _effectPoolL.Dequeue() : Instantiate(hitEffectLeftPrefab, _poolLParent);
            _activeEffectsL.Add(effect);
        }
        else
        {
            effect = _effectPoolD.Count > 0 ? _effectPoolD.Dequeue() : Instantiate(hitEffectDownPrefab, _poolDParent);
            _activeEffectsD.Add(effect);
        }
        
        if (effect == null) return;
        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * baseScale;
        effect.SetActive(true);
        var sr = effect.GetComponent<SpriteRenderer>();
        
        if (sr != null)
        {
            sr.color = accuracy switch
            {
                HitAccuracy.Perfect => perfectColor,
                HitAccuracy.Good => goodColor,
                HitAccuracy.Okay => okayColor,
                HitAccuracy.Miss => missColor,
                _ => Color.white
            };
    
        StartCoroutine(AnimateEffect(effect, sr));
        }
    }
    
    public void SpawnHitEffectForNote(HitAccuracy accuracy, NoteCore note)
    {
        if (note == null) return;
        float noteY = note.GetHitPositionY();
        Vector3 position = new Vector3(note.transform.position.x, noteY, 0f);
        SpawnHitEffect(accuracy, position, note.GetLane());
    }
    
    public void SpawnMissEffect(int lane)
    {
        if (GameBoundaries.Instance == null) return;
        float hitZoneY = GameBoundaries.Instance.HitZoneY;
        float laneX = GameBoundaries.Instance.GetLaneX(lane);
        Vector3 position = new Vector3(laneX, hitZoneY, 0f);
        SpawnHitEffect(HitAccuracy.Miss, position, lane);
    }

    #endregion Spawning

    #region Animation

    private IEnumerator AnimateEffect(GameObject effect, SpriteRenderer sr) // a bunch of lerping
    {
        float timer = 0f;
        Vector3 startScale = Vector3.one * baseScale;
        Vector3 endScale = Vector3.one * (baseScale * scaleMultiplier);
        Color startColor = sr != null ? sr.color : Color.white;
        
        while (timer < effectDuration)
        {
            timer += Time.deltaTime;
            float t = timer / effectDuration;
            
            effect.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            if (sr != null)
            {
                Color color = startColor;
                color.a = Mathf.Lerp(1f, 0f, t);
                sr.color = color;
            }
            
            yield return null;
        }
        
        ReturnEffectToPool(effect); // return once animation finishes
    }

    #endregion Animation

    #region Pool

    private void ReturnEffectToPool(GameObject effect)
    {
        if (effect == null) return;
        effect.SetActive(false);
        if (_activeEffectsL.Contains(effect))
        {
            effect.transform.SetParent(_poolLParent);
            _activeEffectsL.Remove(effect);
            _effectPoolL.Enqueue(effect);
        }
        else
        {
            effect.transform.SetParent(_poolDParent);
            _activeEffectsD.Remove(effect);
            _effectPoolD.Enqueue(effect);
            
        }
        effect.transform.localScale = Vector3.one * baseScale;
        effect.transform.rotation = Quaternion.identity;
    }

    #endregion Pool
}