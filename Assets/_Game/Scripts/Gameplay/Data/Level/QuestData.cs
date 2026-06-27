using System;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    public Quest_Level_0_6 questLevel0_6;
    public Quest_Level_0_8 questLevel0_8;
    public Quest_Level_0_10 questLevel0_10;

    public void ResetQuestLevel0_6()
    {
        questLevel0_6.Reset();
    }

    public void ResetQuestLevel0_8()
    {
        questLevel0_8.Reset();
    }

    public void ResetQuestLevel0_10()
    {
        questLevel0_10.Reset();
    }
}
