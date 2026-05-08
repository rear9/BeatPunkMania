using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Input/InputIconLib")]
public class InputIconLib : ScriptableObject // scriptableobj to prevent losing inspector assigned values
{
    [System.Serializable]
    public class IconEntry
    {
        public string controlPath;
        public Sprite icon;
    }

    [Header("Xbox Icons")]
    public List<IconEntry> xboxIcons = new();

    [Header("PlayStation Icons")]
    public List<IconEntry> playstationIcons = new();

    [Header("Keyboard Icons")]
    public List<IconEntry> keyboardIcons = new();

    [Header("Generic Gamepad Icons")]
    public List<IconEntry> genericIcons = new();

    [Header("Fallback")]
    public Sprite fallbackIcon;

    private Dictionary<string, Sprite> _xboxLookup;
    private Dictionary<string, Sprite> _playstationLookup;
    private Dictionary<string, Sprite> _keyboardLookup;
    private Dictionary<string, Sprite> _genericLookup;

    private void OnEnable() => BuildLookups();

    private void BuildLookups() // building libraries using a Dict
    {
        _xboxLookup        = BuildLookup(xboxIcons);
        _playstationLookup = BuildLookup(playstationIcons);
        _keyboardLookup    = BuildLookup(keyboardIcons);
        _genericLookup     = BuildLookup(genericIcons);
    }

    private Dictionary<string, Sprite> BuildLookup(List<IconEntry> entries)
    {
        var dict = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries.Where(entry => !string.IsNullOrEmpty(entry.controlPath) && entry.icon != null))
        {
            dict[entry.controlPath] = entry.icon;
        }
        return dict;
    }

    public Sprite GetIcon(ControllerType deviceType, string controlPath) // called from scripts that need the icon
    {
        if (_xboxLookup == null) BuildLookups();

        Dictionary<string, Sprite> lookup = deviceType switch
        {
            ControllerType.Xbox        => _xboxLookup,
            ControllerType.PlayStation => _playstationLookup,
            ControllerType.Generic     => _genericLookup,
            _                          => _keyboardLookup
        };

        var key = StripDevicePrefix(controlPath);
        return lookup.GetValueOrDefault(key, fallbackIcon);
    }

    private string StripDevicePrefix(string path) // creates digestible input system syntax for each key
    {
        int slash = path.LastIndexOf('/');
        return slash >= 0 ? path[(slash + 1)..] : path;
    }
}