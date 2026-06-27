using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level03_PickupSequenceStepper : SequenceStepper
{
    public override event EventHandler OnSequenceCompleted;

    [SerializeField] private EventSequencer _sequencer;
    [SerializeField] private List<SequenceStep> _steps = new List<SequenceStep>();

    [Header("Custom Config")]
    [SerializeField] private ConditionalSpawnObjectStep _spawnObjectStep;
    [SerializeField] private DialogComponent _dialogComponent;

    private string _message;
    [Multiline]
    [SerializeField] private string[] _messages;
    
    private bool[] _triggerArray = {
        false,
        false,
        false,
        false,
        false
    };

    private int _messageIndex = 0;
    private ERecipeInputType _targetType = ERecipeInputType.Flour;
    private ERecipeInputType[] _conditionalTargets =
    {
        ERecipeInputType.Flour,
        ERecipeInputType.Chemical
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

        _messageIndex = 0;
        _targetType = _conditionalTargets[_messageIndex];
        _message = _messages[_messageIndex];
    }

    private void OnPlayerPickedupItem(GameplayEvents.PlayerPickedUpItem evt)
    {
        if (!(evt.ItemCarried is ItemIngredient))
            return;

        if (evt.Identity == null)
            return;

        if (_messageIndex > _messages.Length)
        {
            // Unsubscribe on the last message then do nothing
            GameplayEventBus.Instance.Unsubscribe<GameplayEvents.PlayerPickedUpItem>(OnPlayerPickedupItem);
            return;
        }

        ItemIngredient item = evt.ItemCarried as ItemIngredient;

        if (item.RecipeInputType == _targetType)
        {
            // Play Sequence and next is chemical
            string[] msgs = { _message };
            _dialogComponent.SetMessages(msgs);
            StartSequence();
        }
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

        _messageIndex++;
        if (_messageIndex < _messages.Length)
        {
            _targetType = _conditionalTargets[_messageIndex];
            _message = _messages[_messageIndex];
        }

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
