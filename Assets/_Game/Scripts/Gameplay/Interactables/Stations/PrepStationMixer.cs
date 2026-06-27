using Microlight.MicroBar;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PrepStationMixer : InteractableObject
{
    [SerializeField] private RecipeDatabase _recipeDatabase;
    [SerializeField] private ItemCarrierComponent _prepCarrierComponentLeft;
    [SerializeField] private ItemCarrierComponent _prepCarrierComponentRight;
    [SerializeField] private MicroBar _durationBar;
    [SerializeField] private GGTimer _mixTimer;

    [Header("Feedbacks")]
    [SerializeField] MMFeedbacks _mixingFeedback;
    [SerializeField] MMFeedbacks _mixCompleteFeedback;

    private bool _isMixing = false;
    private GameData_Recipe _currentRecipe;

    private void Awake()
    {
        _recipeDatabase = FindFirstObjectByType<RecipeDatabase>();

        _mixTimer = gameObject.AddComponent<GGTimer>();
        _mixTimer.timerId = $"Mix Timer {gameObject.name}";
        _mixTimer.OnTimerCompleted += MixTimer_OnTimerCompleted;
        _mixTimer.OnTimerUpdated += MixTimer_OnTimerUpdated;

        _durationBar.gameObject.SetActive(false);
    }

    private ItemCarrierComponent GetEmptyCarrierComponent()
    {
        if (!_prepCarrierComponentLeft.IsCarrying)
        {
            return _prepCarrierComponentLeft;
        }

        if (!_prepCarrierComponentRight.IsCarrying)
        {
            return _prepCarrierComponentRight;
        }

        return null;
    }

    private ItemCarrierComponent GetActiveCarrierComponent()
    {
        if (_prepCarrierComponentLeft.IsCarrying)
        {
            return _prepCarrierComponentLeft;
        }

        if (_prepCarrierComponentRight.IsCarrying)
        {
            return _prepCarrierComponentRight;
        }

        return null;
    }

    public override void Interact(Transform interactor)
    {
        if (_isMixing)
            return;

        base.Interact(interactor);

        if (interactor.TryGetComponent(out ItemCarrierComponent actorCarrierComponent))
        {
            // actor has item he can drop
            if (actorCarrierComponent.IsCarrying)
            {
                // Drop it if not mixable
                if (!actorCarrierComponent.CarriedItem.GetItemConfig().isMixable)
                {
                    actorCarrierComponent.DropItem();
                    return;
                }

                // drop it on an empty spot
                ItemCarrierComponent emptyCarrierComponent = GetEmptyCarrierComponent();
                if (emptyCarrierComponent != null)
                {
                    ICarryable itemCarried = actorCarrierComponent.DropItem();
                    emptyCarrierComponent.PickupItem(itemCarried);
                }
                // no empty spots
                else
                {
                    // drop actor item
                    actorCarrierComponent.DropItem();
                }
            }
            // actor not carrying anything
            else
            {
                ItemCarrierComponent activeCarrierComponent = GetActiveCarrierComponent();
                if (activeCarrierComponent != null)
                {
                    // give it to actor
                    ICarryable stationItem = activeCarrierComponent.DropItem();
                    actorCarrierComponent.PickupItem(stationItem);
                }
                else
                {
                    // No item to pickup
                }
            }

            // Trigger Mixing if all carriers are active
            StartMixing();
        }
    }

    private void StartMixing()
    {
        if (_isMixing || !AreCarriersActive())
            return;

        // Get ingredients from carriers
        ERecipeInputType[] ingredients =
        {
            _recipeDatabase.ConverToRecipeInputType(_prepCarrierComponentLeft.CarriedItem),
            _recipeDatabase.ConverToRecipeInputType(_prepCarrierComponentRight.CarriedItem)
        };

        // Find matching recipe
        _currentRecipe = _recipeDatabase.GetRecipe(ingredients);

        if (_currentRecipe != null)
        {
            // Start timer
            Mix();
        }
        else
        {
            Debug.LogWarning("No valid recipe found for these ingredients!");
            // Play error feedback (sound, UI flash, etc.)
        }
    }

    private void Mix()
    {
        _isMixing = true;

        ConsumeIngredients();

        // Animation / Progress
        _durationBar.gameObject.SetActive(true);
        _durationBar.Initialize(_currentRecipe.craftingTime);
        _durationBar.UpdateBar(0);

        _mixTimer.StartTimer(_currentRecipe.craftingTime, 1);
    }

    private void MixTimer_OnTimerUpdated(object sender, GGTimer e)
    {
        _durationBar.UpdateBar(e.GetCurrentTime());
    }

    private void MixTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        _durationBar.gameObject.SetActive(false);

        SpawnResult();

        _isMixing = false;
    }

    private void ConsumeIngredients()
    {
        // Destroy or consume the carried items
        _prepCarrierComponentLeft.DespawnItem();
        _prepCarrierComponentRight.DespawnItem();

        // Feedback
        _mixingFeedback?.PlayFeedbacks();
    }

    private void SpawnResult()
    {
        _mixingFeedback?.StopFeedbacks();

        // Spawn the output prefab
        if (_currentRecipe.outputPrefab != null)
        {
            GameObject itemPreppedG = Instantiate(_currentRecipe.outputPrefab, transform.position + Vector3.up, Quaternion.identity);
            ICarryable itemPrepped = itemPreppedG.GetComponent<ICarryable>();

            _prepCarrierComponentLeft.PickupItem(itemPrepped);

            GameplayEventBus.Instance.Invoke(new GameplayEvents.MixerSpawnedItem
            {
                ItemPrepped = itemPrepped
            });
        }

        _mixCompleteFeedback?.PlayFeedbacks();

        // Play completion sound
        //if (_currentRecipe.craftingCompleteSound != null)
        //{
        //    AudioSource.PlayClipAtPoint(_currentRecipe.craftingCompleteSound, transform.position);
        //}
    }

    private bool AreCarriersActive()
    {
        return _prepCarrierComponentLeft != null &&
               _prepCarrierComponentLeft.CarriedItem != null &&
               _prepCarrierComponentRight != null &&
               _prepCarrierComponentRight.CarriedItem != null;
    }
}
