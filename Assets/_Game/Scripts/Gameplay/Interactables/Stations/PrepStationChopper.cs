using Microlight.MicroBar;
using MoreMountains.Feedbacks;
using UnityEngine;

public class PrepStationChopper : InteractableObject
{
    [SerializeField] private RecipeDatabase _recipeDatabase;
    [SerializeField] private ItemCarrierComponent _prepCarrierComponent;
    [SerializeField] private MicroBar _durationBar;
    [SerializeField] private GGTimer _chopTimer;

    [Header("Chop Config")]
    [SerializeField] private int _chopCount = 0;
    [SerializeField] private int _maxChopCount = 3;

    [Header("Feedbacks")]
    [SerializeField] MMFeedbacks _chopFeedback;
    [SerializeField] MMFeedbacks _chopCompleteFeedback;

    private bool _isChopping = false;
    private bool _outputSpawned = false;
    private GameData_Recipe _currentRecipe;

    private void Awake()
    {
        _recipeDatabase = FindFirstObjectByType<RecipeDatabase>();

        _chopTimer = gameObject.AddComponent<GGTimer>();
        _chopTimer.timerId = $"Chop Timer {gameObject.name}";
        _chopTimer.OnTimerCompleted += ChopTimer_OnTimerCompleted;
        _chopTimer.OnTimerUpdated += ChopTimer_OnTimerUpdated;

        _durationBar.gameObject.SetActive(false);
    }

    public override void Interact(Transform interactor)
    {
        if (_isChopping)
            return;

        base.Interact(interactor);

        if (interactor.TryGetComponent(out ItemCarrierComponent actorCarrierComponent))
        {
            // If interactor has an item then drop item here
            if (actorCarrierComponent.IsCarrying)
            {
                // If item is not choppable then drop it out
                if (!actorCarrierComponent.CarriedItem.GetItemConfig().isChoppable)
                {
                    actorCarrierComponent.DropItem();
                    return;
                }

                // if station has no item and is choppable
                if (!_prepCarrierComponent.IsCarrying)
                {
                    _outputSpawned = false;

                    ICarryable itemCarried = actorCarrierComponent.DropItem();
                    _prepCarrierComponent.PickupItem(itemCarried);
                }
                else
                {
                    actorCarrierComponent.DropItem();
                }
            }
            // actor not carrying anything
            else
            {
                // If station has item then chop 1, 2, 3, Spawn
                if (_prepCarrierComponent.IsCarrying)
                {
                    // pickup if item has been spawned
                    if (_outputSpawned)
                    {
                        ICarryable stationItem = _prepCarrierComponent.DropItem();
                        
                        if (stationItem != null)
                            actorCarrierComponent.PickupItem(stationItem);
                    }
                    else
                    {
                        // chop if item ingredient
                        StartChopping();
                    }
                }
            }
        }
    }

    private void StartChopping()
    {
        if (_isChopping)
            return;

        if (_prepCarrierComponent.CarriedItem is ItemIngredient)
        {
            ERecipeInputType[] ingredients =
            {
                _recipeDatabase.ConverToRecipeInputType(_prepCarrierComponent.CarriedItem)
            };

            _currentRecipe = _recipeDatabase.GetRecipe(ingredients);

            if (_currentRecipe != null)
            {
                // Start timer
                Chop();
            }
            else
            {
                Debug.LogWarning("No valid recipe found for these ingredients!");
                // Play error feedback (sound, UI flash, etc.)
            }
        }
    }

    private void Chop()
    {
        _isChopping = true;

        _durationBar.gameObject.SetActive(true);
        _durationBar.Initialize(_currentRecipe.craftingTime);
        _durationBar.UpdateBar(1);

        _chopTimer.StartTimer(_currentRecipe.craftingTime, 1);
    }
    private void ChopTimer_OnTimerUpdated(object sender, GGTimer e)
    {
        float _maxCraftingTime = _currentRecipe.craftingTime;
        float _currentTimeInPercentage = e.GetCurrentTime() / _maxCraftingTime;
        
        _durationBar.UpdateBar(_currentTimeInPercentage);
    }

    private void ChopTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        _durationBar.gameObject.SetActive(false);

        _chopCount++;

        if (_chopCount >= _maxChopCount)
        {
            _chopCount = 0;

            ConsumeIngredients();

            SpawnOutput();
        }
        
        _isChopping = false;
    }
    private void ConsumeIngredients()
    {
        // Destroy or consume the carried items
        _prepCarrierComponent.DespawnItem();

        // Feedback
        _chopFeedback?.PlayFeedbacks();
    }

    private void SpawnOutput()
    {
        _chopFeedback?.StopFeedbacks();

        // Spawn the output prefab
        if (_currentRecipe.outputPrefab != null)
        {
            GameObject itemChoppedG = Instantiate(_currentRecipe.outputPrefab, transform.position + Vector3.up, Quaternion.identity);
            ICarryable itemChopped = itemChoppedG.GetComponent<ICarryable>();

            _prepCarrierComponent.PickupItem(itemChopped);

            _outputSpawned = true;

            GameplayEventBus.Instance.Invoke(new GameplayEvents.ChopperSpawnedItem
            {
                ItemChopped = itemChopped
            });
        }

        _chopCompleteFeedback?.PlayFeedbacks();
    }

}
