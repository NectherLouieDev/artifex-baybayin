using System.Collections.Generic;
using UnityEngine;

public class ResetQuestDataStep : SequenceStep
{
    [SerializeField] private QuestData _data;

    public override void Execute()
    {
        _data.ResetQuestLevel0_6();
        _data.ResetQuestLevel0_8();
        _data.ResetQuestLevel0_10();

        Complete();
    }
}
