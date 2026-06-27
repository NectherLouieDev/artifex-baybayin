using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [SerializeField] private SaveData _saveDataAsset;
    [SerializeField] private string _saveFileName = "bombercook_save.dat";
    [SerializeField] private bool _debugMode = true;

    private Dictionary<ELevelIDs, bool> _levelCompletion = new Dictionary<ELevelIDs, bool>();
    public bool HasSaveFile { get; private set; }

    public event Action OnDataLoaded;
    public event Action OnDataSaved;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeDictionary();
        LoadGame();
    }

    private void InitializeDictionary()
    {
        _levelCompletion.Clear();
        foreach (LevelSaveData lsd in _saveDataAsset.levelSaveMap)
        {
            _levelCompletion[lsd.levelID] = lsd.completed;
        }
    }

    private void SyncToScriptableObject()
    {
        foreach (var kvp in _levelCompletion)
        {
            LevelSaveData lsd = _saveDataAsset.GetLevelSaveData(kvp.Key);
            if (lsd != null)
                lsd.completed = kvp.Value;
        }
    }

    public bool CanSkipChapter0Level1 => IsLevelComplete(ELevelIDs.Chapter_0_Level_1);

    public void CompleteLevel(ELevelIDs levelID)
    {
        if (_levelCompletion.ContainsKey(levelID))
        {
            _levelCompletion[levelID] = true;
            SyncToScriptableObject();
            SaveGame();

            if (_debugMode)
                Debug.Log($"[SaveManager] Level completed: {levelID}");
        }
    }

    public bool IsLevelComplete(ELevelIDs levelID)
    {
        return _levelCompletion.ContainsKey(levelID) && _levelCompletion[levelID];
    }

    public void SaveGame()
    {
        try
        {
            // Convert Dictionary to List for serialization
            SaveFileDataAlt saveData = new SaveFileDataAlt();
            saveData.levelCompletionList = new List<LevelSaveEntry>();

            foreach (var kvp in _levelCompletion)
            {
                saveData.levelCompletionList.Add(new LevelSaveEntry
                {
                    levelID = kvp.Key,
                    completed = kvp.Value
                });
            }

            saveData.saveVersion = Application.version;
            saveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string jsonData = JsonUtility.ToJson(saveData, true);
            string filePath = GetSaveFilePath();

            // Ensure directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(filePath, jsonData);
            HasSaveFile = true;
            OnDataSaved?.Invoke();

            if (_debugMode)
                Debug.Log($"[SaveManager] Game saved to {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
        }
    }

    public void LoadGame()
    {
        string filePath = GetSaveFilePath();

        if (!File.Exists(filePath))
        {
            if (_debugMode)
                Debug.Log($"[SaveManager] No save file found. Starting fresh.");

            HasSaveFile = false;
            OnDataLoaded?.Invoke();
            return;
        }

        try
        {
            string jsonData = File.ReadAllText(filePath);
            SaveFileDataAlt saveData = JsonUtility.FromJson<SaveFileDataAlt>(jsonData);

            if (saveData != null && saveData.levelCompletionList != null)
            {
                // Reset and load
                _levelCompletion.Clear();
                foreach (var entry in saveData.levelCompletionList)
                {
                    _levelCompletion[entry.levelID] = entry.completed;
                }

                // Ensure all levels exist
                EnsureAllLevelsExist();
                SyncToScriptableObject();
                HasSaveFile = true;

                if (_debugMode)
                {
                    int completedCount = 0;
                    foreach (var kvp in _levelCompletion)
                        if (kvp.Value) completedCount++;

                    Debug.Log($"[SaveManager] Loaded {completedCount} completed levels");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load: {e.Message}");
            HasSaveFile = false;
        }

        OnDataLoaded?.Invoke();
    }

    public void DeleteSaveFile()
    {
        string filePath = GetSaveFilePath();
        if (File.Exists(filePath))
            File.Delete(filePath);

        ResetProgress();
    }

    public void ResetProgress()
    {
        foreach (var levelID in _levelCompletion.Keys)
            _levelCompletion[levelID] = false;

        SyncToScriptableObject();
        SaveGame();

        if (_debugMode)
            Debug.Log($"[SaveManager] Progress reset");
    }

    private string GetSaveFilePath()
    {
#if UNITY_EDITOR
        string saveDirectory = Path.Combine(Application.dataPath, "../Saves");
        if (!Directory.Exists(saveDirectory))
            Directory.CreateDirectory(saveDirectory);
        return Path.Combine(saveDirectory, _saveFileName);
#else
            string saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);
            return Path.Combine(saveDirectory, _saveFileName);
#endif
    }

    private void EnsureAllLevelsExist()
    {
        foreach (LevelSaveData lsd in _saveDataAsset.levelSaveMap)
        {
            if (!_levelCompletion.ContainsKey(lsd.levelID))
                _levelCompletion[lsd.levelID] = false;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}

[Serializable]
public class SaveFileDataAlt
{
    public List<LevelSaveEntry> levelCompletionList = new List<LevelSaveEntry>();
    public string saveVersion;
    public string saveDate;
}

[Serializable]
public class LevelSaveEntry
{
    public ELevelIDs levelID;
    public bool completed;
}