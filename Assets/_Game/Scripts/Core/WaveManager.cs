using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the waves using Scriptable Objects
/// </summary>

public class WaveManager : MonoBehaviour
{
    // EventHandlers
    public class WaveStartedArgs : EventArgs
    {
        public int waveIndex = 0;
        public GameData_EnemyWave waveData;
    }
    public event EventHandler<WaveStartedArgs> OnWaveStarted;

    public class WaveCompletedArgs : EventArgs
    {
        public int waveIndex = 0;
        public GameData_EnemyWave waveData;
    }
    public event EventHandler<WaveCompletedArgs> OnWaveCompleted;
    public event EventHandler OnAllWavesCompleted;

    public class EnemySpawnedArgs : EventArgs
    {
        public int waveIndex = 0;
        public int enemiesRemaining = 0;
        public GameData_EnemySpawn enemySpawn;
    }
    public event EventHandler<EnemySpawnedArgs> OnEnemySpawned;

    [Header("Wave Configuration")]
    [SerializeField] private GameData_WaveConfig _waveConfig;
    [SerializeField] private int _startWaveIndex = 0;

    [Header("Spawner References")]
    [SerializeField] private List<CreepSpawner> _spawners = new List<CreepSpawner>();
    [SerializeField] private bool _useRandomSpawner = false;

    [Header("State")]
    [SerializeField] private bool _autoStartWaves = true;
    [SerializeField] private int _currentWaveIndex = -1;
    [SerializeField] private int _enemiesRemainingInWave = 0;
    [SerializeField] private int _totalEnemiesSpawned = 0;
    [SerializeField] private EWaveState _currentState = EWaveState.Idle;
    [SerializeField] private bool _levelCleared = false;

    // Private variables
    private GGTimer _waveTimer;
    private GGTimer _spawnTimer;
    private int _spawnIndex = 0;
    private int _totalEnemiesInWave = 0;
    private List<GameObject> _activeEnemies = new List<GameObject>();
    private GameData_EnemyWave _currentWaveData;
    private List<EnemySpawnEntry> _currentWaveSpawnQueue = new List<EnemySpawnEntry>();

    // Properties
    public EWaveState  CurrentState { get { return _currentState; } }
    public int CurrentWaveIndex { get { return _currentWaveIndex; } }
    public GameData_EnemyWave CurrentWaveData { get { return _currentWaveData; } }
    public int TotalWaves { get { return _waveConfig.TotalWaves; } }
    public int ActiveEnemyCount { get { return _activeEnemies.Count; } }
    public int EnemiesRemainingInWave { get { return _enemiesRemainingInWave; } }
    public float WaveProgress
    {
        get
        {
            return _totalEnemiesInWave > 0 ?
                (float)(_totalEnemiesInWave - _enemiesRemainingInWave) / _totalEnemiesInWave : 0f;
        }
    }
    public bool LevelCleared { get { return _levelCleared; } }
    private bool _allWavesCompleted = false;

    private void Start()
    {
        // Create timers
        _waveTimer = gameObject.AddComponent<GGTimer>();
        _spawnTimer = gameObject.AddComponent<GGTimer>();

        // Subscribe to timer events
        _waveTimer.OnTimerCompleted += WaveTimer_OnTimerCompleted;
        _waveTimer.OnTimerUpdated += WaveTimer_OnTimerUpdated;
        _waveTimer.OnTimerLoop += WaveTimer_OnTimerLoop;

        _spawnTimer.OnTimerCompleted += SpawnTimer_OnTimerCompleted;
        _spawnTimer.OnTimerLoop += SpawnTimer_OnTimerLoop;

        if (_autoStartWaves)
        {
            StartWaves();
        }
    }

    private void Update()
    {
        if (_levelCleared)
            return;

        _levelCleared = _allWavesCompleted && ActiveEnemyCount <= 0;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (_waveTimer != null)
        {
            _waveTimer.OnTimerCompleted -= WaveTimer_OnTimerCompleted;
            _waveTimer.OnTimerUpdated -= WaveTimer_OnTimerUpdated;
            _waveTimer.OnTimerLoop -= WaveTimer_OnTimerLoop;
        }

        if (_spawnTimer != null)
        {
            _spawnTimer.OnTimerCompleted -= SpawnTimer_OnTimerCompleted;
            _spawnTimer.OnTimerLoop -= SpawnTimer_OnTimerLoop;
        }
    }

