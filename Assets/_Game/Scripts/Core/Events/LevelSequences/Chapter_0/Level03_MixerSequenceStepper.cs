using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level03_MixerSequenceStepper : SequenceStepper
{
    public override event EventHandler OnSequenceCompleted;

    [SerializeField] private EventSequencer _sequencer;
    [SerializeField] private List<SequenceStep> _steps = new List<SequenceStep>();

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.MixerSpawnedItem>(OnMixerSpawnedItem);

        // Add steps to sequencer
        _steps.ForEach(step =>
        {
            _sequencer.AddStep(step);
        });
    }

    private void OnMixerSpawnedItem(GameplayEvents.MixerSpawnedItem evt)
    {
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.MixerSpawnedItem>(OnMixerSpawnedItem);

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

        // Unsubscribe
        _sequencer.OnSequenceStarted -= Sequencer_OnSequenceStarted;
        _sequencer.OnStepStarted -= Sequencer_OnStepStarted;
        _sequencer.OnStepCompleted -= Sequencer_OnStepCompleted;
        _sequencer.OnSequenceCompleted -= Sequencer_OnSequenceCompleted;

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
