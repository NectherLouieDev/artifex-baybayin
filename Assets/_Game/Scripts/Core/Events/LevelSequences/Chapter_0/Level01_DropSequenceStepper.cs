using System;
using System.Collections.Generic;
using UnityEngine;

public class Level01_DropSequenceStepper : SequenceStepper
{
    public override event EventHandler OnSequenceCompleted;

    [SerializeField] private EventSequencer _sequencer;
    [SerializeField] private List<SequenceStep> _steps = new List<SequenceStep>();

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.PlayerDroppedItem>(OnPlayerDroppedItem);

        // Add steps to sequencer
        _steps.ForEach(step =>
        {
            _sequencer.AddStep(step);
        });
    }

    private void OnPlayerDroppedItem(GameplayEvents.PlayerDroppedItem evt)
    {
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.PlayerDroppedItem>(OnPlayerDroppedItem);

        StartSequence();
    }

    public override void StartSequence()
    {
        // Subscribe to events
        _sequencer.OnSequenceStarted += Sequencer_OnSequenceStarted;
        _sequencer.OnStepStarted += Sequencer_OnStepStarted;
        _sequencer.OnStepCompleted += Sequencer_OnStepCompleted;
        _sequencer.OnSequenceCompleted += Sequencer_OnSequenceCompleted;

        // Start the sequence
        _sequencer.StartSequence();
    }

    private void Sequencer_OnSequenceCompleted(object sender, System.EventArgs e)
    {
        Debug.Log("All steps completed!");
        OnSequenceCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void Sequencer_OnStepCompleted(object sender, int index)
    {
        Debug.Log($"Step {index} completed");
    }

    private void Sequencer_OnStepStarted(object sender, int index)
    {
        Debug.Log($"Step {index} started");
    }

    private void Sequencer_OnSequenceStarted(object sender, System.EventArgs e)
    {
        Debug.Log("Sequence started!");
    }
}
