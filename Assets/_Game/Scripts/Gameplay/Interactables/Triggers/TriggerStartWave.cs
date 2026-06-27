using UnityEngine;

public class TriggerStartWave : MonoBehaviour
{
    private WaveManager _waveManager;

    private void Start()
    {
        _waveManager = FindFirstObjectByType<WaveManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerIdentity identity))
        {
            Debug.Log("Wave Started ----");
            _waveManager.StartWaves();
        }
    }
}
