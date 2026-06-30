using System.Collections.Generic;
using UnityEngine;

public class ArtifactSpawner : MonoBehaviour
{
    [SerializeField] private Data_ArtifactSpawnPoint data_ArtifactSpawnPoint;
    [SerializeField] private GameObject artifactPrefab;

    void Start()
    {
        SpawnArtifact();
    }

    public void SpawnArtifact()
    {
        int spawnIndex = data_ArtifactSpawnPoint.SelectSpawnIndex();
        Transform selectedPoint = data_ArtifactSpawnPoint.spawnPoints[spawnIndex];
        Instantiate(artifactPrefab, selectedPoint.position, Quaternion.identity);
        data_ArtifactSpawnPoint.AddPreviousSpawnIndex(spawnIndex);
    }
}