    #region Public API

    /// <summary>
    /// Start the wave sequence
    /// </summary>
    public void StartWaves()
    {
        List<GameData_EnemyWave> wavesToUse = GetWaveList();

        if (wavesToUse.Count == 0)
        {
            Debug.LogWarning("No waves configured in WaveManager");
            return;
        }

        foreach (var wave in wavesToUse)
        {
            if (wave != null)
                wave.ResetToInitialValues();
        }

        _currentWaveIndex = _startWaveIndex - 1;
        NextWave();
    }

    /// <summary>
    /// Start a specific wave by index
    /// </summary>
    public void StartWave(int waveIndex)
    {
        List<GameData_EnemyWave> wavesToUse = GetWaveList();

        if (waveIndex < 0 || waveIndex >= wavesToUse.Count)
        {
            Debug.LogError($"Invalid wave index: {waveIndex}");
            return;
        }

        _currentWaveIndex = waveIndex;
        BeginWavePreparation();
    }

    /// <summary>
    /// Stop all wave activity
    /// </summary>
    public void StopWaves()
    {
        if (_waveTimer != null)
            _waveTimer.StopTimer();

        if (_spawnTimer != null)
            _spawnTimer.StopTimer();

        _currentState = EWaveState.Idle;

        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        _activeEnemies.Clear();
    }

