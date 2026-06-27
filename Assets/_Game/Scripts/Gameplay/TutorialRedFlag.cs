using UnityEngine;

public class TutorialRedFlag : MonoBehaviour
{
    private LevelManager _levelManager;

    private void Awake()
    {
        _levelManager = FindFirstObjectByType<LevelManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerIdentity identity))
        {            
            GameplayEventBus.Instance.Invoke(new GameplayEvents.Tutorial_RedFlagTriggered
            {
                Identity = _levelManager.Identity
            });
        }
    }
}
