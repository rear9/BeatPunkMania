using System.Collections;
using UnityEngine;

/// <summary>
/// Handles changing colour through lerps for background light materials on phase changes
/// </summary>

public class PhaseLightingHandler : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material emLightsMaterial;

    [Header("Colours")]
    [ColorUsage(true, true)] [SerializeField] private Color attackColour;
    [ColorUsage(true, true)] [SerializeField] private Color defendColour;
    [ColorUsage(true, true)] [SerializeField] private Color transitionColour;

    [Header("Lerp")]
    [SerializeField] private float lerpDuration = 0.5f;

    [Header("Refs")]
    [SerializeField] private PhaseHandler phaseHandler;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor"); // property getter
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    
    private Coroutine _lerpCoroutine; // optional vars
    private Color _originalEmission;
    private Color _originalBase;

    private void Start()
    {
        if (emLightsMaterial != null)
        {
            emLightsMaterial.EnableKeyword("_EMISSION"); // make sure lighting is enabled & set original colour
            _originalEmission = emLightsMaterial.GetColor(EmissionColor);
            _originalBase = emLightsMaterial.GetColor(BaseColor);
        }
        if (phaseHandler != null) phaseHandler.OnPhaseChanged.AddListener(OnPhaseChanged);
    }

    private void OnDestroy() // reset everything on destroy
    {
        if (phaseHandler != null)
            phaseHandler.OnPhaseChanged.RemoveListener(OnPhaseChanged);

        if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
        if (emLightsMaterial != null)
        {
            emLightsMaterial.SetColor(EmissionColor, _originalEmission);
            emLightsMaterial.SetColor(BaseColor, _originalBase);
        }
    }

    private void OnPhaseChanged(PhaseHandler.Phase phase)
    {
        Color target = phase switch
        {
            PhaseHandler.Phase.Attack => attackColour,
            PhaseHandler.Phase.Defend => defendColour,
            _ => transitionColour
        };
        
        if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine); // to ensure no overlap
        _lerpCoroutine = StartCoroutine(LerpEmission(target));
    }

    private IEnumerator LerpEmission(Color target)
    {
        Color emissionStart = emLightsMaterial.GetColor(EmissionColor);
        Color baseStart = emLightsMaterial.GetColor(BaseColor);
        float elapsed = 0f;

        while (elapsed < lerpDuration) // dt lerp
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lerpDuration;
            emLightsMaterial.SetColor(EmissionColor, Color.Lerp(emissionStart, target, t));
            emLightsMaterial.SetColor(BaseColor, Color.Lerp(baseStart, target, t));
            yield return null;
        }
        emLightsMaterial.SetColor(EmissionColor, target);
        emLightsMaterial.SetColor(BaseColor, target);
        _lerpCoroutine = null;
    }
}