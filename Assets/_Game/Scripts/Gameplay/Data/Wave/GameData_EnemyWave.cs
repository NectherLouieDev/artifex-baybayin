using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData_EnemyWave", menuName = "Scriptable Objects/GameData_EnemyWave")]
public class GameData_EnemyWave : ScriptableObject
{
    [Header("Wave Identification")]
    public string waveName = "Wave";
    public int waveNumber = 1;

    [Header("Spawn Settings")]
    public List<EnemySpawnEntry> enemiesToSpawn = new List<EnemySpawnEntry>();
    public float preWaveDelay = 2f; // Delay before wave starts spawning
    public float spawnInterval = 1f; // Time between each enemy spawn
    public bool spawnSimultaneously = false; // Spawn all enemies at once

    [Header("Wave Completion")]
    public float postWaveDelay = 3f; // Delay after wave completes before next wave

    [Header("Rewards")]
    public int scrapReward = 10;
    public int chemicalReward = 5;
    public int energyCoreReward = 0;

    [Header("Difficulty Scaling")]
    public float healthMultiplier = 1f;
    public float speedMultiplier = 1f;

    [Header("Visuals")]
    public Color waveColor = Color.white;
    public Sprite waveIcon;

    /// <summary>
    /// Reset this ScriptableObject to its original values
    /// Call this before starting a new game/wave
    /// </summary>
    public void ResetToInitialValues()
    {
        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            enemiesToSpawn[i].ResetToInitialValue();
        }
    }
}

[Serializable]
public class EnemySpawnEntry
{
    public GameData_EnemySpawn enemyData;
    [SerializeField] private int _count = 1;
    
    [NonSerialized] public int _runtimeCount;
    public int Count
    {
        get { return _runtimeCount; }
        set { _runtimeCount = value; }
    }

    public void ResetToInitialValue()
    {
        _runtimeCount = _count;
    }
}
