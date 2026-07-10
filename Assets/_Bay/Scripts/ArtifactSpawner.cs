using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactSpawner : MonoBehaviour
{
    [Header("Artifact Configuration")]
    [SerializeField] private GameObject artifactPrefab;
    [SerializeField] public List<Transform> spawnPoints = new List<Transform>();
    private List<int> previousSpawnIndices = new List<int>();

    [Header("Chest Configuration")]
    [SerializeField] private List<GameObject> chestPrefabs = new List<GameObject>();
    [SerializeField] private float raycastMaxDistance = 10f;
    [SerializeField] private LayerMask groundLayerMask = ~0; // Everything by default

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private BaybayinStone _stone;
    private List<GameObject> spawnedChests = new List<GameObject>();
    private int artifactSpawnIndex = -1;

    public void SpawnArtifact()
    {
        // Reset previous spawn indices when spawning new artifact
        previousSpawnIndices.Clear();
        spawnedChests.Clear();
        artifactSpawnIndex = -1;

        int spawnIndex = SelectSpawnIndex();
        Transform selectedPoint = spawnPoints[spawnIndex];

        GameObject g = Instantiate(artifactPrefab, selectedPoint.position + Vector3.up, selectedPoint.localRotation);

        if (g.TryGetComponent(out BaybayinStone stone))
        {
            _stone = stone;
            _stone.Spawn();
        }

        artifactSpawnIndex = spawnIndex;
        AddPreviousSpawnIndex(spawnIndex);

        if (showDebugLogs)
            Debug.Log($"Artifact spawned at index: {spawnIndex}");

        // Spawn chests after artifact
        SpawnChests();
    }

    #region Chest Spawning

    private void SpawnChests()
    {
        if (chestPrefabs == null || chestPrefabs.Count == 0)
        {
            Debug.LogWarning("No chest prefabs assigned! Skipping chest spawning.");
            return;
        }

        if (spawnPoints.Count <= 1)
        {
            Debug.LogWarning("Not enough spawn points to spawn chests!");
            return;
        }

        // Determine how many chests to spawn (one for each prefab)
        int numberOfChests = chestPrefabs.Count;
        int chestsSpawned = 0;
        int maxAttempts = numberOfChests * 10; // Prevent infinite loop
        int attempts = 0;

        // Create a list of available chest prefabs
        List<GameObject> availablePrefabs = new List<GameObject>(chestPrefabs);

        while (chestsSpawned < numberOfChests && attempts < maxAttempts && availablePrefabs.Count > 0)
        {
            attempts++;

            // Select a spawn point that isn't the artifact spawn point
            int spawnIndex = SelectChestSpawnIndex();

            if (spawnIndex == -1)
            {
                if (showDebugLogs)
                    Debug.LogWarning("No available spawn points for chests!");
                break;
            }

            Transform selectedPoint = spawnPoints[spawnIndex];

            // Get ground position via raycast
            Vector3 spawnPosition = GetGroundPosition(selectedPoint.position);

            // Select a random chest prefab from available ones
            int prefabIndex = Random.Range(0, availablePrefabs.Count);
            GameObject selectedPrefab = availablePrefabs[prefabIndex];

            // Remove from available list so each prefab is used once
            availablePrefabs.RemoveAt(prefabIndex);

            // Instantiate chest
            GameObject chest = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

            // Optional: Random rotation
            chest.transform.Rotate(Vector3.up, Random.Range(0f, 360f));

            // Store reference
            spawnedChests.Add(chest);

            // Mark this spawn point as used
            AddPreviousSpawnIndex(spawnIndex);

            chestsSpawned++;

            if (showDebugLogs)
                Debug.Log($"Chest {chestsSpawned} spawned at index: {spawnIndex}, position: {spawnPosition}, prefab: {selectedPrefab.name}");
        }

        if (showDebugLogs)
            Debug.Log($"Spawned {chestsSpawned} chests out of {numberOfChests} available");
    }

    private int SelectChestSpawnIndex()
    {
        // Get all spawn points that are not the artifact spawn point and not already used
        List<int> availableIndices = new List<int>();

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            // Skip artifact spawn point and previously used points
            if (i != artifactSpawnIndex && !previousSpawnIndices.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count == 0)
            return -1;

        return availableIndices[Random.Range(0, availableIndices.Count)];
    }

    private Vector3 GetGroundPosition(Vector3 position)
    {
        // Cast ray downward from the position
        RaycastHit hit;
        Vector3 rayOrigin = new Vector3(position.x, position.y + 5f, position.z);

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastMaxDistance, groundLayerMask))
        {
            if (showDebugLogs)
                Debug.Log($"Raycast hit ground at: {hit.point}, distance: {hit.distance}");

            return hit.point;
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning($"Raycast failed to find ground at {position}. Using original position.");

            // Fallback to original position with slight Y offset
            return new Vector3(position.x, position.y + 0.5f, position.z);
        }
    }

    #endregion

    #region Public Methods

    public void SpawnArtifactWithChests()
    {
        SpawnArtifact();
    }

    public void ClearChests()
    {
        foreach (GameObject chest in spawnedChests)
        {
            if (chest != null)
                Destroy(chest);
        }
        spawnedChests.Clear();
    }

    public void ClearAllSpawnedObjects()
    {
        ClearChests();

        if (_stone != null)
        {
            // Handle stone cleanup if needed
        }

        previousSpawnIndices.Clear();
        artifactSpawnIndex = -1;
    }

    public List<GameObject> GetSpawnedChests()
    {
        return spawnedChests;
    }

    public int GetArtifactSpawnIndex()
    {
        return artifactSpawnIndex;
    }

    public bool IsSpawnPointAvailable(int index)
    {
        if (index < 0 || index >= spawnPoints.Count)
            return false;

        return !previousSpawnIndices.Contains(index);
    }

    public int GetAvailableSpawnPointCount()
    {
        int count = 0;
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!previousSpawnIndices.Contains(i))
                count++;
        }
        return count;
    }

    public void AddChestPrefab(GameObject prefab)
    {
        if (prefab != null && !chestPrefabs.Contains(prefab))
        {
            chestPrefabs.Add(prefab);
        }
    }

    public void RemoveChestPrefab(GameObject prefab)
    {
        if (chestPrefabs.Contains(prefab))
        {
            chestPrefabs.Remove(prefab);
        }
    }

    public void ClearChestPrefabs()
    {
        chestPrefabs.Clear();
    }

    #endregion

    #region Original Methods (Preserved)

    public int SelectSpawnIndex()
    {
        // Use weighted selection to avoid repetition
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!previousSpawnIndices.Contains(i))
                availableIndices.Add(i);
        }

        if (availableIndices.Count == 0)
        {
            // If all indices are used, clear and start fresh
            previousSpawnIndices.Clear();
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                availableIndices.Add(i);
            }
        }

        int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
        return randomIndex;
    }

    public Transform SelectSpawnPoint()
    {
        int index = SelectSpawnIndex();
        return spawnPoints[index];
    }

    public void AddPreviousSpawnIndex(int index)
    {
        if (!previousSpawnIndices.Contains(index))
            previousSpawnIndices.Add(index);
    }

    public void HideStone()
    {
        if (_stone != null)
        {
            _stone.CameraTransform.gameObject.SetActive(false);
            _stone.Hide();
        }
    }

    public void RevealStone()
    {
        if (_stone != null)
        {
            _stone.Reveal();
        }
    }

    #endregion

    #region Debug Visualization

    void OnDrawGizmosSelected()
    {
        // Visualize spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.5f);

                    // Draw raycast line
                    Gizmos.color = Color.green;
                    Vector3 rayOrigin = new Vector3(point.position.x, point.position.y + 5f, point.position.z);
                    Gizmos.DrawLine(rayOrigin, new Vector3(point.position.x, point.position.y - 1f, point.position.z));
                    Gizmos.color = Color.blue;
                }
            }
        }
    }

    #endregion
}