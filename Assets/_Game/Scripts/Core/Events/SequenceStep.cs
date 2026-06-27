using System;
using UnityEngine;

public abstract class SequenceStep : MonoBehaviour
{
    [HideInInspector] public EventSequencer Sequencer;

    public event EventHandler CompleteSignal;

    public abstract void Execute();

    protected void Complete()
    {
        CompleteSignal?.Invoke(this, EventArgs.Empty);
        Sequencer?.OnStepComplete(this);
    }
}