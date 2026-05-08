//using UnityEngine;
//using System.IO;
//using System.Collections.Generic;

//public struct NoteSpawnData
//{
//    public int beat;
//    public NoteType type;
//    public bool isLeftKey;
//}

//public class NoteReaderScript : MonoBehaviour
//{
//    string chartPath;
//    List<NoteSpawnData> chartNotes = new List<NoteSpawnData>();
//    [SerializeField] string playerNum;

//    void Start()
//    {
//        // get path
//        chartPath = Path.Combine(Application.persistentDataPath, "chart_" + playerNum + ".txt");
//        if (!File.Exists(chartPath))
//        {
//            Debug.LogError("Chart unavailable");
//            return;
//        }

//        // extract data
//        string[] lines = File.ReadAllLines(chartPath);
//        foreach (string line in lines)
//        {
//            string[] parts = line.Split(',');

//            // save data
//            NoteSpawnData note = new NoteSpawnData
//            {
//                beat = int.Parse(parts[0]),
//                type = (NoteType)int.Parse(parts[1]),
//                isLeftKey = parts[2] == "L"
//            };
//            chartNotes.Add(note);
//        }

//        Debug.Log("Loaded " + chartNotes.Count + " notes.");
//    }

//    void Update()
//    {

//    }
//}
