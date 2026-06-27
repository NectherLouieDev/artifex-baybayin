using System.Collections.Generic;
using UnityEngine;

public class StartTimerStep : SequenceStep
{
    [SerializeField] private CountdownTimer _timer;

    public override void Execute()
    {
        _timer.StartTimer();

        Complete();
    }
}
