using DG.Tweening;
using UnityEngine;

public class TutorialLevelCanvasCompleteStep : SequenceStep
{
    [SerializeField] private CanvasGroup _successCanvasGroup;
    [SerializeField] private CanvasGroup _failCanvasGroup;
    [SerializeField] private bool _isFadeIn = true;
    [SerializeField] private float _fadeDuration = 0.25f;

    private bool _success = false;
    public void SetSuccess(bool  success)
    {
        _success = success;
    }

    public override void Execute()
    {
        CanvasGroup _cg = _success ? _successCanvasGroup : _failCanvasGroup;

        _cg.alpha = _isFadeIn ? 0 : 1;
        _cg.DOFade(_isFadeIn ? 1 : 0, _fadeDuration)
                .OnComplete(OnFadeComplete);
    }

    private void OnFadeComplete()
    {
        Complete();
    }
}
