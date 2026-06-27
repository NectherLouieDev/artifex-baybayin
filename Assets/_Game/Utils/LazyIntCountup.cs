using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class LazyIntCountup : MonoBehaviour
{
    private TMP_Text _targetText;
    private Tween _tween;

    private int _currentValue = 0;

    public void StartCountup(TMP_Text targetText, int targetValue, float duration = 2.5f, float delay = 0.5f, int initialValue = 0)
    {
        _targetText = targetText;
        _currentValue = initialValue;

        _tween?.Kill();
        _tween = DOTween.To(
            GetCurrentValue,
            SetCurrentValue,
            targetValue,
            duration)
            .SetDelay(delay)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() => _targetText.text = _currentValue.ToString("D6"));
    }

    private float GetCurrentValue()
    {
        return _currentValue;
    }

    private void SetCurrentValue(float value)
    {
        _currentValue = Mathf.RoundToInt(value);
    }
}
