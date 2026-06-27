using System;
using UnityEngine;

[Serializable]
public class Quest_Level_0_10
{
    public int repairCount = 0;
    public int targetCount = 0;
    public bool success = false;

    public void Reset()
    {
        repairCount = 0;
        success = false;
    }

    public string CountString()
    {
        return repairCount.ToString("D2");
    }

    public void IncrementCount(int value)
    {
        repairCount += value;
    }

    public bool CheckSuccess()
    {
        success = repairCount >= targetCount;
        return success;
    }
}