    /// <summary>
    /// Pause the current wave
    /// </summary>
    public void PauseWaves()
    {
        if (_spawnTimer != null && _spawnTimer.IsRunning())
            _spawnTimer.StopTimer();

        // Pause all enemies
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null)
            {
                var agent = enemy.GetComponent<CreepAgentComponent>();
                if (agent != null)
                    agent.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Resume the current wave
    /// </summary>
    public void ResumeWaves()
    {
        if (_currentState == EWaveState.Spawning && _spawnTimer != null)
        {
            _spawnTimer.ResetTimer();
        }

        // Resume all enemies
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null)
            {
                var agent = enemy.GetComponent<CreepAgentComponent>();
                if (agent != null)
                    agent.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Register an enemy with the wave manager
    /// </summary>
    public void RegisterEnemy(GameObject enemy)
    {
        if (!_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);

            // Subscribe to enemy death event
            HealthComponent creepHealthComponent = enemy.GetComponent<HealthComponent>();
            if (creepHealthComponent != null)
            {
                creepHealthComponent.OnDeath += CreepHealthComponent_OnDeath;
            }
        }
    }

    #endregion

    #region Wave Flow Control

    private List<GameData_EnemyWave> GetWaveList()
    {
        return _waveConfig.waves;
    }

    private bool GetLoopWaves()
    {
        return _waveConfig.loopWaves;
    }

    private void NextWave()
    {
        List<GameData_EnemyWave> wavesToUse = GetWaveList();
        bool loopWaves = GetLoopWaves();

        _currentWaveIndex++;

        if (_currentWaveIndex >= wavesToUse.Count)
        {
            if (loopWaves)
            {
                _currentWaveIndex = 0;
            }
            else
            {
                CompleteAllWaves();
                return;
            }
        }

        BeginWavePreparation();
    }

    private void BeginWavePreparation()
    {
        _currentState = EWaveState.Preparing;
        List<GameData_EnemyWave> wavesToUse = GetWaveList();
        _currentWaveData = wavesToUse[_currentWaveIndex];

        Debug.Log($"Preparing wave {_currentWaveIndex + 1}: {_currentWaveData.waveName}");

        GameplayEventBus.Instance.Invoke(new GameplayEvents.WaveStatePreparing
        {
            CurrentWaveNumber = _currentWaveIndex + 1,
            MaxWaveNumber = wavesToUse.Count
        });

        // Build spawn queue
        BuildSpawnQueue();

        // Calculate total enemies
        _totalEnemiesInWave = 0;
        foreach (var entry in _currentWaveSpawnQueue)
        {
            _totalEnemiesInWave += entry.Count;
        }
        _enemiesRemainingInWave = _totalEnemiesInWave;

        // Start pre-wave delay
        if (_currentWaveData.preWaveDelay > 0)
        {
            _waveTimer.StartTimer(_currentWaveData.preWaveDelay, 1, true);
        }
        else
        {
            StartWaveSpawning();
        }
    }

    private void BuildSpawnQueue()
    {
        _currentWaveSpawnQueue.Clear();

        foreach (var entry in _currentWaveData.enemiesToSpawn)
        {
            if (entry.enemyData != null && entry.enemyData._prefab != null)
            {
                _currentWaveSpawnQueue.Add(entry);
            }
            else
            {
                Debug.LogWarning($"Invalid enemy spawn entry in wave {_currentWaveData.waveName}");
            }
        }
    }

    private void StartWaveSpawning()
    {
        _currentState = EWaveState.Spawning;

        Debug.Log($"Starting wave {_currentWaveIndex + 1}: {_currentWaveData.waveName}");

        // Fire wave start event
        OnWaveStarted?.Invoke(this, new WaveStartedArgs
        {
            waveIndex = _currentWaveIndex,
            waveData = _currentWaveData
        });

        if (_currentWaveData.spawnSimultaneously)
        {
            SpawnAllEnemiesSimultaneously();
            _currentState = EWaveState.Active;
        }
        else
        {
            _spawnIndex = 0;
            _spawnTimer.StartTimer(_currentWaveData.spawnInterval, 0, false);
        }
    }

    private void CompleteWave()
    {
        _currentState = EWaveState.Completed;

        Debug.Log($"Wave {_currentWaveIndex + 1} completed!");

        OnWaveCompleted?.Invoke(this, new WaveCompletedArgs
        {
            waveIndex = _currentWaveIndex,
            waveData = _currentWaveData
        });

        // Award resources
        AwardWaveRewards();

        // Check for next wave
        List<GameData_EnemyWave> wavesToUse = GetWaveList();
        bool loopWaves = GetLoopWaves();

        if (_currentWaveIndex + 1 < wavesToUse.Count || loopWaves)
        {
            if (_currentWaveData.postWaveDelay > 0)
            {
                _waveTimer.StartTimer(_currentWaveData.postWaveDelay, 1, true);
            }
            else
            {
                NextWave();
            }
        }
        else
        {
            CompleteAllWaves();
        }
    }

    private void CompleteAllWaves()
    {
        _currentState = EWaveState.Completed;
        Debug.Log("All waves completed!");

        _allWavesCompleted = true;

        OnAllWavesCompleted?.Invoke(this, EventArgs.Empty);

        GameplayEventBus.Instance.Invoke(new GameplayEvents.AllWavesCompleted
        {
            WaveCount = _currentWaveIndex
        });
    }

    private void AwardWaveRewards()
    {
        // Find resource manager and add rewards
        Debug.Log($"Awarded: {_currentWaveData.scrapReward} Scrap, {_currentWaveData.chemicalReward} Chemicals");
    }

    #endregion

    #region Spawning Logic

    private void SpawnAllEnemiesSimultaneously()
    {
        foreach (var entry in _currentWaveSpawnQueue)
        {
            for (int i = 0; i < entry.Count; i++)
            {
                SpawnEnemy(entry);
            }
        }
    }

    private void SpawnNextEnemy()
    {
        if (_spawnIndex >= _currentWaveSpawnQueue.Count)
        {
            _spawnTimer.StopTimer();
            _currentState = EWaveState.Active;

            if (_activeEnemies.Count == 0)
            {
                CompleteWave();
            }
            return;
        }

        EnemySpawnEntry currentEntry = _currentWaveSpawnQueue[_spawnIndex];

        SpawnEnemy(currentEntry);

        currentEntry.Count--;

        if (currentEntry.Count <= 0)
        {
            _spawnIndex++;
        }

        _enemiesRemainingInWave--;
        OnEnemySpawned?.Invoke(this, new EnemySpawnedArgs
        {
            waveIndex = _currentWaveIndex,
            enemiesRemaining = _enemiesRemainingInWave,
            enemySpawn = currentEntry.enemyData
        });
    }

    private void SpawnEnemy(EnemySpawnEntry spawnEntry)
    {
        if (spawnEntry.enemyData == null || spawnEntry.enemyData._prefab == null)
        {
            Debug.LogError("Invalid enemy data or prefab");
            return;
        }

        CreepSpawner selectedSpawner = GetSpawner();
        if (selectedSpawner == null)
        {
            Debug.LogError("No spawner available");
            return;
        }

        // Apply difficulty multipliers
        ApplyDifficultyMultipliers(spawnEntry.enemyData);

        GameObject enemy = selectedSpawner.SpawnEnemy(spawnEntry.enemyData._prefab);

        if (enemy != null)
        {
            RegisterEnemy(enemy);
            _totalEnemiesSpawned++;
        }
    }

    private void ApplyDifficultyMultipliers(GameData_EnemySpawn enemyData)
    {
        // This would apply health/speed multipliers to the enemy
        // Pass these to the enemy component or enemy config component
    }

    private CreepSpawner GetSpawner()
    {
        if (_spawners.Count == 0)
            return null;

        if (_useRandomSpawner)
        {
            return _spawners[UnityEngine.Random.Range(0, _spawners.Count)];
        }
        else
        {
            int index = _totalEnemiesSpawned % _spawners.Count;
            return _spawners[index];
        }
    }

    #endregion

    #region Event Handlers

    private void WaveTimer_OnTimerUpdated(object sender, GGTimer e)
    {
        GameplayEventBus.Instance.Invoke(new GameplayEvents.PreWaveTimerUpdated
        {
            CurrentTime = e.GetCurrentTime(),
            TargetTime = e.TargetTime
        });
    }

    private void WaveTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        if (_currentState == EWaveState.Preparing)
        {
            StartWaveSpawning();
        }
        else if (_currentState == EWaveState.Completed)
        {
            NextWave();
        }
    }

