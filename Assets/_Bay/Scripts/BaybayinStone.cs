using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;

public class BaybayinStone : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private List<GameObject> _visuals = new List<GameObject>();
    [SerializeField] private List<GameObject> _lights = new List<GameObject>();
    [SerializeField] private MMFeedbacks spawnFeedback;

    private GGTimer _completeTimer;

    public Transform CameraTransform {  get { return _cameraTransform; } }

    private void Awake()
    {
        _completeTimer = gameObject.AddComponent<GGTimer>();
        _completeTimer.OnTimerCompleted += CompleteTimer_OnTimerCompleted;
    }

    private void CompleteTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        foreach (GameObject go in _visuals)
        {
            go.SetActive(true);
        }
    }

    public void Spawn()
    {
        foreach (GameObject go in _visuals)
        {
            go.SetActive(false);
        }

        spawnFeedback?.PlayFeedbacks();

        _completeTimer.StartTimer(0.2f, 1);
    }

    public void Hide()
    {
        foreach (GameObject go in _visuals)
        {
            go.SetActive(false);
        }

        foreach (GameObject go in _lights)
        {
            go.SetActive(false);
        }
    }

    public void Reveal()
    {
        foreach (GameObject go in _visuals)
        {
            go.SetActive(true);
        }

        foreach (GameObject go in _lights)
        {
            go.SetActive(true);
        }
    }
}
