using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData_WaveConfig", menuName = "Scriptable Objects/GameData_WaveConfig")]
public class GameData_WaveConfig : ScriptableObject
{
    [Header("Wave Configuration")]
    public string configName = "Default Wave Config";
    public List<GameData_EnemyWave> waves = new List<GameData_EnemyWave>();

    [Header("Global Settings")]
    public bool loopWaves = false;
    public int startWaveIndex = 0;

    [Header("Difficulty")]
    [Range(0f, 3f)]
    public float globalHealthMultiplier = 1f;
    [Range(0f, 3f)]
    public float globalSpeedMultiplier = 1f;
    [Range(0f, 2f)]
    public float globalSpawnRateMultiplier = 1f;

    [Header("Chapter Info")]
    public string chapterName = "Chapter 1";
    public Sprite chapterIcon;
    [TextArea(3, 5)]
    public string chapterDescription;

    public int TotalWaves => waves.Count;

    // Helper method to get a wave with global multipliers applied
    public GameData_EnemyWave GetWave(int index)
    {
        if (index < 0 || index >= waves.Count)
            return null;

        return waves[index];
    }
}
