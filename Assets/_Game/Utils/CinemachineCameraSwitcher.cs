using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraSwitcher : MonoBehaviour
{
    [Header("Camera Array")]
    [SerializeField] private CinemachineCamera[] _cameras;

    [Header("Timing Settings")]
    [SerializeField] private float _switchTime = 5f;
    [SerializeField] private bool _playOnStart = true;
    [SerializeField] private bool _randomizeStart = false;

    [Header("Loop Settings")]
    [SerializeField] private bool _loop = true;
    [SerializeField] private int _startingIndex = 0;

    private float _timer;
    private int _currentIndex = 0;
    private bool _isActive = false;

    private void Start()
    {
        if (_cameras == null || _cameras.Length == 0)
        {
            Debug.LogError("No cameras assigned to SimpleCameraTimer!");
            return;
        }

        // Set starting index
        if (_randomizeStart)
        {
            _currentIndex = Random.Range(0, _cameras.Length);
        }
        else
        {
            _currentIndex = Mathf.Clamp(_startingIndex, 0, _cameras.Length - 1);
        }

        SetActiveCamera(_currentIndex);
        _timer = _switchTime;
        _isActive = _playOnStart;
    }

    private void Update()
    {
        if (!_isActive) return;

        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            SwitchToNextCamera();
            _timer = _switchTime;
        }
    }

    /// <summary>
    /// Switch to the next camera in the array
    /// </summary>
    public void SwitchToNextCamera()
    {
        int nextIndex = _currentIndex + 1;

        if (nextIndex >= _cameras.Length)
        {
            if (_loop)
            {
                nextIndex = 0;
            }
            else
            {
                StopSwitching();
                return;
            }
        }

        SetActiveCamera(nextIndex);
        Debug.Log($"[Camera Timer] Switched to camera {nextIndex + 1}/{_cameras.Length}");
    }

    /// <summary>
    /// Switch to the previous camera in the array
    /// </summary>
    public void SwitchToPreviousCamera()
    {
        int prevIndex = _currentIndex - 1;

        if (prevIndex < 0)
        {
            if (_loop)
            {
                prevIndex = _cameras.Length - 1;
            }
            else
            {
                StopSwitching();
                return;
            }
        }

        SetActiveCamera(prevIndex);
        Debug.Log($"[Camera Timer] Switched to camera {prevIndex + 1}/{_cameras.Length}");
    }

    /// <summary>
    /// Switch to a specific camera by index
    /// </summary>
    public void SwitchToCamera(int index)
    {
        if (index < 0 || index >= _cameras.Length)
        {
            Debug.LogWarning($"Camera index {index} out of range (0-{_cameras.Length - 1})");
            return;
        }

        SetActiveCamera(index);
        _timer = _switchTime; // Reset timer
    }

    /// <summary>
    /// Switch to a specific camera by name
    /// </summary>
    public void SwitchToCamera(string cameraName)
    {
        for (int i = 0; i < _cameras.Length; i++)
        {
            if (_cameras[i].name == cameraName)
            {
                SwitchToCamera(i);
                return;
            }
        }

        Debug.LogWarning($"Camera named '{cameraName}' not found");
    }

    /// <summary>
    /// Set which camera is active (higher priority = active)
    /// </summary>
    private void SetActiveCamera(int index)
    {
        // Reset all cameras to priority 0
        for (int i = 0; i < _cameras.Length; i++)
        {
            if (_cameras[i] != null)
            {
                _cameras[i].Priority = 0;
            }
        }

        // Activate selected camera
        if (_cameras[index] != null)
        {
            _cameras[index].Priority = 10;
            _currentIndex = index;
        }
    }

    /// <summary>
    /// Start automatic switching
    /// </summary>
    public void StartSwitching()
    {
        _isActive = true;
        _timer = _switchTime;
    }

    /// <summary>
    /// Stop automatic switching (keeps current camera active)
    /// </summary>
    public void StopSwitching()
    {
        _isActive = false;
    }

    /// <summary>
    /// Reset to starting camera and restart timer
    /// </summary>
    public void ResetSwitching()
    {
        SetActiveCamera(_startingIndex);
        _timer = _switchTime;
        _isActive = _playOnStart;
    }

    /// <summary>
    /// Change the switch interval (seconds between switches)
    /// </summary>
    public void SetSwitchInterval(float seconds)
    {
        _switchTime = Mathf.Max(0.25f, seconds);
    }

    /// <summary>
    /// Get the currently active camera index
    /// </summary>
    public int GetCurrentCameraIndex()
    {
        return _currentIndex;
    }

    /// <summary>
    /// Get the currently active camera component
    /// </summary>
    public CinemachineCamera GetCurrentCamera()
    {
        return _cameras[_currentIndex];
    }

    /// <summary>
    /// Get total number of cameras
    /// </summary>
    public int GetCameraCount()
    {
        return _cameras.Length;
    }
}