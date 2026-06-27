using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelSaveData
{
    public ELevelIDs levelID;
    public bool completed = false;
}

[CreateAssetMenu(fileName = "SaveData", menuName = "Scriptable Objects/SaveData")]
public class SaveData : ScriptableObject
{
    public List<LevelSaveData> levelSaveMap = new List<LevelSaveData>();

    public LevelSaveData GetLevelSaveData(ELevelIDs levelID)
    {
        foreach (LevelSaveData lsd in levelSaveMap)
        {
            if (lsd.levelID == levelID)
                return lsd;
        }

        return null;
    }

    // Reset all save data
    public void ResetAllData()
    {
        foreach (LevelSaveData lsd in levelSaveMap)
        {
            lsd.completed = false;
        }
    }
}
