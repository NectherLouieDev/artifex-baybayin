using UnityEngine;
using DG.Tweening;

public class LazyRectMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Transform _pointA;
    [SerializeField] private Transform _pointB;

    [Header("Settings")]
    [SerializeField] private float _duration = 1f;
    [SerializeField] private bool _pingPong = true;
    [SerializeField] private bool _playOnStart = true;

    private Tween _currentTween;

    private void Start()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        if (_playOnStart)
            StartPingPong();
    }

    public void StartPingPong()
    {
        if (_pointA == null || _pointB == null) return;

        _currentTween?.Kill();

        _currentTween = _rectTransform.DOMove(_pointB.position, _duration)
            .SetEase(Ease.InOutQuad)
            .SetLoops(_pingPong ? -1 : 0, LoopType.Yoyo);
    }

    public void ResetToA()
    {
        _rectTransform.position = _pointA.position;
    }

    public void MoveToA(float delay = 0)
    {
        _currentTween?.Kill();
        _currentTween = _rectTransform
            .DOMove(_pointA.position, _duration)
            //.SetEase(Ease.InBack)
            .SetDelay(delay);
    }

    public void MoveToB(float delay = 0)
    {
        _currentTween?.Kill();
        _currentTween = _rectTransform
            .DOMove(_pointB.position, _duration)
            //.SetEase(Ease.OutBack)
            .SetDelay(delay);
    }

    public void Stop()
    {
        _currentTween?.Kill();
    }
}