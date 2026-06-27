using Microlight.MicroBar;
using UnityEngine;

public class AssemblyStation : InteractableObject
{
    [SerializeField] private RecipeDatabase _recipeDatabase;
    [SerializeField] private ItemCarrierComponent _carrierComponent;
    [SerializeField] private MicroBar _durationBar;
    [SerializeField] private GGTimer _assemblyTimer;

    private bool _isAssembling = false;
    private GameData_Recipe _currentRecipe;

    private void Awake()
    {
        _recipeDatabase = FindAnyObjectByType<RecipeDatabase>();

        _assemblyTimer = gameObject.AddComponent<GGTimer>();
        _assemblyTimer.timerId = $"Assembly Timer {gameObject.name}";
        _assemblyTimer.OnTimerCompleted += AssemblyTimer_OnTimerCompleted;
        _assemblyTimer.OnTimerUpdated += AssemblyTimer_OnTimerUpdated;

        _durationBar.gameObject.SetActive(false);
    }

    public override void Interact(Transform interactor)
    {
        if (_isAssembling)
            return;

        base.Interact(interactor);

        // Drop
        if (interactor.TryGetComponent(out ItemCarrierComponent actorCarrierComponent))
        {
            // actor is carrying
            if (actorCarrierComponent.IsCarrying)
            {
                // If item is not platable
                if (!actorCarrierComponent.CarriedItem.GetItemConfig().isPlatable)
                {
                    actorCarrierComponent.DropItem();
                    Debug.Log("Not ready for Assembly!");
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

                StartAssembling();
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

    private void StartAssembling()
    {
        if (_isAssembling)
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
            Assemble();
        }
        else
        {
            Debug.LogWarning("No valid recipe found for these prepped ingredients!");
            // Play error feedback (sound, UI flash, etc.)
        }
    }

    private void Assemble()
    {
        _isAssembling = true;

        ConsumeIngredients();

        // Animation / Progress
        _durationBar.gameObject.SetActive(true);
        _durationBar.Initialize(_currentRecipe.craftingTime);
        _durationBar.UpdateBar(0);

        _assemblyTimer.StartTimer(_currentRecipe.craftingTime, 1);
    }

    private void AssemblyTimer_OnTimerUpdated(object sender, GGTimer e)
    {
        _durationBar.UpdateBar(e.GetCurrentTime());
    }

    private void AssemblyTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        _durationBar.gameObject.SetActive(false);

        SpawnOutput();

        _isAssembling = false;
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
            GameObject itemAssembledG = Instantiate(_currentRecipe.outputPrefab, transform.position + Vector3.up, Quaternion.identity);
            ICarryable itemAssembled = itemAssembledG.GetComponent<ICarryable>();

            _carrierComponent.PickupItem(itemAssembled);

            GameplayEventBus.Instance.Invoke(new GameplayEvents.AssemblySpawnedItem
            {
                ItemAssembled = itemAssembled,
                Amount = 1
            });
        }
    }
}
