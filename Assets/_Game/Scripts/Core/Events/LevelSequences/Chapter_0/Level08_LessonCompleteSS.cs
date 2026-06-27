using System;
using System.Collections.Generic;
using UnityEngine;

public class Level08_LessonCompleteSS : SequenceStepper
{
    public override event EventHandler OnSequenceCompleted;

    [SerializeField] private EventSequencer _sequencer;
    [SerializeField] private List<SequenceStep> _steps = new List<SequenceStep>();

    [Header("Level Config")]
    [SerializeField] private TutorialLevelCanvasCompleteStep _tutorialLevelCanvasCompleteStep;
    [SerializeField] private TutorialCompleteLevelStep _tutorialCompleteStep;

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.Tutorial00_GreenFlagTriggered>(OnGreenFlagTriggered);
        GameplayEventBus.Instance.Subscribe<GameplayEvents.Tutorial_RedFlagTriggered>(OnRedFlagTriggered);

        // Add steps to sequencer
        _steps.ForEach(step =>
        {
            _sequencer.AddStep(step);
        });
    }

    private void OnGreenFlagTriggered(GameplayEvents.Tutorial00_GreenFlagTriggered evt)
    {
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.Tutorial00_GreenFlagTriggered>(OnGreenFlagTriggered);
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.Tutorial_RedFlagTriggered>(OnRedFlagTriggered);

        _tutorialLevelCanvasCompleteStep.SetSuccess(true);
        _tutorialCompleteStep.SetSuccess(true);

        StartSequence();
    }

    private void OnRedFlagTriggered(GameplayEvents.Tutorial_RedFlagTriggered evt)
    {
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.Tutorial00_GreenFlagTriggered>(OnGreenFlagTriggered);
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.Tutorial_RedFlagTriggered>(OnRedFlagTriggered);

        _tutorialLevelCanvasCompleteStep.SetSuccess(false);
        _tutorialCompleteStep.SetSuccess(false);

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
