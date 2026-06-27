using System;
using UnityEngine;

[Serializable]
public class Quest_Level_0_8
{
    public int breakableCount = 0;
    public int targetCount = 0;
    public bool success = false;

    public void Reset()
    {
        breakableCount = 0;
        success = false;
    }

    public string CountString()
    {
        return breakableCount.ToString("D2");
    }

    public void IncrementCount(int value)
    {
        breakableCount += value;
    }

    public bool CheckSuccess()
    {
        success = breakableCount >= targetCount;
        return success;
    }
}
