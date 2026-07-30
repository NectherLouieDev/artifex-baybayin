using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SessionResponse
{
    public string sessionToken;
    public string signinUrl;
}

[System.Serializable]
public class SessionStatusResponse
{
    public string status; // "pending", "authorized", "expired"
}

[System.Serializable]
public class ArtifactUnlockResponse
{
    public bool success;
    public string message;
    public string artifactId;
}

public class PlatformIntegration : MonoBehaviour
{
    [Header("GameOn Configuration")]
    [SerializeField] private string gameId = ""; // Must be configured in Inspector
    [SerializeField] private string apiBaseUrl = "https://gameonportal.ph";
    [SerializeField] private float pollInterval = 3f;
    [SerializeField] private float maxPollTime = 120f;

    // Public properties
    public string sessionToken { get; private set; } = "";
    public bool isAuthorized { get; private set; } = false;
    public bool isPolling { get; private set; } = false;

    // Events for other systems to subscribe to
    public event Action OnAuthorized;
    public event Action OnExpired;
    public event Action<string> OnError;
    public event Action OnArtifactUnlocked;
    public event Action OnSessionCreated;

    // Singleton
    private static PlatformIntegration _instance;
    public static PlatformIntegration Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<PlatformIntegration>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlatformIntegration");
                    _instance = go.AddComponent<PlatformIntegration>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private Coroutine pollCoroutine;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Validate configuration
        if (string.IsNullOrEmpty(gameId))
        {
            Debug.LogError("PlatformIntegration: Game ID is not configured! Please set it in the Inspector.");
        }
    }

    void OnDestroy()
    {
        if (pollCoroutine != null)
        {
            StopCoroutine(pollCoroutine);
            pollCoroutine = null;
        }
    }

    #region Public Methods

    /// <summary>
    /// Start the authentication flow by creating a session
    /// </summary>
    public void StartSession()
    {
        if (isAuthorized)
        {
            Debug.LogWarning("PlatformIntegration: Already authorized!");
            OnAuthorized?.Invoke();
            return;
        }

        if (isPolling)
        {
            Debug.LogWarning("PlatformIntegration: Already polling for authorization!");
            return;
        }

        if (string.IsNullOrEmpty(gameId))
        {
            OnError?.Invoke("Game ID not configured!");
            return;
        }

        StartCoroutine(CreateSession());
    }

    /// <summary>
    /// Unlock the artifact (call when game conditions are met)
    /// </summary>
    public void UnlockArtifact()
    {
        if (!isAuthorized)
        {
            Debug.LogWarning("PlatformIntegration: Cannot unlock artifact - not authorized!");
            OnError?.Invoke("Not authorized!");
            return;
        }

        if (string.IsNullOrEmpty(sessionToken))
        {
            Debug.LogWarning("PlatformIntegration: Cannot unlock artifact - no session token!");
            OnError?.Invoke("No session token!");
            return;
        }

        StartCoroutine(UnlockArtifactCoroutine());
    }

    /// <summary>
    /// Check if the player is currently authorized
    /// </summary>
    public bool IsAuthorized()
    {
        return isAuthorized;
    }

    /// <summary>
    /// Get the current session token (use with caution)
    /// </summary>
    public string GetSessionToken()
    {
        return sessionToken;
    }

    /// <summary>
    /// Reset the integration state
    /// </summary>
    public void ResetSession()
    {
        isAuthorized = false;
        isPolling = false;
        sessionToken = "";

        if (pollCoroutine != null)
        {
            StopCoroutine(pollCoroutine);
            pollCoroutine = null;
        }

        Debug.Log("PlatformIntegration: Session reset");
    }

    #endregion

    #region API Calls

    /// <summary>
    /// Create a session with the GameOn API
    /// </summary>
    IEnumerator CreateSession()
    {
        Debug.Log($"PlatformIntegration: Creating session for game ID: {gameId}");

        string url = $"{apiBaseUrl}/api/session";
        string jsonBody = $"{{\"gameId\":\"{gameId}\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                
                Debug.Log($"PlatformIntegration: Session response: {responseJson}");

                try
                {
                    SessionResponse response = JsonUtility.FromJson<SessionResponse>(responseJson);

                    if (!string.IsNullOrEmpty(response.sessionToken) && !string.IsNullOrEmpty(response.signinUrl))
                    {
                        sessionToken = response.sessionToken;
                        OnSessionCreated?.Invoke();

                        // Open sign-in URL in browser
                        Application.OpenURL(response.signinUrl);

                        // Start polling for authorization
                        if (pollCoroutine != null)
                        {
                            StopCoroutine(pollCoroutine);
                        }
                        pollCoroutine = StartCoroutine(PollAuthorization());

                        Debug.Log($"PlatformIntegration: Session created! Polling for authorization...");
                    }
                    else
                    {
                        OnError?.Invoke("Invalid response from server");
                        Debug.LogError("PlatformIntegration: Invalid session response");
                    }
                }
                catch (Exception e)
                {
                    OnError?.Invoke($"Failed to parse response: {e.Message}");
                    Debug.LogError($"PlatformIntegration: Failed to parse session response: {e.Message}");
                }
            }
            else
            {
                string errorMsg = $"Failed to create session: {request.error}";
                OnError?.Invoke(errorMsg);
                Debug.LogError($"PlatformIntegration: {errorMsg}");
            }
        }
    }

    /// <summary>
    /// Poll the API for authorization status
    /// </summary>
    IEnumerator PollAuthorization()
    {
        if (string.IsNullOrEmpty(sessionToken))
        {
            Debug.LogError("PlatformIntegration: Cannot poll - no session token!");
            yield break;
        }

        isPolling = true;
        float elapsedTime = 0f;

        while (elapsedTime < maxPollTime)
        {
            yield return new WaitForSecondsRealtime(pollInterval);
            elapsedTime += pollInterval;

            if (string.IsNullOrEmpty(sessionToken))
            {
                Debug.LogError("PlatformIntegration: Session token lost during polling!");
                break;
            }

            Debug.Log($"PlatformIntegration: Polling authorization... ({(int)elapsedTime}s/{maxPollTime}s)");

            string url = $"{apiBaseUrl}/api/session";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", $"Bearer {sessionToken}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseJson = request.downloadHandler.text;
                    Debug.Log($"PlatformIntegration: Poll response: {responseJson}");

                    try
                    {
                        SessionStatusResponse response = JsonUtility.FromJson<SessionStatusResponse>(responseJson);

                        if (response.status == "authorized")
                        {
                            isAuthorized = true;
                            isPolling = false;
                            OnAuthorized?.Invoke();

                            Debug.Log($"PlatformIntegration: Authorized successfully!");

                            yield break;
                        }
                        else if (response.status == "expired")
                        {
                            isPolling = false;
                            OnExpired?.Invoke();
                            Debug.LogWarning($"PlatformIntegration: Session expired");
                            yield break;
                        }
                        // else "pending" - continue polling
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"PlatformIntegration: Failed to parse poll response: {e.Message}");
                    }
                }
                else
                {
                    // Handle 401 specially - session might have expired
                    if (request.responseCode == 401)
                    {
                        isPolling = false;
                        OnExpired?.Invoke();
                        Debug.LogWarning($"PlatformIntegration: Session expired (401)");
                        yield break;
                    }

                    Debug.LogWarning($"PlatformIntegration: Poll failed: {request.error}");
                    // Continue polling on network errors
                }
            }
        }

        // Timeout
        isPolling = false;
        OnExpired?.Invoke();
        Debug.LogWarning($"PlatformIntegration: Polling timed out after {maxPollTime}s");
    }

    /// <summary>
    /// Unlock the artifact via GameOn API
    /// </summary>
    IEnumerator UnlockArtifactCoroutine()
    {
        Debug.Log($"PlatformIntegration: Unlocking artifact...");

        string url = $"{apiBaseUrl}/api/artifacts/unlock";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes("{}");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {sessionToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log($"PlatformIntegration: Artifact unlock response: {responseJson}");

                try
                {
                    ArtifactUnlockResponse response = JsonUtility.FromJson<ArtifactUnlockResponse>(responseJson);

                    if (response.success)
                    {
                        Debug.Log($"PlatformIntegration: Artifact unlocked successfully!");
                        OnArtifactUnlocked?.Invoke();
                    }
                    else
                    {
                        OnError?.Invoke($"Artifact unlock failed: {response.message}");
                        Debug.LogError($"PlatformIntegration: Artifact unlock failed: {response.message}");
                    }
                }
                catch (Exception e)
                {
                    OnError?.Invoke($"Failed to parse unlock response: {e.Message}");
                    Debug.LogError($"PlatformIntegration: Failed to parse unlock response: {e.Message}");
                }
            }
            else
            {
                string errorMsg = $"Artifact unlock failed: {request.error}";
                OnError?.Invoke(errorMsg);
                Debug.LogError($"PlatformIntegration: {errorMsg}");
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Check if the platform supports touch input
    /// </summary>
    public static bool IsTouchSupported()
    {
#if UNITY_ANDROID || UNITY_IOS
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// Check if running on mobile platform
    /// </summary>
    public static bool IsMobilePlatform()
    {
#if UNITY_ANDROID || UNITY_IOS
        return true;
#else
        return false;
#endif
    }

    #endregion
}