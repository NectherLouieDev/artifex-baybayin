using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveConfiguration : MonoBehaviour
{
    [SerializeField] private WaveManager _waveManager;
    [SerializeField] private GameObject _baseBreakerPrefab;
    [SerializeField] private GameObject _hunterPrefab;

    private void Start()
    {
        // You can also configure waves in code if needed
        ConfigureTestWaves();
    }

    private void ConfigureTestWaves()
    {
        // This is just an example - you'd normally set this in the Inspector
        // But you can also do it programmatically
    }
}