    private void SpawnTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        // This shouldn't happen with infinite loop, but just in case
        if (_currentState == EWaveState.Spawning)
        {
            _spawnTimer.ResetTimer();
        }
    }

    private void WaveTimer_OnTimerLoop(object sender, GGTimer e)
    {
        if (_currentState == EWaveState.Spawning)
        {
            SpawnNextEnemy();
        }
    }

    private void SpawnTimer_OnTimerLoop(object sender, GGTimer e)
    {
        if (_currentState == EWaveState.Spawning)
        {
            SpawnNextEnemy();
        }
    }

    private void CreepHealthComponent_OnDeath(object sender, HealthComponent e)
    {
        if (_activeEnemies.Contains(e.gameObject))
        {
            _activeEnemies.Remove(e.gameObject);
            e.OnDeath -= CreepHealthComponent_OnDeath;

            GameplayEventBus.Instance.Invoke(new GameplayEvents.CreepDeath
            {
                StateHandler = e.GetComponent<CreepStateHandler>(),
                CreepsLeft = _activeEnemies.Count
            });

            if (e.TryGetComponent(out CreepStateHandler stateHandler))
            {
                stateHandler.KillCreep();
            }

            if (_currentState == EWaveState.Active && _activeEnemies.Count == 0)
            {
                CompleteWave();
            }
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Get information about the current wave
    /// </summary>
    public string GetWaveStatus()
    {
        return $"Wave: {_currentWaveIndex + 1}/{TotalWaves} | State: {_currentState} | " +
               $"Active: {_activeEnemies.Count} | Remaining: {_enemiesRemainingInWave}";
    }

    /// <summary>
    /// Get remaining enemy count (active + yet to spawn)
    /// </summary>
    public int GetTotalRemainingEnemies()
    {
        return _activeEnemies.Count + _enemiesRemainingInWave;
    }

    #endregion
}