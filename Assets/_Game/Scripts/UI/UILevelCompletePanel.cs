using DG.Tweening;
using UnityEngine;

public class UILevelCompletePanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup _levelCompleteCanvasGroup;

    private void Start()
    {
        //GameplayEventBus.Instance.Subscribe<GameplayEvents.AllWavesCompleted>(OnAllWavesCompleted);
    }

    private void OnAllWavesCompleted(GameplayEvents.AllWavesCompleted evt)
    {
        _levelCompleteCanvasGroup.DOFade(1, 0.25f);
    }
}
