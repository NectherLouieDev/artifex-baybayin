using System;
using UnityEngine;

public abstract class SequenceStepper : MonoBehaviour
{
    public abstract event EventHandler OnSequenceCompleted;

    public abstract void StartSequence();
}
