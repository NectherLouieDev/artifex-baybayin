using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using PixeLadder.EasyTransition;

// Enums
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Victory
}

// Data Classes
[System.Serializable]
public class MemoryStoneData
{
    public string stoneName;
    public Transform memoryTransform;
    public bool isActivated;
}

[System.Serializable]
public class GameSaveData
{
    public float totalPlayTime;
    public int currentAct;
    public int memoryStonesActivated;
    public bool artifactDiscovered;
    public Vector3 respawnPosition;
    public bool hasRespawnPoint;
}

public class GameManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction _pauseInputAction;
    [SerializeField] private InputAction _exitMenuInputAction;
    [SerializeField] private SceneLoader _sceneLoader;

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    [SerializeField] private int currentAct = 1;
    [SerializeField] private float totalPlayTime = 0f;
    [SerializeField] private bool isGamePaused = false;

    [Header("Respawn System")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 respawnPosition;
    [SerializeField] private bool hasRespawnPoint = false;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private GameObject playerPrefab;
    
    private PlayerIdentity currentPlayer;

    [Header("Memory Stones")]
    [SerializeField] private int memoryStonesActivated = 0;
    [SerializeField] private int totalMemoryStones = 4;
    [SerializeField] private List<MemoryStoneData> memoryStoneData = new List<MemoryStoneData>();

    [Header("Artifact")]
    [SerializeField] private ArtifactSpawner artifactSpawner;
    [SerializeField] private bool artifactDiscovered = false;
    [SerializeField] private TransitionEffect spawnTransitionEffect;

    [Header("Fog System Reference")]
    [SerializeField] private FogManager fogManager;

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text actText;
    [SerializeField] private TMP_Text playTimeText;
    [SerializeField] private TMP_Text memoryStoneProgressText;

    [Header("Audio")]
    [SerializeField] private AudioSource gameAudio;
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip memoryStoneActivationSound;
    [SerializeField] private AudioClip respawnSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Events
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<int> OnActChanged;
    public System.Action<int> OnMemoryStoneActivated;
    public System.Action<bool> OnArtifactDiscovered;
    public System.Action OnPlayerRespawned;

    // Singleton
    public static GameManager Instance;

    // Properties
    public GameState CurrentState => currentState;
    public int CurrentAct => currentAct;
    public bool IsGamePaused => isGamePaused;
    public Vector3 RespawnPosition => respawnPosition;
    public bool HasRespawnPoint => hasRespawnPoint;
    public int MemoryStonesActivated => memoryStonesActivated;
    public bool ArtifactDiscovered => artifactDiscovered;
    public PlayerIdentity CurrentPlayer => currentPlayer;

    // Timers
    private GGTimer respawnTimer;
    private GGTimer victoryTimer;
    private GGTimer spawnDelayTimer;
    private GGTimer spawnTransitionTimer;
    private GGTimer artifactSequenceTimer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        AudioManager.Instance.PlayBGM();

        // Initialize timers
        respawnTimer = gameObject.AddComponent<GGTimer>();
        respawnTimer.timerId = "RespawnTimer";
        respawnTimer.OnTimerCompleted += RespawnTimer_OnTimerCompleted;

        victoryTimer = gameObject.AddComponent<GGTimer>();
        victoryTimer.timerId = "VictoryTimer";
        victoryTimer.OnTimerCompleted += VictoryTimer_OnTimerCompleted;

        spawnDelayTimer = gameObject.AddComponent<GGTimer>();
        spawnDelayTimer.timerId = "Spawn Delayer";
        spawnDelayTimer.OnTimerCompleted += SpawnDelayTimer_OnTimerCompleted;

        spawnTransitionTimer = gameObject.AddComponent<GGTimer>();
        spawnTransitionTimer.timerId = "Spawn Transition Timer";
        spawnTransitionTimer.OnTimerCompleted += SpawnTransitionTimer_OnTimerCompleted;

        artifactSequenceTimer = gameObject.AddComponent<GGTimer>();
        artifactSequenceTimer.timerId = "ArtifactSequenceTimer";
        artifactSequenceTimer.OnTimerCompleted += ArtifactSequenceTimer_OnTimerCompleted;

        // Initialize
        respawnPosition = spawnPoint.position;
        hasRespawnPoint = false;
        memoryStonesActivated = 0;
        artifactDiscovered = false;

        // Hide game over and victory panels
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        spawnDelayTimer.StartTimer(0.1f, 1);
    }

    private void SpawnTransitionTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        SceneTransitioner.Instance.PlayTransition(null, spawnTransitionEffect);
    }

    private void SpawnDelayTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        artifactSpawner.SpawnArtifact();

        artifactSequenceTimer.StartTimer(7, 1);

        spawnTransitionTimer.StartTimer(5.75f, 1);
    }

    private void ArtifactSequenceTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        artifactSpawner.HideStone();
        
        // Find player
        FindPlayer();

        // Start gameplay music
        if (gameAudio != null && gameplayMusic != null)
        {
            gameAudio.clip = gameplayMusic;
            gameAudio.loop = true;
            gameAudio.Play();
        }

        UIManager.Instance.ShowFuelDisplay();
        UIManager.Instance.ShowQuestDisplay();

        // Update UI
        UpdateUI();

        ResumeGame();

        _pauseInputAction.performed += PauseInputAction_performed;
        _pauseInputAction.Enable();

        _exitMenuInputAction.performed += ExitMenuInputAction_performed;
    }

    private void ExitMenuInputAction_performed(InputAction.CallbackContext obj)
    {
        _exitMenuInputAction.performed -= ExitMenuInputAction_performed;
        _exitMenuInputAction.Disable();

        _pauseInputAction.performed -= PauseInputAction_performed;
        _pauseInputAction.Disable();

        ResumeGame();

        _sceneLoader.ChangeScene(ESceneID.MainMenu);
    }

    private void PauseInputAction_performed(InputAction.CallbackContext obj)
    {
        if (!IsGamePaused)
        {
            _exitMenuInputAction.Enable();
            PauseGame();
        }
        else
        {
            _exitMenuInputAction.Disable();
            ResumeGame();
        }

        UIManager.Instance.TogglePauseMenu();
    }

    public void EnablePauseInputAction()
    {
        _pauseInputAction.performed += PauseInputAction_performed;
        _pauseInputAction.Enable();
    }

    public void DisablePauseInputAction()
    {
        _pauseInputAction.performed -= PauseInputAction_performed;
        _pauseInputAction.Disable();
    }

    void Update()
    {
        if (currentState == GameState.Playing && !isGamePaused)
        {
            totalPlayTime += Time.deltaTime;

            // Update play time UI
            if (playTimeText != null)
            {
                TimeSpan time = TimeSpan.FromSeconds(totalPlayTime);
                playTimeText.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
            }
        }
    }

    #region Timer Event Handlers

    private void RespawnTimer_OnTimerCompleted(object sender, GGTimer timer)
    {
        // Hide game over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Respawn player
        if (currentPlayer != null)
        {
            // Reset player position
            currentPlayer.transform.position = respawnPosition;

            // Reset velocity if rigidbody exists
            Rigidbody rb = currentPlayer.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Reset health if player has health component
            HealthComponent health = currentPlayer.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.RespawnHealth();
            }

            // Reset lantern
            if (LanternManager.Instance != null)
            {
                // Refill some fuel on respawn
                LanternManager.Instance.RespawnFuel();
                LanternManager.Instance.TurnOnLantern();
            }

            // Play respawn sound
            if (respawnSound != null && gameAudio != null)
            {
                gameAudio.PlayOneShot(respawnSound);
            }

            if (showDebugLogs)
                Debug.Log($"Player respawned at {respawnPosition}");

            OnPlayerRespawned?.Invoke();

            // Show message
            UIManager.Instance?.ShowMessage("You have been revived!");
        }
        else
        {
            // If player lost, spawn new one
            SpawnPlayer(respawnPosition);
        }
    }

    private void VictoryTimer_OnTimerCompleted(object sender, GGTimer timer)
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        // Show completion message
        UIManager.Instance?.ShowMessage("🏆 You have recovered the Baybayin Stone!");
    }

    #endregion

    #region Player Management

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentPlayer = player.GetComponent<PlayerManager>().Identity;
            respawnPosition = player.transform.position;
            hasRespawnPoint = true;

            SetRespawnPoint(spawnPoint.position);
        }
        else if (playerPrefab != null)
        {
            // Spawn player if not found
            SpawnPlayer(spawnPoint.position);
        }
    }

    public void SetPlayer(GameObject player)
    {
        currentPlayer = player.GetComponent<PlayerManager>().Identity;
        respawnPosition = player.transform.position;
        hasRespawnPoint = true;
    }

    public void SpawnPlayer(Vector3 position)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab not assigned!");
            return;
        }

        GameObject playerGo = Instantiate(playerPrefab, position, Quaternion.identity);
        currentPlayer = playerGo.GetComponent<PlayerManager>().Identity;
        respawnPosition = position;
        hasRespawnPoint = true;

        SetRespawnPoint(respawnPosition);

        if (showDebugLogs)
            Debug.Log($"Player spawned at {position}");
    }

    #endregion

    #region Respawn System

    public void SetRespawnPoint(Vector3 position)
    {
        respawnPosition = position;
        hasRespawnPoint = true;

        if (showDebugLogs)
        {
            Debug.Log($"Respawn point set at {position}");
            Debug.Log($"Spawn point at {spawnPoint.position}");
        }

        // Save to PlayerPrefs for persistence
        PlayerPrefs.SetFloat("RespawnX", position.x);
        PlayerPrefs.SetFloat("RespawnY", position.y);
        PlayerPrefs.SetFloat("RespawnZ", position.z);
        PlayerPrefs.SetInt("HasRespawnPoint", 1);
        PlayerPrefs.Save();
    }

    public void RespawnPlayer()
    {
        if (!hasRespawnPoint)
        {
            // Use default spawn if no respawn point
            respawnPosition = new Vector3(0, 1, 0);
            hasRespawnPoint = true;
        }

        // Show game over
        //if (gameOverPanel != null)
        //{
        //    gameOverPanel.SetActive(true);
        //    if (gameOverSound != null && gameAudio != null)
        //    {
        //        gameAudio.PlayOneShot(gameOverSound);
        //    }
        //}

        // DO THIS
        // Disable Player Input
        // Animate Respawn Anim
        // Respawn

        // Start respawn timer
        if (!respawnTimer.IsRunning())
        {
            respawnTimer.StartTimer(respawnDelay, 1);
        }
        else
        {
            respawnTimer.ResetTimer();
        }
    }

    public void LoadSavedRespawnPoint()
    {
        if (PlayerPrefs.GetInt("HasRespawnPoint", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("RespawnX", 0f);
            float y = PlayerPrefs.GetFloat("RespawnY", 1f);
            float z = PlayerPrefs.GetFloat("RespawnZ", 0f);
            respawnPosition = new Vector3(x, y, z);
            hasRespawnPoint = true;
        }
    }

    #endregion

    #region End Scene
    public void GameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverSound != null && gameAudio != null)
            {
                gameAudio.PlayOneShot(gameOverSound);
            }
        }

        if (!IsGamePaused)
        {
            _exitMenuInputAction.Enable();

            _pauseInputAction.performed -= PauseInputAction_performed;
            _pauseInputAction.Disable();
            
            PauseGame();
        }
    }

    #endregion

    #region Memory Stone System

    public void ActivateMemoryStone(Vector3 position, string stoneName)
    {
        memoryStonesActivated++;

        // Set respawn point
        SetRespawnPoint(position);

        // Play activation sound
        if (memoryStoneActivationSound != null && gameAudio != null)
        {
            gameAudio.PlayOneShot(memoryStoneActivationSound);
        }

        // Update progress
        UpdateUI();

        // Check if all stones activated
        if (memoryStonesActivated >= totalMemoryStones)
        {
            OnAllMemoryStonesActivated();
        }

        OnMemoryStoneActivated?.Invoke(memoryStonesActivated);

        if (showDebugLogs)
            Debug.Log($"Memory Stone '{stoneName}' activated! ({memoryStonesActivated}/{totalMemoryStones})");
    }

    void OnAllMemoryStonesActivated()
    {
        if (showDebugLogs)
            Debug.Log("All Memory Stones activated!");

        UIManager.Instance.ShowMessage("All Memory Stones activated! The path to the artifact is revealed.");

        // Reveal artifact location
        artifactSpawner.RevealStone();
    }

    #endregion

    #region Artifact System

    public void DiscoverArtifact()
    {
        artifactDiscovered = true;

        // Update UI
        UpdateUI();

        // Play victory music
        if (gameAudio != null && victoryMusic != null)
        {
            gameAudio.clip = victoryMusic;
            gameAudio.loop = false;
            gameAudio.Play();
        }

        OnArtifactDiscovered?.Invoke(true);

        if (showDebugLogs)
            Debug.Log("Artifact discovered!");

        // Show victory screen after delay
        if (!victoryTimer.IsRunning())
        {
            victoryTimer.StartTimer(1f, 1);
        }
        else
        {
            victoryTimer.ResetTimer();
        }
    }

    #endregion

    #region Game State Management

    public void SetGameState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(newState);

        if (showDebugLogs)
            Debug.Log($"Game state changed to: {newState}");
    }

    public void SetAct(int act)
    {
        currentAct = act;
        OnActChanged?.Invoke(act);
        UpdateUI();

        UIManager.Instance?.ShowMessage($"Act {act}: {(act == 1 ? "The Awakening" : act == 2 ? "The Seeker" : "The Discovery")}");
    }

    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        Cursor.visible = false;
    }

    #endregion

    #region Scene Management

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    #region UI Updates

    void UpdateUI()
    {
        // Update act text
        if (actText != null)
        {
            string actName = currentAct == 1 ? "The Awakening" :
                            currentAct == 2 ? "The Seeker" :
                            "The Discovery";
            actText.text = $"Act {currentAct}: {actName}";
        }

        // Update memory stone progress
        if (memoryStoneProgressText != null)
        {
            memoryStoneProgressText.text = $"Memory Stones: {memoryStonesActivated}/{totalMemoryStones}";
        }

        UIManager.Instance.UpdateQuestText(memoryStonesActivated, totalMemoryStones);
    }

    #endregion

    #region Save/Load

    public GameSaveData GetSaveData()
    {
        GameSaveData data = new GameSaveData();
        data.totalPlayTime = totalPlayTime;
        data.currentAct = currentAct;
        data.memoryStonesActivated = memoryStonesActivated;
        data.artifactDiscovered = artifactDiscovered;
        data.respawnPosition = respawnPosition;
        data.hasRespawnPoint = hasRespawnPoint;
        return data;
    }

    public void LoadSaveData(GameSaveData data)
    {
        totalPlayTime = data.totalPlayTime;
        currentAct = data.currentAct;
        memoryStonesActivated = data.memoryStonesActivated;
        artifactDiscovered = data.artifactDiscovered;
        respawnPosition = data.respawnPosition;
        hasRespawnPoint = data.hasRespawnPoint;
        UpdateUI();
    }

    #endregion

    #region Debug

    [ContextMenu("Debug - Activate All Memory Stones")]
    void DebugActivateAllStones()
    {
        memoryStonesActivated = totalMemoryStones;
        UpdateUI();
        OnAllMemoryStonesActivated();
    }

    [ContextMenu("Debug - Discover Artifact")]
    void DebugDiscoverArtifact()
    {
        DiscoverArtifact();
    }

    [ContextMenu("Debug - Respawn Player")]
    void DebugRespawn()
    {
        RespawnPlayer();
    }

    #endregion
}