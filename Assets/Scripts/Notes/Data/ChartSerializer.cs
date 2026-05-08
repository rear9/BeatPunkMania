using System.IO;
using UnityEngine;

/// <summary>
/// Helper script that loads chart data and ensures all file paths are correctly formatted to be saved in the computer's file system.
/// </summary>

public static class ChartSerializer
{
    // file naming convention
    private const string FILE_PREFIX = "chart_";
    private const string FILE_EXTENSION = ".json";
    
    #region Save/Load

    public static bool SaveChart(ChartData chart)
    {
        if (chart == null) return false;
        if (chart.notes == null || chart.notes.Count == 0)
        {
            Debug.LogWarning($"chart '{chart.chartName}' is empty");
            return false;
        }
        
        try
        {
            string path = GetChartPath(chart.playerTag, chart.chartName);
            string json = JsonUtility.ToJson(chart, prettyPrint: true);
            
            // ensure directory exists
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory ?? string.Empty);
            }
            
            File.WriteAllText(path, json);
            
            Debug.Log($"saved chart '{chart.chartName}' / ({chart.notes.Count} notes / {chart.bpm} BPM) -> {path}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"failed to save chart '{chart.chartName}' / {e.Message}");
            return false;
        }
    }
    
    public static ChartData LoadChart(string playerTag, string chartName) // load chart and return data
    {
        try
        {
            string path = GetChartPath(playerTag, chartName);
            
            if (!File.Exists(path))
            {
                Debug.LogWarning($"chart file not found at {path}");
                return null;
            }
            
            string json = File.ReadAllText(path);
            ChartData chart = JsonUtility.FromJson<ChartData>(json);
            
            if (chart?.notes == null || chart.notes.Count == 0)
            {
                Debug.LogError($"loaded chart is invalid or empty");
                return null;
            }
            
            Debug.Log($"loaded chart'{chartName}' / {playerTag} / ({chart.notes.Count} notes, / {chart.bpm} BPM)");
            return chart;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"failed to load chart for {playerTag} / {e.Message}");
            return null;
        }
    }

    public static ChartData LoadLatestChart(string playerTag) // load most recent chart for player (convenience helper)
    {
        try
        {
            string directory = GetChartsDirectory();
            
            if (!Directory.Exists(directory))
            {
                Debug.LogWarning($"charts directory doesn't exist");
                return null;
            }
            
            // get all chart files for player
            string[] files = Directory.GetFiles(directory, $"{FILE_PREFIX}{playerTag}_*{FILE_EXTENSION}");
            
            if (files.Length == 0)
            {
                Debug.LogWarning($"no charts found for {playerTag}");
                return null;
            }
            
            // get most recent file
            string latestFile = files[0];
            System.DateTime latestTime = File.GetLastWriteTime(latestFile);
            
            for (int i = 1; i < files.Length; i++)
            {
                System.DateTime fileTime = File.GetLastWriteTime(files[i]);
                if (fileTime > latestTime)
                {
                    latestTime = fileTime;
                    latestFile = files[i];
                }
            }
            
            // load it
            string json = File.ReadAllText(latestFile);
            ChartData chart = JsonUtility.FromJson<ChartData>(json);
            
            Debug.Log($"loaded latest chart for {playerTag} from {latestFile}");
            return chart;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"failed to load latest chart for {playerTag} / {e.Message}");
            return null;
        }
    }
    
    #endregion Save/Load
    
    #region File Management
    
    public static bool ChartExists(string playerTag, string chartName) // chart check (debugging)
    {
        string path = GetChartPath(playerTag, chartName);
        return File.Exists(path);
    }

    public static void DeleteAllCharts() // del all (cleanup)
    {
        try
        {
            string directory = GetChartsDirectory();
            
            if (Directory.Exists(directory))
            {
                string[] files = Directory.GetFiles(directory, $"{FILE_PREFIX}*{FILE_EXTENSION}");
                
                foreach (string file in files)
                {
                    File.Delete(file);
                }
                Debug.Log($"deleted {files.Length} chart files");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"failed to delete all charts: {e.Message}");
        }
    }
    
    #endregion File Management
    
    #region Path Helpers
    
    private static string GetChartPath(string playerTag, string chartName) // clean & return chart name for file (removes invalid chars)
    {
        string directory = GetChartsDirectory();
        string safeChartName = chartName.Replace(" ", "_").Replace(":", "-");
        
        string filename = $"{FILE_PREFIX}{playerTag}_{safeChartName}{FILE_EXTENSION}";
        return Path.Combine(directory, filename);
    }
    
    public static string GetChartsDirectory() => Path.Combine(Application.persistentDataPath, "Charts");
    
    
    #endregion Path Helpers
    
    public static void DebugPrintChart(ChartData chart) // prints chart
    {
        if (chart == null) return;
        Debug.Log($"=== chart: {chart.chartName} ===");
        Debug.Log($"plr: {chart.playerTag}");
        Debug.Log($"bpm: {chart.bpm}");
        Debug.Log($"total: {chart.notes.Count}");
        
        int tapCount = 0;
        int holdCount = 0;
        foreach (var note in chart.notes) // count each type
        {
            if (note.type == NoteType.Tap) tapCount++;
            else holdCount++;
        }
        Debug.Log($"taps: {tapCount} / holds : {holdCount}");
        
        if (chart.notes != null && chart.notes.Count > 0) // print each note
        {
            Debug.Log("--- notes ---");
            for (int i = 0; i < chart.notes.Count; i++)
            {
                var note = chart.notes[i];
                string noteInfo = $"[{i}] beat: {note.beatIndex} / type: {note.type} / lane: {note.lane}";
                if (note.type == NoteType.Hold) noteInfo += $", dur: {note.holdDurationBeats} beats";
                Debug.Log(noteInfo);
            }
        }
    }
}