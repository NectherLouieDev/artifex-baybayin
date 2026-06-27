using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level02_PickupSequenceStepper : SequenceStepper
{
    public override event EventHandler OnSequenceCompleted;

    [SerializeField] private EventSequencer _sequencer;
    [SerializeField] private List<SequenceStep> _steps = new List<SequenceStep>();

    [Header("Custom Config")]
    [SerializeField] private ConditionalSpawnObjectStep _spawnObjectStep;
    [SerializeField] private DialogComponent _dialogComponent;
    [Multiline]
    [SerializeField] private string[] _flourMessages;
    [Multiline]
    [SerializeField] private string[] _chemicalMessages;
    [Multiline]
    [SerializeField] private string[] _spiceMessages;
    [Multiline]
    [SerializeField] private string[] _metalMessages;
    [Multiline]
    [SerializeField] private string[] _crystalMessages;

    private bool[] _triggerArray = {
        false,
        false,
        false,
        false,
        false
    };

    public bool AllTriggered { get { return !_triggerArray.Contains(false); } }

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.PlayerPickedUpItem>(OnPlayerPickedupItem);

        // Add steps to sequencer
        _steps.ForEach(step =>
        {
            _sequencer.AddStep(step);
        });
    }

    private void OnPlayerPickedupItem(GameplayEvents.PlayerPickedUpItem evt)
    {
        if (!(evt.ItemCarried is ItemIngredient))
            return;

        ItemIngredient item = evt.ItemCarried as ItemIngredient;
        Debug.Log(item.RecipeInputType);

        switch(item.RecipeInputType)
        {
            case ERecipeInputType.Flour:
                _dialogComponent.SetMessages(_flourMessages);
                _triggerArray[0] = true;
                break;
            case ERecipeInputType.Chemical:
                _dialogComponent.SetMessages(_chemicalMessages);
                _triggerArray[1] = true;
                break;
            case ERecipeInputType.Spice:
                _dialogComponent.SetMessages(_spiceMessages);
                _triggerArray[2] = true;
                break;
            case ERecipeInputType.Metal:
                _dialogComponent.SetMessages(_metalMessages);
                _triggerArray[3] = true;
                break;
            case ERecipeInputType.Crystal:
                _dialogComponent.SetMessages(_crystalMessages);
                _triggerArray[4] = true;
                break;
        }

        bool conditional = !_triggerArray.Contains(false);
        _spawnObjectStep.SetConditional(conditional);

        //GameplayEventBus.Instance.Unsubscribe<GameplayEvents.PlayerPickedUpItem>(OnPlayerPickedupItem);
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
