using System.Collections.Generic;
using UnityEngine;

public class StartWaveStep : SequenceStep
{
    [SerializeField] private WaveManager _waveManager;

    public override void Execute()
    {
        _waveManager.StartWaves();

        Complete();
    }
}
