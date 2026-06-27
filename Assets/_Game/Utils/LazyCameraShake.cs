using DG.Tweening;
using UnityEngine;

public class LazyCameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float defaultDuration = 0.5f;
    [SerializeField] private float defaultStrength = 0.5f;

    private Vector3 originalPosition;
    private Tween shakeTween;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void Shake(float duration = -1, float strength = -1)
    {
        if (defaultStrength == 0 && defaultDuration == 0)
            return;

        duration = duration > 0 ? duration : defaultDuration;
        strength = strength > 0 ? strength : defaultStrength;

        shakeTween?.Kill();
        transform.localPosition = originalPosition;

        shakeTween = transform.DOShakePosition(duration, strength, vibrato: 1, randomness: 40f, fadeOut: true)
            .OnComplete(() => transform.localPosition = originalPosition);
    }

    public void StopShake()
    {
        shakeTween?.Kill();
        transform.localPosition = originalPosition;
    }
}