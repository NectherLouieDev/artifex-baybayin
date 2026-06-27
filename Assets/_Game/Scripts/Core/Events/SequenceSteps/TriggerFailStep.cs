using System.Collections.Generic;
using UnityEngine;

public class TriggerFailStep : SequenceStep
{
    private LevelManager _levelManager;

    private void Awake()
    {
        _levelManager = FindFirstObjectByType<LevelManager>();
    }

    public override void Execute()
    {
        GameplayEventBus.Instance.Invoke(new GameplayEvents.Tutorial_RedFlagTriggered
        {
            Identity = _levelManager.Identity
        });

        Complete();
    }
}
