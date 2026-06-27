using System;
using UnityEngine;

[Serializable]
public class Quest_Level_0_6
{
    public int blastBombCount = 0;
    public int targetCount = 0;
    public bool success = false;

    public void Reset()
    {
        blastBombCount = 0;
        success = false;
    }

    public string CountString()
    {
        return blastBombCount.ToString("D2");
    }

    public void IncrementCount(int value)
    {
        blastBombCount += value;
    }

    public bool CheckSuccess()
    {
        success = blastBombCount >= targetCount;
        return success;
    }
}
