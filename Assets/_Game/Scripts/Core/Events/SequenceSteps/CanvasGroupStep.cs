using DG.Tweening;
using UnityEngine;

public class CanvasGroupStep : SequenceStep
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private bool _isFadeIn = true;
    [SerializeField] private float _fadeDuration = 0.25f;

    public override void Execute()
    {
        _canvasGroup.alpha = _isFadeIn ? 0 : 1;
        _canvasGroup.DOFade(_isFadeIn ? 1 : 0, _fadeDuration)
                .OnComplete(OnFadeComplete);
    }

    private void OnFadeComplete()
    {
        Complete();
    }
}
