using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactSpawner : MonoBehaviour
{
    [SerializeField] private GameObject artifactPrefab;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] public List<Transform> spawnPoints = new List<Transform>();
    private List<int> previousSpawnIndices = new List<int>();

    public Transform CameraTransform {  get { return cameraTransform; } }

    public void SpawnArtifact()
    {
        int spawnIndex = SelectSpawnIndex();
        Transform selectedPoint = spawnPoints[spawnIndex];
        
        GameObject g = Instantiate(artifactPrefab, selectedPoint.position + Vector3.up, selectedPoint.localRotation);
        
        if (g.TryGetComponent(out BaybayinStone stone))
        {
            stone.Spawn();
            cameraTransform = stone.CameraTransform;
        }

        AddPreviousSpawnIndex(spawnIndex);
    }

    public int SelectSpawnIndex()
    {
        // Use weighted selection to avoid repetition
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!previousSpawnIndices.Contains(i))
                availableIndices.Add(i);
        }

        int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];

        return randomIndex;
    }

    public Transform SelectSpawnPoint()
    {
        // Use weighted selection to avoid repetition
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!previousSpawnIndices.Contains(i))
                availableIndices.Add(i);
        }

        int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];

        return spawnPoints[randomIndex];
    }

    public void AddPreviousSpawnIndex(int index)
    {
        previousSpawnIndices.Add(index);
    }
}
