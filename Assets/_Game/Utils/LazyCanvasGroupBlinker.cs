using UnityEngine;
using UnityEngine.Events;

public class LazyCanvasGroupBlinker : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float _blinkSpeed = 2f; // Blinks per second
    [SerializeField] private float _minAlpha = 0f;
    [SerializeField] private float _maxAlpha = 1f;
    [SerializeField] private bool _playOnStart = false;
    [SerializeField] private bool _useUnscaledTime = false; // For UI that ignores time scale

    [Header("Pulse Mode (Smooth)")]
    [SerializeField] private bool _usePulseMode = false; // Smooth sine wave instead of square wave
    [SerializeField] private bool _pingPong = true; // For pulse mode: go back and forth

    [Header("Events")]
    [SerializeField] private UnityEvent _onBlinkVisible;
    [SerializeField] private UnityEvent _onBlinkHidden;
    [SerializeField] private bool _invert = false; // Start invisible then visible

    private CanvasGroup _canvasGroup;
    private float _timer = 0f;
    private bool _isVisible = true;
    private bool _isBlinking = false;
    private float _blinkDuration; // Half-cycle duration

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _blinkDuration = 1f / _blinkSpeed; // Time for one full cycle (visible -> hidden -> visible)

        if (_playOnStart)
        {
            StartBlinking();
        }
        else if (!_invert)
        {
            // Ensure starting state is visible
            _canvasGroup.alpha = _maxAlpha;
            _isVisible = true;
        }
        else
        {
            _canvasGroup.alpha = _minAlpha;
            _isVisible = false;
        }
    }

    private void Update()
    {
        if (!_isBlinking) return;

        float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (_usePulseMode)
        {
            // Smooth sine wave pulse
            float t = _timer * _blinkSpeed * Mathf.PI * 2; // Convert to radians
            float alpha;

            if (_pingPong)
            {
                // Sine wave between min and max
                alpha = Mathf.Lerp(_minAlpha, _maxAlpha, (Mathf.Sin(t) + 1f) / 2f);
            }
            else
            {
                // One-direction pulse (0 -> 1 -> 0 using abs(sin))
                alpha = Mathf.Lerp(_minAlpha, _maxAlpha, Mathf.Abs(Mathf.Sin(t)));
            }

            _canvasGroup.alpha = alpha;

            // Detect edge crossings for events
            bool currentlyVisible = alpha > (_minAlpha + _maxAlpha) / 2f;
            if (currentlyVisible != _isVisible)
            {
                _isVisible = currentlyVisible;
                if (_isVisible)
                    _onBlinkVisible?.Invoke();
                else
                    _onBlinkHidden?.Invoke();
            }

            _timer += deltaTime;
        }
        else
        {
            // Square wave mode (hard blink)
            _timer += deltaTime;
            float halfCycle = _blinkDuration / 2f;

            bool currentlyVisible = (_timer % _blinkDuration) < halfCycle;

            if (currentlyVisible != _isVisible)
            {
                _isVisible = currentlyVisible;
                _canvasGroup.alpha = _isVisible ? _maxAlpha : _minAlpha;

                if (_isVisible)
                    _onBlinkVisible?.Invoke();
                else
                    _onBlinkHidden?.Invoke();
            }
        }
    }

    /// <summary>
    /// Start the blinking effect
    /// </summary>
    public void StartBlinking()
    {
        _isBlinking = true;
        _timer = 0f;

        // Reset to proper starting state based on invert setting
        if (!_invert)
        {
            _isVisible = true;
            _canvasGroup.alpha = _maxAlpha;
        }
        else
        {
            _isVisible = false;
            _canvasGroup.alpha = _minAlpha;
        }
    }

    /// <summary>
    /// Stop blinking and optionally set final alpha
    /// </summary>
    public void StopBlinking(bool setVisible = true)
    {
        _isBlinking = false;
        _canvasGroup.alpha = setVisible ? _maxAlpha : _minAlpha;
        _isVisible = setVisible;
    }

    /// <summary>
    /// Pause blinking (keeps current alpha)
    /// </summary>
    public void PauseBlinking()
    {
        _isBlinking = false;
    }

    /// <summary>
    /// Resume blinking from current state
    /// </summary>
    public void ResumeBlinking()
    {
        _isBlinking = true;
    }

    /// <summary>
    /// Set blink speed (blinks per second)
    /// </summary>
    public void SetBlinkSpeed(float blinksPerSecond)
    {
        _blinkSpeed = Mathf.Max(0.1f, blinksPerSecond);
        _blinkDuration = 1f / _blinkSpeed;
    }

    /// <summary>
    /// Set alpha range for blinking
    /// </summary>
    public void SetAlphaRange(float min, float max)
    {
        _minAlpha = Mathf.Clamp01(min);
        _maxAlpha = Mathf.Clamp01(max);
    }

    /// <summary>
    /// Force immediate visibility
    /// </summary>
    public void ForceVisible()
    {
        if (!_isBlinking)
        {
            _canvasGroup.alpha = _maxAlpha;
        }
        _isVisible = true;
    }

    /// <summary>
    /// Force immediate hidden
    /// </summary>
    public void ForceHidden()
    {
        if (!_isBlinking)
        {
            _canvasGroup.alpha = _minAlpha;
        }
        _isVisible = false;
    }
}