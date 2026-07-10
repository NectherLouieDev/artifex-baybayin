using Microlight.MicroBar;
using MoreMountains.Feedbacks;
using UnityEngine;

public class KawayanChestStation : InteractableObject
{
    [SerializeField] private RecipeDatabase _recipeDatabase;
    [SerializeField] private ItemCarrierComponent _carrierComponent;
    [SerializeField] private MicroBar _durationBar;
    [SerializeField] private GGTimer _assemblyTimer;
    [SerializeField] private MMFeedbacks _spawnFeedbacks;
    [SerializeField] private Transform _chestLid;
    [SerializeField] private GameObject _icon;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private GameObject _particleObject;

    private bool _isOpening = false;
    private GameData_Recipe _currentRecipe;

    private bool _chestOpen = false;

    private void Awake()
    {
        _recipeDatabase = FindAnyObjectByType<RecipeDatabase>();

        _assemblyTimer = gameObject.AddComponent<GGTimer>();
        _assemblyTimer.timerId = $"Assembly Timer {gameObject.name}";
        _assemblyTimer.OnTimerCompleted += AssemblyTimer_OnTimerCompleted;
        _assemblyTimer.OnTimerUpdated += AssemblyTimer_OnTimerUpdated;

        _durationBar.gameObject.SetActive(false);
        _icon.SetActive(true);
    }

    public override void Interact(Transform interactor)
    {
        if (_isOpening)
            return;

        if (_chestOpen)
        {
            UIManager.Instance.ShowQuickMessage("No more Items!");
            return;
        }

        base.Interact(interactor);

        // Drop
        if (interactor.TryGetComponent(out ItemCarrierComponent actorCarrierComponent))
        {
            // actor is carrying
            if (actorCarrierComponent.IsCarrying)
            {
                // If item is not tnalak
                if (!actorCarrierComponent.CarriedItem.GetItemConfig().isKawayan)
                {
                    actorCarrierComponent.DropItem();
                    Debug.Log("Item cannot open Chest!");
                    UIManager.Instance.ShowQuickMessage("Item cannot open Chest!");
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

                StartOpeningChest();
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

    private void StartOpeningChest()
    {
        if (_isOpening)
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
            OpenChest();
        }
        else
        {
            Debug.LogWarning("No valid recipe found for these prepped ingredients!");
            //UIManager.Instance.ShowQuickMessage("No valid recipe found for these prepped ingredients!");
            // Play error feedback (sound, UI flash, etc.)
        }
    }

    private void OpenChest()
    {
        _isOpening = true;

        ConsumeIngredients();

        // Animation / Progress
        _durationBar.gameObject.SetActive(false);
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

        _spawnFeedbacks?.PlayFeedbacks();

        _chestLid.localRotation = Quaternion.Euler(-45, 0, 0);

        _icon.SetActive(false);

        _audioSource.Stop();

        _particleObject.SetActive(false);

        SpawnOutput();

        _isOpening = false;

        _chestOpen = true;
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

            //_carrierComponent.PickupItem(itemAssembled);
            _carrierComponent.DropItem();

            GameplayEventBus.Instance.Invoke(new GameplayEvents.AssemblySpawnedItem
            {
                ItemAssembled = itemAssembled,
                Amount = 1
            });
        }
    }
}
