using Microlight.MicroBar;
using UnityEngine;

public class CookStationFryer : InteractableObject
{
    [SerializeField] private RecipeDatabase _recipeDatabase;
    [SerializeField] private ItemCarrierComponent _carrierComponent;
    [SerializeField] private MicroBar _durationBar;
    [SerializeField] private GGTimer _fryTimer;

    private bool _isFrying = false;
    private GameData_Recipe _currentRecipe;

    private void Awake()
    {
        _recipeDatabase = FindFirstObjectByType<RecipeDatabase>();

        _fryTimer = gameObject.AddComponent<GGTimer>();
        _fryTimer.timerId = $"Fry Timer {gameObject.name}";
        _fryTimer.OnTimerCompleted += FryTimer_OnTimerCompleted;
        _fryTimer.OnTimerUpdated += FryTimer_OnTimerUpdated;

        _durationBar.gameObject.SetActive(false);
    }

    public override void Interact(Transform interactor)
    {
        if (_isFrying)
            return;

        base.Interact(interactor);

        // Drop
        if (interactor.TryGetComponent(out ItemCarrierComponent actorCarrierComponent))
        {
            // actor is carrying
            if (actorCarrierComponent.IsCarrying)
            {
                // If item is not fryable
                if (!actorCarrierComponent.CarriedItem.GetItemConfig().isFryable)
                {
                    actorCarrierComponent.DropItem();
                    Debug.Log("Not Fryable Item!");
                    return;
                }

                // if fryer has nothing
                if (!_carrierComponent.IsCarrying)
                {
                    ICarryable itemCarried = actorCarrierComponent.DropItem();
                    _carrierComponent.PickupItem(itemCarried);
                }
                else
                {
                    actorCarrierComponent.DropItem();
                }

                StartFrying();
            }
            // actor not carrying anything
            else
            {
                ICarryable stationItem = _carrierComponent.DropItem();
                if (stationItem != null)
                    actorCarrierComponent.PickupItem(stationItem);
            }
        }
    }

    private void StartFrying()
    {
        if (_isFrying)
            return;

        // Get ingredients from carriers
        ERecipeInputType[] ingredients =
        {
            _recipeDatabase.ConverToRecipeInputType(_carrierComponent.CarriedItem)
        };

        // Find recipe
        _currentRecipe = _recipeDatabase.GetRecipe(ingredients);

        if (_currentRecipe != null)
        {
            // Change to Timer
            Fry();
        }
        else
        {
            Debug.LogWarning("No valid recipe found for these prepped ingredients!");
            // Play error feedback (sound, UI flash, etc.)
        }
    }

    private void Fry()
    {
        _isFrying = true;

        ConsumeIngredients();

        // Animation / Progress
        _durationBar.gameObject.SetActive(true);
        _durationBar.Initialize(_currentRecipe.craftingTime);
        _durationBar.UpdateBar(0);

        _fryTimer.StartTimer(_currentRecipe.craftingTime, 1);
    }

    private void FryTimer_OnTimerUpdated(object sender, GGTimer e)
    {
        _durationBar.UpdateBar(e.GetCurrentTime());
    }

    private void FryTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        _durationBar.gameObject.SetActive(false);

        SpawnOutput();

        _isFrying = false;
    }

    private void ConsumeIngredients()
    {
        _carrierComponent.DespawnItem();

        // Feedbacks
    }

    private void SpawnOutput()
    {
        if (_currentRecipe.outputPrefab != null)
        {
            GameObject itemCookedG = Instantiate(_currentRecipe.outputPrefab, transform.position + Vector3.up, Quaternion.identity);
            ICarryable itemCooked = itemCookedG.GetComponent<ICarryable>();

            _carrierComponent.PickupItem(itemCooked);

            GameplayEventBus.Instance.Invoke(new GameplayEvents.FryerSpawnedItem
            {
                ItemCooked = itemCooked
            });
        }
    }
}
